---
doc_id: KB-040
title: Existing API Surface (As-Is)
module: api
source_files:
  - V.SMART/V.SMART.Api/Program.cs
  - V.SMART/V.SMART.Api/Controllers/AuthController.cs
  - V.SMART/V.SMART.Api/Controllers/CurrencyController.cs
  - V.SMART/V.SMART.Api/Auth/JwtTokenService.cs
  - V.SMART/V.SMART.Api/Auth/ApiAuthStateProvider.cs
  - V.SMART/V.SMART.Api/appsettings.json
entities: [User, Currency, TenantInfo]
api_endpoints:
  - "POST /api/auth/login"
  - "GET /api/currencies"
  - "GET /api/currencies/{id}"
  - "POST /api/currencies"
  - "PUT /api/currencies/{id}"
  - "DELETE /api/currencies/{id}"
database_tables: [Users, Currency, Tenants]
business_rules: [BR-AUTH-001]
status: complete
confidence: confirmed
last_verified: 2026-08-12
dependencies: [KB-013, KB-014]
---

# Existing API Surface (As-Is)

> **The entire HTTP API today is 6 endpoints across 2 controllers.** This document is the
> complete inventory. What still needs building is in
> [`api-readiness-assessment.md`](api-readiness-assessment.md).

## Host configuration

`V.SMART.Api/Program.cs`:

| Concern | Configuration |
|---|---|
| Controllers | `AddControllers()`, `MapControllers()` |
| OpenAPI | Swashbuckle, `SwaggerDoc("v1", "V.SMART API")`, Bearer security scheme; **`UseSwagger`/`UseSwaggerUI` only in Development** |
| CORS | single policy `"AngularDev"` → origins `http://localhost:4200`, any header, any method |
| AuthN | JWT Bearer; validates issuer, audience, lifetime, signing key; `ClockSkew = 1 min` |
| AuthZ | `AddAuthorization()` with **no policies registered** |
| Pipeline | `UseCors("AngularDev")` → `UseAuthentication()` → `UseAuthorization()` → `MapControllers()` |
| Tenancy | `ITenantProvider` / `ITenantDbContextFactory` scoped; `MasterDbContext` via `AddDbContext`; `ApplicationDbContext` built per-scope from the resolved tenant |
| Mapping | `AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfileMarker).Assembly))` |
| Registered domain services | **`ICurrencyService` only** — plus `IUnitOfWork`, `ForeignKeyUsageChecker`, `ILoggingService`, `IPasswordHasher<User>`, `CurrentUserService`, `UserSession`, `JwtTokenService` (singleton), `AuthenticationStateProvider → ApiAuthStateProvider` |

**Notable absences (Confirmed):** no HTTPS redirection, no exception-handling middleware,
no `ProblemDetails`, no rate limiting, no response compression, no health checks, no
request logging, no API versioning.

## Endpoint reference

### `POST /api/auth/login`

`AuthController.Login` · `[AllowAnonymous]`

**Request**
```json
{ "username": "string (required)", "password": "string (required)" }
```

**Responses**

| Status | Body | Condition |
|---|---|---|
| 200 | `{ "token", "username", "userId", "tenantId", "role" }` | success |
| 400 | `{ "message": "Unable to resolve tenant. Check host or wwwroot/config/tenant.json." }` | `ITenantProvider.GetCurrentTenant()` returned `null` |
| 401 | `{ "message": "Invalid username or password." }` | `LoginAsync` returned `null` |

**Auth required.** None.
**Business logic executed.** `ITenantProvider.GetCurrentTenant()` →
`IUnitOfWork.Users.LoginAsync` (BR-AUTH-001) → `JwtTokenService.CreateToken(user, tenant.Id)`.
**Entities.** `TenantInfo`, `User`.
**Token.** HS256; claims `ClaimTypes.Name`, `UserId`, `TenantId`, `ClaimTypes.Role`;
issuer `V.SMART.Api`; audience `V.SMART.Angular`; expiry `Jwt:ExpiresMinutes` (480).

**Contract gaps.** No refresh token. No screen-permission claims — the client cannot know
what to render, and the server cannot authorise beyond role. No tenant selector in the
request (see [multi-tenancy](../architecture/multi-tenancy.md) problem 1). Error body shape
`{ message }` is ad-hoc, not `ProblemDetails`.

**Defect (Confirmed).** A database failure inside `LoginAsync` is swallowed and returns
`null` (`UserRepository.cs:44-48`), so an outage is reported to the user as
"Invalid username or password".

---

### `GET /api/currencies`

`CurrencyController.GetAll` · `[Authorize]`

**Query parameters.** `pageNumber` (default 1), `pageSize` (default 10), `currName`,
`createdBy`, `fromDate`, `toDate` — the last four are packed into a
`Dictionary<string, object>` and passed to `SearchWithDynamicFilterAsync`.

**Response 200.** `{ "items": CurrencyVM[], "totalCount": int, "pageNumber": int, "pageSize": int }`

**Business logic.** `ICurrencyService.SearchWithDynamicFilterAsync(pageNumber, pageSize, filters)`.

**Note.** `fromDate`/`toDate` are passed as **strings** into an untyped dictionary — the
service parses them. Weak typing at the boundary; worth fixing in the convention.

---

### `GET /api/currencies/{id:int}`

200 `CurrencyVM` · 404 `{ "message": "Currency not found." }`.
Calls `ICurrencyService.GetByIdAsync(id)`.

---

### `POST /api/currencies`

**Request.** `CurrencyVM` — validated by `DataAnnotations` on the VM:
`CurrName` required ≤100, `CurrSub` required ≤100,
`Symbol` required and matching `[$€₹¥£₩₿₽]`.

| Status | Body |
|---|---|
| 201 | `CurrencyVM`, `Location: /api/currencies/{CurrId}` |
| 400 | `ValidationProblem(ModelState)` (model-binding failures) **or** `{ "message" }` (service rejection, e.g. duplicate) |

**Business logic.** `ICurrencyService.CreateAsync(vm)` → `(bool success, string message, CurrencyVM? entity)`.

**Note.** Two different 400 body shapes from one endpoint — `ValidationProblem` and
`{ message }`. Standardise in the convention.

---

### `PUT /api/currencies/{id:int}`

200 `CurrencyVM` · 400 as above. `ICurrencyService.UpdateAsync(id, vm)`.
**Note.** No check that `vm.CurrId` matches the route `id`.

---

### `DELETE /api/currencies/{id:int}`

| Status | Body | Condition |
|---|---|---|
| 204 | — | deleted |
| 400 | `{ "message" }` | `CanDeleteCurrencyAsync` refused (referential integrity) |
| 404 | `{ "message": "Currency not found." }` | not found |

**Business logic.** `CanDeleteCurrencyAsync(id)` then `DeleteCurrencyByCurrIdAsync(id)` —
the standard two-step delete-guard pattern (see BR-SO-001 for the same shape in Sales).

---

## 🚨 Authorization state of the existing API

`[Authorize]` on `CurrencyController` means **"any authenticated user of any tenant with a
valid token"**. It does **not** check `UserRight` for the `"Currency"` screen.

Concretely, today: a user whose `UserRight` row for `Currency` has
`CanView = CanCreate = CanEdit = CanDelete = false` — a user for whom the Blazor UI hides
the entire Currency screen — **can create, edit, and delete currencies through the API**.

This generalises to every controller that follows this template. It is the single most
important thing to fix before the API grows.
See [ADR-004](../decisions/ADR-004-server-side-authorization.md).

## Contract conventions established so far

Worth keeping (they are already consistent):

| Convention | Form |
|---|---|
| Base path | `/api/{plural-kebab-resource}` |
| Paged list | `GET /api/x?pageNumber&pageSize&<filters>` → `{ items, totalCount, pageNumber, pageSize }` |
| Single | `GET /api/x/{id:int}` |
| Create | `POST /api/x` → 201 + `Location` |
| Update | `PUT /api/x/{id:int}` → 200 |
| Delete | `DELETE /api/x/{id:int}` → 204, or 400 with the service's user-facing message |
| Payload type | the existing `…VM` ViewModel, unchanged |

Worth fixing before replication:

| Problem | Fix |
|---|---|
| Two 400 shapes | one `ProblemDetails` shape everywhere |
| Untyped `Dictionary<string, object>` filters | typed query DTO per resource |
| No route/body id consistency check | validate in the controller |
| No permission check | `[RequireScreen("Currency", Right.Edit)]` filter |
| No versioning | `/api/v1/…` |
| No correlation id | middleware |

## Ancillary

`V.SMART.Api/Auth/ApiAuthStateProvider.cs` implements `AuthenticationStateProvider` over
`IHttpContextAccessor` so that shared code expecting Blazor's auth abstraction
(e.g. `CurrentUserService`) works inside API requests. Small but essential — it is why
existing services run unchanged under HTTP.

`V.SMART.Api/V.SMART.Api.http` and `Properties/launchSettings.json` define the dev
endpoint (`http://localhost:5144`, matching the Angular pilot's `environment.apiBaseUrl`).
