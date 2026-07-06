// Converts the 19 golden borrower param sets (snake_case, serial-date form) into the camelCase
// ScheduleParameters shape the cbs-api /api/repayment/compute endpoint expects, and writes them
// as a single asset the Angular schedule-generation screen loads for one-click real examples.
const fs = require('fs');
const path = require('path');

const goldenDir = path.join(__dirname, '..', 'tests', 'IDCOL.CBS.RepaymentEngine.RegressionFixtures', 'GoldenData');
const outFile = path.join(__dirname, '..', '..', 'cbs-web', 'public', 'borrower-examples.json');

const LABELS = {
  BPCL: 'BPCL — Annuity, rate change + capitalization',
  CWTP: 'CWTP — Level principal, rate step',
  DHRL: 'DHRL (USD) — Scheduled %, variable rate',
  DPL: 'DPL — Level Principal',
  EKCL: 'EKCL — PPMT',
  GHEL: 'GHEL — Level Principal',
  HYDRON: 'Hydron — Level Principal',
  IHL: 'IHL — Annuity + capitalization',
  KPCL: 'KPCL (USD) — Scheduled %, variable rate',
  KZFL: 'KZFL — PPMT, monthly',
  MAGBL: 'MAGBL — PPMT, capitalization',
  MCML: 'MCML — Scheduled (workbook amounts)',
  NSGL: 'NSGL — Level, monthly',
  PABL: 'PABL — PPMT, 6 disbursements',
  PPL: 'PPL — rate changes + day adjustments',
  QPSL: 'QPSL — Scheduled (workbook amounts)',
  SCBL: 'SCBL — Scheduled (workbook)',
  SKS: 'SKS Edible Oil — Scheduled + capitalization',
  THERMAX: 'Thermax — monthly, capitalization',
};

function toCamelParams(pp) {
  const out = {
    projectName: pp.project_name ?? '',
    currency: pp.currency ?? 'BDT',
    loanAmount: pp.loan_amount ?? 0,
    interestRate: pp.interest_rate ?? 0,
    dayCountBasis: pp.day_count_basis ?? 360,
    numInstallments: pp.num_installments ?? 0,
    principalType: pp.principal_type ?? 'Level Principal',
    paymentFrequency: pp.payment_frequency ?? 4,
    interestGracePeriodEnd: pp.interest_grace_period_end ?? null,
    principalGracePeriodEnd: pp.principal_grace_period_end ?? null,
    interestCapitalized: pp.interest_capitalized ?? false,
    interestCapitalizedUntil: pp.interest_capitalized_until ?? null,
    openingBalanceIncludesPeriodDisbursements: pp.opening_balance_includes_period_disbursements ?? true,
    disbursements: (pp.disbursements ?? []).map(([dateSerial, amount]) => ({ dateSerial, amount })),
    repaymentDates: pp.repayment_dates ?? [],
    interestRateChangeEvents: (pp.interest_rate_change_events ?? []).map(([dateSerial, rate]) => ({ dateSerial, rate })),
    interestDayAdjustments: (pp.interest_day_adjustments ?? []).map((a) => ({
      payDate: a.pay_date, balance: a.balance ?? 'opening_balance', rate: a.rate ?? null, days: a.days ?? null,
    })),
    principalSchedulePercentages: pp.principal_schedule_percentages ?? null,
    principalScheduleAmounts: pp.principal_schedule_amounts ?? null,
    interestRatesByPeriod: pp.interest_rates_by_period ?? null,
    interestPaymentAmounts: pp.interest_payment_amounts ?? null,
    openingBalanceAmounts: pp.opening_balance_amounts ?? null,
    closingBalanceAmounts: pp.closing_balance_amounts ?? null,
    annuityRecalculateOnRateOrDisbursement: pp.annuity_recalculate_on_rate_or_disbursement ?? false,
    annuityUsePeriodRate: pp.annuity_use_period_rate ?? false,
    totalDebtServiceMode: pp.total_debt_service_mode ?? null,
    totalDebtServiceIncludesCapitalizedInterest: pp.total_debt_service_includes_capitalized_interest ?? false,
  };
  return out;
}

const examples = [];
for (const file of fs.readdirSync(goldenDir)) {
  if (!file.endsWith('.json') || file.startsWith('_')) continue;
  const g = JSON.parse(fs.readFileSync(path.join(goldenDir, file), 'utf-8'));
  examples.push({
    key: g.key,
    label: LABELS[g.key] ?? g.key,
    parameters: toCamelParams(g.params),
  });
}

examples.sort((a, b) => a.key.localeCompare(b.key));
fs.mkdirSync(path.dirname(outFile), { recursive: true });
fs.writeFileSync(outFile, JSON.stringify(examples, null, 2));
console.log(`Wrote ${examples.length} borrower examples to ${outFile}`);
