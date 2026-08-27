---
doc_id: KB-040
title: Existing API Surface (As-Is)
module: api
source_files:
  - V.SMART/V.SMART.Api/Program.cs
  - V.SMART/V.SMART.Api/ApiRoutes.cs
  - V.SMART/V.SMART.Api/Controllers/AuthController.cs
  - V.SMART/V.SMART.Api/Controllers/CurrencyController.cs
  - V.SMART/V.SMART.Api/Controllers/MeController.cs
  - V.SMART/V.SMART.Api/Auth/JwtTokenService.cs
  - V.SMART/V.SMART.Api/Auth/RefreshTokenService.cs
  - V.SMART/V.SMART.Api/Auth/IRefreshTokenService.cs
  - V.SMART/V.SMART.Api/Auth/ApiAuthStateProvider.cs
  - V.SMART/V.SMART.Api/appsettings.json
entities: [User, Currency, TenantInfo, UserRight, Screens, RefreshToken]
api_endpoints:
  - "POST /api/v1/auth/login"
  - "POST /api/v1/auth/refresh"
  - "POST /api/v1/auth/logout"
  - "GET /api/v1/currencies"
  - "GET /api/v1/currencies/{id}"
  - "POST /api/v1/currencies"
  - "PUT /api/v1/currencies/{id}"
  - "DELETE /api/v1/currencies/{id}"
  - "GET /api/v1/me"
  - "GET /api/v1/reference/gst-rates"
  - "GET /api/v1/reference/states"
  - "GET /api/v1/reference/uoms"
  - "GET /api/v1/reference/currencies"
  - "GET /api/v1/reference/screens"
  - "GET /api/v1/reference/terms"
database_tables: [Users, Currency, Tenants, UserRights, Screens, RefreshTokens]
business_rules: [BR-AUTH-001, BR-AUTH-002, BR-TEN-002]
status: complete
confidence: confirmed
last_verified: 2026-08-28
dependencies: [KB-013, KB-014]
---

# Existing API Surface (As-Is)

> **The entire HTTP API today is 7 endpoints across 3 controllers.** This document is the
> complete inventory. What still needs building is in
> [`api-readiness-assessment.md`](api-readiness-assessment.md).

> **Updated by M2-A06 (2026-08-20).** Every **error** response below is now
> `application/problem+json`; success responses are unchanged. The one deliberate breaking
> change is `DELETE /api/v1/currencies/{id}`, which answers **409** instead of 400 when the
> delete guard refuses. See [_Error contract_](#error-contract-m2-a06) below.

> **Updated by M2-B01 (2026-08-21).** Every route moved under **`/api/v1`** (ADR-002 §6).
> The old `/api/auth` and `/api/currencies` paths were **removed, not aliased**, and return
> `404`. The one existing consumer — the Angular pilot at `frontend/vsmart-erp/` — was
> deliberately **not** updated; see [_Versioning_](#versioning-m2-b01) below.

## Host configuration

`V.SMART.Api/Program.cs`:

| Concern                    | Configuration                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| -------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Controllers                | `AddControllers()`, `MapControllers()`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| OpenAPI                    | Swashbuckle, `SwaggerDoc("v1", "V.SMART API")`, Bearer security scheme; **`UseSwagger`/`UseSwaggerUI` only in Development**                                                                                                                                                                                                                                                                                                                                                                                                            |
| CORS                       | single policy `"SpaOrigins"` (M2-A05, replacing `"AngularDev"`) → origins from the `Cors:AllowedOrigins` configuration array (`CorsOptions.cs`), any header, any method, `AllowCredentials` also configurable (default `false`). Empty by default in `appsettings.json` (fails closed — no origin allowed until one is configured); `appsettings.Development.json` sets `http://localhost:4200` and `http://127.0.0.1:4300`. Real production origins are deliberately not invented — Q-16 (deployment topology) is explicitly deferred |
| AuthN                      | JWT Bearer; validates issuer, audience, lifetime, signing key; `ClockSkew = 1 min`                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| AuthZ                      | `AddAuthorization()` with **no policies registered**                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| Pipeline                   | **`UseErrorContract()`** (correlation id → global exception handler → status-code problem bodies; M2-A06) → `UseCors("SpaOrigins")` (M2-A05) → `UseAuthentication()` → `UseAuthorization()` → `MapControllers()`                                                                                                                                                                                                                                                                                                                       |
| Tenancy                    | `ITenantProvider` / `ITenantDbContextFactory` scoped; `MasterDbContext` via `AddDbContext`; `ApplicationDbContext` built per-scope from the resolved tenant                                                                                                                                                                                                                                                                                                                                                                            |
| Mapping                    | `AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfileMarker).Assembly))`                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| Registered domain services | **`ICurrencyService` only** — plus `IUnitOfWork`, `ForeignKeyUsageChecker`, `ILoggingService`, `IPasswordHasher<User>`, `CurrentUserService`, `UserSession`, `JwtTokenService` (singleton), `AuthenticationStateProvider → ApiAuthStateProvider`                                                                                                                                                                                                                                                                                       |

**Notable absences (Confirmed, re-verified 2026-08-20):** no HTTPS redirection,
no rate limiting, no response compression, no health checks, no
request logging. **Exception middleware and `ProblemDetails` are no longer absent** — added by M2-A06 (see _Error contract_ below). **Nor is API versioning** — M2-B01 moved every route under `/api/v1` (see [_Versioning_](#versioning-m2-b01) below).

## Endpoint reference

### `POST /api/v1/auth/login`

`AuthController.Login` · `[AllowAnonymous]`

**Request**

```json
{
  "tenant": "string (required)",
  "username": "string (required)",
  "password": "string (required)"
}
```

**M2-A05 added `tenant`** — matches ADR-002 §5's `{ tenant, username, password }` exactly.
Resolved against `MasterDbContext.Tenants` by `Name` **or** `Hostname`, via
`ITenantProvider.SetTenant(request.Tenant)` (a new "step 0" resolution path — see
[multi-tenancy](../architecture/multi-tenancy.md)'s Resolution order) called **before** any
tenant-scoped service is resolved.

**Responses**

| Status | Body                                                                                         | Condition                                                                                                                                                                                                                                                                                               |
| ------ | -------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 200    | `{ "token", "refreshToken", "tokenExpiresAtUtc", "username", "userId", "tenantId", "role" }` | success                                                                                                                                                                                                                                                                                                 |
| 400    | `problem+json`, `type: …/tenant-unresolved`, `title: "Unable to resolve tenant."`            | `ITenantProvider.GetCurrentTenant()` returned `null` after `SetTenant(request.Tenant)` — the message deliberately does not echo the submitted value or distinguish "no such tenant" from any other resolution failure (M2-A05: tenant identifiers are not secrets, but the KB flags them as enumerable) |
| 401    | `problem+json`, `type: …/unauthenticated`, `title: "Invalid username or password."`          | `LoginAsync` returned `null`                                                                                                                                                                                                                                                                            |

The `401` is unchanged from before M2-A06 — one title for every authentication failure. The
`400` message was shortened by M2-A05 (dropped "Check host or wwwroot/config/tenant.json." —
that fallback file no longer exists for the API, and the primary path is now the request's
own `tenant` field, not a host or a config file).

**Auth required.** None.
**Business logic executed.** `ITenantProvider.SetTenant(request.Tenant)` →
`ITenantProvider.GetCurrentTenant()` → `IUnitOfWork.Users.LoginAsync` (BR-AUTH-001) →
`JwtTokenService.CreateToken(user, tenant.Id)` → `IRefreshTokenService.IssueAsync(user.UserId)`
(M2-A04). `IUnitOfWork`/`IRefreshTokenService`/`IUserRightService` are resolved from an
injected `IServiceProvider` **after** the tenant check, not constructor-injected — see
`AuthController.cs`'s own constructor comment for why constructor injection cannot work here.
**Entities.** `TenantInfo`, `User`, `RefreshToken` (M2-A04).
**Token.** HS256; claims `ClaimTypes.Name`, `UserId`, `TenantId`, `ClaimTypes.Role`;
issuer `V.SMART.Api`; audience `V.SMART.Angular`; expiry `Jwt:ExpiresMinutes`
(**15**, M2-A04 — was 480 before this task; the response's `tokenExpiresAtUtc` is the
authoritative value, not a client-side recomputation from the config key).

**M2-A04 — breaking change for the response body.** `refreshToken` (string) and
`tokenExpiresAtUtc` (UTC ISO-8601) are new fields, inserted after `token`; the four original
fields keep their names, types and order. Breaking for the Angular pilot
(`frontend/vsmart-erp/`), which M2-C11 already archived — recorded, not a live concern.

**Contract gaps, updated by M2-A04.** ~~No refresh token.~~ **Closed** — see
`POST /api/v1/auth/refresh` and `POST /api/v1/auth/logout` below. No screen-permission
claims — the client cannot know what to render, and the server cannot authorise beyond role
(unchanged; `GET /api/v1/me` is the answer). ~~No tenant selector in the request~~ **Closed
2026-08-27 (M2-A05)** — see [multi-tenancy](../architecture/multi-tenancy.md) problem 1,
closed. _(The ad-hoc `{ message }` error body was replaced by `problem+json` in M2-A06.)_

**Defect (Confirmed).** A database failure inside `LoginAsync` is swallowed and returns
`null` (`UserRepository.cs:44-48`), so an outage is reported to the user as
"Invalid username or password".

---

### `POST /api/v1/auth/refresh` — added by M2-A04 (2026-08-27)

`AuthController.Refresh` · `[AllowAnonymous]` — the access token this call authenticates with
may already be expired; the refresh token itself is the credential (harness allow-list
entry, `ExemptEndpointAllowList.AnonymousActions["AuthController.Refresh"]`).

**Request**

```json
{ "tenant": "string (required)", "refreshToken": "string (required)" }
```

**M2-A05 added `tenant`**, for the identical reason `Login`'s addition exists: an expired
access token carries no claim to re-derive the tenant from, and for a cross-origin SPA the
Host-header fallback can never match either. The client resends the same value it logged in
with, bound before rotation is attempted.

**Responses**

| Status | Body                                                                                    | Condition                                                                                                                                                                        |
| ------ | --------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 200    | `{ "token", "refreshToken", "tokenExpiresAtUtc" }`                                      | rotation succeeded                                                                                                                                                               |
| 400    | `problem+json`, `type: …/tenant-unresolved`, `title: "Unable to resolve tenant."`       | `request.Tenant` matched no row                                                                                                                                                  |
| 401    | `problem+json`, `type: …/unauthenticated`, `title: "Invalid or expired refresh token."` | the presented token is unknown, revoked, expired, or its user is no longer active — **all four reasons produce this identical body**; the distinction is logged server-side only |

**Rotation.** One-time use: the presented token is revoked in the same call that mints its
replacement (`RefreshTokenService.RotateAsync`), so replaying a used or stolen-then-rotated
token always fails.

**Tenant binding (BR-TEN-002), M2-A05.** Not derived from a JWT claim — an expired access
token authenticates nobody, so there is no claim to read. Tenant context now comes from the
request's own `tenant` field, bound via `ITenantProvider.SetTenant()` before
`IRefreshTokenService`/`IUnitOfWork` are resolved (the pre-M2-A05 Host-header/`tenant.json`
fallback never actually worked for a cross-origin SPA — it is documented as closed in
[multi-tenancy](../architecture/multi-tenancy.md) problem 1). A refresh token issued in
tenant A's database is simply absent from tenant B's — database-per-tenant isolation makes
"cannot permit a tenant switch" structural, not an extra check this endpoint has to get
right.

**Transport decision (Q-16 dependency), recorded per the task's own requirement.** The
refresh token travels in the JSON request/response body, **not** an `HttpOnly` cookie. An
`HttpOnly` cookie would need a real answer to Q-16 (reverse-proxy/TLS topology, cookie
domain, `SameSite`) to be configured correctly; Q-16 is Unknown. Body transport is
topology-agnostic and does not presuppose that answer. **This is a deliberate, disclosed
trade-off, not a final design**: a body-transported refresh token is exposed to the same
`localStorage`/XSS surface KB-013 already flags for the access token, and moving to a
cookie once Q-16 is answered is a natural hardening step for whichever task owns that
(M2-C02 already carries the localStorage/XSS note).

**Business logic executed.** `IRefreshTokenService.RotateAsync` → (on success)
`IUnitOfWork.Users.FirstOrDefaultAsync` (a second, independent `IsActive` read) →
`ITenantProvider.GetCurrentTenant()` → `JwtTokenService.CreateToken`.
**Entities.** `RefreshToken`, `User`.

---

### `POST /api/v1/auth/logout` — added by M2-A04 (2026-08-27)

`AuthController.Logout` · `[AllowAnonymous]` — must be reachable with an already-expired
access token so a client can always end its session; revocation is keyed on the presented
refresh token, not on bearer auth (harness allow-list entry,
`ExemptEndpointAllowList.AnonymousActions["AuthController.Logout"]`).

**Request**

```json
{ "tenant": "string (required)", "refreshToken": "string (required)" }
```

**M2-A05 added `tenant`** — `IRefreshTokenService` is tenant-scoped, same reason as `Refresh`.

**Responses**

| Status | Body                                                                              | Condition                                                                                                   |
| ------ | --------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| 204    | _(none)_                                                                          | tenant resolved and the revoke ran — see below                                                              |
| 400    | `problem+json`, `type: …/tenant-unresolved`, `title: "Unable to resolve tenant."` | `request.Tenant` matched no row — a distinct, structural failure, not a leak about the token's own validity |

**Revocation scope — decided and documented, per the task's own requirement.** Revokes
**exactly the one presented token**, not every token belonging to the user. The request
contract is "the refresh token to revoke" (singular), matching one-session-per-device
logout. A "sign out everywhere" capability is a natural extension, left to whichever future
task needs it rather than assumed here.

**Idempotent and opaque.** Revoking an unknown or already-revoked token still returns `204`
— the response must never leak whether a token was ever valid.

**Business logic executed.** `IRefreshTokenService.RevokeAsync`.
**Entities.** `RefreshToken`.

---

### `GET /api/v1/me`

`MeController.Get` · `[Authorize]` · `[NoScreenRight]` · **added by M2-A07 (2026-08-20)**

The SPA bootstrap call: who the caller is, which tenant, which role, and the caller's
**complete screen-rights map**, in one request. `POST /api/auth/login` returns no rights and
ADR-004 §2 forbids putting them in the JWT, so the map comes from here, freshly.

> **Presentation only (ADR-004 §3).** This map exists so the client can render a
> permission-correct navigation and disable controls. It is **never** the enforcement point —
> the server re-checks every request through `ScreenRightAuthorizationFilter`, and nothing
> about this endpoint relaxes that filter.

**Request.** No body, no parameters, no query string. The caller is identified entirely by the
bearer token — there is deliberately no parameter that could name another user or tenant.

**Responses**

| Status | Body                                                                                              | Condition                                                                                                                                                                                                                                              |
| ------ | ------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 200    | see below                                                                                         | success                                                                                                                                                                                                                                                |
| 401    | JwtBearer challenge (M2-A06 status-code problem body)                                             | no token, or an invalid/expired one                                                                                                                                                                                                                    |
| 401    | `problem+json`, `type: …/invalid-token`, `title: "The access token is missing a required claim."` | the token carries no usable `UserId` or `TenantId` claim (missing, unparseable or `<= 0`) — the same body and the same rule as the screen-right filter (KB-105 §7.2, D-3)                                                                              |
| 500    | `problem+json`, `type: …/unhandled`                                                               | the rights load failed. **It never degrades to an empty map**: an empty map renders as "no permissions" and reads as a permission problem rather than an outage. `UserRightsProvider` does not negative-cache, so nothing is written on failure either |

**200 body**

```json
{
  "userId": 7,
  "userName": "vivek",
  "tenantId": 3,
  "role": "Administrator",
  "rights": {
    "Currency": {
      "view": true,
      "create": false,
      "edit": true,
      "delete": false,
      "hidden": false
    },
    "Sales Order": {
      "view": true,
      "create": true,
      "edit": true,
      "delete": true,
      "hidden": true
    }
  }
}
```

**Contract — the parts a client must not get wrong**

| Aspect                      | Rule                                                                                                                                                                                                                                                                                                 |
| --------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `rights` key                | `Screens.ScreenName` **verbatim**, ordinal and case-sensitive, so a lookup is the one-step equivalent of `RightsHelper.cs:8`                                                                                                                                                                         |
| **Absent rows are omitted** | A screen the caller holds no `UserRight` row for has **no key**. The response never materialises 152 all-`false` entries. **The client's default for a missing key is `deny`** (BR-AUTH-002, and the `?? false` Blazor has always applied). A client that defaults a missing key to "allow" is wrong |
| Duplicates                  | If duplicate `(UserId, ScreenId)` rows exist (Q-27, unresolved), the map keeps the **first** — the same row `ScreenRightSet.Has`/`FirstOrDefault` decides on, so the rendered map and the enforced map cannot disagree                                                                               |
| `hidden`                    | Carries `IsHide`. Navigation is filtered on `view && !hidden`; it is **not** a second gate — the filter never consults it                                                                                                                                                                            |
| Ordering                    | The `rights` object is keyed, so the repository's undefined row order (no `OrderBy`, `UserRightsRepository.cs:24-27`) cannot leak into the contract                                                                                                                                                  |
| Size                        | Bounded by the 152 seeded screens; no paging                                                                                                                                                                                                                                                         |
| Caching                     | Idempotent and safe. Fresh within the M2-A01-03 rights-cache window (default 60 s absolute). **No HTTP cache headers** are set — one outliving that window would defeat the point of not putting rights in the JWT                                                                                   |

**Auth required.** Authentication **yes**; screen right **no** — a caller's own identity is not
a screen, and gating this endpoint on a right would deadlock the SPA, which cannot know its
rights until it has read this. The exemption is declared explicitly and auditably with
`[NoScreenRight("…")]` (KB-105 §2.4), never by omission.

**Business logic executed.** None. The controller is thin (ADR-002 §2): read claims → one
`IUserRightsProvider.GetAsync(tenantId, userId, ct)` → map → return. It touches no other
service and issues no query of its own.

**Rights source.** `IUserRightsProvider` — **the same seam and the same cache the screen-right
filter uses**. Not the repository: a second read path would eventually disagree with the
filter, and a UI offering a button the API refuses is worse than no button.

**Tenancy.** `tenantId` is the JWT `TenantId` claim (BR-TEN-002) and is passed to the provider,
whose cache key is `screenrights:v1:{tenantId}:{userId}`. Two users with the same `UserId` in
different tenants therefore receive their own tenant's rights — verified for a claim that
resolves to a real `Tenants` row. **No `TenantInfo` member other
than the id is returned** — that record carries a plaintext credentialed connection string
(R-01), so this endpoint never reads the tenant row at all. **Caveat (R-44/Q-37):** for a
`TenantId` claim that does **not** resolve, `TenantProvider` falls back to host-based
resolution while this cache still keys on the claimed id — bounded today because
`AuthController` only mints resolvable ids, but not a guarantee for every possible caller.

**Role.** The `ClaimTypes.Role` claim minted by `JwtTokenService.cs:34`, i.e. the same
expression `AuthController`'s login response returns, so the two cannot disagree inside one
token's lifetime. Empty string, never null, when the token carries no role.
`CurrentUserService.GetUserRoleAsync()` is **not** called (R-18: it reads claim type `"role"`
while both providers write `ClaimTypes.Role`, so it always returns `""`). The role `ERPAdmin`
that `NavMenu.razor:36,148` and `Home.razor:240` name does not exist in `UserRole` (R-31) and
is not propagated here.

**Deliberately not in the response.** `User.IsViewOnly` (already fully expressed as
`UserRight` rows), `User.HideManualJobOrderButton` (one button on one M3 screen),
`User.LevelAuthorization` and `UserAuthority` (approval authority — ADR-004 §4 enforces it
inside the approval endpoints; **deferred to M3-4**), `User.IsProductionBased` (reaches the
Blazor menu only via the QR-login path, which the API does not have), the tenant display name
(no consumer), and every secret. Per-item justification is in `MeController`'s own XML docs.

**Route decision (M2-A07, for M2-B01).** Ships at `/api/v1/me` — the name ADR-004 §3, KB-105
and KB-080 §9 all use and M2-C02 will code against — making it the **first versioned route** in
an API whose other routes are still unversioned. The `v1` is a **literal in the route
template**, not versioning infrastructure. When M2-B01 lands it must re-express this route
through the versioning convention **without changing the URL**; the URL is the contract.

---

### `GET /api/v1/currencies`

`CurrencyController.GetAll` · `[Authorize]` · `[RequireScreen("Currency")]` `[RequireRight(Right.View)]`

> **First real consumer: `M2-D01` (2026-08-28).** This contract — as documented below — was
> re-verified directly against source before the Angular Currency Master feature was built
> against it, and matched exactly; no divergence found between this document and
> `CurrencyController.cs`/`CurrencyQuery.cs`/`PagedQuery.cs` as they stand today. One gap the
> slice found in the **client's** own generated-code integration, not in this contract, is
> recorded as R-80 in [technical-debt-register.md](../risks/technical-debt-register.md):
> `deleteCurrency()`'s generated request loses a 409's `title` because its `responseType`
> (set for the empty `204` success body) also governs how the error body is parsed.

> **403 (M2-A02, 2026-08-24).** Every one of the five Currency endpoints below now answers
> `403 application/problem+json`, `type: …/screen-right-denied`,
> `title: "Screen right denied."`, `detail: "You do not have the '{right}' right for the
'Currency' screen."`, with `screen` and `right` extensions, when the caller's `UserRight`
> row for `"Currency"` does not grant the action's right — or when there is no such row at
> all. The check runs in the **authorization** stage, ahead of model binding, so an
> unauthorized caller with an invalid body gets `403`, never `400`. Anonymous callers still
> get `401`: authentication failure and authorization failure stay distinguishable.

> **Rewritten by M2-B02 (2026-08-20)** to the paged list contract — ADR-002 §2 and its
> [§2a addendum](../decisions/ADR-002-rest-api-layer.md). This is the **reference
> implementation**: every future list endpoint copies it. **M2-B01 (2026-08-21)** then moved
> its route prefix from `api/currencies` to `api/v1/currencies`.

**Query parameters** — one bound `CurrencyQuery` record, not six loose parameters.

| Parameter    | Type          | Default | Notes                                                                                                              |
| ------------ | ------------- | ------- | ------------------------------------------------------------------------------------------------------------------ |
| `pageNumber` | int           | **1**   | 1-based; `< 1` → 400                                                                                               |
| `pageSize`   | int           | **20**  | max **100**; outside that → 400. **Changed:** this endpoint defaulted to 10 before M2-B02                          |
| `sort`       | string        | —       | comma-separated, `-` prefix descending, e.g. `-createdDate,currName`. Absent → today's `OrderByDescending(CurrId)` |
| `currName`   | string        | —       | case-insensitive contains                                                                                          |
| `createdBy`  | string        | —       | case-insensitive contains                                                                                          |
| `fromDate`   | **date-time** | —       | `CreatedDate >= fromDate.Date`. **Was `string?`**                                                                  |
| `toDate`     | **date-time** | —       | `CreatedDate <=` end of that day (inclusive). **Was `string?`**                                                    |

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
_"Query-parameter names are camel case"_ for why the default (the C# property name) is wrong
here. Binding stays case-insensitive, so `PageSize` is still accepted.

**Errors.** 400 `application/problem+json` with an `errors` dictionary keyed by field —
the same camel-case names as the parameters (`errors.pageSize`, `errors.fromDate`) — for:
`pageNumber < 1`, `pageSize` outside 1–100, an unparseable `fromDate`/`toDate`,
`fromDate > toDate`, and a `sort` field that is unknown or repeated. 401 when unauthenticated.
**403** when the caller has no `CanView` on `"Currency"` (M2-A02).

**Business logic.** `ICurrencyService.SearchWithDynamicFilterAsync(pageNumber, pageSize, filters, sort)`
— an **additive** overload added by M2-B02; the three-argument member is unchanged and delegates
to it with `sort: null`. The typed query is mapped to the service's `Dictionary<string, object>`
by `V.SMART.Api.Contracts.FilterDictionaryAdapter`; that dictionary never appears on the wire.

---

### `GET /api/v1/currencies/{id:int}`

200 `CurrencyVM` · 404 `problem+json`, `type: …/not-found`, `title: "Currency not found."`.
**403** `problem+json`, `type: …/screen-right-denied`, when the caller has no `CanView` on
`"Currency"` — decided before the lookup, so a denied caller cannot probe which ids exist
(M2-A02). Calls `ICurrencyService.GetByIdAsync(id)`.

---

### `POST /api/v1/currencies`

**Request.** `CurrencyVM` — validated by `DataAnnotations` on the VM:
`CurrName` required ≤100, `CurrSub` required ≤100,
`Symbol` required and matching `[$€₹¥£₩₿₽]`.

| Status  | Body                                                                                                                                                |
| ------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| 201     | `CurrencyVM`, `Location: /api/v1/currencies/{CurrId}`                                                                                               |
| 400     | `problem+json`, `type: …/validation-failed`, with an `errors` dictionary keyed by field carrying `CurrencyVM`'s `DataAnnotations` messages verbatim |
| **403** | `problem+json`, `type: …/screen-right-denied` — caller has no `CanCreate` on `"Currency"` (M2-A02)                                                  |
| 409     | `problem+json`, `type: …/business-rule`, `title` = the service's message verbatim (e.g. `"Currency name already exists."`)                          |

**Business logic.** `ICurrencyService.CreateAsync(vm)` → `(bool success, string message, CurrencyVM? entity)`.

**Note (M2-A06).** The two 400 shapes are gone. A model/`DataAnnotations` failure is the
canonical `400`; a **service rejection is now `409`**, because it is a business-rule refusal
(ADR-002 §4). See [_Error contract_](#error-contract-m2-a06).

---

### `PUT /api/v1/currencies/{id:int}`

200 `CurrencyVM` · 400 / 409 as above · **403** when the caller has no `CanEdit` on
`"Currency"` (M2-A02). `ICurrencyService.UpdateAsync(id, vm)`.
**Note.** No check that `vm.CurrId` matches the route `id`.

---

### `DELETE /api/v1/currencies/{id:int}`

| Status  | Body                                                                                | Condition                                                                                         |
| ------- | ----------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| 204     | —                                                                                   | deleted                                                                                           |
| **409** | `problem+json`, `type: …/business-rule`, `title` = the guard's message **verbatim** | `CanDeleteCurrencyAsync` refused (referential integrity) — **was 400 before M2-A06**              |
| 404     | `problem+json`, `type: …/not-found`, `title: "Currency not found."`                 | not found                                                                                         |
| **403** | `problem+json`, `type: …/screen-right-denied`                                       | caller has no `CanDelete` on `"Currency"` — decided before `CanDeleteCurrencyAsync` runs (M2-A02) |

**Business logic.** `CanDeleteCurrencyAsync(id)` then `DeleteCurrencyByCurrIdAsync(id)` —
the standard two-step delete-guard pattern (see BR-SO-001 for the same shape in Sales).

---

### `POST /api/v1/files`

**M2-B06, 2026-08-21.** `multipart/form-data`, replacing Blazor's `IBrowserFile` upload path.
Form fields: `file` (required), `refType`, `docType`.

| Status  | Body                                                                                                                      | Condition                                                       |
| ------- | ------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------- |
| 201     | `FileUploadResponse` — `id`, `fileName`, `filePath`, `contentType`, `sizeBytes`; `Location` header points at the download | stored                                                          |
| 400     | `problem+json`, `type: …/validation-error`                                                                                | no file, empty file, or an extension not on the allow-list      |
| **409** | `problem+json`, `type: …/business-rule`, `title: "File name already exists."`                                             | the duplicate-name rule of `CorrespondenceUpload.razor:341-347` |
| **413** | `problem+json`, `type: …/payload-too-large`                                                                               | above `FileStorage:MaxUploadBytes`                              |

**Not idempotent.** Every POST creates a new file — the `Guid.NewGuid()` prefix guarantees a
distinct name — and a new `Correspondence` row. A client that retries a timed-out request creates
a duplicate. A general idempotency-key mechanism is [M2-B12-03](../execution/tasks/M2-B12-03.md);
until it lands, clients must not retry blind.

### `GET /api/v1/files/{id:int}`

| Status | Body                                                                              | Condition                                                                                           |
| ------ | --------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| 200    | the bytes, with the resolved `Content-Type` and `Content-Disposition: attachment` | found                                                                                               |
| 404    | `problem+json`, `type: …/not-found`, `title: "File not found."`                   | **unknown id, another tenant's id, or a row whose bytes are gone — deliberately indistinguishable** |

**Security controls, all tested** (`tests/V.SMART.Api.Tests/FileEndpointSecurityTests.cs`):

- **Authentication and rights.** `[Authorize]` plus `[RequireScreen("Correspondences")]`;
  upload needs `Create`, download needs `View`. An unauthenticated download endpoint over a
  per-tenant folder tree would be a cross-tenant data leak, so the download is gated too.
- **Path traversal is structurally impossible.** `{id:int}` is a route constraint, so `../`,
  `%2e%2e%2f` and every other traversal string fails to match the route and never reaches the
  action. Behind that, a _stored_ path is canonicalised and proved to be inside the uploads root
  (`UploadPaths.IsInsideRoot`) before any file is opened, so a poisoned `Correspondence.FilePath`
  row cannot become an arbitrary file read.
- **Tenant isolation is structural, not a check.** `{id}` resolves through
  `IUnitOfWork.Correspondances`, scoped to the tenant-resolved `ApplicationDbContext`. A tenant-B
  token queries tenant B's database. "Not yours" and "does not exist" return byte-identical
  responses, so the endpoint is not an existence oracle.
- **Content type is validated, never trusted.** Membership of a 24-extension **allow-list**
  (`UploadContentTypes`) is the gate; the type stored and later served is this API's own mapping,
  not the browser-supplied header. The list is copied verbatim from the only extension check that
  existed — `CorrespondenceUpload.razor:213-220` — so nothing uploadable through Blazor is refused
  and nothing new is permitted. `.svg` is served as `application/octet-stream` rather than
  `image/svg+xml`: an SVG is script-bearing, and serving one natively from the SPA origin would
  make the upload endpoint a stored-XSS vector.

**Two size limits, deliberately.** `[RequestSizeLimit]`/`[RequestFormLimits]` refuse anything over
**20 MB** before the action runs — that attribute needs a compile-time constant — and the action
then applies `FileStorage:MaxUploadBytes`, which a deployment may lower. 20 MB is the same ceiling
`WebFileUploadService.cs:101` passes to `OpenReadStream`, so the HTTP path is never more permissive
than the Blazor path. Note the correspondence _screen_ refuses at 5 MB
(`CorrespondenceUpload.razor:222`) — a page-level rule, not a storage rule, not carried here; a
deployment wanting parity sets `FileStorage:MaxUploadBytes`.

**Storage.** Byte-identical on-disk results to `WebFileUploadService` for the same inputs:
`uploads/{drawings|correspondences}/{safeCompany}/{safeRefType}/{guid}_{name}`, per-tenant
segmentation by `tenant.Hostname`, the same `Path.GetInvalidFileNameChars()` stripping and
lowercasing. `FileStorage:Root` is configurable because `WebRootPath` resolves to a _different_
directory in each host; point both hosts at one path and either can read the other's files. Whether
that root is durable in the target deployment is **Unknown** — see `Q-16`.

### `GET /api/v1/currencies/export` · `POST /api/v1/currencies/import` · `GET /api/v1/currencies/import-template`

**M2-B06, 2026-08-21.** The **reference implementation** of the ADR-005 Excel contract, on one
resource only. Rolling it out to the rest is per-module work (KB-080 §10). Currency was chosen
because it is the one resource that already has a full controller, so the endpoints could be added
without inventing a service surface at the same time.

`ExcelExportService` and `ExcelTemplateService` are **wrapped, not modified** — verified by diff.
`[Authorize]` + `[RequireScreen("Currency")]`; export and template need `View`, import needs
`Create`.

### `GET /api/v1/reference/*` (M2-B09)

Six read-only lookup lists behind one tenant-keyed output cache. Full design, measurements and
the tenancy classification of each list: **[KB-124](reference-data-and-caching.md)**.

| Endpoint                           | Returns                                   | Rows (measured 2026-08-21) |
| ---------------------------------- | ----------------------------------------- | -------------------------: |
| `GET /api/v1/reference/gst-rates`  | `{ igst[], cgstSgst[] }`, paired by index |                    12 + 12 |
| `GET /api/v1/reference/states`     | `StateDto[]`                              |                         40 |
| `GET /api/v1/reference/uoms`       | `UomDto[]`                                |                         49 |
| `GET /api/v1/reference/currencies` | `CurrencyDto[]` — flat, no rate feed      |                          3 |
| `GET /api/v1/reference/screens`    | `ScreenDto[]` — the permission vocabulary |                    **150** |
| `GET /api/v1/reference/terms`      | `TermsDto[]`, active only                 |                          0 |

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

| Status  | Meaning                                  | Body                                                                           |
| ------- | ---------------------------------------- | ------------------------------------------------------------------------------ |
| 400     | model binding / `DataAnnotations`        | `errors` dictionary keyed by field, messages verbatim from the VM              |
| 401     | unauthenticated, or credentials rejected | minimal — no more informative than before M2-A06                               |
| 403     | screen right denied                      | KB-105 §7.1 shape, with `screen` and `right` extension members                 |
| 404     | not found                                | minimal                                                                        |
| **409** | **business-rule refusal**                | `title` carries the **service's own message verbatim** (BR-SO-001)             |
| 500     | unhandled                                | `traceId` only — no exception message, type or stack trace, in any environment |
| 503     | tenant context unavailable               | constant title; never a connection string (R-01)                               |

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
- **How a `409` is produced.** The services signal a refusal by _returning_ a tuple, not by
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

|                   |                                                                                                                                                                                   |
| ----------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Mechanism         | one constant, `V.SMART/V.SMART.Api/ApiRoutes.cs` → `public const string V1 = "api/v1";`                                                                                           |
| Usage             | `[Route($"{ApiRoutes.V1}/currencies")]` — legal in an attribute because `V1` is `const`                                                                                           |
| Package added     | **none.** `Asp.Versioning.Mvc` is deliberately not referenced: one version, no negotiation, no per-version OpenAPI document to complicate the client generation M2-B10 depends on |
| `Program.cs`      | **unchanged.** `MapControllers()` is the only route mapping and carries no path                                                                                                   |
| `Location` header | now `/api/v1/currencies/{CurrId}` — `CreatedAtAction` derives it from the action's route, so it followed the prefix automatically                                                 |

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

## Authorization state of the existing API

**Updated by M2-A02 (2026-08-24).** Every endpoint the API exposes is now either screen-right
enforced, explicitly and auditably exempt, or anonymous by design. Nothing is unannotated.

| Controller                            | Declaration                                                                                                                                              | Evidence                                                                  |
| ------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| `CurrencyController`                  | `[Authorize]` + `[RequireScreen("Currency")]`, one `[RequireRight]` per action: `View` on both GETs, `Create` on POST, `Edit` on PUT, `Delete` on DELETE | `Controllers/CurrencyController.cs:13,21` and `:53,71,81,95,109`          |
| `CurrencyExcelController`             | `[RequireScreen("Currency")]`, `View` on the exports, `Create` on the import                                                                             | `Controllers/CurrencyExcelController.cs:40`                               |
| `FilesController`                     | `[RequireScreen("Correspondences")]`                                                                                                                     | `Controllers/FilesController.cs:41`                                       |
| `MeController`, `ReferenceController` | `[NoScreenRight(reason)]` — reachable by every authenticated user, with the reason recorded in the attribute                                             | `Controllers/MeController.cs:37`, `Controllers/ReferenceController.cs:33` |
| `AuthController`                      | `[AllowAnonymous]` by design — login cannot require a token                                                                                              | `Controllers/AuthController.cs:57`                                        |

The rule enforced is BR-AUTH-002 exactly as `RightsHelper.cs:7-20` states it: per user, per
screen, **deny by default** (`?? false`). No `UserRight` row for `"Currency"` means no right,
so the caller receives `403`. `IsHide` is never consulted — it hides a screen from navigation
and neither grants nor revokes an operation (ADR-004 §1; KB-105 T-4).

**What this closed.** Before M2-A02, `[Authorize]` on `CurrencyController` meant only _"any
authenticated user of any tenant with a valid token"_. A user whose `UserRight` row for
`Currency` had `CanView = CanCreate = CanEdit = CanDelete = false` — a user for whom the
Blazor UI hides the entire Currency screen — **could create, edit and delete currencies
through the API**. That is the hole ADR-004 names, and for Currency it is now shut.

> **Breaking change, deliberate.** Any client whose user lacks the relevant Currency right
> now receives `403` where it previously received `200`/`201`/`204`. This is the correction of
> a security defect, not a regression, and it is recorded here so it is not later misdiagnosed
> as one. Verified 2026-08-24: no Angular code under `frontend/nexgen-web/src` calls
> `/api/v1/currencies`, so there is no live SPA consumer to break.

**Still open (R-03 is partially, not fully, closed).** Two directions of KB-105 D-4 remain
switched off: an authenticated action on a controller carrying _no_ `[RequireScreen]` is still
passed through by `ScreenRightAuthorizationFilter.cs:69-72` rather than denied, and
`ScreenRightStartupValidator.cs:83-88` still skips it rather than refusing to start. Both
files' comments say M2-A02 turns that on; M2-A02's own scope forbids editing
`V.SMART/V.SMART.Api/Authorization/**`, so it did not. Recorded as **Q-71** in
[`open-questions.md`](../open-questions.md). It is now _feasible_ — after this task every
endpoint is annotated or exempt — and it is the guard that stops controller number sixty-one
from silently shipping unprotected.

> **M2-A01-02 (2026-08-20) — how the mechanism is built.**
> `V.SMART/V.SMART.Api/Authorization/` holds `[RequireScreen]`, `[RequireRight]`,
> `[NoScreenRight]`, `IUserRightsProvider`/`UserRightsProvider`, `ScreenRightSet`,
> `ScreenCatalogue`, `ScreenRightStartupValidator` and `ScreenRightAuthorizationFilter`,
> registered globally in `Program.cs`. `M2-A03` generalises M2-A02's matrix into the
> merge-blocking harness. Specification:
> [KB-105](../architecture/server-side-authorization-spec.md).

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

> **Writing a new controller? Read [KB-114 `controller-conventions.md`](controller-conventions.md)**,
> not this section. It is the complete, compiled, **frozen** specification (M2-B03,
> 2026-08-24) — route shape, authorization, paging, errors, workflow commands, the
> `[ProducesResponseType]` set M2-B10 depends on, and a conformance checklist. The table below
> is a summary of _what is true today_; KB-114 is what a new controller must do.

Worth keeping (they are already consistent):

| Convention   | Form                                                                                                                             |
| ------------ | -------------------------------------------------------------------------------------------------------------------------------- |
| Base path    | `/api/v1/{plural-kebab-resource}` — the `api/v1` segment comes from `ApiRoutes.V1`, never a literal (M2-B01)                     |
| Paged list   | `GET /api/v1/x?pageNumber&pageSize&<filters>` → `{ items, totalCount, pageNumber, pageSize }`                                    |
| Single       | `GET /api/v1/x/{id:int}`                                                                                                         |
| Create       | `POST /api/v1/x` → 201 + `Location`                                                                                              |
| Update       | `PUT /api/v1/x/{id:int}` → 200                                                                                                   |
| Delete       | `DELETE /api/v1/x/{id:int}` → 204, or **409** `problem+json` with the service's user-facing message verbatim in `title` (M2-A06) |
| Payload type | the existing `…VM` ViewModel, unchanged                                                                                          |

Worth fixing before replication:

| Problem                                      | Fix                          |
| -------------------------------------------- | ---------------------------- |
| Untyped `Dictionary<string, object>` filters | typed query DTO per resource |
| No route/body id consistency check           | validate in the controller   |

Fixed and no longer listed: _two 400 shapes_ and _no correlation id_ — both closed by
**M2-A06** (see [_Error contract_](#error-contract-m2-a06)); _no versioning_ — closed by
**M2-B01** (see [_Versioning_](#versioning-m2-b01)); _no permission check_ — closed for
Currency by **M2-A02**, which is now the convention every controller copies:
`[RequireScreen("<seeded screen name>")]` on the class, one `[RequireRight(Right.X)]` per
action.

## Row scope and account gates (M2-A08)

Two behaviours that are not visible in the endpoint list but change what endpoints return.
Full evidence: [KB-108](../architecture/row-scope-and-account-gates.md).

**1. Row scope on list endpoints — a filter, not a refusal.** A scoped list endpoint returns
only the rows in the caller's scope. An out-of-scope row is simply **absent** from the
collection, exactly as `LeadService.cs:141-144` behaves today. A caller whose scope is empty
receives an **empty page and `200`** — never `403`, never everything. Scope is resolved per
request from the `UserId` and `TenantId` claims (never from the token payload), and applied
**at the query** via `IQueryable<T>.ApplyRowScope(scope)`, before filter, sort, count and
paging. **`totalCount` is counted within scope**, which differs from the Blazor list
(`LeadsList.razor:396-401` counts unscoped) — a deliberate, recorded change.

Today exactly one entity is scoped — `Leads` — and no endpoint over it exists yet.
`RowScopeStartupValidator` refuses to start the host if an action serves a scoped entity
without declaring `[RowScoped(typeof(T))]` or `[NoRowScope("<justification>")]`.

**2. Direct fetch of an out-of-scope row by id → `404`,** with the **same body a genuinely
missing row returns** (`ProblemResults.OutOfScopeProblem`). `403` would confirm the row exists.
This applies to every scoped resource, without exception.

**3. Account gates on authentication.** `POST /api/auth/login`:

| Condition                | Status    | `type`                    | Body `title`                                                                                                                     |
| ------------------------ | --------- | ------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| Unresolved tenant        | `400`     | `…/tenant-unresolved`     | unchanged (M2-A06)                                                                                                               |
| Bad credentials          | `401`     | `…/unauthenticated`       | `"Invalid username or password."`                                                                                                |
| **Trial expired**        | **`403`** | `…/trial-expired`         | `"Your trial period has expired. Please contact Administrator."` — verbatim from `Login.razor:273`                               |
| **Device mismatch**      | `403`     | `…/device-not-recognised` | `"This account is already registered on another mobile device."` / `"…another desktop."` — **evaluator exists, NOT wired; Q-40** |
| **Platform not allowed** | `403`     | `…/platform-not-allowed`  | `"Mobile login is not allowed."` / `"Desktop login is not allowed."` — **not wired; Q-40**                                       |

`403` and not `401` because the credential _was_ accepted: re-prompting for a password, which
is what a `401` tells a client to do, cannot resolve any of these. The messages are product UX
and are surfaced verbatim (ADR-002 §4). **They must not be collapsed into one status** — a
support desk that cannot tell an expired trial from a wrong password resets passwords all day.
**M2-A06** (see [_Error contract_](#error-contract-m2-a06)).

## Ancillary

`V.SMART.Api/Auth/ApiAuthStateProvider.cs` implements `AuthenticationStateProvider` over
`IHttpContextAccessor` so that shared code expecting Blazor's auth abstraction
(e.g. `CurrentUserService`) works inside API requests. Small but essential — it is why
existing services run unchanged under HTTP.

`V.SMART.Api/V.SMART.Api.http` and `Properties/launchSettings.json` define the dev
endpoint (`http://localhost:5144`, matching the Angular pilot's `environment.apiBaseUrl`).
