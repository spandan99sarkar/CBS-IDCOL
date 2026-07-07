import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

export interface ReportCatalogEntry {
  key: string;
  name: string;
  group: string;
  description: string;
}

export interface ReportColumn {
  key: string;
  label: string;
  kind: string; // text | money | int | rate | date | status
}

export interface ReportResult {
  title: string;
  generatedAtUtc: string;
  columns: ReportColumn[];
  rows: Record<string, unknown>[];
  totals: Record<string, unknown>;
}

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private readonly base = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  catalog() {
    return this.http.get<ReportCatalogEntry[]>(`${this.base}/reports/catalog`);
  }

  run(key: string, from?: string, to?: string) {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    return this.http.get<ReportResult>(`${this.base}/reports/${key}`, { params });
  }
}
