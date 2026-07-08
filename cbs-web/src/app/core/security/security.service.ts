import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

export interface SecurityInstrument {
  id: string;
  category: string;
  instrumentFamily: string;
  loanType: string | null;
  clientName: string;
  projectName: string;
  instrumentNumber: string | null;
  issuingBank: string | null;
  currency: string;
  leafValueOrInitialAmount: number;
  currentBalance: number;
  issueDate: string | null;
  expiryDate: string | null;
  verificationStatus: string;
  autoRenewal: boolean;
  lifecycleState: string;
  daysLeft: number | null;
  recommendedAction: string;
  marketValue: number | null;
  forcedSaleValue: number | null;
  idcolPortionPercent: number;
  eligibleSecurityPercent: number;
  eligibleAmount: number;
  provider: string | null;
  rating: string | null;
  covenantType: string | null;
  complianceStatus: string | null;
  nextDueDate: string | null;
  remarks: string | null;
}

export interface SecuritySummary {
  asOf: string;
  total: number;
  security: number;
  covenant: number;
  expired: number;
  expiringIn30: number;
  expiringIn90: number;
  totalEligibleSecurity: number;
  byFamily: { family: string; count: number }[];
  actionsNeeded: { action: string; count: number }[];
}

@Injectable({ providedIn: 'root' })
export class SecurityService {
  private readonly base = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  list(category?: string, family?: string) {
    let params = new HttpParams();
    if (category) params = params.set('category', category);
    if (family) params = params.set('family', family);
    return this.http.get<SecurityInstrument[]>(`${this.base}/security`, { params });
  }

  summary() {
    return this.http.get<SecuritySummary>(`${this.base}/security/summary`);
  }

  generateLetter(id: string, letterType: string) {
    return this.http.post<{ refNo: string; letterType: string; body: string }>(
      `${this.base}/security/${id}/letters/generate`, { letterType });
  }

  letters(family?: string) {
    let params = new HttpParams();
    if (family) params = params.set('family', family);
    return this.http.get<{ family: string; letterType: string; purpose: string }[]>(
      `${this.base}/security/letters`, { params });
  }
}
