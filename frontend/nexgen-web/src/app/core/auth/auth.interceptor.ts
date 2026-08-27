import { HttpErrorResponse, type HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, from, switchMap, throwError } from 'rxjs';

import { AuthService } from './auth.service';
import { RefreshCoordinator } from './refresh-coordinator';
import { TokenStore } from './token-store';

/** `/api/v1/auth/*` — login, refresh, logout. None takes a bearer token (all
 * `[AllowAnonymous]`), and none may trigger the 401 → refresh → retry path itself, or a
 * failed refresh would refresh-retry-refresh forever. */
function isAuthEndpoint(url: string): boolean {
  return url.includes('/api/v1/auth/');
}

/**
 * M2-C02 — bearer injection plus the single-flight 401 refresh.
 *
 * `error.interceptor.ts` runs after this one in the chain (registration order in
 * `app.config.ts`) and normalises whatever this interceptor lets through — including the
 * hard-logout case below, which still rethrows the original 401 so the caller's own error
 * handling (e.g. a component showing an inline error) still runs.
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const tokens = inject(TokenStore);
  const refreshCoordinator = inject(RefreshCoordinator);
  const auth = inject(AuthService);
  const router = inject(Router);

  const authenticated = !isAuthEndpoint(request.url) && tokens.accessToken;
  const outgoing = authenticated
    ? request.clone({ setHeaders: { Authorization: `Bearer ${tokens.accessToken}` } })
    : request;

  return next(outgoing).pipe(
    catchError((error: unknown) => {
      const is401 = error instanceof HttpErrorResponse && error.status === 401;
      if (!is401 || isAuthEndpoint(request.url)) {
        return throwError(() => error);
      }

      return from(refreshCoordinator.refresh()).pipe(
        switchMap((refreshed) => {
          if (!refreshed) {
            auth.hardLogoutLocally();
            void router.navigate(['/login'], { queryParams: { reason: 'session-expired' } });
            return throwError(() => error);
          }
          // Retry exactly once, with the freshly rotated token — never re-enters this
          // catchError for a second 401 on the retried request; RxJS's own single
          // subscription to `next(retried)` here means a second failure propagates as a
          // plain error, not another refresh attempt.
          const retried = request.clone({
            setHeaders: { Authorization: `Bearer ${tokens.accessToken}` },
          });
          return next(retried);
        }),
      );
    }),
  );
};
