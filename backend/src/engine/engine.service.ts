import { Injectable } from '@nestjs/common';

export interface Disbursement {
  date: number;
  amount: number;
  note?: string;
}

export interface RateEvent {
  date: number;
  rate: number;
}

export interface Holiday {
  date: string;
  name: string;
}

export interface LoanParams {
  project_name: string;
  currency: string;
  loan_amount: number;
  financial_close: number;
  interest_rate: number;
  day_count_basis: number;
  num_installments: number;
  principal_type: string;
  payment_frequency: number;
  interest_grace_period_end: number;
  principal_grace_period_end: number;
  interest_capitalized: boolean;
  interest_capitalized_until?: number;
  opening_balance_includes_period_disbursements: boolean;
  disbursements: [number, number][];
  repayment_dates: number[];
  interest_rate_change_events: [number, number][];
  principal_schedule_percentages?: number[];
  principal_schedule_amounts?: number[];
  annuity_recalculate_on_rate_or_disbursement: boolean;
  annuity_use_period_rate: boolean;
  total_debt_service_includes_capitalized_interest: boolean;
  interest_rates_by_period?: number[];
  interest_payment_amounts?: number[];
  opening_balance_amounts?: number[];
  closing_balance_amounts?: number[];
  interest_day_adjustments?: any[];
  total_debt_service_mode?: string;
}

export interface ScheduleRow {
  idx: number;
  payDate: number;
  periodStart: number;
  openingBal: number;
  periodRate: number;
  interest: number;
  cashInterest: number;
  capInterest: number;
  cumInt: number;
  principal: number;
  cumPri: number;
  tds: number;
  closingBal: number;
  isCapRow: boolean;
  isGrace: boolean;
  isIntGrace: boolean;
  newDisb: number;
  dailyRows: any[];
  days: number;
}

@Injectable()
export class EngineService {
  private readonly EPOCH = Date.UTC(1899, 11, 30);
  private readonly DAYMS = 86400000;

  serialToDate(s: number): Date {
    return new Date(this.EPOCH + s * this.DAYMS);
  }

  isoToSerial(iso: string): number | null {
    if (!iso) return null;
    const [y, m, d] = iso.split('-').map(Number);
    return Math.round((Date.UTC(y, m - 1, d) - this.EPOCH) / this.DAYMS);
  }

  serialToISO(s: number): string {
    return this.serialToDate(s).toISOString().slice(0, 10);
  }

  getRateAtDate(baseRate: number, rateEvents: [number, number][], serial: number): number {
    let eff = baseRate;
    const sortedEvents = (rateEvents || []).slice().sort((a, b) => a[0] - b[0]);
    for (const [ed, er] of sortedEvents) {
      if (serial >= ed) eff = er;
      else break;
    }
    return eff;
  }

  accrueInterest(
    startDate: number,
    endDate: number,
    startBalance: number,
    disbursements: [number, number][],
    baseRate: number,
    periodRate: number,
    rateEvents: [number, number][],
    basis: number,
  ): number {
    if (endDate <= startDate) return 0;
    if (!rateEvents || rateEvents.length === 0) {
      let acc = 0, bal = startBalance, seg = startDate;
      const sortedDisb = disbursements.slice().sort((a, b) => a[0] - b[0]);
      for (const [d, amt] of sortedDisb) {
        if (d > seg) acc += (bal * periodRate * (d - seg)) / basis;
        bal += amt;
        seg = d;
      }
      acc += (bal * periodRate * (endDate - seg)) / basis;
      return acc;
    }
    let curRate = this.getRateAtDate(baseRate, rateEvents, startDate),
      bal = startBalance,
      seg = startDate,
      acc = 0;
    const events: [number, string, number][] = [];
    for (const [ed, er] of rateEvents) {
      if (startDate < ed && ed <= endDate) events.push([ed, 'rate', er]);
    }
    for (const [d, amt] of disbursements) {
      if (startDate < d && d <= endDate) events.push([d, 'disbursement', amt]);
    }
    events.sort((a, b) => a[0] - b[0] || (a[1] === 'rate' ? 0 : 1) - (b[1] === 'rate' ? 0 : 1));
    for (const [ed, typ, val] of events) {
      if (ed > seg) acc += (bal * curRate * (ed - seg)) / basis;
      if (typ === 'rate') curRate = val;
      else bal += val;
      seg = ed;
    }
    acc += (bal * curRate * (endDate - seg)) / basis;
    return acc;
  }

  dailyAccrualForPeriod(
    startDate: number,
    endDate: number,
    startBalance: number,
    disbursements: [number, number][],
    baseRate: number,
    periodRate: number,
    rateEvents: [number, number][],
    basis: number,
  ): any[] {
    const rows = [];
    const useRE = rateEvents && rateEvents.length > 0;
    for (let d = startDate; d < endDate; d++) {
      let bal = startBalance, disbToday = 0;
      for (const [dd, amt] of disbursements) {
        if (dd > startDate && dd <= d) bal += amt;
        if (dd === d) disbToday += amt;
      }
      const rate = useRE ? this.getRateAtDate(baseRate, rateEvents, d) : periodRate;
      rows.push({
        date: d,
        dow: this.serialToDate(d).getUTCDay(),
        opening: bal,
        rate,
        daily: (bal * rate) / basis,
        disb: disbToday,
      });
    }
    return rows;
  }

  dailyFromTotal(startDate: number, endDate: number, balance: number, totalInterest: number, basis: number): any[] {
    const rows = [];
    const days = endDate - startDate;
    if (days <= 0) return rows;
    const denom = (balance * days) / basis;
    const rate = denom > 0 ? totalInterest / denom : 0;
    const per = totalInterest / days;
    for (let d = startDate; d < endDate; d++) {
      rows.push({
        date: d,
        dow: this.serialToDate(d).getUTCDay(),
        opening: balance,
        rate,
        daily: per,
        disb: 0,
      });
    }
    return rows;
  }

  addMonthsSerial(s: number, n: number, day?: number): number {
    const d = this.serialToDate(s);
    let y = d.getUTCFullYear(),
      mo = d.getUTCMonth() + n;
    y += Math.floor(mo / 12);
    mo = ((mo % 12) + 12) % 12;
    const useDay = day || d.getUTCDate();
    const last = new Date(Date.UTC(y, mo + 1, 0)).getUTCDate();
    return Math.round((Date.UTC(y, mo + 1, Math.min(useDay, last)) - this.EPOCH) / this.DAYMS);
  }

  isBusinessDay(s: number, holidaySet: Set<number>, weekend: number[]): boolean {
    if (weekend.includes(this.serialToDate(s).getUTCDay())) return false;
    if (holidaySet.has(s)) return false;
    return true;
  }

  adjustBD(s: number, rule: string, holidaySet: Set<number>, weekend: number[]): number {
    if (rule === 'none' || (weekend.length === 0 && holidaySet.size === 0)) return s;
    const step = rule === 'preceding' ? -1 : 1;
    let x = s,
      g = 0;
    while (!this.isBusinessDay(x, holidaySet, weekend) && g < 200) {
      x += step;
      g++;
    }
    return x;
  }

  generateRepaymentDates(params: any): number[] {
    const {
      financial_close: fc,
      num_installments: nInst,
      gen_mode: genMode,
      manual_dates: manualDates,
      first_payment: first,
      payment_frequency: freq,
      payment_day: pday,
      bd_rule: bdRule,
      holidays,
      weekend_fri_sat,
      weekend_sat_sun,
    } = params;

    let payDates: number[] = [];
    const holidaySet = new Set<number>((holidays || []).map(h => this.isoToSerial(h.date)));
    const weekend = weekend_fri_sat ? [5, 6] : weekend_sat_sun ? [6, 0] : [];

    if (genMode === 'manual') {
      payDates = (manualDates || []).filter(Boolean).map(d => this.isoToSerial(d));
    } else {
      if (!first) return [];
      const firstSerial = typeof first === 'string' ? this.isoToSerial(first) : first;
      const stepMonths = 12 / freq;
      payDates.push(firstSerial);
      let cur = firstSerial;
      for (let k = 1; k < nInst; k++) {
        cur = this.addMonthsSerial(cur, stepMonths, pday);
        payDates.push(cur);
      }
    }

    return payDates.map(s => this.adjustBD(s, bdRule, holidaySet, weekend)).sort((a, b) => a - b);
  }

  generateSchedule(P: any): ScheduleRow[] {
    // If repayment_dates are not provided, generate them
    if (!P.repayment_dates || P.repayment_dates.length === 0) {
      P.repayment_dates = this.generateRepaymentDates(P);
    }

    // Handle due date inclusion by adding day adjustments
    if (P.due_date_include && (!P.interest_day_adjustments || P.interest_day_adjustments.length === 0)) {
      P.interest_day_adjustments = P.repayment_dates.map((pd: number) => ({
        pay_date: pd,
        balance: 'opening_balance',
        days: 1,
      }));
    }

    const baseRate = P.interest_rate,
      basis = P.day_count_basis;
    const intGrace = P.interest_grace_period_end,
      priGrace = P.principal_grace_period_end;
    const nInst = P.num_installments, ptype = P.principal_type;
    const capitalize = !!P.interest_capitalized, capUntil = P.interest_capitalized_until ?? null;
    const payFreq = P.payment_frequency || 4;
    const periodRates = P.interest_rates_by_period || null;
    const rateEvents = P.interest_rate_change_events || [];
    const annuityRecalc = !!P.annuity_recalculate_on_rate_or_disbursement;
    const annuityUsePeriod = !!P.annuity_use_period_rate;
    const tdsMode = P.total_debt_service_mode || 'cash';
    const tdsIncCap = !!P.total_debt_service_includes_capitalized_interest;
    const openingIncNew = P.opening_balance_includes_period_disbursements !== false;
    const dayAdj = P.interest_day_adjustments || [];
    const intOverride = P.interest_payment_amounts || null;
    const openOverride = P.opening_balance_amounts || null;
    const closeOverride = P.closing_balance_amounts || null;
    const pctSched = P.principal_schedule_percentages || null;
    const amtSched = P.principal_schedule_amounts || null;

    const disb = (P.disbursements || []).slice().sort((a, b) => a[0] - b[0]);
    const payDates = (P.repayment_dates || []).slice().sort((a, b) => a - b);

    if (disb.length === 0 || payDates.length === 0) {
      return [];
    }

    const firstDD = disb[0][0];
    let balance = disb.filter(([d]) => d <= firstDD).reduce((s, [, a]) => s + a, 0);
    let pending = disb.filter(([d]) => d > firstDD);

    let lastIntDate = firstDD, intBaseBal = balance, instPaid = 0, cumInt = 0, cumPri = 0, annuityPmt = null;
    const rows: ScheduleRow[] = [];

    for (let i = 0; i < payDates.length; i++) {
      const payDate = payDates[i];
      const periodRate = periodRates ? periodRates[i] : baseRate;
      const prevDate = i > 0 ? payDates[i - 1] : firstDD;

      const newPeriod = [], stillPending = [];
      for (const [d, amt] of pending) {
        if (prevDate < d && d <= payDate) newPeriod.push([d, amt]);
        else stillPending.push([d, amt]);
      }
      pending = stillPending;
      const newTotal = newPeriod.reduce((s, [, a]) => s + a, 0);
      let principalBase = balance + newTotal;
      let openingBal = openingIncNew ? principalBase : balance;
      if (openOverride) {
        openingBal = openOverride[i];
        principalBase = openingBal;
      }

      const isCapRow = (capUntil !== null && payDate <= capUntil) || (capitalize && payDate <= intGrace);
      const isDeferredZero = payDate <= intGrace && !isCapRow;

      let interest = 0, capInterest = 0, intDateAdvanced = false, dailyRows = null;

      if (intOverride) {
        const io = intOverride[i];
        intDateAdvanced = true;
        if (isCapRow) capInterest = io;
        else interest = io;
      } else if (!isDeferredZero) {
        const accDisb: [number, number][] = openingIncNew ? disb.filter(([d]) => lastIntDate < d && d <= payDate) : [];
        let accrued = this.accrueInterest(lastIntDate, payDate, intBaseBal, accDisb, baseRate, periodRate, rateEvents, basis);
        dailyRows = this.dailyAccrualForPeriod(lastIntDate, payDate, intBaseBal, accDisb, baseRate, periodRate, rateEvents, basis);
        for (const adj of dayAdj) {
          if (adj.pay_date === payDate) {
            const mode = adj.balance || 'opening_balance';
            const ab = mode === 'interest_base_balance' ? intBaseBal : mode === 'principal_base' ? principalBase : openingBal;
            accrued += (ab * (adj.rate ?? periodRate) * (adj.days ?? 0)) / basis;
          }
        }
        if (dailyRows.length) {
          const dsum = dailyRows.reduce((a, x) => a + x.daily, 0), diff = accrued - dsum;
          if (Math.abs(diff) > 1e-6) dailyRows[dailyRows.length - 1].daily += diff;
        }
        intDateAdvanced = true;
        if (isCapRow) capInterest = accrued;
        else interest = accrued;
      } else {
        // Advance date even if interest is deferred/zero
        intDateAdvanced = true;
      }

      const remaining = nInst - instPaid;
      let principal = 0;
      if (payDate <= priGrace || remaining <= 0) {
        principal = 0;
      } else {
        if (ptype === 'Annuity') {
          let reset = annuityPmt === null;
          if (annuityPmt !== null && annuityRecalc) {
            const prevPR = (periodRates && i > 0) ? periodRates[i - 1] : periodRate;
            reset = newPeriod.length > 0 || periodRate !== prevPR;
          }
          if (reset) {
            const rForPmt = annuityUsePeriod ? periodRate : baseRate;
            const rq = rForPmt / payFreq;
            annuityPmt = principalBase * rq / (1 - Math.pow(1 + rq, -remaining));
          }
          principal = remaining === 1 ? principalBase : annuityPmt - interest;
        } else if (ptype === 'PPMT Principal' || ptype === 'Quarterly Installment' || ptype === 'PPMT') {
          if (annuityPmt === null) {
            const rq = baseRate / payFreq;
            annuityPmt = principalBase * rq / (1 - Math.pow(1 + rq, -nInst));
          }
          const perInt = principalBase * (baseRate / payFreq);
          principal = remaining === 1 ? principalBase : annuityPmt - perInt;
        } else if (ptype === 'Scheduled Percentage Principal' || ptype === 'Scheduled Principal' || ptype === 'Percentage Schedule') {
          if (amtSched) principal = amtSched[instPaid];
          else if (pctSched) principal = P.loan_amount * pctSched[instPaid];
          if (remaining === 1) principal = principalBase;
        } else {
          principal = principalBase / remaining;
        }
        instPaid++;
      }

      let closingBal = principalBase + capInterest - principal;
      if (closeOverride) closingBal = closeOverride[i];
      cumInt += interest;
      cumPri += principal;
      const shownInt = capInterest + interest;
      if (!dailyRows) dailyRows = this.dailyFromTotal(prevDate, payDate, openingBal, shownInt, basis);

      let tds;
      if (tdsMode === 'annuity_pmt' && ptype === 'Annuity' && annuityPmt !== null && payDate > priGrace) tds = annuityPmt;
      else if (tdsIncCap) tds = shownInt + principal;
      else tds = interest + principal;

      rows.push({
        idx: i,
        payDate,
        periodStart: prevDate,
        openingBal,
        periodRate,
        interest: shownInt,
        cashInterest: interest,
        capInterest,
        cumInt,
        principal,
        cumPri,
        tds,
        closingBal,
        isCapRow,
        isGrace: payDate <= priGrace,
        isIntGrace: payDate <= intGrace,
        newDisb: newTotal,
        dailyRows,
        days: payDate - prevDate,
      });

      balance = closingBal;
      if (intDateAdvanced) {
        lastIntDate = payDate;
        intBaseBal = closingBal;
      }
    }
    return rows;
  }
}
