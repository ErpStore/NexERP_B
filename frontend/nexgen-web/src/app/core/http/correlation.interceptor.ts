import type { HttpInterceptorFn } from '@angular/common/http';

/**
 * M2-C02 — attaches `X-Correlation-Id` to every outgoing request, matching the header name
 * the server reads and echoes (`V.SMART.Api/Middleware/CorrelationId.cs:25,41`). A fresh
 * random id per request, not per session — each request gets its own trace, which is what
 * makes a `traceId` useful for correlating one failed call with one server-side log line.
 *
 * `crypto.randomUUID()` — available in every browser this workspace targets (secure
 * contexts only, which `https://`/`localhost` both are); no UUID library dependency needed.
 */
export const correlationInterceptor: HttpInterceptorFn = (request, next) =>
  next(
    request.clone({
      setHeaders: { 'X-Correlation-Id': crypto.randomUUID() },
    }),
  );
