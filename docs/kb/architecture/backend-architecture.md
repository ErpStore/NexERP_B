---
doc_id: KB-011
title: Backend Architecture (As-Is)
module: architecture
source_files:
  - V.SMART/V.SMART.Shared/Repository/Repository.cs
  - V.SMART/V.SMART.Shared/Repository/UnitOfWork.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgPoService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/InventoryService/StockManagerService.cs
  - V.SMART/V.SMART.Shared/Services/CalculationService.cs
  - V.SMART/V.SMART.Shared/Services/ReportViewer/ReportService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/ReportExecutor.cs
entities: []
api_endpoints: []
database_tables: []
business_rules: [BR-CALC-001, BR-STK-001]
status: complete
confidence: confirmed
last_verified: 2026-08-20
dependencies: [KB-010, KB-013]
---

# Backend Architecture (As-Is)

## Shape

Classic layered / Repository + Unit-of-Work, with a service layer that speaks DTOs.

```
IBusinessService (285 files, 128,518 LOC)
   ├─ takes and returns ViewModels (never entities across the boundary)
   ├─ maps via AutoMapper (148 profiles)
   ├─ opens explicit DB transactions for multi-table writes
   └─ depends on IUnitOfWork
        └─ IRepository<T> / typed repositories (382 files)
             └─ ApplicationDbContext (EF Core 9, per-tenant)
```

**Why this matters for the migration:** the service boundary is already a good API
boundary. Services accept and return serialisable ViewModels with `DataAnnotations`
validation attributes, which map cleanly to JSON request/response bodies and to
client-side schemas. **Confirmed** — see `ViewModels/MasterViewModel/AccountsViewModel/CurrencyVM.cs`
and the working proof in `V.SMART.Api/Controllers/CurrencyController.cs`.

## Generic repository

`Repository/Repository.cs` (328 LOC) — `Repository<T> : IRepository<T> where T : class`.

Surface: `GetAsync(id)`, `GetWithIncludeAsync(predicate, includes[])`,
`GetAllAsync(bool asNoTracking)`, `GetAllAsync(predicate)`,
`GetAllWithIncludeAsync(predicate, includes[])`, `CreateAsync`, `CreateRangeAsync`,
`UpdateAsync`, `DeleteAsync`, `GetQueryable()`.

Registered open-generic: `AddScoped(typeof(IRepository<>), typeof(Repository<>))`.

**Note (Confirmed):** `GetQueryable()` leaks `IQueryable<T>` upward, and business services
use it heavily (composing `.Include(...).Where(...).AnyAsync(...)` in the service layer).
This is pragmatic but means the repository abstraction does not actually isolate EF —
relevant only if the persistence technology is ever changed, which is not proposed.

## Unit of Work

`Repository/UnitOfWork.cs` (817 LOC) exposes ~190 typed repository properties
(`MfgPos`, `MfgPoSubs`, `MfgDcSubs`, `PurchPos`, `StockAdds`, `StockIssues`,
`StockIssueTracks`, `Users`, `UserRights`, …) plus:

- `Task<int> SaveAsync()` → `_db.SaveChangesAsync()`
- `Task<IDbContextTransaction> BeginTransactionAsync()` → `_db.Database.BeginTransactionAsync()`
- `void DetachEntity<TEntity>(TEntity)`

**Transaction usage is real, not decorative:** 302 `BeginTransaction*` occurrences across
the codebase, present in every multi-table service (Payments, Receipts, Advance
Adjustment, Salary, Staff Loan, Attendance, Stock Issue Request, Store Inter Transfer,
Tool Crib, Labour DC/GRN/SCN/Invoice, SCN Gen, MIN, Costing, Master Inspection, …).
**Confirmed.**

**But:** 91 call sites in Razor pages call `_unitOfWork.SaveAsync()` directly, outside any
service transaction. These are page-orchestrated writes and are the riskiest part of the
`@code` extraction work.

## Business service conventions (observed, consistent)

Sampled across `MfgPoService`, `StockManagerService`, `SCNGenService`, `CurrencyService`,
`AppointmentLetterService`:

| Convention | Form |
|---|---|
| Read a list with filters + paging | `Task<(List<TVm> items, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)` |
| Read one | `Task<TVm?> Get…ByIdAsync(int id)` |
| Create / update (upsert) | `Task<TVm> Upsert…Async(TVm vm)` or `Task<(bool success, string message, TVm? entity)> CreateAsync/UpdateAsync(...)` |
| Delete guard | `Task<(bool CanDelete, string Message)> CanDelete…Async(int id)` — returns a **user-facing message** |
| Delete | `Task<bool> Delete…ByIdAsync(int id)` |
| Duplicate check | `Task<bool> IsDuplicate…Async(...)` |
| Next document number | `Task<string> GetNext…NoAsync(...)` |
| Typeahead | `Task<IEnumerable<TVm>> Search…Async(string searchText)` |
| Error handling | `try/catch` → `_logs.LogDeveloperError(ex, context)` → rethrow wrapped `Exception` |

This regularity is the single most useful property of the codebase for the migration: a
REST controller template can be generated per service with very little per-service
thought. See [`api/api-readiness-assessment.md`](../api/api-readiness-assessment.md).

**The dictionary stays in the service; the controller takes a typed DTO (M2-B02, 2026-08-20,
Confirmed).** `SearchWithDynamicFilterAsync` is declared **134 times** across
`V.SMART.Shared/BusinessLayer/` with that identical three-parameter signature, consumed by **67**
nested `*FilterBuilder` classes — and **no** service takes a sort parameter; ordering is hardcoded
per service (`CurrencyService.cs:279`, `OrderByDescending(x => x.CurrId)`). The API does **not**
change that. A per-resource typed query record binds at the controller and
`V.SMART.Api.Contracts.FilterDictionaryAdapter` maps it — explicitly, never by reflection — onto
the dictionary the service already takes, so the service is untouched and the dictionary never
appears on the wire (ADR-002 §2a).

Sort is the one thing the dictionary cannot carry: every `*FilterBuilder.ApplyFilter` ends in
`_ => query` (`CurrencyService.cs:206`), so an unrecognised key is silently discarded and the
request would answer 200 while sorting nothing. It is therefore delivered as an **additive
overload** — `SearchWithDynamicFilterAsync(int, int, Dictionary<string, object>?, string? sort)`
— with the existing member delegating to it. `CurrencyService` is so far the only one of the 134
that has it; the rest convert inside their own module's wave. When adding it to a service, pair
the existing `*FilterBuilder` with a `*SortBuilder` whose field `switch` is an explicit allow-list
and which **throws** on an unknown field (`CurrencyService.cs:227-322` is the reference).

## The calculation engine — the crown jewel

`Services/CalculationService.cs` implements `ICalculationService.UpdateTotalsAsync(ICalculationDocument)`.
Every transactional document (quotation, sales order, DC, invoice, purchase order, GRN,
credit/debit note, …) implements `ICalculationDocument` / `ICalculationDocumentSubItem`
(`Utility_Constants/`), so **one implementation computes totals for the whole ERP**.

Ordered algorithm (Confirmed, `CalculationService.cs:12-114`):

1. `TotalGrossAmount` = Σ line gross
2. Discount: if any line has a discount amount, header discount = Σ line discounts;
   otherwise header discount is either a fixed amount (`DiscAmtOrPer == true`) or
   `Gross × DiscountPercent / 100`
3. `TotalBasicAmount` = Gross − Discount
4. Packing / Insurance: percentage applied to `TotalBasicAmount` when the corresponding
   `…AmtOrPer` flag is false
5. `TotalTaxable` = Basic + Freight + Packing + Insurance
6. Tax:
   - **item-wise** (`HasItemWiseTax`): each line's taxable base = line gross − line
     discount − proportional header discount + proportional freight/packing/insurance,
     where proportion = `LineGross / Σ LineGross`; CGST/SGST/IGST accumulated per line rate
   - **header-wise**: `TotalTaxable × rate / 100`
7. TCS: fixed amount or `(taxable + taxes) × TCSPercent / 100`
8. Grand total = taxable + taxes + TCS + `OtherCharges`; if `IsRoundOffEnabled`,
   `Math.Round(…, 0, MidpointRounding.AwayFromZero)` and `RoundOff` = difference

Permitted GST rates are enumerated in `Utility_Constants/CommonConstants.cs`
(`IGSTRates` 0–28%, `GSTRates` = half rates for CGST/SGST).

**Preserve verbatim.** Do not reimplement this in TypeScript. The Angular client may compute
an optimistic preview, but the server value must always win. See
[`decisions/ADR-003-react-stack.md`](../decisions/ADR-003-react-stack.md).

## Reporting subsystem

Two independent mechanisms, both server-side and both reusable over HTTP unchanged.

### 1. Printed documents — FastReport

`Services/ReportViewer/ReportService.Generate_Report(int id, string fileName, string parameter, bool cancel, string screenName, string procedureName)`
→ `byte[]` PDF.

Flow (Confirmed):
1. Resolve tenant; require a connection string.
2. Locate `.frx` at `{reportRoot}/{tenant.Hostname}/{fileName}`, else
   `{reportRoot}/default/{fileName}`.
3. Register `MsSqlDataConnection`; inject the **tenant connection string** into the report.
4. Require a `Sp_Print_CompanyDetails` data source and a data source named after
   `procedureName`; bind `@{parameter} = id`.
5. Apply `PrintSetting` rows for the screen: watermark, logo, ISO logo, number of copies,
   copy name.
6. Export via `PDFSimpleExport`.

104 `.frx` templates exist under `wwwroot/templates/` in 5 folders: `default`,
`acucom.bhargavisofttech.co.in`, `sns.bhargavisofttech.co.in`,
`srinuenggind.bhargavisofttech.co.in`, `sharadaelectrou1.bhargavisofttech.co.in`.

**Consequence:** reports render directly from the database, bypassing the service layer
entirely. An Angular frontend needs only an endpoint returning the PDF bytes. **Nothing about
reporting needs to be rebuilt.**

### 2. List/analysis reports — stored procedures

`ReportExecutor.ExecuteAsync<T>(string procedureName, params SqlParameter[])` builds
`EXEC dbo.{procedureName} @p1, @p2…`, runs `FromSqlRaw` with a **300-second** command
timeout, returns `AsNoTracking()` list of keyless entity type `T`.

Keyless result types are registered as `DbSet`s on `ApplicationDbContext`
(e.g. `LabourTrackSummaryVM`, `PoPendingDetailsVM`) — a pragmatic EF idiom.

**94 distinct `Sp_*` procedure names are referenced from C#/Razor. Only 13 have DDL in
`Existing Store Procedures/StoredProcedures/` — and only **12** of those 13 are actually
called by the application (see R-04). The other 82 exist only inside the live
tenant databases. This is risk **R-04** and must be resolved before any environment can be
rebuilt from source. See [`risks/technical-debt-register.md`](../risks/technical-debt-register.md).

## Integrations with external systems

| Integration | Endpoint(s) | Code |
|---|---|---|
| GST **e-Invoice** (IRN generation) | `https://www.alankitgst.com/eInvoiceGateway/eivital/v1.04/auth`, `…/eicore/v1.03/Invoice`; `https://developers.eraahi.com/eInvoiceGateway/…` | `E_Invoice/E_InvoiceHelper/EinvoiceApiHelper.cs`, `BusinessLayer/.../EinvoiceDatabaseService.cs` (2,136 LOC) |
| GST **e-Way Bill** | `https://newewaybill.alankitgst.com/ewaybillgateway/v1.03/{auth,ewayapi}`; `https://developers.eraahi.com/api/ewaybillapi/v1.03/…` | `E_Invoice/EwayHelper/EwayAPIHelper.cs`, `EWayDatabaseService.cs` |
| **IFSC bank lookup** | `https://ifsc.razorpay.com/` | `BusinessLayer/BusinessService/MasterService/AccountsService/BankService.cs` |
| **SMTP email** | per-user config on the `User` entity (`EmailServerName`, `EmailPortNo`, `EmailAppPassword`) | `User.cs` fields; senders in Accounts services |
| **Biometric attendance import** | Excel file, mapped by `BiometricExcelSetting` | `BusinessLayer/.../AttendanceService` |

Two gateway vendors (Alankit and eRaahi) are both wired, with `IEncrypt` /
`ILicenseProductKey` abstractions and BouncyCastle for payload crypto. **Inferred:** the
active gateway is selected by configuration/tenant, not by build. Which one each tenant
uses is **Unknown** — see [`open-questions.md`](../open-questions.md) Q-04.

## Logging and monitoring

`FileLoggingService` writes plain text files under `{AppContext.BaseDirectory}/App_Data/Logs`:

- `UserLogs/{yyyy-MM-dd}_User_{UserName}.txt` — timestamp, username, machine, IP, screen,
  action, info
- `DeveloperLogs/` — INFO / ERROR with stack traces

There is **no** structured logging, no correlation IDs, no log aggregation, no metrics, no
health checks, no APM. `Microsoft.Extensions.Logging` is configured only at default levels.
**Confirmed.**

The user-action log is a genuine audit trail with business value (who did what, on which
screen) and should be preserved as a capability — but re-implemented against a proper sink
when the API layer is built. See [`migration/migration-strategy.md`](../migration/migration-strategy.md) Phase 5.

## Error handling

- Services: `try/catch` → `LogDeveloperError` → `throw new Exception("…", ex)` (message
  loss is common; the inner exception is preserved).
- Delete guards return `(bool, string message)` tuples with user-facing text — these
  messages are effectively part of the product's UX and must survive the migration.
- Blazor Server: `UseExceptionHandler("/Error")` for non-dev; `DetailedErrors = true` is
  set unconditionally in `AddServerSideBlazor` — a production information-disclosure issue.
- API: no exception-handling middleware, no `ProblemDetails` standardisation. **Gap.**

## Composition root

**Confirmed (M2-B07, 2026-08-19.** On branch `migration/M2-B07-add-vsmart-domain`; not yet
merged to `master`.)

There is **one** composition root for the domain graph:

```
V.SMART/V.SMART.Shared/DependencyInjection/ServiceCollectionExtensions.cs

    public static IServiceCollection AddVSmartDomain(
        this IServiceCollection services, IConfiguration configuration)
```

It registers `MasterDbContext`, `ITenantProvider`/`ITenantDbContextFactory`, the
tenant-resolved `ApplicationDbContext` **as a factory delegate** (never `AddDbContext` — that
would break database-per-tenant isolation), AutoMapper via the `MappingProfileMarker` assembly
scan, `AddScoped(typeof(IRepository<>), typeof(Repository<>))`, `IUnitOfWork`, the
cross-cutting services (`CurrentUserService`, `ILoggingService`, `ForeignKeyUsageChecker`,
`UserSession`, `ExcelExportService`, `IExcelTemplateService`) and the ~240 business services
and repositories.

All three hosts call it exactly once: `V.SMART/V.SMART.Api/Program.cs:87`,
`V.SMART/V.SMART.Web/Program.cs:212`, `V.SMART/V.SMART/MauiProgram.cs:216`. Before this, the
Blazor and MAUI hosts registered the same graph independently — 242 and 243 registrations —
and had drifted (KB-060 R-26); the API registered exactly one business service.

### The host seams — deliberately *not* in `AddVSmartDomain()`

Each host supplies a different implementation, so registering them centrally would erase the
seam:

| Seam | Web | MAUI | API |
|---|---|---|---|
| `IPathProvider` | `WebPathProvider` (Scoped) | `DesktopPathProvider` (Scoped) | **none** — M2-B08 |
| `IFileUploadService` | `WebFileUploadService` (Scoped) | `MauiFileUploadService` (Scoped) | **none** — M2-B06 |
| `IFileOpener` | `WebFileOpener` (**Scoped**) | `DesktopFileOpener` (**Singleton**) | **none** — M2-B06 |
| `AuthenticationStateProvider` | `CustomAuthStateProvider` | `CustomAuthStateProvider` | `ApiAuthStateProvider` |
| `IJSRuntime` | framework | framework | **none** (not a web-API concept) |
| `HttpClient` / `IHttpClientFactory` | `AddHttpClient()` | bare scoped `HttpClient` | `AddHttpClient()` only |

The `IFileOpener` Scoped-vs-Singleton divergence is pre-existing and was preserved, not
normalised — see KB-060 R-26.

### The consequence for `V.SMART.Api`

Six registrations in `AddVSmartDomain()` remain **unresolvable in the API** until M2-B06 and
M2-B08 supply the missing seams — expected, not a defect: `ReportService`, `IUserService`,
`IGSTITCService`, `IUserThemePreferenceService`, `ICompanyService`, `IItemService`. Every
other registration resolves.

That is enforced, not asserted: `tests/V.SMART.Shared.Tests/DependencyInjection/AddVSmartDomainTests.cs`
builds the graph with test doubles for the seams and calls
`BuildServiceProvider(validateScopes: true, validateOnBuild: true)`. Because of the six gaps
above, the API host cannot itself run with `ValidateOnBuild = true` yet.

**Rule for adding a service.** One line, in `ServiceCollectionExtensions.cs`, in the `#region`
matching its folder under `BusinessLayer/BusinessService/`. Never in a host's `Program.cs`
unless it is a host seam.
