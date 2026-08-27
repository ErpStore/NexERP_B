---
doc_id: KB-014
title: Multi-Tenancy (As-Is)
module: architecture
source_files:
  - V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantProvider.cs
  - V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantDbContextFactory.cs
  - V.SMART/V.SMART.Shared/Data/TenantInfo.cs
  - V.SMART/V.SMART.Api/Controllers/AuthController.cs
  - V.SMART/V.SMART.Api/Cors/CorsOptions.cs
  - V.SMART/V.SMART.Web/Program.cs
entities: [TenantInfo]
api_endpoints:
  - "POST /api/v1/auth/login"
  - "POST /api/v1/auth/refresh"
  - "POST /api/v1/auth/logout"
database_tables: [Tenants]
business_rules: [BR-TEN-001, BR-TEN-002]
status: complete
confidence: confirmed
last_verified: 2026-08-27
dependencies: [KB-012, KB-013]
---

# Multi-Tenancy (As-Is)

## Model

**Database per tenant**, resolved per request, no discriminator column.

`MasterDbContext.Tenants` holds `TenantInfo { Id, Name, Hostname, ConnectionString }`.
`TenantDbContextFactory.CreateDbContext()` builds a fresh
`ApplicationDbContext` bound to `_tenantProvider.GetCurrentTenant().ConnectionString`
with a 60-second command timeout.

Registration in both hosts:

```csharp
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();
builder.Services.AddScoped<ApplicationDbContext>(sp =>
    sp.GetRequiredService<ITenantDbContextFactory>().CreateDbContext());
```

## Resolution order

`TenantProvider.GetCurrentTenant()` tries, in order (**Confirmed**,
`TenantProvider.cs`, re-verified 2026-08-27 after M2-A05):

0. **Explicit binding (M2-A05, 2026-08-27)** — `SetTenant(identifier)` sets a private
   `_manualTenant` field; the next `GetCurrentTenant()` call resolves it by `Name` **or**
   `Hostname`, the same two columns step 3 already matches. `AuthController`'s
   `Login`/`Refresh`/`Logout` actions call `SetTenant(request.Tenant)` before resolving any
   tenant-scoped service, because ADR-002 §5's `{ tenant, username, password }` login body
   arrives too late for the JWT-claim/host steps below — ASP.NET Core constructs a
   controller, resolving every constructor-injected service, **before** it model-binds
   `[FromBody]` parameters, so a naive resolution order would build the tenant-scoped
   `ApplicationDbContext` before the request body's `tenant` field was ever readable. This
   step existed as a no-op setter (`_manualTenant` assigned, never read) before M2-A05; it
   is now the fix, not new plumbing.
1. **JWT `TenantId` claim** — `HttpContext.User.FindFirst("TenantId")` → lookup by `Id`.
   Used by `V.SMART.Api` for every request after login.
2. **Request host** — `HttpContext.Request.Host.Host` → lookup by `Hostname`.
   Used by `V.SMART.Web` (each tenant has its own subdomain).
3. **`wwwroot/config/tenant.json`** — `{ Tenant, HostName, ServerPath, CompanyName }` →
   lookup by `Name` or `Hostname`. Used by the MAUI desktop client. **No longer used by the
   API**: `V.SMART.Api/wwwroot/config/tenant.json` was deleted by M2-A05 (it had shipped a
   real dev-tenant pin, `"BSPL"`, into every build including a would-be production one — see
   _Problem 2_, closed, below); `V.SMART.Web/V.SMART`'s own copies are separate files,
   untouched.

Result cached in a private `_cached` field for the lifetime of the scoped instance.
Returns `null` and writes to `Console` if every step fails.

Known tenants, inferred from report-template folders under
`V.SMART.Shared/wwwroot/templates/`: `acucom`, `sns`, `srinuenggind`, `sharadaelectrou1`
— all `*.bhargavisofttech.co.in` — plus `default`. **Confirmed** (folder names);
**Inferred** (that these are the complete production tenant set).

> **BR-TEN-001 (Confirmed):** Every database operation is scoped to exactly one tenant
> database. Cross-tenant queries are structurally impossible through
> `ApplicationDbContext`.

> **BR-TEN-002 (Confirmed):** The **API resolves tenant from the JWT**, so tenant identity
> is bound at login and carried in a signed token. The **Web host resolves from hostname**,
> so tenant identity is bound to the URL.

## Tenant-scoped file paths

`TenantProvider` also resolves per-tenant file locations (desktop only, from
`tenant.json`'s `ServerPath` + `CompanyName`):

- `GetTenantLogoPath()` → `{ServerPath}/{CompanyName}/CompanyLogos`
- `GetTenantCorrespondenceUploadPath()` → `{ServerPath}/{CompanyName}/Correspondences`

Directories are created on demand.

Report templates use a different scheme: `{reportRoot}/{tenant.Hostname}/{file}.frx`
falling back to `{reportRoot}/default/{file}.frx` (`ReportService.Generate_Report`).

**Two different per-tenant path conventions coexist** (`CompanyName` vs `Hostname`) — a
consistency wrinkle to normalise when file handling moves to HTTP.

## Problems for an Angular SPA

| #   | Problem                                                                                  | Detail                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| --- | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | ~~**Host-based resolution breaks**~~ — **closed 2026-08-27 (M2-A05)**                    | The SPA is served from its own origin and calls the API cross-origin, so `Request.Host.Host` is the _API's_ host, not the tenant's — confirmed, not merely predicted (`frontend/nexgen-web/playwright.config.ts:3`'s `127.0.0.1:4300` never matched anything the old CORS policy or host resolution allowed). Closed by resolution step 0 above: `AuthController` binds the tenant from the login request body before the chicken-and-egg point is ever reached.                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| 2   | ~~**`tenant.json` fallback is a single global value**~~ — **closed 2026-08-27 (M2-A05)** | `V.SMART.Api/wwwroot/config/tenant.json` pinned the API to one tenant (`"BSPL"`, a real committed value that would have shipped into any environment copying `wwwroot/` verbatim, production included). Deleted outright rather than merely gated to `Development` — step 0 now covers the API's real traffic, and `TenantProvider.cs`'s own `LoadTenantConfigFromJson()` already handled a missing file gracefully (returns `null`), so no code change was needed to make its absence safe. `V.SMART.Web`'s own copy never existed; `V.SMART` (MAUI)'s is a separate file, untouched.                                                                                                                                                                                                                                                                                                                              |
| 3   | ~~**CORS is dev-only**~~ — **closed 2026-08-27 (M2-A05)**                                | `Program.cs`'s `"AngularDev"` policy, hardcoded to `http://localhost:4200`, is replaced by `CorsOptions` (`V.SMART.Api/Cors/CorsOptions.cs`), bound from the `Cors` configuration section — a real per-environment origin list, empty by default in `appsettings.json` (fails closed) and populated in `appsettings.Development.json`. Real production origin values are deliberately **not** invented: Q-16 (deployment topology) is explicitly deferred (`docs/kb/open-questions.md`), and the task's own Prerequisites treat that as licence to ship the mechanism, not the values.                                                                                                                                                                                                                                                                                                                              |
| 4   | ~~**Failure is silent**~~ — **closed on the API side, 2026-08-20 (M2-A06)**              | `GetCurrentTenant()` still returns `null` and writes to `Console.WriteLine`, and `TenantDbContextFactory.cs:19` still throws `NullReferenceException` on `.ConnectionString` — **neither is changed**, because both are in `V.SMART.Shared` and serve the live Blazor app. What changed is what the **API caller** sees: `ExceptionHandlingMiddleware` recognises that exact throwing frame and answers `503` `application/problem+json` (`type: …/tenant-unresolved`, title _"Tenant context is unavailable."_) with a `traceId` and **no connection string** (R-01). `503` rather than a `4xx` because `TenantProvider.cs:77-80` swallows every exception before returning `null`, so an unknown tenant and a MasterDb outage are indistinguishable at that point and blaming the caller would be guessing (R-19). **Still open under Blazor Server**, where the `NullReferenceException` is unhandled as before. |
| 5   | **No tenant-connection secret management**                                               | Connection strings, with credentials, are stored in plaintext in the `Tenants` table.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |

### Proposed resolution — see [ADR-002](../decisions/ADR-002-rest-api-layer.md)

_(Proposal, not current behaviour.)_ Options for pre-login tenant identity:

- **A. Tenant in the login request** — the SPA sends `{ tenant, username, password }`;
  the tenant comes from the SPA's own subdomain (`acucom.app.example.com`) or a picker.
  Lowest friction, keeps one API deployment.
- **B. `X-Tenant` header on every request** — resolved before auth for `/api/v1/auth/*`,
  from the JWT afterwards. Requires anti-forgery care.
- **C. One API host per tenant** — preserves the current host-based logic verbatim; costs
  N deployments.

**Recommended: A**, with the tenant echoed into the JWT (which already happens) so that
every post-login request keeps working through resolution step 1 unchanged.

**Implemented 2026-08-27 (M2-A05), exactly as recommended — option A, not B or C.** The SPA
sends `{ tenant, username, password }`; the tenant identifier is plain text the user types
(no subdomain-derivation exists, since no per-tenant subdomain deployment exists to derive
one from — Q-16 remains explicitly deferred). Every subsequent request still resolves
through step 1 unchanged, exactly as this section predicted.

## Migration impact

The tenancy design itself is **sound and was preserved**. Only the _resolution front door_
needed work, and it was small (one login-request field, one CORS config, one error path
already closed by M2-A06). No change was made to `TenantDbContextFactory`, `TenantInfo`, or
the database-per-tenant model.

## Health checks read `MasterDbContext.Tenants` directly — `ITenantProvider` cannot be used

Added by **M2-B11** (2026-08-21). Recorded here because the reason is a property of _this_
design, and rediscovering it costs half a task.

`GET /health/ready` probes tenant databases, and it **must not** go through
`ITenantProvider`/`ITenantDbContextFactory`. `TenantProvider.GetCurrentTenant()` resolves from
`_httpContextAccessor?.HttpContext?.User?.FindFirst("TenantId")` (`TenantProvider.cs:33-34`),
falling back to the request host and then `tenant.json`; `TenantDbContextFactory` depends on it
(`TenantDbContextFactory.cs:7-12`). **A health probe has no `HttpContext`, no JWT and no user**
— the endpoints are anonymous precisely because an orchestrator cannot present one — so none of
those resolution steps can produce a tenant.

`V.SMART/V.SMART.Api/HealthChecks/TenantDatabaseHealthCheck.cs` therefore reads
`MasterDbContext.Tenants` (`MasterDbContext.cs:8`), takes a **configurable subset** (default:
the lowest one `Id`, because probing every tenant on every poll does not scale and lets one sick
tenant unready the whole service), and opens a `SqlConnection` on
`TenantInfo.ConnectionString` directly with a clamped connect timeout. It reports each probed
tenant as an opaque `tenant-{Id}` and never discloses the `Name`, the `Hostname`, the connection
string or any exception text.

This is the **only** correct shape for an unauthenticated probe in a database-per-tenant system;
it is not a hack, and it does not weaken tenant isolation — nothing it reads leaves the process
except a status word. Full contract: [KB-113](observability.md) §2–3.
