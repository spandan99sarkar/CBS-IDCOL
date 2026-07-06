export interface Customer {
  id: string;
  customerNo: string;
  customerType: string;
  name: string;
  businessUnitCode: string;
  mobile: string | null;
  email: string | null;
  sectorCode: string | null;
  kycStatus: string;
  riskLevel: string;
  source: string;
}

export interface CreateCustomerRequest {
  customerNo: string;
  customerType: string;
  name: string;
  businessUnitCode: string;
  mobile: string | null;
  email: string | null;
  sectorCode: string | null;
  kycStatus: string;
  riskLevel: string;
}

export interface Product {
  id: string;
  productCode: string;
  productName: string;
  productType: string;
  interestType: string;
  repaymentMethod: string;
  dayCountBasis: number;
  suggestedRatePercent: number;
  isActive: boolean;
}

export interface CreateProductRequest {
  productCode: string;
  productName: string;
  productType: string;
  interestType: string;
  repaymentMethod: string;
  dayCountBasis: number;
  gracePeriodMonths: number;
  prepaymentAllowed: boolean;
  penaltyAllowed: boolean;
  suggestedRatePercent: number;
  lowerRatePercent: number;
  upperRatePercent: number;
}

export interface Sanction {
  id: string;
  sanctionId: string;
  customerNo: string;
  productCode: string;
  projectName: string;
  loanCurrency: string;
  loanAmount: number;
  grantAmount: number;
  agreementDate: string;
  noOfPrincipalRepayments: number;
  status: string;
}

export interface CreateSanctionRequest {
  sanctionId: string;
  customerId: string;
  customerNo: string;
  productCode: string;
  projectName: string;
  industryType: string | null;
  loanCurrency: string;
  loanAmount: number;
  grantCurrency: string;
  grantAmount: number;
  agreementDate: string;
  expiryDate: string | null;
  interestRateType: string;
  initialInterestRatePercent: number;
  loanTenorMonths: number;
  noOfPrincipalRepayments: number;
  interestGracePeriodMonths: number;
  principalMoratoriumMonths: number;
  repaymentMethod: string;
  principalFrequency: number;
  interestFrequency: number;
  dayCountBasis: number;
  lpcRatePercent: number;
  creditRating: string | null;
}
