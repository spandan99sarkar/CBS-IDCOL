export interface ReceiptGlLine {
  glCode: string;
  description: string;
  debit: number;
  credit: number;
}

export interface Receipt {
  id: string;
  referenceNo: string;
  sanctionRef: string;
  customerNo: string;
  projectName: string;
  currency: string;
  paymentMode: string;
  instrumentNo: string | null;
  instrumentAmount: number;
  valueDate: string;
  receiveDate: string;
  principalAmount: number;
  interestAmount: number;
  lpcAmount: number;
  status: string; // Pending | Verified
  enteredBy: string;
  verifiedBy: string | null;
  glLines: ReceiptGlLine[];
}

export interface EnterReceiptRequest {
  sanctionId: string;
  sanctionRef: string;
  customerNo: string;
  projectName: string;
  currency: string;
  paymentMode: string;
  instrumentNo: string | null;
  bankName: string | null;
  instrumentAmount: number;
  valueDate: string;
  receiveDate: string;
  lpcDate: string | null;
  principalAmount: number;
  interestAmount: number;
  lpcAmount: number;
}
