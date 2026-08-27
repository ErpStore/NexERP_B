---
doc_id: KB-001
title: Executive Summary of the Existing System
module: all
source_files:
  - NexGen-ERP---2025-master.sln
  - V.SMART/V.SMART.Shared/V.SMART.Shared.csproj
  - V.SMART/V.SMART.Web/Program.cs
  - V.SMART/V.SMART.Api/Program.cs
  - V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: complete
confidence: confirmed
last_verified: 2026-08-12
dependencies: []
---

# Executive Summary — Existing System

## What this product is

**V.SMART / NexGen ERP** is a mature, multi-tenant ERP for **discrete manufacturing**
(job-shop / make-to-order engineering companies, Indian market). It is not a generic
"sales + inventory" ERP: its centre of gravity is the manufacturing document chain —
enquiry → quotation → sales order → job order → route card → production → despatch →
invoice, plus outsourcing/subcontracting, tool crib, QC inspection, machine maintenance,
and GST-compliant statutory output (e-Invoice / e-Way Bill / GST ITC-04).

**Confirmed.** Evidence: `V.SMART.Shared/Layout/NavMenu.razor` (32 navigation groups),
`V.SMART.Shared/Data/ApplicationDbContext.cs` (196 `DbSet<>` declarations),
`V.SMART.Shared/Pages/` (333 Razor pages, 440 routes).

## Scale (measured, not estimated)

| Metric | Value | Source |
|---|---|---|
| .NET projects | 4 | `NexGen-ERP---2025-master.sln` |
| C# files | 1,596 | file scan |
| Razor components | 364 (333 under `Pages/`) | file scan |
| Routes (`@page`) | 440 | `grep '@page' Pages/` |
| EF Core entity sets | 196 | `ApplicationDbContext.cs` |
| Entity classes | 210 files / 17,274 LOC | `Data/` |
| Business services | 285 files / 128,518 LOC | `BusinessLayer/` |
| Repositories | 382 files | `Repository/` |
| ViewModels (DTOs) | 274 files / 21,465 LOC | `ViewModels/` |
| AutoMapper profiles | 148 files | `Mappings/` |
| **Razor page code** | **321,661 LOC, of which ~183,975 (57%) is inside `@code` blocks** | measured |
| EF migrations | 219 files (~2.5M LOC, snapshot-dominated) | `Migrations/` |
| FastReport templates | 104 `.frx` across 5 tenant folders | `wwwroot/templates/` |
| Stored procedures referenced from code | 94 distinct `Sp_*` names | code scan |
| Stored procedure DDL in repo | **13 files only** | `Existing Store Procedures/` |
| Automated tests | **0** | no test project in solution |

## Technology stack (confirmed)

| Layer | Technology |
|---|---|
| Runtime | .NET 9, C# latest, nullable enabled |
| Web UI | **Blazor Server** (`AddInteractiveServerComponents`, `MapRazorComponents<App>()`) |
| Desktop/mobile UI | .NET MAUI Blazor Hybrid (`V.SMART`, net9.0-android/ios/maccatalyst/windows) |
| Component library | MudBlazor 8.11 + Bootstrap 5 CSS + `QuickGrid` |
| Charts | Blazor-ApexCharts 6.1 |
| ORM | EF Core 9 (SQL Server), code-first |
| Mapping | AutoMapper 16.1 |
| Reporting | FastReport.OpenSource 2026.1 → PDF (`PdfSimple`) |
| Excel | EPPlus 8.5, ClosedXML 0.105, ExcelDataReader 3.7, OpenXml 3.3 |
| Barcode/QR | ZXing.Net, QRCoder |
| Crypto | BouncyCastle (e-Invoice payload handling) |
| API (new, partial) | ASP.NET Core Web API + JWT Bearer + Swashbuckle |
| Frontend pilot (new, partial) | Angular 19.2 + PrimeNG 19.1 |

## Architecture in one line

```
Razor page (.razor, 57% C# in @code)
      ↓ DI
IBusinessService → BusinessService   (ViewModel in / ViewModel out, AutoMapper)
      ↓
IUnitOfWork → IRepository<T> → Repository<T>
      ↓
ApplicationDbContext (per-tenant, connection string resolved at request scope)
      ↓
SQL Server (one database per tenant) + 94 stored procedures for reports
```

Plus a **separate `MasterDbContext`** holding a single `Tenants` table that maps
hostname → tenant connection string.

## The three findings that determine the migration

### 1. The service layer is genuinely reusable — this is the good news

Business services already speak **ViewModels, not entities**, are registered as scoped
DI services, are UI-framework-agnostic in ~97% of cases, and already use explicit
transactions. Only **14 of 285** business-layer files reference the `Pages` namespace and
only **19** reference any Blazor/MudBlazor type (mostly `IBrowserFile`, `IJSRuntime`,
`MudBlazor.Icons`). A REST controller can call them directly.

**This is already proven in the repository**: `V.SMART.Api/Controllers/CurrencyController.cs`
wraps the untouched `ICurrencyService` in a clean paged CRUD API with zero changes to the
business layer.

### 2. But ~184k lines of logic sit inside Razor `@code` blocks — this is the bad news

The average page is ~987 LOC; the largest is 6,528 LOC
(`Pages/SalesAndLabour_pages/LabourDcOut_Pages/LabourDcOutgoingUpsert.razor`). Inspection
of `MfgPOUpsert.razor` (4,383 LOC) shows the `@code` block contains real business
behaviour, not just view state: line-level quantity balancing (`UpdateQuantities`),
row validation (`ValidateRowAsync`), short-close (`ShortClosePo`), item cancellation with
transaction checks (`CancelItem`), and PO cancellation (`CancelPO`).

**Discarding the Blazor UI without first extracting this logic would lose real ERP
behaviour.** This is the single largest work item in the migration and the main driver of
per-module complexity estimates.

### 3. Authorization is enforced **only in the UI** — this is the security blocker

`Shared/BaseUserRightsComponent.cs` loads `UserRight` rows and exposes `CanView`,
`CanCreate`, `CanEdit`, `CanDelete`, `IsHidden`; 296 of 333 pages inherit it. **No
business service or repository performs any permission check** (verified by grep across
`BusinessLayer/`, `Repository/`, `Services/` — the only `CanDelete` matches there are
referential-integrity checks, not authorization).

An Angular SPA calling a REST API makes the client untrusted. **Server-side authorization
must be added before any module is exposed over HTTP**, otherwise every endpoint is
effectively public to any authenticated user. See
[`decisions/ADR-004-server-side-authorization.md`](decisions/ADR-004-server-side-authorization.md).

## Verdict on the central question

> *Can the existing backend support a new Angular frontend without modification?*

**No — but it needs additions, not a rewrite.** The domain layer stays. What must be built:

| Required | Why | Effort |
|---|---|---|
| REST controller layer (~60–80 controllers) | No HTTP surface exists except Auth + Currency | Large but mechanical |
| Server-side permission enforcement | Today it is UI-only (finding 3) | Medium, cross-cutting |
| Extraction of `@code` business logic into services | ~184k LOC, partially business logic (finding 2) | Largest item, per-module |
| Refresh tokens + logout/revocation | JWT is 480 min, no refresh, no revocation | Small |
| Tenant resolution for SPA origin | Host-based resolution breaks when SPA and API differ in host | Small |
| File upload/download over HTTP | Currently `IBrowserFile` / local paths | Small |
| Report delivery over HTTP | FastReport already returns `byte[]` — just needs an endpoint | Small |

**No backend business logic should be rewritten.** See
[`decisions/ADR-001-keep-existing-backend.md`](decisions/ADR-001-keep-existing-backend.md).

## Critical risks (detail in [`risks/technical-debt-register.md`](risks/technical-debt-register.md))

| # | Risk | Severity |
|---|---|---|
| R-01 | Live database credentials committed in `appsettings.json` (the local SA login, plus a **third party's** host `154.61.76.112` with a valid password, commented but present — owner-confirmed 2026-08-26 that host is not this project's; see R-01's correction note) | **Critical** |
| R-02 | JWT signing secret committed in `V.SMART.Api/appsettings.json` | **Critical** |
| R-03 | Authorization enforced only in UI components | **Critical** |
| R-04 | **82** of 94 stored procedures have no DDL in source control (one scripted file is dead code) | **Critical** |
| R-05 | Zero automated tests, zero CI | **High** |
| R-06 | ~184k LOC of logic trapped in Razor `@code` | **High** |
| R-07 | FIFO stock issue silently under-allocates when batch balance is insufficient | **High** |
| R-08 | Copy-paste bugs in delete-guard chains (e.g. `MfgPoService.cs:504`, `:525`) | **Medium** |

## Recommended next step

Do **not** start Angular feature work yet. Phase 2 begins with: server-side authorization
design, an API contract convention, and a vertical slice (Customer Master) proving the
controller → service → Angular path end-to-end. See
[`migration/migration-strategy.md`](migration/migration-strategy.md).
