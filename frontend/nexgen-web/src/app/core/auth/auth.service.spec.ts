import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { AuthService } from './auth.service';
import { PermissionService } from './permission.service';
import { TokenStore } from './token-store';

const LOGIN_URL = '/api/v1/auth/login';
const REFRESH_URL = '/api/v1/auth/refresh';
const LOGOUT_URL = '/api/v1/auth/logout';
const ME_URL = '/api/v1/me';

function loginResponse() {
  return {
    token: 'access-1',
    refreshToken: 'refresh-1',
    tokenExpiresAtUtc: '2026-01-01T00:15:00Z',
    username: 'alice',
    userId: 7,
    tenantId: 3,
    role: 'Administrator',
  };
}

function meResponse() {
  return {
    userId: 7,
    userName: 'alice',
    tenantId: 3,
    role: 'Administrator',
    rights: {
      'Sales Order': { view: true, create: true, edit: false, delete: false, hidden: false },
    },
  };
}

/** `firstValueFrom(...)`'s `await` continuation — the call chaining `login → /me` — resumes
 * on a microtask, not synchronously with `flush()`. A macrotask tick reliably drains every
 * pending microtask ahead of it regardless of exactly how many the RxJS/HttpClient chain
 * uses internally, which is more robust here than guessing a fixed number of
 * `Promise.resolve()` hops. */
function flushMicrotasks(): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, 0));
}

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('login: success populates user, permissions and status via a real login -> /me sequence', async () => {
    const promise = service.login('acme', 'alice', 'secret');

    const loginReq = httpMock.expectOne(LOGIN_URL);
    expect(loginReq.request.body).toEqual({
      tenant: 'acme',
      username: 'alice',
      password: 'secret',
    });
    loginReq.flush(loginResponse());
    await flushMicrotasks();

    const meReq = httpMock.expectOne(ME_URL);
    meReq.flush(meResponse());

    const result = await promise;

    expect(result).toEqual({ ok: true });
    expect(service.status()).toBe('authenticated');
    expect(service.user()).toEqual({
      userId: 7,
      userName: 'alice',
      tenantId: 3,
      role: 'Administrator',
      rights: {
        'Sales Order': { view: true, create: true, edit: false, delete: false, hidden: false },
      },
    });

    const permissions = TestBed.inject(PermissionService);
    expect(permissions.forScreen('Sales Order')().create).toBe(true);
  });

  it('login: a 401 leaves status anonymous and reports invalid-credentials, without a /me call', async () => {
    const promise = service.login('acme', 'alice', 'wrong');

    httpMock
      .expectOne(LOGIN_URL)
      .flush(
        { status: 401, title: 'Invalid username or password.', type: '/problems/unauthenticated' },
        { status: 401, statusText: 'Unauthorized' },
      );

    const result = await promise;

    expect(result).toEqual({ ok: false, failure: { reason: 'invalid-credentials' } });
    expect(service.status()).toBe('anonymous');
    httpMock.expectNone(ME_URL);
  });

  // M2-A05 — the failure this task's own AuthController.TenantUnresolvedProblem produces,
  // distinct from a credential failure: the tenant field itself matched no row.
  it('login: an unresolved tenant reports tenant-unresolved, without a /me call', async () => {
    const promise = service.login('no-such-tenant', 'alice', 'secret');

    httpMock
      .expectOne(LOGIN_URL)
      .flush(
        { status: 400, title: 'Unable to resolve tenant.', type: '/problems/tenant-unresolved' },
        { status: 400, statusText: 'Bad Request' },
      );

    const result = await promise;

    expect(result).toEqual({
      ok: false,
      failure: { reason: 'tenant-unresolved', message: 'Unable to resolve tenant.' },
    });
    expect(service.status()).toBe('anonymous');
    httpMock.expectNone(ME_URL);
  });

  it('bootstrap: with no refresh token settles straight to anonymous, no HTTP call at all', async () => {
    await service.bootstrap();

    expect(service.status()).toBe('anonymous');
    await expect(service.whenBootstrapped()).resolves.toBeUndefined();
  });

  it('bootstrap: with a live refresh token, rotates it and re-derives identity via /me', async () => {
    // TokenStore is a private implementation detail AuthService owns; seed it the same way a
    // real (future, non-in-memory) custody model would have rehydrated it before bootstrap()
    // runs — this test proves bootstrap's refresh-success path independently of login's.
    const tokens = TestBed.inject(TokenStore);
    tokens.setSession('stale-access', 'stale-refresh', new Date('2020-01-01'), 'acme');

    const bootstrapPromise = service.bootstrap();

    const refreshReq = httpMock.expectOne(REFRESH_URL);
    // M2-A05 — the same tenant the session was opened with is resent, not re-derived: an
    // expired access token carries no claim to read it from.
    expect(refreshReq.request.body).toEqual({ tenant: 'acme', refreshToken: 'stale-refresh' });
    refreshReq.flush({
      token: 'fresh-access',
      refreshToken: 'fresh-refresh',
      tokenExpiresAtUtc: '2026-01-01T00:15:00Z',
    });
    await flushMicrotasks();
    httpMock.expectOne(ME_URL).flush(meResponse());

    await bootstrapPromise;

    expect(service.status()).toBe('authenticated');
    expect(tokens.accessToken).toBe('fresh-access');
    expect(tokens.refreshToken).toBe('fresh-refresh');
    // Rotation does not change the tenant — setSession's original value stands.
    expect(tokens.tenant).toBe('acme');
  });

  it('bootstrap: a refresh token with no stored tenant settles to anonymous, no HTTP call', async () => {
    // Not reachable through TokenStore's own public API today (setSession requires tenant),
    // but the guard exists precisely so a future custody model cannot silently fire a
    // guaranteed-400 refresh call if the two ever desynchronise — proven directly here.
    const tokens = TestBed.inject(TokenStore);
    vi.spyOn(tokens, 'refreshToken', 'get').mockReturnValue('stale-refresh');
    vi.spyOn(tokens, 'tenant', 'get').mockReturnValue(null);

    await service.bootstrap();

    expect(service.status()).toBe('anonymous');
    httpMock.expectNone(REFRESH_URL);
  });

  it('logout: revokes server-side, clears the token store and resets to anonymous', async () => {
    // Establish a session first.
    const loginPromise = service.login('acme', 'alice', 'secret');
    httpMock.expectOne(LOGIN_URL).flush(loginResponse());
    await flushMicrotasks();
    httpMock.expectOne(ME_URL).flush(meResponse());
    await loginPromise;

    const tokens = TestBed.inject(TokenStore);
    expect(tokens.hasSession()).toBe(true);

    const logoutPromise = service.logout();
    const logoutReq = httpMock.expectOne(LOGOUT_URL);
    // M2-A05 — same tenant-resend reason as refresh.
    expect(logoutReq.request.body).toEqual({ tenant: 'acme', refreshToken: 'refresh-1' });
    logoutReq.flush(null, { status: 204, statusText: 'No Content' });
    await logoutPromise;

    expect(service.status()).toBe('anonymous');
    expect(service.user()).toBeNull();
    expect(tokens.hasSession()).toBe(false);
    expect(tokens.tenant).toBeNull();

    const permissions = TestBed.inject(PermissionService);
    expect(permissions.forScreen('Sales Order')().create).toBe(false);
  });

  it('custody: a full login -> logout cycle writes nothing to localStorage or sessionStorage', async () => {
    const localSetSpy = vi.spyOn(Storage.prototype, 'setItem');

    const loginPromise = service.login('acme', 'alice', 'secret');
    httpMock.expectOne(LOGIN_URL).flush(loginResponse());
    await flushMicrotasks();
    httpMock.expectOne(ME_URL).flush(meResponse());
    await loginPromise;

    const logoutPromise = service.logout();
    httpMock.expectOne(LOGOUT_URL).flush(null, { status: 204, statusText: 'No Content' });
    await logoutPromise;

    // Storage.prototype.setItem covers both localStorage and sessionStorage, since both
    // inherit from the same prototype — one spy proves zero writes to either, including the
    // new tenant field (M2-A05), which lives in the same in-memory TokenStore as the tokens.
    expect(localSetSpy).not.toHaveBeenCalled();

    localSetSpy.mockRestore();
  });
});
