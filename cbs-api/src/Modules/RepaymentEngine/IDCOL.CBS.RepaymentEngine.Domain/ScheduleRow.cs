namespace IDCOL.CBS.RepaymentEngine.Domain;

/// <summary>
/// One installment row of a generated repayment schedule. Field meanings mirror the reference
/// engine's ScheduleRow so outputs can be compared 1:1 against the validated golden fixtures.
/// </summary>
public sealed class ScheduleRow
{
    public int Idx { get; set; }
    public double PayDate { get; set; }
    public double PeriodStart { get; set; }
    public double OpeningBal { get; set; }
    public double PeriodRate { get; set; }

    /// <summary>Interest shown on the row = cash interest + capitalized interest.</summary>
    public double Interest { get; set; }
    public double CashInterest { get; set; }
    public double CapInterest { get; set; }
    public double CumInt { get; set; }

    public double Principal { get; set; }
    public double CumPri { get; set; }

    /// <summary>Total debt service for the period (interest + principal, per the configured mode).</summary>
    public double Tds { get; set; }
    public double ClosingBal { get; set; }

    public bool IsCapRow { get; set; }
    public bool IsGrace { get; set; }
    public bool IsIntGrace { get; set; }
    public double NewDisb { get; set; }
    public int Days { get; set; }
}
