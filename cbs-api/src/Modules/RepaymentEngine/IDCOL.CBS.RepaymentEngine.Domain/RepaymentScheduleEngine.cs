namespace IDCOL.CBS.RepaymentEngine.Domain;

/// <summary>
/// The heart of the CBS: generates a full repayment schedule from loan parameters. This is a
/// faithful port of generateSchedule() in the validated reference engine
/// (backend/src/engine/engine.service.ts), which was itself validated against IDCOL's real Excel
/// workbooks. Faithfulness is proven by the RegressionFixtures test project, which reproduces all
/// 19 real borrower schedules within tolerance.
///
/// Covers: level/equal principal, annuity (PPMT), and scheduled-percentage/amount principal
/// types; interest capitalization during grace; independent interest vs principal grace periods;
/// multi-disbursement dynamic balances; effective-dated rate changes; per-period rate/amount
/// overrides (for reverse-engineered historical schedules); and manual interest day adjustments.
/// </summary>
public static class RepaymentScheduleEngine
{
    public static List<ScheduleRow> Generate(ScheduleParameters p)
    {
        // Auto-generate repayment dates only if none were supplied.
        var payDatesSource = p.RepaymentDates is { Count: > 0 }
            ? p.RepaymentDates
            : GenerateRepaymentDates(p);

        // "Include due date" convenience: express it as a +1-day adjustment per installment.
        var dayAdj = p.InterestDayAdjustments;
        if (p.DueDateInclude && dayAdj.Count == 0)
        {
            dayAdj = payDatesSource
                .Select(pd => new InterestDayAdjustment { PayDate = pd, Balance = "opening_balance", Days = 1 })
                .ToList();
        }

        var baseRate = p.InterestRate;
        double basis = p.DayCountBasis;
        var intGrace = p.InterestGracePeriodEnd ?? double.NegativeInfinity;
        var priGrace = p.PrincipalGracePeriodEnd ?? double.NegativeInfinity;
        var nInst = p.NumInstallments;
        var ptype = p.PrincipalType;
        var capitalize = p.InterestCapitalized;
        var capUntil = p.InterestCapitalizedUntil;
        var payFreq = p.PaymentFrequency == 0 ? 4 : p.PaymentFrequency;
        var periodRates = p.InterestRatesByPeriod;
        var rateEvents = p.InterestRateChangeEvents;
        var annuityRecalc = p.AnnuityRecalculateOnRateOrDisbursement;
        var annuityUsePeriod = p.AnnuityUsePeriodRate;
        var tdsMode = p.TotalDebtServiceMode ?? "cash";
        var tdsIncCap = p.TotalDebtServiceIncludesCapitalizedInterest;
        var openingIncNew = p.OpeningBalanceIncludesPeriodDisbursements;
        var intOverride = p.InterestPaymentAmounts;
        var openOverride = p.OpeningBalanceAmounts;
        var closeOverride = p.ClosingBalanceAmounts;
        var pctSched = p.PrincipalSchedulePercentages;
        var amtSched = p.PrincipalScheduleAmounts;

        var disb = p.Disbursements.OrderBy(d => d.DateSerial).ToList();
        var payDates = payDatesSource.OrderBy(d => d).ToList();
        var rows = new List<ScheduleRow>();
        if (disb.Count == 0 || payDates.Count == 0) return rows;

        var firstDd = disb[0].DateSerial;
        var balance = disb.Where(d => d.DateSerial <= firstDd).Sum(d => d.Amount);
        var pending = disb.Where(d => d.DateSerial > firstDd).ToList();

        var lastIntDate = firstDd;
        var intBaseBal = balance;
        var instPaid = 0;
        double cumInt = 0, cumPri = 0;
        double? annuityPmt = null;

        for (var i = 0; i < payDates.Count; i++)
        {
            var payDate = payDates[i];
            var periodRate = periodRates != null ? periodRates[i] : baseRate;
            var prevDate = i > 0 ? payDates[i - 1] : firstDd;

            // Split still-pending disbursements into those landing in this period vs. later.
            var newPeriod = new List<Disbursement>();
            var stillPending = new List<Disbursement>();
            foreach (var d in pending)
            {
                if (prevDate < d.DateSerial && d.DateSerial <= payDate) newPeriod.Add(d);
                else stillPending.Add(d);
            }
            pending = stillPending;

            var newTotal = newPeriod.Sum(d => d.Amount);
            var principalBase = balance + newTotal;
            var openingBal = openingIncNew ? principalBase : balance;
            if (openOverride != null)
            {
                openingBal = openOverride[i];
                principalBase = openingBal;
            }

            var isCapRow = (capUntil.HasValue && payDate <= capUntil.Value) || (capitalize && payDate <= intGrace);
            var isDeferredZero = payDate <= intGrace && !isCapRow;

            double interest = 0, capInterest = 0;
            var intDateAdvanced = false;

            if (intOverride != null)
            {
                var io = intOverride[i];
                intDateAdvanced = true;
                if (isCapRow) capInterest = io; else interest = io;
            }
            else if (!isDeferredZero)
            {
                var accDisb = openingIncNew
                    ? disb.Where(d => lastIntDate < d.DateSerial && d.DateSerial <= payDate).ToList()
                    : new List<Disbursement>();
                var accrued = InterestAccrualCalculator.Accrue(
                    lastIntDate, payDate, intBaseBal, accDisb, baseRate, periodRate, rateEvents, basis);

                foreach (var adj in dayAdj)
                {
                    if (adj.PayDate == payDate)
                    {
                        var ab = adj.Balance == "interest_base_balance" ? intBaseBal
                            : adj.Balance == "principal_base" ? principalBase
                            : openingBal;
                        accrued += ab * (adj.Rate ?? periodRate) * (adj.Days ?? 0) / basis;
                    }
                }

                intDateAdvanced = true;
                if (isCapRow) capInterest = accrued; else interest = accrued;
            }
            else
            {
                // Interest deferred and not capitalized this period, but the accrual anchor still advances.
                intDateAdvanced = true;
            }

            var remaining = nInst - instPaid;
            double principal = 0;
            if (payDate <= priGrace || remaining <= 0)
            {
                principal = 0;
            }
            else
            {
                if (ptype == "Annuity")
                {
                    var reset = annuityPmt == null;
                    if (annuityPmt != null && annuityRecalc)
                    {
                        var prevPr = (periodRates != null && i > 0) ? periodRates[i - 1] : periodRate;
                        reset = newPeriod.Count > 0 || periodRate != prevPr;
                    }
                    if (reset)
                    {
                        var rForPmt = annuityUsePeriod ? periodRate : baseRate;
                        var rq = rForPmt / payFreq;
                        annuityPmt = principalBase * rq / (1 - Math.Pow(1 + rq, -remaining));
                    }
                    principal = remaining == 1 ? principalBase : annuityPmt!.Value - interest;
                }
                else if (ptype is "PPMT Principal" or "Quarterly Installment" or "PPMT")
                {
                    if (annuityPmt == null)
                    {
                        var rq = baseRate / payFreq;
                        annuityPmt = principalBase * rq / (1 - Math.Pow(1 + rq, -nInst));
                    }
                    var perInt = principalBase * (baseRate / payFreq);
                    principal = remaining == 1 ? principalBase : annuityPmt!.Value - perInt;
                }
                else if (ptype is "Scheduled Percentage Principal" or "Scheduled Principal" or "Percentage Schedule")
                {
                    // Clamped, not indexed directly: a schedule reconstructed from a historical
                    // workbook can have an amount/percentage list a row or two short of
                    // num_installments (e.g. a trailing/leading stub the source extraction
                    // dropped). Falling back to the last known entry is safer than throwing for
                    // otherwise-valid, mostly-complete real-world data.
                    if (amtSched != null) principal = amtSched[Math.Min(instPaid, amtSched.Length - 1)];
                    else if (pctSched != null) principal = p.LoanAmount * pctSched[Math.Min(instPaid, pctSched.Length - 1)];
                    if (remaining == 1) principal = principalBase;
                }
                else
                {
                    // Level/Equal Principal and any unmapped type: split remaining balance evenly.
                    principal = principalBase / remaining;
                }
                instPaid++;
            }

            var closingBal = principalBase + capInterest - principal;
            if (closeOverride != null) closingBal = closeOverride[i];
            cumInt += interest;
            cumPri += principal;
            var shownInt = capInterest + interest;

            double tds;
            if (tdsMode == "annuity_pmt" && ptype == "Annuity" && annuityPmt != null && payDate > priGrace)
                tds = annuityPmt.Value;
            else if (tdsIncCap)
                tds = shownInt + principal;
            else
                tds = interest + principal;

            rows.Add(new ScheduleRow
            {
                Idx = i,
                PayDate = payDate,
                PeriodStart = prevDate,
                OpeningBal = openingBal,
                PeriodRate = periodRate,
                Interest = shownInt,
                CashInterest = interest,
                CapInterest = capInterest,
                CumInt = cumInt,
                Principal = principal,
                CumPri = cumPri,
                Tds = tds,
                ClosingBal = closingBal,
                IsCapRow = isCapRow,
                IsGrace = payDate <= priGrace,
                IsIntGrace = payDate <= intGrace,
                NewDisb = newTotal,
                Days = (int)(payDate - prevDate),
            });

            balance = closingBal;
            if (intDateAdvanced)
            {
                lastIntDate = payDate;
                intBaseBal = closingBal;
            }
        }

        return rows;
    }

    /// <summary>Ports generateRepaymentDates() - auto/manual date generation with holiday adjustment.</summary>
    public static List<double> GenerateRepaymentDates(ScheduleParameters p)
    {
        var holidaySet = new HashSet<int>(p.Holidays
            .Select(h => SerialDate.FromIso(h.Date))
            .Where(s => s.HasValue)
            .Select(s => s!.Value));
        var weekend = p.WeekendFriSat ? new List<int> { 5, 6 }
            : p.WeekendSatSun ? new List<int> { 6, 0 }
            : new List<int>();

        var payDates = new List<double>();

        if (p.GenMode == "manual")
        {
            payDates = p.ManualDates
                .Where(d => !string.IsNullOrEmpty(d))
                .Select(d => (double)SerialDate.FromIso(d)!.Value)
                .ToList();
        }
        else
        {
            if (p.FirstPayment is null) return new List<double>();
            var firstSerial = p.FirstPayment.Value;
            var stepMonths = 12 / p.PaymentFrequency;
            payDates.Add(firstSerial);
            var cur = firstSerial;
            for (var k = 1; k < p.NumInstallments; k++)
            {
                cur = SerialDate.AddMonths(cur, stepMonths, p.PaymentDay);
                payDates.Add(cur);
            }
        }

        return payDates
            .Select(s => (double)BusinessDayAdjuster.Adjust(s, p.BdRule, holidaySet, weekend))
            .OrderBy(s => s)
            .ToList();
    }
}
