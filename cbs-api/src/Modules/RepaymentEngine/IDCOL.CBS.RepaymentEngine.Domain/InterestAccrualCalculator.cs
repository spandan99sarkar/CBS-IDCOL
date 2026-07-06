namespace IDCOL.CBS.RepaymentEngine.Domain;

/// <summary>
/// Day-count interest accrual with mid-period rate-change and disbursement awareness. Faithful
/// port of accrueInterest()/getRateAtDate() from the reference engine. Uses double arithmetic to
/// reproduce the reference (JS number) results exactly; callers round to currency precision only
/// at the persistence boundary.
/// </summary>
public static class InterestAccrualCalculator
{
    /// <summary>The effective rate in force on <paramref name="serial"/> given a base rate and
    /// an effective-dated list of rate changes.</summary>
    public static double GetRateAtDate(double baseRate, IReadOnlyList<RateChangeEvent> rateEvents, double serial)
    {
        var eff = baseRate;
        foreach (var ev in rateEvents.OrderBy(e => e.DateSerial))
        {
            if (serial >= ev.DateSerial) eff = ev.Rate;
            else break;
        }
        return eff;
    }

    /// <summary>
    /// Interest accrued between two serial dates on a running balance, applying disbursements
    /// (which raise the balance) and rate-change events (which change the applicable rate) at
    /// their exact effective dates within the period.
    /// </summary>
    public static double Accrue(
        double startDate,
        double endDate,
        double startBalance,
        IReadOnlyList<Disbursement> disbursements,
        double baseRate,
        double periodRate,
        IReadOnlyList<RateChangeEvent> rateEvents,
        double basis)
    {
        if (endDate <= startDate) return 0;

        // Fast path: no rate events - only disbursements segment the period, all at periodRate.
        if (rateEvents.Count == 0)
        {
            double acc = 0, bal = startBalance, seg = startDate;
            foreach (var dd in disbursements.OrderBy(d => d.DateSerial))
            {
                if (dd.DateSerial > seg) acc += bal * periodRate * (dd.DateSerial - seg) / basis;
                bal += dd.Amount;
                seg = dd.DateSerial;
            }
            acc += bal * periodRate * (endDate - seg) / basis;
            return acc;
        }

        // General path: merge rate-change and disbursement events, rate changes sorting first on
        // an equal date (matches the reference comparator's tiebreak).
        var curRate = GetRateAtDate(baseRate, rateEvents, startDate);
        double balance = startBalance, segStart = startDate, accrued = 0;

        var events = new List<(double Date, int Kind, double Value)>(); // Kind: 0 = rate, 1 = disbursement
        foreach (var ev in rateEvents)
            if (startDate < ev.DateSerial && ev.DateSerial <= endDate)
                events.Add((ev.DateSerial, 0, ev.Rate));
        foreach (var dd in disbursements)
            if (startDate < dd.DateSerial && dd.DateSerial <= endDate)
                events.Add((dd.DateSerial, 1, dd.Amount));

        events.Sort((a, b) =>
        {
            var byDate = a.Date.CompareTo(b.Date);
            return byDate != 0 ? byDate : a.Kind.CompareTo(b.Kind);
        });

        foreach (var (date, kind, value) in events)
        {
            if (date > segStart) accrued += balance * curRate * (date - segStart) / basis;
            if (kind == 0) curRate = value;
            else balance += value;
            segStart = date;
        }

        accrued += balance * curRate * (endDate - segStart) / basis;
        return accrued;
    }
}
