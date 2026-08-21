---
doc_id: KB-040
title: Existing API Surface (As-Is)
module: api
source_files:
  - V.SMART/V.SMART.Api/Program.cs
  - V.SMART/V.SMART.Api/ApiRoutes.cs
  - V.SMART/V.SMART.Api/Controllers/AuthController.cs
  - V.SMART/V.SMART.Api/Controllers/CurrencyController.cs
  - V.SMART/V.SMART.Api/Auth/JwtTokenService.cs
  - V.SMART/V.SMART.Api/Auth/ApiAuthStateProvider.cs
  - V.SMART/V.SMART.Api/appsettings.json
entities: [User, Currency, TenantInfo]
api_endpoints:
  - "POST /api/v1/auth/login"
  - "GET /api/v1/currencies"
  - "GET /api/v1/currencies/{id}"
  - "POST /api/v1/currencies"
  - "PUT /api/v1/currencies/{id}"
  - "DELETE /api/v1/currencies/{id}"
  - "GET /api/v1/reference/gst-rates"
  - "GET /api/v1/reference/states"
  - "GET /api/v1/reference/uoms"
  - "GET /api/v1/reference/currencies"
  - "GET /api/v1/reference/screens"
  - "GET /api/v1/reference/terms"
database_tables: [Users, Currency, Tenants]
business_rules: [BR-AUTH-001]
status: complete
confidence: confirmed
last_verified: 2026-08-21
dependencies: [KB-013, KB-014]
---

# Existing API Surface (As-Is)

> **The entire HTTP API today is 6 endpoints across 2 controllers.** This document is the
> complete inventory. What still needs building is in
> [`api-readiness-assessment.md`](api-readiness-assessment.md).

> **Updated by M2-A06 (2026-08-20).** Every **error** response below is now
> `application/problem+json`; success responses are unchanged. The one deliberate breaking
> change is `DELETE /api/v1/currencies/{id}`, which answers **409** instead of 400 when the
> delete guard refuses. See [*Error contract*](#error-contract-m2-a06) below.

> **Updated by M2-B01 (2026-08-21).** Every route moved under **`/api/v1`** (ADR-002 §6).
> The old `/api/auth` and `/api/currencies` paths were **removed, not aliased**, and return
> `404`. The one existing consumer — the Angular pilot at `frontend/vsmart-erp/` — was
> deliberately **not** updated; see [*Versioning*](#versioning-m2-b01) below.

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
request logging. **Exception middleware and `ProblemDetails` are no longer absent** — added by M2-A06 (see *Error contract* below). **Nor is API versioning** — M2-B01 moved every route under `/api/v1` (see [*Versioning*](#versioning-m2-b01) below).

## Endpoint reference

### `POST /api/v1/auth/login`

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

### `GET /api/v1/currencies`

`CurrencyController.GetAll` · `[Authorize]`

> **Rewritten by M2-B02 (2026-08-20)** to the paged list contract — ADR-002 §2 and its
> [§2a addendum](../decisions/ADR-002-rest-api-layer.md). This is the **reference
> implementation**: every future list endpoint copies it. **M2-B01 (2026-08-21)** then moved
> its route prefix from `api/currencies` to `api/v1/currencies`.

**Query parameters** — one bound `CurrencyQuery` record, not six loose parameters.

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `pageNumber` | int | **1** | 1-based; `< 1` → 400 |
| `pageSize` | int | **20** | max **100**; outside that → 400. **Changed:** this endpoint defaulted to 10 before M2-B02 |
| `sort` | string | — | comma-separated, `-` prefix descending, e.g. `-createdDate,currName`. Absent → today's `OrderByDescending(CurrId)` |
| `currName` | string | — | case-insensitive contains |
| `createdBy` | string | — | case-insensitive contains |
| `fromDate` | **date-time** | — | `CreatedDate >= fromDate.Date`. **Was `string?`** |
| `toDate` | **date-time** | — | `CreatedDate <=` end of that day (inclusive). **Was `string?`** |

**Sortable fields** (unknown → 400 listing these): `currId`, `currName`, `currSub`, `symbol`,
`isSystemDefined`, `createdBy`, `createdDate`.

**Response 200.** `{ "items": CurrencyVM[], "totalCount": int, "pageNumber": int, "pageSize": int }`
— unchanged on the wire. The type is now the shared generic
`V.SMART.Api.Contracts.PagedResult<T>`; the controller-local `PagedCurrencyResponse` is gone.
The OpenAPI document names it `CurrencyVMPagedResult` (observed in `/swagger/v1/swagger.json`,
2026-08-20). `totalCount` is the **filtered, unpaged** count.

The parameter names above are the **wire** names and are what `/swagger/v1/swagger.json`
advertises (re-observed 2026-08-20 after the casing fix: `currName`, `createdBy`, `fromDate`,
`toDate`, `pageNumber`, `pageSize`, `sort`). Each is pinned by an explicit
`[FromQuery(Name = …)]` on the query record — see the ADR-002 §2a paragraph
*"Query-parameter names are camel case"* for why the default (the C# property name) is wrong
here. Binding stays case-insensitive, so `PageSize` is still accepted.

**Errors.** 400 `application/problem+json` with an `errors` dictionary keyed by field —
the same camel-case names as the parameters (`errors.pageSize`, `errors.fromDate`) — for:
`pageNumber < 1`, `pageSize` outside 1–100, an unparseable `fromDate`/`toDate`,
`fromDate > toDate`, and a `sort` field that is unknown or repeated. 401 when unauthenticated.

**Business logic.** `ICurrencyService.SearchWithDynamicFilterAsync(pageNumber, pageSize, filters, sort)`
— an **additive** overload added by M2-B02; the three-argument member is unchanged and delegates
to it with `sort: null`. The typed query is mapped to the service's `Dictionary<string, object>`
by `V.SMART.Api.Contracts.FilterDictionaryAdapter`; that dictionary never appears on the wire.

---

### `GET /api/v1/currencies/{id:int}`

200 `CurrencyVM` · 404 `problem+json`, `type: …/not-found`, `title: "Currency not found."`.
Calls `ICurrencyService.GetByIdAsync(id)`.

---

### `POST /api/v1/currencies`

**Request.** `CurrencyVM` — validated by `DataAnnotations` on the VM:
`CurrName` required ≤100, `CurrSub` required ≤100,
`Symbol` required and matching `[$€₹¥£₩₿₽]`.

| Status | Body |
|---|---|
| 201 | `CurrencyVM`, `Location: /api/v1/currencies/{CurrId}` |
| 400 | `problem+json`, `type: …/validation-failed`, with an `errors` dictionary keyed by field carrying `CurrencyVM`'s `DataAnnotations` messages verbatim |
| 409 | `problem+json`, `type: …/business-rule`, `title` = the service's message verbatim (e.g. `"Currency name already exists."`) |

**Business logic.** `ICurrencyService.CreateAsync(vm)` → `(bool success, string message, CurrencyVM? entity)`.

**Note (M2-A06).** The two 400 shapes are gone. A model/`DataAnnotations` failure is the
canonical `400`; a **service rejection is now `409`**, because it is a business-rule refusal
(ADR-002 §4). See [*Error contract*](#error-contract-m2-a06).

---

### `PUT /api/v1/currencies/{id:int}`

200 `CurrencyVM` · 400 / 409 as above. `ICurrencyService.UpdateAsync(id, vm)`.
**Note.** No check that `vm.CurrId` matches the route `id`.

---

### `DELETE /api/v1/currencies/{id:int}`

| Status | Body | Condition |
|---|---|---|
| 204 | — | deleted |
| **409** | `problem+json`, `type: …/business-rule`, `title` = the guard's message **verbatim** | `CanDeleteCurrencyAsync` refused (referential integrity) — **was 400 before M2-A06** |
| 404 | `problem+json`, `type: …/not-found`, `title: "Currency not found."` | not found |

**Business logic.** `CanDeleteCurrencyAsync(id)` then `DeleteCurrencyByCurrIdAsync(id)` —
the standard two-step delete-guard pattern (see BR-SO-001 for the same shape in Sales).

---

### `GET /api/v1/reference/*` (M2-B09)

Six read-only lookup lists behind one tenant-keyed output cache. Full design, measurements and
the tenancy classification of each list: **[KB-124](reference-data-and-caching.md)**.

| Endpoint | Returns | Rows (measured 2026-08-21) |
|---|---|---:|
| `GET /api/v1/reference/gst-rates` | `{ igst[], cgstSgst[] }`, paired by index | 12 + 12 |
| `GET /api/v1/reference/states` | `StateDto[]` | 40 |
| `GET /api/v1/reference/uoms` | `UomDto[]` | 49 |
| `GET /api/v1/reference/currencies` | `CurrencyDto[]` — flat, no rate feed | 3 |
| `GET /api/v1/reference/screens` | `ScreenDto[]` — the permission vocabulary | **150** |
| `GET /api/v1/reference/terms` | `TermsDto[]`, active only | 0 |

**Authentication** is required on all six. **`[NoScreenRight]`**, not `[RequireScreen]`:
reference data is a precondition for rendering any screen, so no single screen owns it, and
gating it would deadlock the UI for the same reason it would for `GET /api/v1/me`
(KB-105 §2.4).

**Caching.** `Cache-Control: private, max-age=60` (configurable via
`Caching:ReferenceDataSeconds`); the server-side key **includes the `TenantId` claim**, and the
policy disables caching entirely rather than falling back to an unkeyed entry when it cannot
establish the tenant. There is no invalidation by design — these lists are edited through Blazor
screens that know nothing about this cache, so a short TTL is honest where a phantom
invalidation path would not be.

> **`/reference/screens` returns 150, not the 152 that `ScreenCatalogue.cs` compiles.** The
> endpoint reports what the database holds; the catalogue is wrong. See **R-65**.

> **`/reference/currencies` is not `/api/v1/currencies`.** The latter is the paged, writable
> CRUD surface for the Currency master; this is a flat cached list for populating a selector.

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
- **`traceId`** is `Activity.Current?.Id ?? HttpContext.TraceIdentifier` and is returned in the
  `X-Correlation-Id` header on every response that passes through `UseErrorContract()` (all
  controller and error paths, success included) — the header and the body's `traceId` are the
  same value. **A caller-supplied `X-Correlation-Id` is ignored**; the id is always generated
  server-side. **One gap, observed 2026-08-20 during M2-A06's close-out review:**
  `GET /swagger/index.html` (Development only) returns `200` with **no** `X-Correlation-Id`,
  because `UseSwagger`/`UseSwaggerUI` are registered ahead of `UseErrorContract()` in
  `Program.cs`. No API endpoint is affected; whoever next reorders the pipeline (M2-A05,
  M2-B01) should move Swagger registration after the error contract rather than assume the
  header is unconditional.
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

## Versioning (M2-B01)

**Confirmed** — implemented 2026-08-21 by task M2-B01, [ADR-002 §6](../decisions/ADR-002-rest-api-layer.md#6-versioning-and-generation)
("All routes under `/api/v1`"). Every one of the six endpoints above moved from `/api/…` to
`/api/v1/…`. **The old paths were removed, not aliased** — they return `404`.

| | |
|---|---|
| Mechanism | one constant, `V.SMART/V.SMART.Api/ApiRoutes.cs` → `public const string V1 = "api/v1";` |
| Usage | `[Route($"{ApiRoutes.V1}/currencies")]` — legal in an attribute because `V1` is `const` |
| Package added | **none.** `Asp.Versioning.Mvc` is deliberately not referenced: one version, no negotiation, no per-version OpenAPI document to complicate the client generation M2-B10 depends on |
| `Program.cs` | **unchanged.** `MapControllers()` is the only route mapping and carries no path |
| `Location` header | now `/api/v1/currencies/{CurrId}` — `CreatedAtAction` derives it from the action's route, so it followed the prefix automatically |

**Every future controller composes its route from `ApiRoutes.V1`.** A literal `"api/v1"` in a
controller is a review reject: the whole value of this task is that the version string exists
in exactly one place when a `v2` is one day needed.

**Deliberate breakage, recorded rather than fixed.** The Angular 19 pilot at
`frontend/vsmart-erp/` is the only consumer of the old paths and now calls two routes that no
longer exist:

- `frontend/vsmart-erp/src/app/core/auth/auth.service.ts:54` — `${environment.apiBaseUrl}/api/auth/login`
- `frontend/vsmart-erp/src/app/features/currency/currency.service.ts:18` — `${environment.apiBaseUrl}/api/currencies`

M2-B01 did **not** update them. Per [ADR-002 §Consequences](../decisions/ADR-002-rest-api-layer.md#consequences)
the pilot's fate is [M2-C11](../execution/tasks/M2-C11.md)'s to decide, and mixing that into a
route-prefix change would put two tasks in one diff. Whoever picks the pilot up inherits exactly
these two lines.

## 🚨 Authorization state of the existing API

`[Authorize]` on `CurrencyController` means **"any authenticated user of any tenant with a
valid token"**. It does **not** check `UserRight` for the `"Currency"` screen.

Concretely, today: a user whose `UserRight` row for `Currency` has
`CanView = CanCreate = CanEdit = CanDelete = false` — a user for whom the Blazor UI hides
the entire Currency screen — **can create, edit, and delete currencies through the API**.

This generalises to every controller that follows this template. It is the single most
important thing to fix before the API grows.
See [ADR-004](../decisions/ADR-004-server-side-authorization.md).

> **M2-A01-02 (2026-08-20) — the mechanism now exists; the hole above is still open.**
> `V.SMART/V.SMART.Api/Authorization/` holds `[RequireScreen]`, `[RequireRight]`,
> `[NoScreenRight]`, `IUserRightsProvider`/`UserRightsProvider` (no cache — M2-A01-03 adds
> one behind that seam), `ScreenRightSet`, `ScreenCatalogue`, `ScreenRightStartupValidator`
> and `ScreenRightAuthorizationFilter`, registered globally in `Program.cs`. **No controller
> declares the attributes yet**, so every paragraph above still describes today's behaviour
> exactly: the filter passes unannotated endpoints through untouched and all six existing
> endpoints respond as before. `M2-A02` annotates `CurrencyController`; `M2-A03` proves it
> with the permission matrix. R-03 stays open until both land.
> Specification: [KB-105](../architecture/server-side-authorization-spec.md).

> **M2-A01-03 (2026-08-20) — rights are resolved per request, never carried in the JWT.**
> `UserRightsProvider` now reads through a singleton `IMemoryCache` keyed
> `screenrights:v1:{tenantId}:{userId}` with an **absolute** TTL from
> `Authorization:RightsCacheSeconds` (default 60 s; `0` disables the cache; above 300 fails
> startup). The token still carries only `Name`, `UserId`, `TenantId` and `Role` — ADR-004 §2
> forbids rights in the JWT, and `JwtTokenService` is unchanged. Because all five `UserRight`
> write sites run in the Blazor host, a permission change takes effect in the API within the
> TTL rather than immediately (KB-105 §8.6, Q-29). Still no controller is annotated, so all six
> endpoints respond exactly as described above.

## Contract conventions established so far

Worth keeping (they are already consistent):

| Convention | Form |
|---|---|
| Base path | `/api/v1/{plural-kebab-resource}` — the `api/v1` segment comes from `ApiRoutes.V1`, never a literal (M2-B01) |
| Paged list | `GET /api/v1/x?pageNumber&pageSize&<filters>` → `{ items, totalCount, pageNumber, pageSize }` |
| Single | `GET /api/v1/x/{id:int}` |
| Create | `POST /api/v1/x` → 201 + `Location` |
| Update | `PUT /api/v1/x/{id:int}` → 200 |
| Delete | `DELETE /api/v1/x/{id:int}` → 204, or **409** `problem+json` with the service's user-facing message verbatim in `title` (M2-A06) |
| Payload type | the existing `…VM` ViewModel, unchanged |

Worth fixing before replication:

| Problem | Fix |
|---|---|
| Untyped `Dictionary<string, object>` filters | typed query DTO per resource |
| No route/body id consistency check | validate in the controller |
| No permission check | `[RequireScreen("Currency", Right.Edit)]` filter |

Fixed and no longer listed: *two 400 shapes* and *no correlation id* — both closed by
**M2-A06** (see [*Error contract*](#error-contract-m2-a06)); *no versioning* — closed by
**M2-B01** (see [*Versioning*](#versioning-m2-b01)).

## Ancillary

`V.SMART.Api/Auth/ApiAuthStateProvider.cs` implements `AuthenticationStateProvider` over
`IHttpContextAccessor` so that shared code expecting Blazor's auth abstraction
(e.g. `CurrentUserService`) works inside API requests. Small but essential — it is why
existing services run unchanged under HTTP.

`V.SMART.Api/V.SMART.Api.http` and `Properties/launchSettings.json` define the dev
endpoint (`http://localhost:5144`, matching the Angular pilot's `environment.apiBaseUrl`).
