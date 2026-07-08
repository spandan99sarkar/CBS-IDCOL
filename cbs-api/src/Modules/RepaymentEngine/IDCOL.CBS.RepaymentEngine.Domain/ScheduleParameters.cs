namespace IDCOL.CBS.RepaymentEngine.Domain;

/// <summary>One tranche drawdown: [DateSerial, Amount].</summary>
public sealed class Disbursement
{
    public double DateSerial { get; set; }
    public double Amount { get; set; }
}

/// <summary>An effective-dated interest-rate change (floating/stepped rate loans).</summary>
public sealed class RateChangeEvent
{
    public double DateSerial { get; set; }
    public double Rate { get; set; }
}

/// <summary>
/// A manual per-period interest day-count adjustment (e.g. PPL, where a rate boundary lands
/// mid-period and one extra/fewer day of interest is applied against a chosen balance).
/// </summary>
public sealed class InterestDayAdjustment
{
    public double PayDate { get; set; }
    public string Balance { get; set; } = "opening_balance"; // opening_balance | interest_base_balance | principal_base
    public double? Rate { get; set; }
    public double? Days { get; set; }
}

public sealed class Holiday
{
    public string Date { get; set; } = "";
    public string Name { get; set; } = "";
}

/// <summary>
/// The full input contract for the repayment engine - a faithful C# counterpart of the
/// LoanParams object consumed by the validated reference engine
/// (backend/src/engine/engine.service.ts). Interest rates are decimal fractions (0.06 == 6%).
/// </summary>
public sealed class ScheduleParameters
{
    public string ProjectName { get; set; } = "";
    public string Currency { get; set; } = "BDT";
    public double LoanAmount { get; set; }
    public double InterestRate { get; set; }
    public int DayCountBasis { get; set; } = 360;
    public int NumInstallments { get; set; }
    public string PrincipalType { get; set; } = "Level Principal";
    public int PaymentFrequency { get; set; } = 4;

    public double? InterestGracePeriodEnd { get; set; }
    public double? PrincipalGracePeriodEnd { get; set; }

    public bool InterestCapitalized { get; set; }
    public double? InterestCapitalizedUntil { get; set; }

    public bool OpeningBalanceIncludesPeriodDisbursements { get; set; } = true;
    public bool DueDateInclude { get; set; }

    public List<Disbursement> Disbursements { get; set; } = new();
    public List<double> RepaymentDates { get; set; } = new();
    public List<RateChangeEvent> InterestRateChangeEvents { get; set; } = new();
    public List<InterestDayAdjustment> InterestDayAdjustments { get; set; } = new();

    public double[]? PrincipalSchedulePercentages { get; set; }
    public double[]? PrincipalScheduleAmounts { get; set; }
    public double[]? InterestRatesByPeriod { get; set; }
    public double[]? InterestPaymentAmounts { get; set; }
    public double[]? OpeningBalanceAmounts { get; set; }
    public double[]? ClosingBalanceAmounts { get; set; }

    public bool AnnuityRecalculateOnRateOrDisbursement { get; set; }
    public bool AnnuityUsePeriodRate { get; set; }
    public bool TotalDebtServiceIncludesCapitalizedInterest { get; set; }
    public string? TotalDebtServiceMode { get; set; }

    // Auto date-generation inputs (used only when RepaymentDates is empty; not exercised by the
    // 19 historical fixtures, which all supply explicit dates, but ported for completeness).
    public double? FinancialClose { get; set; }
    public string GenMode { get; set; } = "auto";
    public List<string> ManualDates { get; set; } = new();
    public double? FirstPayment { get; set; }
    public int PaymentDay { get; set; } = 15;
    public string BdRule { get; set; } = "none";
    public List<Holiday> Holidays { get; set; } = new();
    public bool WeekendFriSat { get; set; }
    public bool WeekendSatSun { get; set; }
}
