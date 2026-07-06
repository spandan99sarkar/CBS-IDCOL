import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import {
  Disbursement, InitiateDisbursementRequest, PostDisbursementRequest, ReviewDisbursementRequest
} from './disbursement.models';

@Injectable({ providedIn: 'root' })
export class DisbursementService {
  private readonly base = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  list() {
    return this.http.get<Disbursement[]>(`${this.base}/disbursements`);
  }
  initiate(request: InitiateDisbursementRequest) {
    return this.http.post<{ id: string }>(`${this.base}/disbursements/initiate`, request);
  }
  review(id: string, request: ReviewDisbursementRequest) {
    return this.http.post<void>(`${this.base}/disbursements/${id}/review`, request);
  }
  post(id: string, request: PostDisbursementRequest) {
    return this.http.post<void>(`${this.base}/disbursements/${id}/post`, request);
  }
}
