namespace IDCOL.CBS.RepaymentEngine.Domain;

/// <summary>
/// Weekend/holiday-aware business-day adjustment. Ports isBusinessDay()/adjustBD(). Weekend days
/// use JS getUTCDay() numbering (Sunday = 0 ... Saturday = 6); the Bangladesh default weekend is
/// Friday/Saturday (5, 6).
/// </summary>
public static class BusinessDayAdjuster
{
    public static bool IsBusinessDay(double serial, HashSet<int> holidays, IReadOnlyList<int> weekend)
    {
        if (weekend.Contains(SerialDate.DayOfWeek(serial))) return false;
        if (holidays.Contains((int)serial)) return false;
        return true;
    }

    /// <summary>
    /// Shifts a date off a weekend/holiday to the nearest business day per the rule
    /// ("preceding" or "succeeding"); "none" leaves the date unchanged.
    /// </summary>
    public static int Adjust(double serial, string rule, HashSet<int> holidays, IReadOnlyList<int> weekend)
    {
        if (rule == "none" || (weekend.Count == 0 && holidays.Count == 0)) return (int)serial;

        var step = rule == "preceding" ? -1 : 1;
        var x = (int)serial;
        var guard = 0;
        while (!IsBusinessDay(x, holidays, weekend) && guard < 200)
        {
            x += step;
            guard++;
        }
        return x;
    }
}
