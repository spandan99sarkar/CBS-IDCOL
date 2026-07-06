export interface DisbursementInput {
  dateSerial: number;
  amount: number;
}

export interface RateChangeEventInput {
  dateSerial: number;
  rate: number;
}

export interface InterestDayAdjustmentInput {
  payDate: number;
  balance: string;
  rate: number | null;
  days: number | null;
}

/** Mirrors the cbs-api ScheduleParameters contract (camelCase, Excel-serial dates). */
export interface ScheduleParameters {
  projectName: string;
  currency: string;
  loanAmount: number;
  interestRate: number; // decimal fraction (0.06 = 6%)
  dayCountBasis: number;
  numInstallments: number;
  principalType: string;
  paymentFrequency: number;
  interestGracePeriodEnd: number | null;
  principalGracePeriodEnd: number | null;
  interestCapitalized: boolean;
  interestCapitalizedUntil: number | null;
  openingBalanceIncludesPeriodDisbursements: boolean;
  disbursements: DisbursementInput[];
  repaymentDates: number[];
  interestRateChangeEvents: RateChangeEventInput[];
  interestDayAdjustments: InterestDayAdjustmentInput[];
  principalSchedulePercentages: number[] | null;
  principalScheduleAmounts: number[] | null;
  interestRatesByPeriod: number[] | null;
  interestPaymentAmounts: number[] | null;
  openingBalanceAmounts: number[] | null;
  closingBalanceAmounts: number[] | null;
  annuityRecalculateOnRateOrDisbursement: boolean;
  annuityUsePeriodRate: boolean;
  totalDebtServiceMode: string | null;
  totalDebtServiceIncludesCapitalizedInterest: boolean;
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
  days: number;
}

export interface BorrowerExample {
  key: string;
  label: string;
  parameters: ScheduleParameters;
}
