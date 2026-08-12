/**
 * LEARNING — Signals for auth state
 * ---------------------------------
 * `signal()` holds reactive state. When the value changes, any template or
 * `computed()` that reads it updates automatically — similar to a Blazor
 * property that triggers re-render, but without Zone.js change detection for that value.
 *
 * Token storage: localStorage survives page refresh (unlike Blazor circuit auth).
 */
import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  userId: number;
  tenantId: number;
  role: string;
}

const TOKEN_KEY = 'vsmart_jwt';
const USER_KEY = 'vsmart_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenSignal = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly userSignal = signal<LoginResponse | null>(this.readStoredUser());

  /** Read-only views of auth state for components. */
  readonly token = this.tokenSignal.asReadonly();
  readonly user = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.tokenSignal());

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router
  ) {}

  async login(username: string, password: string): Promise<void> {
    /**
     * LEARNING — Observables vs async/await
     * HttpClient returns an Observable (lazy stream). `firstValueFrom` converts
     * the first emission into a Promise so we can `await` it like C# Task.
     */
    const response = await firstValueFrom(
      this.http.post<LoginResponse>(`${environment.apiBaseUrl}/api/auth/login`, {
        username,
        password
      } satisfies LoginRequest)
    );

    localStorage.setItem(TOKEN_KEY, response.token);
    localStorage.setItem(USER_KEY, JSON.stringify(response));
    this.tokenSignal.set(response.token);
    this.userSignal.set(response);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.tokenSignal.set(null);
    this.userSignal.set(null);
    void this.router.navigateByUrl('/login');
  }

  private readStoredUser(): LoginResponse | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as LoginResponse;
    } catch {
      return null;
    }
  }
}
