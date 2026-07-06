import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { EnterReceiptRequest, Receipt } from './collection.models';

@Injectable({ providedIn: 'root' })
export class CollectionService {
  private readonly base = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  list() {
    return this.http.get<Receipt[]>(`${this.base}/collections`);
  }
  enter(request: EnterReceiptRequest) {
    return this.http.post<{ id: string }>(`${this.base}/collections`, request);
  }
  verify(id: string, comment: string | null) {
    return this.http.post<void>(`${this.base}/collections/${id}/verify`, { comment });
  }
}
