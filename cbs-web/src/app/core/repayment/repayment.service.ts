import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { BorrowerExample, ScheduleParameters, ScheduleRow } from './repayment.models';

@Injectable({ providedIn: 'root' })
export class RepaymentService {
  constructor(private readonly http: HttpClient) {}

  /** Runs the .NET repayment engine over the given parameters. */
  compute(parameters: ScheduleParameters) {
    return this.http.post<ScheduleRow[]>(`${environment.apiBaseUrl}/repayment/compute`, parameters);
  }

  /** Loads the 19 real IDCOL borrower parameter sets bundled as a static asset. */
  loadExamples() {
    return this.http.get<BorrowerExample[]>('/borrower-examples.json');
  }
}
