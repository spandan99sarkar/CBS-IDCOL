export interface DisbursementGlLine {
  glCode: string;
  description: string;
  debit: number;
  credit: number;
}

export interface Disbursement {
  id: string;
  referenceNo: string;
  disbursementNo: number;
  sanctionRef: string;
  customerNo: string;
  projectName: string;
  loanCurrency: string;
  status: string; // Suggested | Proposed | Processed
  suggestedLoanAmount: number;
  suggestedGrantAmount: number;
  justifiedLoanAmount: number | null;
  justifiedGrantAmount: number | null;
  effectiveLoanAmount: number;
  effectiveGrantAmount: number;
  initiatedBy: string;
  proposedBy: string | null;
  postedBy: string | null;
  disbursementMode: string | null;
  valueDate: string | null;
  glLines: DisbursementGlLine[];
}

export interface InitiateDisbursementRequest {
  sanctionId: string;
  sanctionRef: string;
  customerNo: string;
  projectName: string;
  loanCurrency: string;
  suggestedLoanAmount: number;
  suggestedGrantAmount: number;
  buRemarks: string | null;
}

export interface ReviewDisbursementRequest {
  justifiedLoanAmount: number;
  justifiedGrantAmount: number;
  cadRemarks: string | null;
}

export interface PostDisbursementRequest {
  disbursementMode: string;
  valueDate: string;
  glLines: DisbursementGlLine[];
}
