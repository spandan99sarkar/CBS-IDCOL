export interface LoginRequest {
  username: string;
  plainTextPassword: string;
}

export interface LoginResponse {
  succeeded: boolean;
  token: string | null;
  failureReason: string | null;
}

export interface CurrentUser {
  userId: string;
  username: string;
  displayName: string;
  businessUnit: string;
  roleCodes: string[];
}
