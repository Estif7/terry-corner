import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, from, map, of, throwError } from 'rxjs';
import { AuthResponse, AuthUser, LoginRequest, RegisterRequest } from '../models/auth.models';
import { SupabaseService } from './supabase.service';
import { ToastService } from './toast.service';

const USER_KEY = 'tc_user';
const ACCESS_TOKEN_KEY = 'tc_access_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly supabase = inject(SupabaseService);
  private readonly toast = inject(ToastService);

  private readonly userSignal = signal<AuthUser | null>(this.readStoredUser());
  readonly user = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.userSignal() !== null);
  readonly isStaffLevel = computed(() =>
    this.userSignal()?.roles.some((r) => ['Manager', 'Admin'].includes(r)) ?? false,
  );
  readonly isAdmin = computed(() => this.userSignal()?.roles.includes('Admin') ?? false);

  login(request: LoginRequest): Observable<AuthResponse> {
    const client = this.supabase.client;

    if (client) {
      return from(
        client.auth.signInWithPassword({
          email: request.email.trim(),
          password: request.password,
        }),
      ).pipe(
        map(({ data, error }) => {
          if (error || !data.user) {
            const msg = error?.message || 'Invalid email or password.';
            this.toast.error(msg);
            throw new Error(msg);
          }

          const user: AuthUser = {
            id: data.user.id,
            email: data.user.email ?? request.email,
            fullName: data.user.user_metadata?.['full_name'] || data.user.user_metadata?.['name'] || 'Terry Admin',
            roles: ['Admin', 'Manager'],
          };

          const res: AuthResponse = {
            accessToken: data.session?.access_token ?? '',
            refreshToken: data.session?.refresh_token ?? '',
            expiresAtUtc: new Date(Date.now() + 3600 * 1000).toISOString(),
            user,
          };

          this.persistSession(res);
          this.toast.success(`Welcome back, ${user.fullName}!`);
          return res;
        }),
      );
    }

    // Fallback if Supabase is not configured yet (local setup mode)
    if (request.password === 'admin123' || request.password === 'terrycorner') {
      const user: AuthUser = {
        id: 'local-admin',
        email: request.email.trim(),
        fullName: 'Terry Corner Admin',
        roles: ['Admin', 'Manager'],
      };

      const res: AuthResponse = {
        accessToken: 'local-token',
        refreshToken: 'local-refresh-token',
        expiresAtUtc: new Date(Date.now() + 86400 * 1000).toISOString(),
        user,
      };

      this.persistSession(res);
      this.toast.success('Signed in as Terry Admin (Local Setup Mode)');
      return of(res);
    }

    this.toast.error('Invalid credentials. (Hint: configure Supabase in settings, or use password "admin123")');
    return throwError(() => new Error('Invalid credentials.'));
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    const client = this.supabase.client;
    if (client) {
      return from(
        client.auth.signUp({
          email: request.email.trim(),
          password: request.password,
          options: {
            data: {
              full_name: request.fullName,
              phone: request.phoneNumber,
            },
          },
        }),
      ).pipe(
        map(({ data, error }) => {
          if (error || !data.user) {
            const msg = error?.message || 'Failed to create account.';
            this.toast.error(msg);
            throw new Error(msg);
          }

          const user: AuthUser = {
            id: data.user.id,
            email: data.user.email ?? request.email,
            fullName: request.fullName,
            roles: ['Admin', 'Manager'],
          };

          const res: AuthResponse = {
            accessToken: data.session?.access_token ?? '',
            refreshToken: data.session?.refresh_token ?? '',
            expiresAtUtc: new Date(Date.now() + 3600 * 1000).toISOString(),
            user,
          };

          this.persistSession(res);
          this.toast.success('Account created successfully!');
          return res;
        }),
      );
    }

    return of({} as AuthResponse);
  }

  logout(): void {
    const client = this.supabase.client;
    if (client) {
      client.auth.signOut().catch(() => {});
    }

    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    this.userSignal.set(null);
    this.toast.info('Signed out successfully.');
  }

  hasRole(role: string): boolean {
    return this.userSignal()?.roles.includes(role as any) ?? false;
  }

  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  private persistSession(res: AuthResponse): void {
    localStorage.setItem(USER_KEY, JSON.stringify(res.user));
    localStorage.setItem(ACCESS_TOKEN_KEY, res.accessToken);
    this.userSignal.set(res.user);
  }

  private readStoredUser(): AuthUser | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as AuthUser;
    } catch {
      return null;
    }
  }
}
