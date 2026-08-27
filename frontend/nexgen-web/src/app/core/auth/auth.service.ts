import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { AuthApiService, MeApiService, type MeResponse } from '../api/auth-api';
import { API_PROBLEM_TYPES, isProblemType, type ApiProblem } from '../http/api-problem';
import { PermissionService } from './permission.service';
import { TokenStore } from './token-store';
import type {
  AuthStatus,
  LoginFailure,
  ScreenRight,
  ScreenRights,
  UserIdentity,
} from './auth.models';

/**
 * M2-C02 — session lifecycle: login, silent bootstrap, logout. Signals only, exposed
 * read-only. Token custody sits behind {@link TokenStore}, never on a public signal here —
 * see that file for the full token-custody decision and its reasoning.
 *
 * **Login request shape, verified against the real contract, not this task's own plan.**
 * `AuthController.LoginRequest` is `{ username, password }` — **no `tenant` field.** Tenant
 * resolution stays Host-header-based today (`ITenantProvider`, unchanged); adding a tenant
 * selector to the login request is explicitly `M2-A05`'s job, not this one's
 * (`docs/kb/execution/tasks/M2-A04.md` Files That Must Not Change). This service and the
 * login form built on it therefore take only username + password, a deliberate deviation
 * from this task's own Flow diagram, recorded here rather than invented against.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly #api = inject(AuthApiService);
  readonly #me = inject(MeApiService);
  readonly #tokens = inject(TokenStore);
  readonly #permissions = inject(PermissionService);

  readonly #status = signal<AuthStatus>('unknown');
  readonly #user = signal<UserIdentity | null>(null);

  readonly status = this.#status.asReadonly();
  readonly user = this.#user.asReadonly();

  /** Resolves once the app-start bootstrap (see {@link bootstrap}) has settled `status` to
   * something other than `'unknown'`. `authGuard` awaits this before deciding — otherwise a
   * route activated mid-bootstrap would see a stale `'unknown'` and guess wrong. Set once,
   * by `bootstrap()` itself; a guard call before `bootstrap()` has ever run would hang, which
   * is deliberately impossible — `bootstrap()` is invoked from `provideAppInitializer`
   * (`app.config.ts`), which the router does not start resolving routes ahead of.
   *
   * **Declaration order matters here and is not incidental.** `#resolveBootstrap` must be
   * declared *before* `#bootstrapSettled`: class field initializers install private fields
   * onto the instance in declaration order, and a `Promise` executor runs **synchronously**
   * at construction time. With the fields in the other order, the executor's
   * `this.#resolveBootstrap = resolve` assignment ran before that private field had been
   * installed at all — `TypeError: Cannot write private member #resolveBootstrap to an
   * object whose class did not declare it`, caught by `auth.service.spec.ts`, not by
   * `npm run build`/`typecheck` (both passed while this bug was live; TypeScript's static
   * check of field access does not model JS's runtime field-installation order). */
  #resolveBootstrap!: () => void;
  #bootstrapSettled = new Promise<void>((resolve) => {
    this.#resolveBootstrap = resolve;
  });

  whenBootstrapped(): Promise<void> {
    return this.#bootstrapSettled;
  }

  async login(
    username: string,
    password: string,
  ): Promise<{ ok: true } | { ok: false; failure: LoginFailure }> {
    this.#status.set('authenticating');
    try {
      const response = await firstValueFrom(this.#api.login({ body: { username, password } }));
      if (!response.token || !response.refreshToken || !response.tokenExpiresAtUtc) {
        this.#status.set('anonymous');
        return { ok: false, failure: { reason: 'unknown' } };
      }
      this.#tokens.setSession(
        response.token,
        response.refreshToken,
        new Date(response.tokenExpiresAtUtc),
      );
      await this.#bootstrapIdentity();
      return { ok: true };
    } catch (error) {
      this.#status.set('anonymous');
      return { ok: false, failure: mapLoginFailure(error) };
    }
  }

  /**
   * Called once at app start. Attempts a silent refresh against whatever refresh token
   * {@link TokenStore} holds — which, under the in-memory custody decision, is **always
   * empty on a fresh page load** (a hard refresh loses the session; see `TokenStore`'s doc
   * comment for why). This still does the right thing when custody changes to something
   * that survives a reload: it is the one place that decides `'anonymous'` vs
   * `'authenticated'` at startup, so the rest of the app never has to guess.
   */
  async bootstrap(): Promise<void> {
    try {
      if (!this.#tokens.refreshToken) {
        this.#status.set('anonymous');
        return;
      }
      const response = await firstValueFrom(
        this.#api.refresh({ body: { refreshToken: this.#tokens.refreshToken } }),
      );
      if (!response.token || !response.refreshToken || !response.tokenExpiresAtUtc) {
        this.#tokens.clear();
        this.#status.set('anonymous');
        return;
      }
      this.#tokens.rotate(
        response.token,
        response.refreshToken,
        new Date(response.tokenExpiresAtUtc),
      );
      await this.#bootstrapIdentity();
    } catch {
      this.#tokens.clear();
      this.#status.set('anonymous');
    } finally {
      this.#resolveBootstrap();
    }
  }

  /**
   * Rotates the token pair using the current refresh token, without re-fetching `/me` —
   * unlike {@link bootstrap}, which does. Used by `auth.interceptor.ts`'s single-flight
   * 401 handler: a mid-session refresh does not need to re-derive identity/rights, only a
   * live access token to retry the request that triggered it. Returns `false` (and clears
   * the session) on any failure — the interceptor's job, not this method's, is deciding what
   * "failure" means for the request in flight (hard logout).
   */
  async refreshTokens(): Promise<boolean> {
    if (!this.#tokens.refreshToken) {
      return false;
    }
    try {
      const response = await firstValueFrom(
        this.#api.refresh({ body: { refreshToken: this.#tokens.refreshToken } }),
      );
      if (!response.token || !response.refreshToken || !response.tokenExpiresAtUtc) {
        return false;
      }
      this.#tokens.rotate(
        response.token,
        response.refreshToken,
        new Date(response.tokenExpiresAtUtc),
      );
      return true;
    } catch {
      return false;
    }
  }

  /** Clears session state without calling the server — the interceptor's hard-logout path
   * on a failed refresh. A separate, narrower operation than {@link logout}, which also
   * revokes server-side; here the refresh token is already known-bad, so there is nothing
   * left to revoke. */
  hardLogoutLocally(): void {
    this.#tokens.clear();
    this.#user.set(null);
    this.#permissions.clear();
    this.#status.set('anonymous');
  }

  async logout(): Promise<void> {
    const refreshToken = this.#tokens.refreshToken;
    this.#tokens.clear();
    this.#user.set(null);
    this.#permissions.clear();
    this.#status.set('anonymous');
    if (refreshToken) {
      try {
        await firstValueFrom(this.#api.logout({ body: { refreshToken } }));
      } catch {
        // Best-effort: the client-side session is already cleared regardless. A failed
        // revocation call does not resurrect the local session, and the token still expires
        // server-side on its own (M2-A04) even if this call is lost.
      }
    }
  }

  async #bootstrapIdentity(): Promise<void> {
    const me = await firstValueFrom(this.#me.getMe());
    this.#user.set(normaliseIdentity(me));
    this.#permissions.setRights(normaliseRights(me.rights));
    this.#status.set('authenticated');
  }
}

function normaliseIdentity(me: MeResponse): UserIdentity {
  return {
    userId: me.userId ?? 0,
    userName: me.userName ?? '',
    tenantId: me.tenantId ?? 0,
    role: me.role ?? '',
    rights: normaliseRights(me.rights),
  };
}

function normaliseRights(rights: MeResponse['rights']): ScreenRights {
  const result: Record<string, ScreenRight> = {};
  for (const [screenName, right] of Object.entries(rights ?? {})) {
    result[screenName] = {
      view: right.view ?? false,
      create: right.create ?? false,
      edit: right.edit ?? false,
      delete: right.delete ?? false,
      hidden: right.hidden ?? false,
    };
  }
  return result;
}

/**
 * Maps the server's actual, distinguishable failure reasons (M2-A06 problem+json). Never
 * distinguishes "unknown username" from "wrong password" — the server's own `401` body
 * doesn't either.
 */
function mapLoginFailure(error: unknown): LoginFailure {
  if (!(error instanceof HttpErrorResponse)) {
    return { reason: 'unknown' };
  }
  if (error.status === 0) {
    return { reason: 'network' };
  }
  const problem = error.error as ApiProblem | null;
  if (error.status === 401) {
    // Deliberately no message: one body for every credential failure (AuthController's own
    // UnauthenticatedProblem), so the client never has anything field-specific to leak.
    return { reason: 'invalid-credentials' };
  }
  if (isProblemType(problem, API_PROBLEM_TYPES.tenantUnresolved)) {
    return { reason: 'tenant-unresolved', message: problem?.title };
  }
  if (isProblemType(problem, API_PROBLEM_TYPES.trialExpired)) {
    return { reason: 'trial-expired', message: problem?.title };
  }
  if (
    isProblemType(problem, API_PROBLEM_TYPES.deviceNotRecognised) ||
    isProblemType(problem, API_PROBLEM_TYPES.platformNotAllowed)
  ) {
    return { reason: 'account-gate', message: problem?.title };
  }
  return { reason: 'unknown' };
}
