import {
  HttpClient,
  HttpErrorResponse,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { authInterceptor } from './auth.interceptor';
import { TokenStore } from './token-store';

/** See `auth.service.spec.ts`'s identical helper — RxJS's own internal chaining between the
 * refresh call's completion and the retried `next(retried)` call resumes on a microtask, not
 * synchronously with `flush()`. A macrotask tick drains it deterministically. */
function flushMicrotasks(): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, 0));
}

const PROTECTED_URL = '/api/v1/reference/currencies';
const REFRESH_URL = '/api/v1/auth/refresh';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let tokens: TokenStore;

  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    tokens = TestBed.inject(TokenStore);
    tokens.setSession('stale-access', 'live-refresh', new Date('2020-01-01'));
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('attaches the bearer token to a non-auth request', async () => {
    const promise = firstValueFrom(http.get(PROTECTED_URL));

    const req = httpMock.expectOne(PROTECTED_URL);
    expect(req.request.headers.get('Authorization')).toBe('Bearer stale-access');
    req.flush({});

    await promise;
  });

  it('does not attach a bearer token to /api/v1/auth/* calls', async () => {
    const promise = firstValueFrom(http.post(REFRESH_URL, { refreshToken: 'live-refresh' }));

    const req = httpMock.expectOne(REFRESH_URL);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({ token: 'x', refreshToken: 'y', tokenExpiresAtUtc: '2026-01-01T00:00:00Z' });

    await promise;
  });

  it('single-flight: three concurrent 401s trigger exactly one refresh call, and each original is retried once', async () => {
    const promises = [
      firstValueFrom(http.get(`${PROTECTED_URL}/1`)),
      firstValueFrom(http.get(`${PROTECTED_URL}/2`)),
      firstValueFrom(http.get(`${PROTECTED_URL}/3`)),
    ];

    // All three fail with 401 first.
    for (const path of ['1', '2', '3']) {
      httpMock
        .expectOne(`${PROTECTED_URL}/${path}`)
        .flush(null, { status: 401, statusText: 'Unauthorized' });
    }

    // Exactly one refresh call, shared by all three waiters.
    const refreshReq = httpMock.expectOne(REFRESH_URL);
    refreshReq.flush({
      token: 'fresh-access',
      refreshToken: 'fresh-refresh',
      tokenExpiresAtUtc: '2026-01-01T00:15:00Z',
    });
    await flushMicrotasks();

    // Each original request is retried exactly once, now with the fresh token.
    for (const path of ['1', '2', '3']) {
      const retried = httpMock.expectOne(`${PROTECTED_URL}/${path}`);
      expect(retried.request.headers.get('Authorization')).toBe('Bearer fresh-access');
      retried.flush({ ok: true });
    }

    await Promise.all(promises);

    // Only one refresh call total — httpMock.verify() in afterEach would also catch a
    // second, unexpected one as an "unmatched request", but this is the assertion that
    // actually names the property under test.
    httpMock.expectNone(REFRESH_URL);
  });

  it('a failed refresh clears the session and rethrows the original 401 (hard logout, no retry loop)', async () => {
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    const promise = firstValueFrom(http.get(PROTECTED_URL)).catch((e: HttpErrorResponse) => e);

    httpMock.expectOne(PROTECTED_URL).flush(null, { status: 401, statusText: 'Unauthorized' });

    const refreshReq = httpMock.expectOne(REFRESH_URL);
    refreshReq.flush(null, { status: 401, statusText: 'Unauthorized' });

    // Only reachable via the .catch() rejection path above, so this is always the error.
    const error = (await promise) as HttpErrorResponse;

    expect(error.status).toBe(401);
    expect(tokens.hasSession()).toBe(false);
    expect(navigateSpy).toHaveBeenCalledWith(['/login'], {
      queryParams: { reason: 'session-expired' },
    });

    // No second refresh attempt off the back of the failure itself.
    httpMock.expectNone(REFRESH_URL);
  });

  it('a 401 on the refresh call itself never retries — it is excluded from this whole path', async () => {
    const promise = firstValueFrom(http.post(REFRESH_URL, { refreshToken: 'live-refresh' })).catch(
      (e: HttpErrorResponse) => e,
    );

    httpMock.expectOne(REFRESH_URL).flush(null, { status: 401, statusText: 'Unauthorized' });

    // Only reachable via the .catch() rejection path above, so this is always the error.
    const error = (await promise) as HttpErrorResponse;

    expect(error.status).toBe(401);
    httpMock.expectNone(REFRESH_URL); // no second, "refresh the refresh" attempt
  });
});
