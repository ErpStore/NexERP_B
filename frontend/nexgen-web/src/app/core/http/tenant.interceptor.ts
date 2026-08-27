import type { HttpInterceptorFn } from '@angular/common/http';

/**
 * M2-C02 — the tenant header, registered as a real interceptor even though it currently has
 * nothing to add.
 *
 * **Why this is a no-op today, disclosed rather than invented around.** Tenant resolution is
 * entirely server-side and Host-header-based (`ITenantProvider`/`TenantProvider.cs`) — the
 * client sends no tenant identifier of any kind, and `AuthController.LoginRequest` has no
 * `tenant` field (verified against the real contract, not this task's own Flow diagram; see
 * `auth.service.ts`'s doc comment). That resolution path happens to work today because this
 * workstation's sole tenant is hostnamed literally `"localhost"` — it is not a real
 * multi-tenant solution and was never meant to be one. **Cross-origin SPA tenant resolution
 * is `M2-A05`'s task, explicitly** (`docs/kb/execution/tasks/M2-C02.md` Prerequisites: "Soft
 * — if cross-origin cookies are not yet configured, record the limitation rather than
 * working around it"). This interceptor is the seam M2-A05 fills in — most likely adding an
 * `X-Tenant-Id`/similar header here once the server has a header-based (or subdomain-based)
 * resolution path to read it from — rather than something this task invents unilaterally
 * against an API that does not yet accept it.
 */
export const tenantInterceptor: HttpInterceptorFn = (request, next) => next(request);
