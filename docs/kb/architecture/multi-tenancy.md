---
doc_id: KB-014
title: Multi-Tenancy (As-Is)
module: architecture
source_files:
  - V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantProvider.cs
  - V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantDbContextFactory.cs
  - V.SMART/V.SMART.Shared/Data/TenantInfo.cs
  - V.SMART/V.SMART.Api/wwwroot/config/tenant.json
  - V.SMART/V.SMART.Web/Program.cs
entities: [TenantInfo]
api_endpoints: []
database_tables: [Tenants]
business_rules: [BR-TEN-001, BR-TEN-002]
status: complete
confidence: confirmed
last_verified: 2026-08-20
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
`TenantProvider.cs:26-80`):

1. **JWT `TenantId` claim** — `HttpContext.User.FindFirst("TenantId")` → lookup by `Id`.
   Used by `V.SMART.Api`.
2. **Request host** — `HttpContext.Request.Host.Host` → lookup by `Hostname`.
   Used by `V.SMART.Web` (each tenant has its own subdomain).
3. **`wwwroot/config/tenant.json`** — `{ Tenant, HostName, ServerPath, CompanyName }` →
   lookup by `Name` or `Hostname`. Used by the MAUI desktop client and as an API fallback.

Result cached in a private `_cached` field for the lifetime of the scoped instance.
Returns `null` and writes to `Console` if all three fail.

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

## Problems for a React SPA

| # | Problem | Detail |
|---|---|---|
| 1 | **Host-based resolution breaks** | The SPA will be served from its own origin (or a CDN) and call the API cross-origin. `Request.Host.Host` at the API will be the *API's* host, not the tenant's. Only step 1 (JWT claim) will work — **but the JWT is only issued *after* login, and login itself needs a tenant to resolve the user.** This is a genuine chicken-and-egg gap. |
| 2 | **`tenant.json` fallback is a single global value** | `V.SMART.Api/wwwroot/config/tenant.json` pins the API to one tenant. Acceptable for a dev spike; unusable in multi-tenant production. |
| 3 | **CORS is dev-only** | `Program.cs` policy `"AngularDev"` allows exactly `http://localhost:4200`. Needs a real, per-environment origin list. |
| 4 | ~~**Failure is silent**~~ — **closed on the API side, 2026-08-20 (M2-A06)** | `GetCurrentTenant()` still returns `null` and writes to `Console.WriteLine`, and `TenantDbContextFactory.cs:19` still throws `NullReferenceException` on `.ConnectionString` — **neither is changed**, because both are in `V.SMART.Shared` and serve the live Blazor app. What changed is what the **API caller** sees: `ExceptionHandlingMiddleware` recognises that exact throwing frame and answers `503` `application/problem+json` (`type: …/tenant-unresolved`, title *"Tenant context is unavailable."*) with a `traceId` and **no connection string** (R-01). `503` rather than a `4xx` because `TenantProvider.cs:77-80` swallows every exception before returning `null`, so an unknown tenant and a MasterDb outage are indistinguishable at that point and blaming the caller would be guessing (R-19). **Still open under Blazor Server**, where the `NullReferenceException` is unhandled as before. |
| 5 | **No tenant-connection secret management** | Connection strings, with credentials, are stored in plaintext in the `Tenants` table. |

### Proposed resolution — see [ADR-002](../decisions/ADR-002-rest-api-layer.md)

*(Proposal, not current behaviour.)* Options for pre-login tenant identity:

- **A. Tenant in the login request** — the SPA sends `{ tenant, username, password }`;
  the tenant comes from the SPA's own subdomain (`acucom.app.example.com`) or a picker.
  Lowest friction, keeps one API deployment.
- **B. `X-Tenant` header on every request** — resolved before auth for `/api/auth/*`,
  from the JWT afterwards. Requires anti-forgery care.
- **C. One API host per tenant** — preserves the current host-based logic verbatim; costs
  N deployments.

**Recommended: A**, with the tenant echoed into the JWT (which already happens) so that
every post-login request keeps working through resolution step 1 unchanged.

## Migration impact

The tenancy design itself is **sound and should be preserved**. Only the *resolution
front door* needs work, and it is small (one login-request field, one CORS config, one
error path). No change to `TenantDbContextFactory`, `TenantInfo`, or the
database-per-tenant model is proposed.
