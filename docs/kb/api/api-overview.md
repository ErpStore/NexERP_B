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
last_verified: 2026-08-20
dependencies: [KB-013, KB-014]
---

# Existing API Surface (As-Is)

> **The entire HTTP API today is 6 endpoints across 2 controllers.** This document is the
> complete inventory. What still needs building is in
> [`api-readiness-assessment.md`](api-readiness-assessment.md).

> **Updated by M2-A06 (2026-08-20).** Every **error** response below is now
> `application/problem+json`; success responses are unchanged. The one deliberate breaking
> change is `DELETE /api/currencies/{id}`, which answers **409** instead of 400 when the
> delete guard refuses. See [*Error contract*](#error-contract-m2-a06) below.

## Host configuration

`V.SMART.Api/Program.cs`:

| Concern | Configuration |
|---|---|
| Controllers | `AddControllers()`, `MapControllers()` |
| OpenAPI | Swashbuckle, `SwaggerDoc("v1", "V.SMART API")`, Bearer security scheme; **`UseSwagger`/`UseSwaggerUI` only in Development** |
| CORS | single policy `"AngularDev"` → origins `http://localhost:4200`, any header, any method |
| AuthN | JWT Bearer; validates issuer, audience, lifetime, signing key; `ClockSkew = 1 min` |
| AuthZ | `AddAuthorization()` with **no policies registered** |
| Pipeline | **`UseErrorContract()`** (correlation id → global exception handler → status-code problem bodies; M2-A06) → `UseCors("AngularDev")` → `UseAuthentication()` → `UseAuthorization()` → `MapControllers()` |
| Tenancy | `ITenantProvider` / `ITenantDbContextFactory` scoped; `MasterDbContext` via `AddDbContext`; `ApplicationDbContext` built per-scope from the resolved tenant |
| Mapping | `AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfileMarker).Assembly))` |
| Registered domain services | **`ICurrencyService` only** — plus `IUnitOfWork`, `ForeignKeyUsageChecker`, `ILoggingService`, `IPasswordHasher<User>`, `CurrentUserService`, `UserSession`, `JwtTokenService` (singleton), `AuthenticationStateProvider → ApiAuthStateProvider` |

**Notable absences (Confirmed, re-verified 2026-08-20):** no HTTPS redirection,
no rate limiting, no response compression, no health checks, no
request logging, no API versioning. **Exception middleware and `ProblemDetails` are no longer absent** — added by M2-A06 (see *Error contract* below).

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
| 400 | `problem+json`, `type: …/tenant-unresolved`, `title: "Unable to resolve tenant. Check host or wwwroot/config/tenant.json."` | `ITenantProvider.GetCurrentTenant()` returned `null` |
| 401 | `problem+json`, `type: …/unauthenticated`, `title: "Invalid username or password."` | `LoginAsync` returned `null` |

Both messages are unchanged from before M2-A06 — only the body shape changed. The `401`
deliberately says no more than it did: one title for every authentication failure.

**Auth required.** None.
**Business logic executed.** `ITenantProvider.GetCurrentTenant()` →
`IUnitOfWork.Users.LoginAsync` (BR-AUTH-001) → `JwtTokenService.CreateToken(user, tenant.Id)`.
**Entities.** `TenantInfo`, `User`.
**Token.** HS256; claims `ClaimTypes.Name`, `UserId`, `TenantId`, `ClaimTypes.Role`;
issuer `V.SMART.Api`; audience `V.SMART.Angular`; expiry `Jwt:ExpiresMinutes` (480).

**Contract gaps.** No refresh token. No screen-permission claims — the client cannot know
what to render, and the server cannot authorise beyond role. No tenant selector in the
request (see [multi-tenancy](../architecture/multi-tenancy.md) problem 1). *(The ad-hoc
`{ message }` error body was replaced by `problem+json` in M2-A06.)*

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

200 `CurrencyVM` · 404 `problem+json`, `type: …/not-found`, `title: "Currency not found."`.
Calls `ICurrencyService.GetByIdAsync(id)`.

---

### `POST /api/currencies`

**Request.** `CurrencyVM` — validated by `DataAnnotations` on the VM:
`CurrName` required ≤100, `CurrSub` required ≤100,
`Symbol` required and matching `[$€₹¥£₩₿₽]`.

| Status | Body |
|---|---|
| 201 | `CurrencyVM`, `Location: /api/currencies/{CurrId}` |
| 400 | `problem+json`, `type: …/validation-failed`, with an `errors` dictionary keyed by field carrying `CurrencyVM`'s `DataAnnotations` messages verbatim |
| 409 | `problem+json`, `type: …/business-rule`, `title` = the service's message verbatim (e.g. `"Currency name already exists."`) |

**Business logic.** `ICurrencyService.CreateAsync(vm)` → `(bool success, string message, CurrencyVM? entity)`.

**Note (M2-A06).** The two 400 shapes are gone. A model/`DataAnnotations` failure is the
canonical `400`; a **service rejection is now `409`**, because it is a business-rule refusal
(ADR-002 §4). See [*Error contract*](#error-contract-m2-a06).

---

### `PUT /api/currencies/{id:int}`

200 `CurrencyVM` · 400 / 409 as above. `ICurrencyService.UpdateAsync(id, vm)`.
**Note.** No check that `vm.CurrId` matches the route `id`.

---

### `DELETE /api/currencies/{id:int}`

| Status | Body | Condition |
|---|---|---|
| 204 | — | deleted |
| **409** | `problem+json`, `type: …/business-rule`, `title` = the guard's message **verbatim** | `CanDeleteCurrencyAsync` refused (referential integrity) — **was 400 before M2-A06** |
| 404 | `problem+json`, `type: …/not-found`, `title: "Currency not found."` | not found |

**Business logic.** `CanDeleteCurrencyAsync(id)` then `DeleteCurrencyByCurrIdAsync(id)` —
the standard two-step delete-guard pattern (see BR-SO-001 for the same shape in Sales).

---

## Error contract (M2-A06)

**Confirmed** — implemented 2026-08-20 by task M2-A06, ADR-002 §4. Every error response from
every endpoint is `application/problem+json`. Success responses are untouched.

| Status | Meaning | Body |
|---|---|---|
| 400 | model binding / `DataAnnotations` | `errors` dictionary keyed by field, messages verbatim from the VM |
| 401 | unauthenticated, or credentials rejected | minimal — no more informative than before M2-A06 |
| 403 | screen right denied | KB-105 §7.1 shape, with `screen` and `right` extension members |
| 404 | not found | minimal |
| **409** | **business-rule refusal** | `title` carries the **service's own message verbatim** (BR-SO-001) |
| 500 | unhandled | `traceId` only — no exception message, type or stack trace, in any environment |
| 503 | tenant context unavailable | constant title; never a connection string (R-01) |

- **`type`** is a stable URI under `https://api.v-smart.local/problems/`
  (`V.SMART/V.SMART.Api/Middleware/ProblemTypes.cs`). KB-041's illustrative
  `https://api.vsmart/errors/…` is superseded by it.
- **`traceId`** is `Activity.Current?.Id ?? HttpContext.TraceIdentifier` and is returned on
  **every** response — success included — in the `X-Correlation-Id` header. The header and the
  body's `traceId` are the same value. **A caller-supplied `X-Correlation-Id` is ignored**; the
  id is always generated server-side.
- **How a `409` is produced.** The services signal a refusal by *returning* a tuple, not by
  throwing, so middleware cannot see it. The controller maps it, via the
  `ProblemResults.BusinessRuleProblem(message)` extension
  (`V.SMART/V.SMART.Api/Middleware/ProblemResults.cs`). **Every future controller uses that
  helper** — see INV-040 in [`../investigation-registry.md`](../investigation-registry.md).
- **The API never interprets the message string.** The boolean is the signal; the string is
  passed through untouched. The consequence, recorded rather than guessed at, is
  Q-34 in [`../open-questions.md`](../open-questions.md): a refusal tuple that actually
  carries not-found semantics is reported as 409.
- The single producer of every body is
  `V.SMART/V.SMART.Api/Middleware/ApiProblems.cs`; the pipeline is registered by
  `UseErrorContract()` before `UseCors` in `Program.cs`.

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
| Delete | `DELETE /api/x/{id:int}` → 204, or **409** `problem+json` with the service's user-facing message verbatim in `title` (M2-A06) |
| Payload type | the existing `…VM` ViewModel, unchanged |

Worth fixing before replication:

| Problem | Fix |
|---|---|
| Untyped `Dictionary<string, object>` filters | typed query DTO per resource |
| No route/body id consistency check | validate in the controller |
| No permission check | `[RequireScreen("Currency", Right.Edit)]` filter |
| No versioning | `/api/v1/…` |

Fixed and no longer listed: *two 400 shapes* and *no correlation id* — both closed by
**M2-A06** (see [*Error contract*](#error-contract-m2-a06)).

## Ancillary

`V.SMART.Api/Auth/ApiAuthStateProvider.cs` implements `AuthenticationStateProvider` over
`IHttpContextAccessor` so that shared code expecting Blazor's auth abstraction
(e.g. `CurrentUserService`) works inside API requests. Small but essential — it is why
existing services run unchanged under HTTP.

`V.SMART.Api/V.SMART.Api.http` and `Properties/launchSettings.json` define the dev
endpoint (`http://localhost:5144`, matching the Angular pilot's `environment.apiBaseUrl`).
