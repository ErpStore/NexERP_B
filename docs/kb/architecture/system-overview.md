---
doc_id: KB-010
title: System Overview (As-Is)
module: architecture
source_files:
  - NexGen-ERP---2025-master.sln
  - V.SMART/V.SMART.Web/Program.cs
  - V.SMART/V.SMART/MauiProgram.cs
  - V.SMART/V.SMART.Api/Program.cs
  - V.SMART/V.SMART.Shared/V.SMART.Shared.csproj
  - V.SMART/V.SMART.Shared/Routes.razor
entities: []
api_endpoints: []
database_tables: [Tenants]
business_rules: []
status: complete
confidence: confirmed
last_verified: 2026-08-12
dependencies: [KB-011, KB-012, KB-013]
---

# System Overview (As-Is)

## Solution composition

`NexGen-ERP---2025-master.sln` contains four projects.

| Project | SDK | Target | Role |
|---|---|---|---|
| **V.SMART.Shared** | `Microsoft.NET.Sdk.Razor` | `net9.0`; `net9.0-windows10.0.19041.0` | **The entire application.** Entities, DbContexts, repositories, business services, ViewModels, AutoMapper profiles, all 333 Razor pages, layout, shared components, reporting, e-Invoice integration. |
| **V.SMART.Web** | `Microsoft.NET.Sdk.Web` | `net9.0` | Blazor **Server** host. Owns DI composition root (`Program.cs`, 34,813 bytes, 242 DI registrations) and web-specific `IFileOpener` / `IPathProvider` / `IFileUploadService`. |
| **V.SMART** | `Microsoft.NET.Sdk.Razor`, `UseMaui` | android / ios / maccatalyst / windows | .NET MAUI **Blazor Hybrid** desktop+mobile host. Duplicate DI composition root (`MauiProgram.cs`, 38,572 bytes). |
| **V.SMART.Api** | `Microsoft.NET.Sdk.Web` | `net9.0` | **New, ~10% built.** REST API for the future SPA. 2 controllers, JWT bearer, Swagger, CORS for `http://localhost:4200`. |

`V.SMART.Web`, `V.SMART`, and `V.SMART.Api` all `ProjectReference` `V.SMART.Shared`.
There is no reference between the three hosts.

**Confirmed.** Evidence: the four `.csproj` files and the `.sln`.

## Hosting model

```
                    ┌──────────────────────────┐
                    │      V.SMART.Shared      │
                    │  entities, repos, svcs,  │
                    │  ViewModels, 333 pages   │
                    └───────────┬──────────────┘
        ┌───────────────────────┼────────────────────────┐
        │                       │                        │
┌───────▼────────┐   ┌──────────▼─────────┐   ┌──────────▼──────────┐
│  V.SMART.Web   │   │      V.SMART       │   │    V.SMART.Api      │
│  Blazor Server │   │  MAUI Blazor Hybrid│   │  REST (JWT), ~10%   │
│  SignalR circuit│  │  local WebView     │   │  → future React SPA │
└───────┬────────┘   └──────────┬─────────┘   └──────────┬──────────┘
        └───────────────────────┴────────────────────────┘
                                │
                  ┌─────────────▼──────────────┐
                  │  MasterDbContext (Tenants) │
                  │  → per-tenant connection    │
                  └─────────────┬──────────────┘
                                │
                  ┌─────────────▼───────────────────────────┐
                  │ SQL Server: one database per tenant     │
                  │ 196 entity tables + 94 stored procedures│
                  └─────────────────────────────────────────┘
```

`V.SMART.Web/Program.cs` pipeline (confirmed, in order):
`UseExceptionHandler("/Error")` + `UseHsts` (non-dev) → `UseForwardedHeaders` →
`UseHttpsRedirection` → `UseStaticFiles` → `UseAntiforgery` →
`MapRazorComponents<App>().AddInteractiveServerRenderMode().AddAdditionalAssemblies(typeof(V.SMART.Shared._Imports).Assembly)`.

There is **no** `UseAuthentication()` / `UseAuthorization()` middleware in the Blazor Server
host — authentication is entirely in-circuit via a custom `AuthenticationStateProvider`.
**Confirmed.**

## Layering

The intended layer order (stated in `.github/copilot-instructions.md` and verified to hold
in practice for the service→repository boundary):

```
Razor Page (.razor) → IBusinessService → BusinessService → IRepository → Repository → DbContext
```

Deviations found (all **Confirmed**):

| Deviation | Count | Evidence |
|---|---|---|
| Business-layer files that reference the `Pages` namespace | 6 | `BusinessLayer/.../ApprovalService.cs`, `IApprovalService.cs`, `EinvoiceDatabaseService.cs`, `StockAddIssPosition.cs`, `MaintenanceProcessService.cs`, `GetRatingsService.cs` |
| Entity / ViewModel / Mapping files referencing `Pages` | 8 | incl. `Data/ApplicationDbContext.cs`, `Data/OutSourcing/MaterialReq.cs`, `Data/SalesAndLabour/PerformaInvoice/PerformaInv.cs` |
| Business-layer files referencing Blazor/MudBlazor types | 19 | `IJSRuntime` ×4, `IBrowserFile` ×2, `MudBlazor.Icons` ×6, `MudBlazor.Interfaces` ×1 |
| Razor pages calling `_unitOfWork.SaveAsync()` directly | 91 call sites | `grep SaveAsync Pages/` |

The worst single example: `IApprovalService.cs` — a **business-layer interface** — declares
`using static V.SMART.Shared.Pages.Planning_Module_Pages.Authorization_Pages.Authorization;`,
i.e. the domain contract depends on a Razor page class. This must be untangled before the
approval workflow can be exposed over HTTP.

## Cross-cutting services

| Service | Registered as | Purpose |
|---|---|---|
| `ITenantProvider` / `ITenantDbContextFactory` | Scoped | Per-request tenant + DbContext resolution — see [multi-tenancy](multi-tenancy.md) |
| `AuthenticationStateProvider` → `CustomAuthStateProvider` | Scoped | In-memory claims principal — see [auth-and-permissions](auth-and-permissions.md) |
| `CurrentUserService` | Scoped | Username / UserId from claims; machine name; IP |
| `UserSession` | Scoped | `UserName`, `UserId`, `DatabaseName`, `HostName` |
| `IUnitOfWork` → `UnitOfWork` | Scoped | ~190 typed repository properties + `SaveAsync` + `BeginTransactionAsync` |
| `ICalculationService` → `CalculationService` | Scoped | **The single document totals/tax engine** — see [business rules](../business-rules/business-rule-inventory.md#br-calc) |
| `ILoggingService` → `FileLoggingService` | Scoped | Flat-file logs under `App_Data/Logs/{UserLogs,DeveloperLogs}` |
| `ReportService` | Scoped | FastReport `.frx` → PDF bytes |
| `IReportExecutor` → `ReportExecutor` | Scoped | `EXEC dbo.<proc>` → typed list, 300 s timeout |
| `ExcelExportService`, `IExcelTemplateService` | Scoped | EPPlus/ClosedXML export + import templates |
| `IColumnPreferenceService` | Scoped | Per-user, per-screen grid column visibility (`UserColumnPreference.ColumnJson`) |
| `ThemeStateService` | Scoped | Light/dark theme state |
| `SessionTimeoutService` | **Singleton** | Idle tracking — see note below |
| `ForeignKeyUsageChecker` | Scoped | Generic referential-integrity guard before delete |
| `IFileUploadService`, `IFileOpener`, `IPathProvider` | Scoped, host-specific | Abstractions implemented differently by Web vs MAUI |

> **Defect (Confirmed):** `SessionTimeoutService` is registered `AddSingleton` in
> `V.SMART.Web/Program.cs` but holds a single `_lastActivity` field. In a multi-user
> Blazor Server deployment, all users share one idle timer. Any user's activity resets
> every other user's timeout.

## Background processing

**None.** No `IHostedService`, `BackgroundService`, `PeriodicTimer`, Hangfire, or Quartz
anywhere in the solution. **Confirmed** by grep. All work is request/interaction-driven.

Implication for the SPA: nothing needs to be preserved here, but long-running operations
(large reports, Excel imports) currently block the Blazor circuit and would block an HTTP
request in the same way.

## Configuration and environments

| Setting | Location | Notes |
|---|---|---|
| `ConnectionStrings:MasterDb` | `V.SMART.Web/appsettings.json`, `V.SMART.Api/appsettings.json` | **Contains live SA credentials in source control.** A production host (`154.61.76.112,1533`, db `IQSmartDb_Master`, user `bspl`) is present as a commented line. |
| `Jwt:Secret` / `Issuer` / `Audience` / `ExpiresMinutes` | `V.SMART.Api/appsettings.json` | Secret committed. `ExpiresMinutes: 480`. |
| `AppEnvironment` | injected in-memory in `V.SMART.Web/Program.cs` as `"Web"` | Host discriminator |
| Tenant fallback | `wwwroot/config/tenant.json` | Used by MAUI and API when host lookup fails |
| Per-tenant runtime config | `Tenants` table in `MasterDb` | Hostname → connection string |

No `appsettings.Production.json`, no user-secrets, no environment-variable overrides, no
Key Vault. **Confirmed.**

## Build and deployment

- Build: Visual Studio / `dotnet build` on the `.sln`. No `Directory.Build.props`, no
  central package management.
- CI/CD: **none.** `.github/` contains only `copilot-instructions.md` and
  `prompts/convert-to-zoho-ui.prompt.md` — no workflows.
- MAUI packaging: MSIX signing thumbprint and `AppInstallerUri=D:\` are hardcoded in
  `V.SMART/V.SMART.csproj` — a developer-machine artefact.
- Deployment target: **Inferred** from tenant hostnames
  (`acucom.bhargavisofttech.co.in`, `sns.…`, `srinuenggind.…`, `sharadaelectrou1.…`) that
  the Web host runs behind a reverse proxy per tenant subdomain. `UseForwardedHeaders`
  with `KnownNetworks`/`KnownProxies` cleared supports this. **Inferred.**

## Testing

**Zero.** No test project in the solution; no `*.Tests.csproj` anywhere in the tree; no
test framework package references. **Confirmed.**
