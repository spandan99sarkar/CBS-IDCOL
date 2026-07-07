import { ScheduleParameters, ScheduleRow } from '../repayment/repayment.models';

export type FacilityVersionEventType =
  | 'Original' | 'Reschedule' | 'Restructure' | 'RateChange' | 'Prepayment' | 'MoratoriumExtension';

export interface FacilityVersion {
  id: string;
  versionSequence: number;
  eventType: FacilityVersionEventType;
  status: 'Active' | 'Superseded';
  effectiveDate: string;
  label: string;
  sourceFile: string | null;
  rateBeforePercent: number | null;
  rateAfterPercent: number | null;
  tenorMonthsBefore: number | null;
  tenorMonthsAfter: number | null;
  capitalizedAmount: number;
  waivedAmount: number;
  overdueAmountRolledIn: number;
  regulatoryReference: string | null;
  parameters: ScheduleParameters;
}

export interface Facility {
  id: string;
  sanctionId: string;
  lenderCode: string;
  currency: string;
  versions: FacilityVersion[];
}

export interface CreateOriginalFacilityRequest {
  sanctionId: string;
  lenderCode: string;
  currency: string;
  effectiveDate: string;
  parameters: ScheduleParameters;
}

export interface AddFacilityVersionRequest {
  eventType: FacilityVersionEventType;
  effectiveDate: string;
  label: string;
  sourceFile: string | null;
  rateBeforePercent: number | null;
  rateAfterPercent: number | null;
  tenorMonthsBefore: number | null;
  tenorMonthsAfter: number | null;
  capitalizedAmount: number;
  waivedAmount: number;
  overdueAmountRolledIn: number;
  regulatoryReference: string | null;
  parameters: ScheduleParameters;
}

export interface ApplyInstallmentOverrideRequest {
  interestOverride: number | null;
  openingBalanceOverride: number | null;
  closingBalanceOverride: number | null;
}

export type { ScheduleRow };
