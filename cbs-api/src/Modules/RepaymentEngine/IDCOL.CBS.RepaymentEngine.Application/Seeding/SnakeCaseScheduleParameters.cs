using System.Text.Json;
using IDCOL.CBS.RepaymentEngine.Domain;

namespace IDCOL.CBS.RepaymentEngine.Application.Seeding;

/// <summary>
/// Snake-case counterpart of the reference (NestJS) engine's parameter shape - the format every
/// borrower extraction (original schedules and reschedule/restructure/prepayment events alike)
/// is stored in. Deserialized with <see cref="JsonNamingPolicy.SnakeCaseLower"/>, then mapped to
/// the domain's <see cref="ScheduleParameters"/>. Shared by the regression-fixture test project
/// and the dev-data seed routine so the mapping is defined exactly once.
/// </summary>
public sealed class SnakeCaseScheduleParameters
{
    public string? ProjectName { get; set; }
    public string? Currency { get; set; }
    public double? LoanAmount { get; set; }
    public double? InterestRate { get; set; }
    public int? DayCountBasis { get; set; }
    public int? NumInstallments { get; set; }
    public string? PrincipalType { get; set; }
    public int? PaymentFrequency { get; set; }
    public double? FinancialClose { get; set; }
    public double? InterestGracePeriodEnd { get; set; }
    public double? PrincipalGracePeriodEnd { get; set; }
    public bool? InterestCapitalized { get; set; }
    public double? InterestCapitalizedUntil { get; set; }
    public bool? OpeningBalanceIncludesPeriodDisbursements { get; set; }
    public bool? DueDateInclude { get; set; }
    public double[][]? Disbursements { get; set; }
    public double[]? RepaymentDates { get; set; }
    public double[][]? InterestRateChangeEvents { get; set; }
    public SnakeCaseDayAdjustment[]? InterestDayAdjustments { get; set; }
    public double[]? PrincipalSchedulePercentages { get; set; }
    public double[]? PrincipalScheduleAmounts { get; set; }
    public double[]? InterestRatesByPeriod { get; set; }
    public double[]? InterestPaymentAmounts { get; set; }
    public double[]? OpeningBalanceAmounts { get; set; }
    public double[]? ClosingBalanceAmounts { get; set; }
    public bool? AnnuityRecalculateOnRateOrDisbursement { get; set; }
    public bool? AnnuityUsePeriodRate { get; set; }
    public bool? TotalDebtServiceIncludesCapitalizedInterest { get; set; }
    public string? TotalDebtServiceMode { get; set; }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public static SnakeCaseScheduleParameters FromJson(JsonElement element) =>
        element.Deserialize<SnakeCaseScheduleParameters>(Options)!;

    public static SnakeCaseScheduleParameters FromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return FromJson(doc.RootElement);
    }

    // An empty per-installment override array carries no information an absent one doesn't -
    // there's no case where "explicitly zero entries" should mean something different from "not
    // supplied" - but the engine indexes these arrays unconditionally whenever they're non-null,
    // so a stray `[]` from an extraction (rather than omitting the field) would throw at row 0.
    private static double[]? NullIfEmpty(double[]? arr) => arr is { Length: 0 } ? null : arr;

    public ScheduleParameters ToDomain()
    {
        var sp = new ScheduleParameters
        {
            ProjectName = ProjectName ?? "",
            Currency = Currency ?? "BDT",
            LoanAmount = LoanAmount ?? 0,
            InterestRate = InterestRate ?? 0,
            DayCountBasis = DayCountBasis ?? 360,
            NumInstallments = NumInstallments ?? 0,
            PrincipalType = PrincipalType ?? "Level Principal",
            PaymentFrequency = PaymentFrequency ?? 4,
            FinancialClose = FinancialClose,
            InterestGracePeriodEnd = InterestGracePeriodEnd,
            PrincipalGracePeriodEnd = PrincipalGracePeriodEnd,
            InterestCapitalized = InterestCapitalized ?? false,
            InterestCapitalizedUntil = InterestCapitalizedUntil,
            OpeningBalanceIncludesPeriodDisbursements = OpeningBalanceIncludesPeriodDisbursements ?? true,
            DueDateInclude = DueDateInclude ?? false,
            RepaymentDates = RepaymentDates?.ToList() ?? new List<double>(),
            PrincipalSchedulePercentages = NullIfEmpty(PrincipalSchedulePercentages),
            PrincipalScheduleAmounts = NullIfEmpty(PrincipalScheduleAmounts),
            InterestRatesByPeriod = NullIfEmpty(InterestRatesByPeriod),
            InterestPaymentAmounts = NullIfEmpty(InterestPaymentAmounts),
            OpeningBalanceAmounts = NullIfEmpty(OpeningBalanceAmounts),
            ClosingBalanceAmounts = NullIfEmpty(ClosingBalanceAmounts),
            AnnuityRecalculateOnRateOrDisbursement = AnnuityRecalculateOnRateOrDisbursement ?? false,
            AnnuityUsePeriodRate = AnnuityUsePeriodRate ?? false,
            TotalDebtServiceIncludesCapitalizedInterest = TotalDebtServiceIncludesCapitalizedInterest ?? false,
            TotalDebtServiceMode = TotalDebtServiceMode,
        };

        if (Disbursements != null)
            sp.Disbursements = Disbursements
                .Select(d => new Disbursement { DateSerial = d[0], Amount = d[1] })
                .ToList();

        if (InterestRateChangeEvents != null)
            sp.InterestRateChangeEvents = InterestRateChangeEvents
                .Select(e => new RateChangeEvent { DateSerial = e[0], Rate = e[1] })
                .ToList();

        if (InterestDayAdjustments != null)
            sp.InterestDayAdjustments = InterestDayAdjustments
                .Select(a => new InterestDayAdjustment
                {
                    PayDate = a.PayDate,
                    Balance = a.Balance ?? "opening_balance",
                    Rate = a.Rate,
                    Days = a.Days,
                })
                .ToList();

        return sp;
    }
}

public sealed class SnakeCaseDayAdjustment
{
    public double PayDate { get; set; }
    public string? Balance { get; set; }
    public double? Rate { get; set; }
    public double? Days { get; set; }
}
