using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
using IDCOL.CBS.Security.Domain;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IDCOL.CBS.Api.Infrastructure;

/// <summary>
/// Dev-only seed of the Loan Security &amp; Covenant register for the seeded borrowers, so the security
/// dashboard shows a realistic spread of live / expiring / expired instruments (driving the
/// recommended-action engine) and covenant compliance. Each borrower gets a small collateral +
/// covenant portfolio: a bank guarantee or lien-marked FDR, a land mortgage with a Bangladesh-Bank
/// eligibility haircut, and insurance / credit-rating / financial-statement covenant obligations.
/// Expiry dates are spread around the reference date (2026-07-07) so the dashboard buckets populate.
/// Idempotent: no-ops if any instrument already exists.
/// </summary>
public static class SecurityCovenantSeed
{
    private static readonly DateOnly Ref = new(2026, 7, 7);

    // Expiry offsets (days from the reference date) rotated across borrowers to populate every
    // dashboard bucket: already-expired, due within 30, within 90, and comfortably live.
    private static readonly int[] ExpiryOffsets = { -35, 12, 26, 63, 180, 410, -10, 45, 120, 300 };

    private static readonly string[] Banks = { "Sonali Bank PLC", "Pubali Bank PLC", "The City Bank PLC", "BRAC Bank PLC", "Eastern Bank PLC" };
    private static readonly string[] Branches = { "Motijheel Corporate", "Gulshan", "Dhanmondi", "Agrabad", "Principal Branch" };
    private static readonly string[] RatingAgencies = { "CRISL", "Emerging Credit Rating Ltd", "Alpha Credit Rating", "National Credit Ratings Ltd" };
    private static readonly string[] Insurers = { "Green Delta Insurance PLC", "Pragati Insurance PLC", "Reliance Insurance Ltd", "Sadharan Bima Corporation" };
    private static readonly string[] Valuators = { "Geodetic Survey Services", "Vison Consultants", "AQL Associates" };
    private static readonly string[] Ratings = { "AAA", "AA+", "AA", "A+", "A", "BBB+" };

    public static async Task SeedAsync(LoanLifecycleDbContext db)
    {
        if (await db.SecurityInstruments.AnyAsync()) return;

        var agreements = await db.LoanAgreements.AsNoTracking().OrderBy(a => a.SanctionId).ToListAsync();
        var instruments = new List<SecurityCovenantInstrument>();
        var i = 0;

        foreach (var a in agreements)
        {
            var idx = i++;
            var loan = a.LoanAmount;
            var ccy = a.LoanCurrency;
            var bank = Banks[idx % Banks.Length];
            var branch = Branches[idx % Branches.Length];
            var off1 = ExpiryOffsets[idx % ExpiryOffsets.Length];
            var off2 = ExpiryOffsets[(idx + 3) % ExpiryOffsets.Length];
            var off3 = ExpiryOffsets[(idx + 6) % ExpiryOffsets.Length];

            // --- Security collateral ---
            if (idx % 2 == 0)
            {
                instruments.Add(SecurityCovenantInstrument.Create(
                    Guid.NewGuid(), SecurityCategory.Security, InstrumentFamily.BankGuarantee, "Term Loan-IF",
                    a.Id, a.CustomerNo, a.ProjectName, a.ProjectName, $"BG-{a.SanctionId}-{1000 + idx}",
                    bank, branch, ccy, Math.Round(loan * 0.10m, 2), 0m, Ref.AddDays(-365), Ref.AddDays(off1),
                    "Verified", false, InstrumentLifecycleState.Live, null, null, 0m, 100m, null, null, null,
                    null, null, null, "Advance payment / performance guarantee.", "dev-seed"));
            }
            else
            {
                var fdr = Math.Round(loan * 0.05m, 2);
                instruments.Add(SecurityCovenantInstrument.Create(
                    Guid.NewGuid(), SecurityCategory.Security, InstrumentFamily.FDR, "Term Loan-IF",
                    a.Id, a.CustomerNo, a.ProjectName, a.ProjectName, $"FDR-{a.SanctionId}-{2000 + idx}",
                    bank, branch, ccy, fdr, fdr, Ref.AddDays(-200), Ref.AddDays(off1),
                    "Verified", true, InstrumentLifecycleState.Live, null, null, 0m, 100m, null, null, null,
                    null, null, null, "Lien-marked FDR held as cash collateral.", "dev-seed"));
            }

            // Land mortgage with a Bangladesh-Bank eligibility haircut.
            var market = Math.Round(loan * 1.50m, 2);
            var fsv = Math.Round(loan * 1.20m, 2);
            instruments.Add(SecurityCovenantInstrument.Create(
                Guid.NewGuid(), SecurityCategory.Security, InstrumentFamily.LandMortgage, "Term Loan-IF",
                a.Id, a.CustomerNo, a.ProjectName, a.ProjectName, $"MORT-{a.SanctionId}",
                null, null, ccy, market, 0m, Ref.AddDays(-500), null, "Verified", false,
                InstrumentLifecycleState.Live, market, fsv, 100m, 50m,
                Valuators[idx % Valuators.Length], null, "Project site, industrial plot",
                null, null, null, "Registered mortgage of project land & building.", "dev-seed"));

            // --- Covenants ---
            instruments.Add(SecurityCovenantInstrument.Create(
                Guid.NewGuid(), SecurityCategory.Covenant, InstrumentFamily.InsurancePolicy, "Term Loan-IF",
                a.Id, a.CustomerNo, a.ProjectName, a.ProjectName, $"INS-{a.SanctionId}-{idx}",
                null, null, ccy, Math.Round(loan * 1.10m, 2), 0m, Ref.AddDays(off2 - 365), Ref.AddDays(off2),
                "Verified", false, InstrumentLifecycleState.Live, Math.Round(loan * 1.10m, 2),
                Math.Round(loan * 1.10m, 2), 100m, 0m, Insurers[idx % Insurers.Length], null, null,
                "Comprehensive Insurance", Compliance(off2), Ref.AddDays(off2),
                "Assets insured with lender's clause.", "dev-seed"));

            instruments.Add(SecurityCovenantInstrument.Create(
                Guid.NewGuid(), SecurityCategory.Covenant, InstrumentFamily.CreditRating, "Term Loan-IF",
                a.Id, a.CustomerNo, a.ProjectName, a.ProjectName, $"CR-{a.SanctionId}-{idx}",
                null, null, ccy, 0m, 0m, Ref.AddDays(off3 - 365), Ref.AddDays(off3),
                "Verified", false, InstrumentLifecycleState.Live, null, null, 0m, 0m,
                RatingAgencies[idx % RatingAgencies.Length], Ratings[idx % Ratings.Length], null,
                "Annual Credit Rating", Compliance(off3), Ref.AddDays(off3),
                "Annual entity credit rating obligation.", "dev-seed"));

            instruments.Add(SecurityCovenantInstrument.Create(
                Guid.NewGuid(), SecurityCategory.Covenant, InstrumentFamily.FinancialStatement, "Term Loan-IF",
                a.Id, a.CustomerNo, a.ProjectName, a.ProjectName, $"FS-{a.SanctionId}-FY2025",
                null, null, ccy, 0m, 0m, null, Ref.AddDays(off1 + 30), "Pending", false,
                InstrumentLifecycleState.Live, null, null, 0m, 0m, "ACNABIN Chartered Accountants",
                null, null, "Audited Financial Statement", Compliance(off1 + 30), Ref.AddDays(off1 + 30),
                "Submission of FY2025 audited financial statements.", "dev-seed"));
        }

        db.SecurityInstruments.AddRange(instruments);
        await db.SaveChangesAsync();

        Log.Information(
            "Security & covenant seed: {N} instruments across {B} borrowers (security collateral + covenant obligations).",
            instruments.Count, agreements.Count);
    }

    private static string Compliance(int expiryOffset) => expiryOffset switch
    {
        < 0 => ComplianceStatus.NotComplied,
        <= 30 => ComplianceStatus.PendingReminder,
        _ => ComplianceStatus.Complied,
    };
}
