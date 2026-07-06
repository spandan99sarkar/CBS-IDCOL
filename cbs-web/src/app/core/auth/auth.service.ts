import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CurrentUser, LoginRequest, LoginResponse } from './auth.models';
import { decodeJwtPayload } from './jwt.decode';

const TOKEN_STORAGE_KEY = 'idcol_cbs_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenSignal = signal<string | null>(localStorage.getItem(TOKEN_STORAGE_KEY));

  readonly token = this.tokenSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.tokenSignal());
  readonly currentUser = computed<CurrentUser | null>(() => {
    const token = this.tokenSignal();
    if (!token) {
      return null;
    }

    const claims = decodeJwtPayload(token);
    if (!claims) {
      return null;
    }

    const roleClaim = claims['role'];
    const roleCodes = Array.isArray(roleClaim) ? roleClaim : roleClaim ? [roleClaim as string] : [];

    return {
      userId: String(claims['sub'] ?? ''),
      username: String(claims['unique_name'] ?? ''),
      displayName: String(claims['displayName'] ?? ''),
      businessUnit: String(claims['businessUnit'] ?? ''),
      roleCodes: roleCodes as string[]
    };
  });

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest) {
    return this.http.post<LoginResponse>(`${environment.apiBaseUrl}/auth/login`, request);
  }

  setToken(token: string): void {
    localStorage.setItem(TOKEN_STORAGE_KEY, token);
    this.tokenSignal.set(token);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    this.tokenSignal.set(null);
  }
}
