namespace IDCOL.CBS.RepaymentEngine.Domain;

/// <summary>
/// The typed events a Facility's schedule can go through over its life, validated against IDCOL's
/// real reschedule/restructure/prepayment history across the portfolio. ORIGINAL is version 0
/// (the as-sanctioned schedule); every later version is one of these.
/// </summary>
public enum FacilityVersionEventType
{
    Original,
    Reschedule,
    Restructure,
    RateChange,
    Prepayment,
    MoratoriumExtension,
}
