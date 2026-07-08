import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ScheduleRow } from '../repayment/repayment.models';
import {
  AddFacilityVersionRequest, ApplyInstallmentOverrideRequest, CreateOriginalFacilityRequest, Facility
} from './facility.models';

@Injectable({ providedIn: 'root' })
export class FacilityService {
  private readonly base = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  bySanction(sanctionId: string) {
    return this.http.get<Facility[]>(`${this.base}/facilities/by-sanction/${sanctionId}`);
  }
  createOriginal(request: CreateOriginalFacilityRequest) {
    return this.http.post<{ id: string }>(`${this.base}/facilities/original`, request);
  }
  addVersion(facilityId: string, request: AddFacilityVersionRequest) {
    return this.http.post<{ versionId: string }>(`${this.base}/facilities/${facilityId}/versions`, request);
  }
  getSchedule(facilityId: string, versionId: string) {
    return this.http.get<ScheduleRow[]>(`${this.base}/facilities/${facilityId}/versions/${versionId}/schedule`);
  }
  applyOverride(facilityId: string, versionId: string, installmentIndex: number, request: ApplyInstallmentOverrideRequest) {
    return this.http.patch<void>(
      `${this.base}/facilities/${facilityId}/versions/${versionId}/installments/${installmentIndex}`, request);
  }
}
