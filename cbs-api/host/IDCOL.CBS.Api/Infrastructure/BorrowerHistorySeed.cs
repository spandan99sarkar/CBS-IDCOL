using System.Text.Json;
using IDCOL.CBS.CreditSanction.Domain;
using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
using IDCOL.CBS.PartyKyc.Domain;
using IDCOL.CBS.ProductConfig.Domain;
using IDCOL.CBS.RepaymentEngine.Application.Seeding;
using IDCOL.CBS.RepaymentEngine.Domain;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IDCOL.CBS.Api.Infrastructure;

/// <summary>
/// Dev-only seed of the real historical schedule for all 19 IDCOL borrowers - the original (as
/// sanctioned) schedule plus every reschedule/restructure/prepayment/rate-change/moratorium event
/// extracted from "Loan Repayment Schedule (The Heart)". Each borrower becomes a real, browsable
/// Customer -> Sanction -> Facility (one per lender tranche) -> FacilityVersion chain, so the
/// reschedule/restructure capability is exercised with the actual historical data rather than only
/// proven in the abstract. Guarded the same way as the rest of <see cref="Program"/>'s dev
/// bootstrap: Development environment + Database:UseSqliteForLocalDevelopment only, and only runs
/// once (skipped if any Facility already exists).
/// </summary>
public static class BorrowerHistorySeed
{
    private static readonly string[] BorrowerKeys =
    {
        "BPCL", "CWTP", "DHRL", "DPL", "EKCL", "GHEL", "HYDRON", "IHL", "KPCL", "KZFL",
        "MAGBL", "MCML", "NSGL", "PABL", "PPL", "QPSL", "SCBL", "SKS", "THERMAX",
    };

    private static readonly JsonSerializerOptions FileOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task SeedAsync(LoanLifecycleDbContext db)
    {
        if (await db.Facilities.AnyAsync()) return;

        var originalsDir = Path.Combine(AppContext.BaseDirectory, "SeedData", "Originals");
        var reschedulesDir = Path.Combine(AppContext.BaseDirectory, "SeedData", "Reschedules");

        var fixedProduct = LoanProduct.Create(
            Guid.NewGuid(), "TL-FIXED", "Term Loan (Fixed Rate)", "Term Loan", "Fixed",
            "Level Principal", 360, 0, true, true, 9m, 6m, 14m, "dev-seed");
        var floatProduct = LoanProduct.Create(
            Guid.NewGuid(), "TL-FLOAT", "Term Loan (Floating Rate)", "Term Loan", "Floating",
            "Level Principal", 360, 0, true, true, 7m, 4m, 12m, "dev-seed");
        db.LoanProducts.AddRange(fixedProduct, floatProduct);

        var skipped = new List<string>();

        foreach (var key in BorrowerKeys)
        {
            var originalPath = Path.Combine(originalsDir, key + ".json");
            var reschedulePath = Path.Combine(reschedulesDir, key + ".json");
            if (!File.Exists(originalPath) || !File.Exists(reschedulePath))
            {
                skipped.Add(key);
                continue;
            }

            using var originalDoc = JsonDocument.Parse(File.ReadAllText(originalPath));
            var originalRoot = originalDoc.RootElement;
            var originalParams = SnakeCaseScheduleParameters.FromJson(originalRoot);

            var rescheduleFile = JsonSerializer.Deserialize<RescheduleBorrowerFile>(
                File.ReadAllText(reschedulePath), FileOptions)!;

            var isFloating = originalRoot.TryGetProperty("interest_rate_change_events", out var rateEvents)
                && rateEvents.ValueKind == JsonValueKind.Array && rateEvents.GetArrayLength() > 0;

            var financialClose = originalRoot.GetProperty("financial_close").GetDouble();
            var agreementDate = DateOnly.FromDateTime(SerialDate.ToDate(financialClose));
            var projectName = originalRoot.TryGetProperty("project_name", out var pn) ? pn.GetString() ?? key : key;
            var loanAmount = originalRoot.GetProperty("loan_amount").GetDecimal();
            var currency = originalRoot.TryGetProperty("currency", out var cur) ? cur.GetString() ?? "BDT" : "BDT";
            var dayCountBasis = originalRoot.TryGetProperty("day_count_basis", out var dcb) ? dcb.GetInt32() : 360;
            var numInstallments = originalRoot.GetProperty("num_installments").GetInt32();
            var paymentFrequency = originalRoot.TryGetProperty("payment_frequency", out var pf) ? pf.GetInt32() : 4;
            var interestRatePercent = originalRoot.GetProperty("interest_rate").GetDecimal() * 100m;
            var repaymentMethod = MapRepaymentMethod(
                originalRoot.TryGetProperty("principal_type", out var pt) ? pt.GetString() : null);

            var interestGraceMonths = GraceMonths(originalRoot, "interest_grace_period_end", financialClose);
            var principalMoratoriumMonths = GraceMonths(originalRoot, "principal_grace_period_end", financialClose);
            var tenorMonths = (int)Math.Round(numInstallments * 12m / paymentFrequency);

            var customer = Customer.Create(
                Guid.NewGuid(), key, "Institutional", projectName, "CAD",
                null, null, null, "Verified", "Low", "dev-seed", "dev-seed");
            db.Customers.Add(customer);

            var sanction = LoanAgreement.Create(
                Guid.NewGuid(), key, customer.Id, key, isFloating ? "TL-FLOAT" : "TL-FIXED",
                projectName, null, currency, loanAmount, currency, 0m,
                agreementDate, null, isFloating ? "Floating" : "Fixed", interestRatePercent, tenorMonths,
                numInstallments, interestGraceMonths, principalMoratoriumMonths,
                repaymentMethod, paymentFrequency, paymentFrequency, dayCountBasis,
                2m, null, "dev-seed");
            sanction.Sign("dev-seed");
            db.LoanAgreements.Add(sanction);

            foreach (var facilityData in rescheduleFile.Facilities)
            {
                var isPrimaryTranche = facilityData.LenderCode == "IDCOL";
                var versionsToAppend = facilityData.Versions;

                string version0ParamsJson;
                DateOnly version0EffectiveDate;

                if (isPrimaryTranche)
                {
                    // The primary IDCOL tranche's true as-sanctioned schedule comes from the
                    // original (pre-reschedule) golden fixture; the reschedule extraction only
                    // covers events AFTER that baseline.
                    version0ParamsJson = JsonSerializer.Serialize(originalParams.ToDomain());
                    version0EffectiveDate = agreementDate;
                }
                else
                {
                    // Secondary/co-financier tranches (e.g. BPCL's Trust Bank, MAGBL's BIFFL and
                    // Additional facilities) have no separate original-schedule fixture - the
                    // earliest event the extraction actually found becomes this facility's
                    // baseline "version 0", consistent with what samples were actually provided.
                    var baseline = versionsToAppend[0];
                    version0ParamsJson = JsonSerializer.Serialize(
                        SnakeCaseScheduleParameters.FromJson(baseline.Params).ToDomain());
                    version0EffectiveDate = ParseDate(baseline.EffectiveDate) ?? agreementDate;
                    versionsToAppend = versionsToAppend.Skip(1).ToList();
                }

                var facility = Facility.CreateOriginal(
                    Guid.NewGuid(), sanction.Id, facilityData.LenderCode, facilityData.Currency,
                    version0EffectiveDate, version0ParamsJson, "dev-seed");

                // The extraction agents already resolved each event's true chronological order
                // (filenames alone are not reliable - see e.g. BPCL/PPL's notes); still sort
                // defensively since AddVersion rejects an out-of-order effective date.
                foreach (var version in versionsToAppend.OrderBy(v => ParseDate(v.EffectiveDate)))
                {
                    var effectiveDate = ParseDate(version.EffectiveDate);
                    if (effectiveDate is null) continue;

                    var versionParams = SnakeCaseScheduleParameters.FromJson(version.Params).ToDomain();
                    var result = facility.AddVersion(
                        Guid.NewGuid(), MapEventType(version.EventType), effectiveDate.Value, version.Label,
                        version.SourceFile, version.RateBeforePercent, version.RateAfterPercent,
                        version.TenorMonthsBefore, version.TenorMonthsAfter, version.CapitalizedAmount,
                        version.WaivedAmount, version.OverdueAmountRolledIn, version.RegulatoryReference,
                        JsonSerializer.Serialize(versionParams), "dev-seed");

                    if (result.IsFailure)
                    {
                        Log.Warning(
                            "Borrower history seed: skipped {Key}/{Lender} version '{Label}' - {Error}",
                            key, facilityData.LenderCode, version.Label, result.Error);
                    }
                }

                db.Facilities.Add(facility);
            }
        }

        if (skipped.Count > 0)
            Log.Warning("Borrower history seed: no seed data found for {Keys}", string.Join(", ", skipped));

        await db.SaveChangesAsync();
        Log.Information(
            "Borrower history seed: loaded {Count} real IDCOL borrowers (original + every reschedule/restructure/prepayment event) as live Facility/FacilityVersion data.",
            BorrowerKeys.Length - skipped.Count);
    }

    private static DateOnly? ParseDate(string? iso)
    {
        var serial = SerialDate.FromIso(iso);
        return serial.HasValue ? DateOnly.FromDateTime(SerialDate.ToDate(serial.Value)) : null;
    }

    private static int GraceMonths(JsonElement root, string propertyName, double financialClose)
    {
        if (!root.TryGetProperty(propertyName, out var el) || el.ValueKind != JsonValueKind.Number) return 0;
        var end = el.GetDouble();
        var months = (int)Math.Round((end - financialClose) / 30.4368);
        return Math.Max(0, months);
    }

    private static string MapRepaymentMethod(string? principalType) => principalType switch
    {
        "Annuity" => "Annuity",
        "PPMT Principal" => "PPMT Principal",
        "Scheduled Principal" or "Scheduled Percentage Principal" or "Percentage Schedule" => "Scheduled Principal",
        _ => "Level Principal",
    };

    private static FacilityVersionEventType MapEventType(string eventType) => eventType switch
    {
        "RESCHEDULE" => FacilityVersionEventType.Reschedule,
        "RESTRUCTURE" => FacilityVersionEventType.Restructure,
        "RATE_CHANGE" => FacilityVersionEventType.RateChange,
        "PREPAYMENT" => FacilityVersionEventType.Prepayment,
        "MORATORIUM_EXTENSION" => FacilityVersionEventType.MoratoriumExtension,
        _ => FacilityVersionEventType.Reschedule,
    };
}

public sealed class RescheduleBorrowerFile
{
    public string BorrowerKey { get; set; } = "";
    public List<RescheduleFacility> Facilities { get; set; } = new();
}

public sealed class RescheduleFacility
{
    public string LenderCode { get; set; } = "";
    public string Currency { get; set; } = "BDT";
    public List<RescheduleVersion> Versions { get; set; } = new();
}

public sealed class RescheduleVersion
{
    public string EventType { get; set; } = "";
    public string? SourceFile { get; set; }
    public string Label { get; set; } = "";
    public string EffectiveDate { get; set; } = "";
    public decimal? RateBeforePercent { get; set; }
    public decimal? RateAfterPercent { get; set; }
    public int? TenorMonthsBefore { get; set; }
    public int? TenorMonthsAfter { get; set; }
    public decimal CapitalizedAmount { get; set; }
    public decimal WaivedAmount { get; set; }
    public decimal OverdueAmountRolledIn { get; set; }
    public string? RegulatoryReference { get; set; }
    public JsonElement Params { get; set; }
}
