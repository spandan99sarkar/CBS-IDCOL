using System.Text.Json.Serialization;
using IDCOL.CBS.RepaymentEngine.Domain;

namespace IDCOL.CBS.RepaymentEngine.RegressionFixtures;

/// <summary>Root of a golden fixture file: the exact input params plus the reference engine's rows.</summary>
public sealed class GoldenFile
{
    public string Key { get; set; } = "";
    public GoldenParams Params { get; set; } = new();
    public List<GoldenRow> Rows { get; set; } = new();
}

/// <summary>
/// Snake-case counterpart of the reference engine input. Deserialized with
/// JsonNamingPolicy.SnakeCaseLower, then mapped to the domain's <see cref="ScheduleParameters"/>.
/// Nullable so absent fields fall back to the domain defaults.
/// </summary>
public sealed class GoldenParams
{
    public string? ProjectName { get; set; }
    public string? Currency { get; set; }
    public double? LoanAmount { get; set; }
    public double? InterestRate { get; set; }
    public int? DayCountBasis { get; set; }
    public int? NumInstallments { get; set; }
    public string? PrincipalType { get; set; }
    public int? PaymentFrequency { get; set; }
    public double? InterestGracePeriodEnd { get; set; }
    public double? PrincipalGracePeriodEnd { get; set; }
    public bool? InterestCapitalized { get; set; }
    public double? InterestCapitalizedUntil { get; set; }
    public bool? OpeningBalanceIncludesPeriodDisbursements { get; set; }
    public bool? DueDateInclude { get; set; }
    public double[][]? Disbursements { get; set; }
    public double[]? RepaymentDates { get; set; }
    public double[][]? InterestRateChangeEvents { get; set; }
    public GoldenDayAdjustment[]? InterestDayAdjustments { get; set; }
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
            InterestGracePeriodEnd = InterestGracePeriodEnd,
            PrincipalGracePeriodEnd = PrincipalGracePeriodEnd,
            InterestCapitalized = InterestCapitalized ?? false,
            InterestCapitalizedUntil = InterestCapitalizedUntil,
            OpeningBalanceIncludesPeriodDisbursements = OpeningBalanceIncludesPeriodDisbursements ?? true,
            DueDateInclude = DueDateInclude ?? false,
            RepaymentDates = RepaymentDates?.ToList() ?? new List<double>(),
            PrincipalSchedulePercentages = PrincipalSchedulePercentages,
            PrincipalScheduleAmounts = PrincipalScheduleAmounts,
            InterestRatesByPeriod = InterestRatesByPeriod,
            InterestPaymentAmounts = InterestPaymentAmounts,
            OpeningBalanceAmounts = OpeningBalanceAmounts,
            ClosingBalanceAmounts = ClosingBalanceAmounts,
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

public sealed class GoldenDayAdjustment
{
    public double PayDate { get; set; }
    public string? Balance { get; set; }
    public double? Rate { get; set; }
    public double? Days { get; set; }
}

/// <summary>One expected row from the reference engine. camelCase to match the golden JSON.</summary>
public sealed class GoldenRow
{
    [JsonPropertyName("idx")] public int Idx { get; set; }
    [JsonPropertyName("payDate")] public double PayDate { get; set; }
    [JsonPropertyName("openingBal")] public double OpeningBal { get; set; }
    [JsonPropertyName("periodRate")] public double PeriodRate { get; set; }
    [JsonPropertyName("interest")] public double Interest { get; set; }
    [JsonPropertyName("cashInterest")] public double CashInterest { get; set; }
    [JsonPropertyName("capInterest")] public double CapInterest { get; set; }
    [JsonPropertyName("principal")] public double Principal { get; set; }
    [JsonPropertyName("tds")] public double Tds { get; set; }
    [JsonPropertyName("closingBal")] public double ClosingBal { get; set; }
    [JsonPropertyName("days")] public int Days { get; set; }
}
