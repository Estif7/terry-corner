export type UserRole = 'Customer' | 'Staff' | 'Manager' | 'Admin';

export interface AuthUser {
  id: string;
  email: string;
  fullName: string;
  roles: UserRole[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  phoneNumber: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
  user: AuthUser;
}
