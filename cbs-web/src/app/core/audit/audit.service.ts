import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AuditLogEntry } from './audit.models';

@Injectable({ providedIn: 'root' })
export class AuditService {
  constructor(private readonly http: HttpClient) {}

  getRecent(take = 50) {
    return this.http.get<AuditLogEntry[]>(`${environment.apiBaseUrl}/audit`, { params: { take } });
  }
}
