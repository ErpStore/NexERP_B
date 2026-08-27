import { HttpErrorResponse, type HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

import type { ApiProblem } from './api-problem';

/**
 * M2-C02 — **the single place that parses a problem body into a typed `ApiProblem`.**
 * Nothing else in this workspace parses an error body (KB-050 §Error handling). Every
 * server error is already `application/problem+json`-shaped (M2-A06); this interceptor's
 * job is to guarantee that shape holds for **every** failure this app can see, including the
 * ones the server never had a chance to answer (a network failure, a CORS rejection, a
 * response that is not JSON at all) — so no downstream consumer ever has to special-case
 * "what if `error.error` isn't a problem body this time."
 *
 * Branches on `type`, never on `title` — `title` is human prose (KB-050's own rule; see
 * `api-problem.ts`'s `API_PROBLEM_TYPES`). A `409`'s `title` is carried through **verbatim**,
 * untouched by this interceptor — it is the one place BR-SO-001's business-rule sentence is
 * allowed to reach a human unmodified.
 */
export const errorInterceptor: HttpInterceptorFn = (request, next) =>
  next(request).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        throw error;
      }
      return throwError(
        () =>
          new HttpErrorResponse({
            error: normaliseProblem(error),
            headers: error.headers,
            status: error.status,
            statusText: error.statusText,
            url: error.url ?? undefined,
          }),
      );
    }),
  );

function normaliseProblem(error: HttpErrorResponse): ApiProblem {
  const body = error.error as Partial<ApiProblem> | string | null | undefined;

  if (body && typeof body === 'object') {
    // Already problem+json-shaped (the server's normal case, M2-A06). Passed through as-is
    // — this interceptor normalises the *envelope* every failure arrives in, not the
    // server's own contract, which stays authoritative. No cast needed: every ApiProblem
    // field is already optional, so Partial<ApiProblem> and ApiProblem are the same type.
    return body;
  }

  // A network failure (status 0), a CORS rejection, or any response the server sent before
  // M2-A06's middleware could shape it (a proxy 502, a raw text 500) — there is no problem
  // body to read. Synthesise the one field every consumer can safely rely on (`status`);
  // everything else stays `undefined` rather than guessed.
  return { status: error.status || undefined };
}
