import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ClassificationResult, RunClassificationRequest } from './classification.models';

@Injectable({ providedIn: 'root' })
export class ClassificationService {
  private readonly base = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  list() {
    return this.http.get<ClassificationResult[]>(`${this.base}/classification`);
  }
  run(request: RunClassificationRequest) {
    return this.http.post<{ runId: string }>(`${this.base}/classification/run`, request);
  }
}
