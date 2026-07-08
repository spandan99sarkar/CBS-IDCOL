import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import {
  CreateCustomerRequest, CreateProductRequest, CreateSanctionRequest,
  Customer, Product, Sanction
} from './lifecycle.models';

@Injectable({ providedIn: 'root' })
export class LifecycleService {
  private readonly base = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  listCustomers() {
    return this.http.get<Customer[]>(`${this.base}/customers`);
  }
  createCustomer(request: CreateCustomerRequest) {
    return this.http.post<{ id: string }>(`${this.base}/customers`, request);
  }

  listProducts() {
    return this.http.get<Product[]>(`${this.base}/products`);
  }
  createProduct(request: CreateProductRequest) {
    return this.http.post<{ id: string }>(`${this.base}/products`, request);
  }

  listSanctions() {
    return this.http.get<Sanction[]>(`${this.base}/sanctions`);
  }
  createSanction(request: CreateSanctionRequest) {
    return this.http.post<{ id: string }>(`${this.base}/sanctions`, request);
  }
  signSanction(id: string) {
    return this.http.post<void>(`${this.base}/sanctions/${id}/sign`, {});
  }
}
