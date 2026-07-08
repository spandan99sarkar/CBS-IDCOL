export interface ClassificationAccountInput {
  accountId: string;
  accountRef: string;
  customerNo: string;
  projectName: string;
  currency: string;
  financeType: string;
  tenorMonths: number;
  isCmsme: boolean;
  outstanding: number;
  overdueMonths: number;
  interestSuspense: number;
  eligibleCollateral: number;
  qualitativeOverride: string | null;
}

export interface RunClassificationRequest {
  asOfDate: string;
  accounts: ClassificationAccountInput[];
}

export interface ClassificationResult {
  id: string;
  asOfDate: string;
  accountRef: string;
  customerNo: string;
  projectName: string;
  currency: string;
  financeType: string;
  tenorMonths: number;
  tenorBucket: string | null;
  outstanding: number;
  overdueMonths: number;
  interestSuspense: number;
  eligibleCollateral: number;
  status: string;
  isQualitativeOverride: boolean;
  provisionType: string;
  provisionRatePercent: number;
  provisionBase: number;
  provisionRequired: number;
}
