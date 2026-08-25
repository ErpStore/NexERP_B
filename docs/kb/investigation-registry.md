---
doc_id: KB-003
title: Investigation Registry
module: meta
status: active
confidence: n/a
last_verified: 2026-08-25
---

# Investigation Registry

**Read this before investigating anything.** If a relevant row is `Complete` and not
stale, reuse its findings and cite the doc_id. Re-investigate only when no entry exists,
the entry is stale, or current code contradicts it.

Statuses: `Complete` · `Partial` (usable, with stated gaps) · `In Progress` · `Not Started`.

## Completed investigations

| ID | Topic | Status | Evidence (paths examined) | Findings | Verified |
|---|---|---|---|---|---|
| INV-001 | Solution structure, projects, hosting model | Complete | `*.sln`, 4 `*.csproj`, `V.SMART.Web/Program.cs`, `MauiProgram.cs`, `V.SMART.Api/Program.cs` | [KB-010](architecture/system-overview.md) | 2026-08-12 |
| INV-002 | Backend layering, repository/UoW, service conventions | Complete | `Repository/Repository.cs`, `Repository/UnitOfWork.cs`, `BusinessLayer/**`, `Services/**` | [KB-011](architecture/backend-architecture.md) | 2026-08-12 |
| INV-003 | Data model, DbContexts, entities, migrations, seed data | Complete | `Data/ApplicationDbContext.cs` (196 DbSets), `Data/MasterDbContext.cs`, `Data/**`, `Migrations/` | [KB-012](architecture/database-architecture.md) | 2026-08-12 |
| INV-004 | Authentication, screen rights, approval authority | Complete | `Authentication/Custom AuthenticationStateProvider.cs`, `Repository/MasterRepository/Admins/UserRepository.cs`, `Shared/BaseUserRightsComponent.cs`, `Shared/RightsHelper.cs`, `Data/Master/Admin_Module/*`, `V.SMART.Api/Auth/*` | [KB-013](architecture/auth-and-permissions.md); server-side reproduction spec'd in [KB-105](architecture/server-side-authorization-spec.md) *(M2-A01-01, 2026-08-18 — re-verified against current code, **no contradiction with KB-013 found**; status and Verified date deliberately unchanged)* | 2026-08-12 |
| INV-005 | Multi-tenancy resolution and isolation | Complete | `Services/MultiCompanyService/*`, `Data/TenantInfo.cs`, `wwwroot/config/tenant.json` | [KB-014](architecture/multi-tenancy.md) | 2026-08-12 |
| INV-006 | Existing UI: routing, layout, components, `@code` density | Complete *(amended 2026-08-19 by M2-C04-01 — theme persistence; amended again 2026-08-23 by M2-C04-01 — how PrimeNG 22 consumes the token layer, the answer to Q-67; amended 2026-08-23 by M2-C04-02 — the DataAnnotations to Angular validator mapping surface, and the cross-field caveat; see below)* | `Routes.razor`, `Layout/NavMenu.razor` (888 LOC), `Components/` (22), `Pages/` (333 files, 440 routes), measured `@code` share | [KB-015](architecture/frontend-architecture-existing.md) | 2026-08-12 |
| INV-007 | Module inventory and inter-module dependency graph | Complete | `NavMenu.razor`, `Data/` folders, `BusinessLayer/` folders, `Ref*SubId` FK scan | [KB-020](modules/module-inventory.md) | 2026-08-12 |
| INV-008 | Existing API surface | Complete *(re-verified 2026-08-21 by M2-B01 — **route surface changed**, see the amendment below)* | `V.SMART.Api/**` (2 controllers, 6 endpoints) | [KB-040](api/api-overview.md) | 2026-08-21 |
| INV-009 | Reporting: FastReport + stored procedures | Complete *(amended 2026-08-12)* | `Services/ReportViewer/ReportService.cs`, `ReportExecutor.cs`, `wwwroot/templates/` (104 `.frx`), `Existing Store Procedures/` (13 `.sql`, of which only **12** are called → gap is **82**, not 81). **Scoped** name-extraction command, since the unscoped one now returns 111 by matching this KB's own prose: `grep -rhoE "Sp_[A-Za-z0-9_]+" --include=*.cs --include=*.razor --exclude-dir=obj --exclude-dir=bin V.SMART \| sort -u` | [KB-011](architecture/backend-architecture.md#reporting-subsystem), [ADR-005](decisions/ADR-005-reporting-and-printing.md), R-04 | 2026-08-12 |
| INV-010 | External integrations (e-Invoice, e-Way, IFSC, SMTP, biometric) | Complete | `E_Invoice/**`, `EinvoiceDatabaseService.cs`, `EWayDatabaseService.cs`, `BankService.cs`, URL scan | [KB-011](architecture/backend-architecture.md#integrations-with-external-systems) | 2026-08-12 |
| INV-021 | Angular pilot: scope and value | Complete | `frontend/vsmart-erp/src/**`, `package.json` | [KB-015](architecture/frontend-architecture-existing.md#the-angular-19-pilot-frontendvsmart-erp) | 2026-08-12 |
| INV-022 | Background jobs / scheduled tasks | Complete | grep for `IHostedService`, `BackgroundService`, `PeriodicTimer`, Hangfire, Quartz — **none exist** | [KB-010](architecture/system-overview.md#background-processing) | 2026-08-12 |
| INV-023 | Testing and CI | Complete *(historical — **superseded 2026-08-19**: the first test project landed with M0-12-01. Read this row as a statement about 2026-08-12, not about now)* | no test project in `.sln`; `.github/` has no workflows | [KB-010](architecture/system-overview.md#testing), R-05 | 2026-08-12 |
| INV-031 | Test-harness feasibility: hosting `ApplicationDbContext` in a test process | Complete | `V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs`, `.../Data/HumanResource/Attendance/Attendance.cs`, `.../Data/Inspection/**`, `.../Data/Master/Inventory_module/Item.cs`, `.../Data/Inventory(Stock)/StockAdd.cs`, `.../Services/MultiCompanyService/{I,}TenantDbContextFactory.cs`, `.../Services/CurrentUserService.cs`, plus **executed** spikes under EF Core 9.0.5 InMemory and Sqlite | **InMemory works, Sqlite does not** — full findings below | 2026-08-19 |
| INV-036 | Testing an EF-backed business service through `IUnitOfWork` — the general recipe | Complete | `V.SMART/V.SMART.Shared/Repository/Repository.cs:70-83,103-115,137-172,323-326`, `.../Repository/InventoryStockRepository/StockAddRepository.cs:20`, `.../Repository/IRepository/IUnitOfWork.cs:84,270-272`, `.../Repository/UnitOfWork.cs:485,672,793`, plus the executed suite in `tests/V.SMART.Shared.Tests/Services/StockManagerServiceCharacterisationTests.cs` (36/36 green, run twice) | **Recipe (Confirmed, M0-13):** mock `IUnitOfWork`, configure only the repository properties the service under test touches plus `SaveAsync`, and back each with the **real** repository over **one** real `ApplicationDbContext` (InMemory — INV-031). Two non-obvious constraints, each of which silently yields a test that asserts nothing if missed: **(1) `Repository<T>` never persists** — `CreateAsync` only calls `_dbSet.AddAsync` (`:70-83`), `UpdateAsync` only `_dbSet.Update` (`:103-115`), `DeleteAsync` only `_dbSet.Remove` (`:137-172`) — so the mock's `SaveAsync` **must** forward to `context.SaveChangesAsync()`, or nothing the service does reaches the store. **(2) One context instance for everything** — `GetQueryable()` returns `_dbSet.AsQueryable()` (`:323-326`), a *tracking* query, so the entities a service mutates are the instances a test asserts on, but only when repositories and assertions share one context; `TestDbContextFactory.CreateContext()` returns a fresh context per call over a shared database name, so call it once per test. Making the mock's `SaveAsync` throw on the *n*th call is also the only practical way to drive a non-`InvalidOperationException` into a service's `catch`, which is how the exception-translation paths were pinned. **Rejected alternative (Confirmed):** the real `UnitOfWork` over a fake `ITenantDbContextFactory` — its constructor (`UnitOfWork.cs:485`; `StockAdds` at `:672`, `SaveAsync` at `:793`) instantiates roughly 190 repositories per test and additionally needs `IPasswordHasher<User>`. **Negative results:** the InMemory provider cannot pin sort tie-breaking (it sorts stably, SQL Server does not), SQL null-equality semantics, or `[Precision]` rounding — keep test data clear of all three. | [KB-030](business-rules/business-rule-inventory.md), `tests/V.SMART.Shared.Tests/Infrastructure/StockScenarioBuilder.cs` | 2026-08-19 |
| INV-035 | Default administrator seed: removal mechanics and per-tenant impact | Complete | `V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs:1136-1148` (pre-change), `.../Migrations/ApplicationDbContextModelSnapshot.cs`, `.../Migrations/20260217110637_InitialCreate.cs:7196-7200,7232-7236,7562`, `.../Repository/MasterRepository/Admins/UserRepository.cs:34-49`, `.../Pages/.../Login.razor:345-349`, `.../UserRights_Pages/UserRights.razor:82-215`, `.../Identity_Pages/RegisterUpsert.razor:426,444`, `.../AdminService/UserRightService.cs:32-87`, `.../Shared/RightsHelper.cs:7-20`, `V.SMART.Api/Controllers/AuthController.cs:47`, `.../Data/MasterDbContext.cs:5-9`, `.../Data/TenantInfo.cs:3-9`, plus **executed** `dotnet ef migrations add` probes (three, all reverted) | **Executed, not reasoned.** (1) Removing the `HasData` row **does** scaffold `DeleteData(table:"Users", keyColumn:"UserId", keyValue:1)` — and the generated `Down()` re-`InsertData`s the published hash. (2) It would **not** be blocked: all three FKs to `Users` are `Cascade`, so it would succeed and silently cascade-delete the user's rights/authorities/theme rows — the task file's `Restrict` premise was **wrong**. (3) A non-constant `HasData` value is **accepted** by the tooling but useless: two consecutive `migrations add` runs with `Guid.NewGuid()` emitted two different `UpdateData` literals, so "random per deployment" is impossible through `HasData`. (4) No must-change-password field exists. (5) `UserId == 1` is an undeclared superuser (R-40). Negative results and full detail below. | [KB-104](security/default-admin-removal-runbook.md), R-09, R-40 | 2026-08-19 |
| INV-029 | Version-control state, repository visibility, and toolchain/build baseline | Complete *(visibility finding corrected 2026-08-12 by INV-034 — see below; do not cite INV-029 alone for visibility. **Solution-build gap closed 2026-08-17 by M0-15** — see below.)* | `git ls-remote`, `git log`, `git status --porcelain`, `git grep -l "<secret>" HEAD`, `dotnet --list-sdks`, `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj`, `dotnet build V.SMART/V.SMART.Web/V.SMART.Web.csproj`, `dotnet build NexGen-ERP---2025-master.sln`, `dotnet workload list` | [KB-080 §6](execution/README.md#findings-from-this-planning-pass-that-changed-m0), [KB-083](execution/prompt-template.md#verified-repository-commands), [KB-086](execution/M0-15-build-baseline.md) | 2026-08-18 |
| INV-039 | DI composition drift across `V.SMART.Api/Program.cs`, `V.SMART.Web/Program.cs`, `V.SMART/MauiProgram.cs`, ahead of `M2-B07` | Complete | All three composition roots read line-by-line 2026-08-19 (task file's own line numbers found stale — shifted by `M0-03-02`/`M0-03-03`): `V.SMART.Api/Program.cs` (125 lines, registers only `ICurrencyService`, no `IRepository<>`); `V.SMART.Web/Program.cs` (660 lines); `V.SMART/MauiProgram.cs` (652 lines, no call to `StartupConfigurationValidator` unlike the other two hosts) | **Drift is ~5x the task file's estimate**: 16 domain registrations diverge between Web and MAUI, not 3 — 8 services (`IContractReviewService`, `IRouteCardService`, `IRouteCardRepository`, `IRouteCardSubRepository`, `IProductionReturnAssyRepository`, `IProductionReturnAssySubRepository`, `IProductionSCNAssyRepository`, `IProductionSCNAssySubRepository`) are registered in MAUI's `MauiProgram.cs` but were missing from `V.SMART.Web/Program.cs`, so `/contractReviewMasterList` and `/routeCardList` and siblings threw a DI resolution error in the Blazor host (reachability of those routes in production not independently confirmed — Unknown). `IFileOpener`'s lifetime already diverges by host (Singleton in MAUI `:274`, Scoped in Web `:267`) — a pre-existing inconsistency (R-26), not something a shared composition root can silently resolve without a decision. `MasterDbContext` is registered in Api and Web via plain `AddDbContext` but **not** in MAUI, which instead reads `ConnectionStrings__MasterDb` from the environment first (M0-03-02 pattern, `MauiAppBuilder.Configuration` has no env-var provider by default). Host-coupled `V.SMART.Shared` services that a plain `AddVSmartDomain()` cannot safely resolve in the API without a decision: `IPathProvider`, `IFileOpener`, `IFileUploadService` (each host supplies its own implementation) — `ReportService`, `IUserService`, `IGSTITCService`, `IUserThemePreferenceService` transitively depend on one of these and stay unresolvable in `V.SMART.Api` even after this task, which is expected, not a defect. Authorization DI is explicitly out of scope per [KB-105 §6.2](architecture/server-side-authorization-spec.md) — it belongs in a sibling `AddVSmartApiAuthorization()` in the API host, never inside `AddVSmartDomain()`. **CORRECTIONS (Confirmed, `M2-B07` attempt 2, 2026-08-19 — the first pass above was measured by eye; this pass normalised and set-differenced every registration call in both files):** (1) **The MAUI-only set is 6, not 8.** `IContractReviewService` and `IRouteCardService` are registered in the Blazor host as well — `V.SMART/V.SMART.Web/Program.cs:467` and `:518` — and are merely *duplicated* inside `MauiProgram.cs` (`:364,474` and `:521,523`), which is what made them look one-sided. The dependent claim that `/contractReviewMasterList` and `/routeCardList` threw a DI error in Blazor is therefore **unsupported**; Q-31's premise was corrected accordingly. (2) **The drift is symmetric and the first pass saw only one side:** 7 registrations were Web-only and missing from MAUI — `IAssemblyDefLabourService`, `IEstimateService`, `IJobOrderRepository`, `IJobOrderSubRepository`, `ILabourTrackRepository`, `IPrPoRatingService`, `IToolCribServices`. 13 domain registrations diverge in total, not 16. (3) **Exact counts:** Web 242 matched lines → 240 distinct → 239 real (`:253` is commented out); MAUI 243 → 239 distinct; union 249. (4) **The host-coupled list is 6, not 4:** add `ICompanyService` (`CompanyService.cs:27` — `IFileUploadService` *and* a bare `HttpClient`) and `IItemService` (`ItemService.cs:35` — `IFileUploadService`). (5) **Two seams the first pass missed entirely**, both found by running `ValidateOnBuild`: `AuthenticationStateProvider` (taken by `CurrentUserService`, registered per-host in all three, including the API at `Program.cs:89`) and `IHttpClientFactory` (taken by `BankService`, `PaymentsService`, `ReceiptsService`, `AdvaceAdjustmentService`; supplied only by `AddHttpClient()`, which Web had and the API did not). (6) **`IExcelTemplateService` is domain, not host UI** despite sitting among the UI registrations in both former roots — `ExcelTemplateService.cs:27-32` takes only `IUnitOfWork`, `ICommonService`, `CurrentUserService`, `ILoggingService`, and 7 business services inject it. It was moved into `AddVSmartDomain()`. **Negative result:** with `IPathProvider`, `IFileUploadService`, `IFileOpener`, `IJSRuntime`, a bare `HttpClient`, `AuthenticationStateProvider` and `AddHttpClient()` supplied, the *entire* 249-registration graph passes `BuildServiceProvider(validateScopes: true, validateOnBuild: true)` — there is no other hole. | [KB-060 R-26](risks/technical-debt-register.md), [KB-105 §6.2](architecture/server-side-authorization-spec.md), `tests/V.SMART.Shared.Tests/DependencyInjection/AddVSmartDomainTests.cs` | 2026-08-19 |
| INV-040 | How the service layer signals a business-rule refusal, and how the API maps it to `409` | Complete | `V.SMART.Api/Controllers/CurrencyController.cs:64,77,87`; `BusinessLayer/**` swept for refusal shapes: `CanDelete*Async` (79 implementation methods across 61 service files, e.g. `CurrencyService.cs:188`, `SalaryService.cs:206`, `StaffLoanService.cs:233`, `AttendanceService.cs:102`), `CanItemCancel*` (10, e.g. `PuchPoService.cs:151`, `PurchaseGRNService.cs:1064`, `MaterialReqService.cs:825`, `LabourGRNService.cs:1709`, `PurchaseQuoteService.cs:1134`), `ValidateDeleteAsync` (3: `SubConGRNService.cs:1454`, `ProductionReturnAssyService.cs:460`, `ProductionReturnCompService.cs:797`), `ServiceResult` (1, nested in `StoreService.cs:304-313`, used by `UpsertStoreAsync` at `:173`); `ICurrencyService.cs:14-15`; `CurrencyService.cs:77,85,110,114,120,123,129,153,188-211`; `throw new *Exception` counted across `BusinessLayer/**` | **Refusals are signalled by TUPLE RETURN, not by exception.** The pervasive form is the two-element delete guard `(bool CanDelete, string Message)` — **79 methods / 61 files**, spanning HR, Inventory, Master, Labour, Production and Outsourcing — plus three near-identical variants (`(bool CanItemCancel, string Message)` ×10, `(bool IsValid, string Message)` ×3, one nested `ServiceResult`). A returned value is invisible to middleware, so **the controller must do the mapping**. **Decision (M2-A06, binding on all 60–80 future controllers): a controller helper**, `ProblemResults.BusinessRuleProblem(message)` in `V.SMART/V.SMART.Api/Middleware/ProblemResults.cs` — *not* a domain exception type, because `V.SMART.Shared` is live under Blazor Server and must not change, and because exceptions are not a trustworthy refusal channel there (below). The boolean is the signal; the message is passed through untouched (BR-SO-001). **NEGATIVE RESULTS:** (1) the three-element create/update tuple `(bool Success, string Message, T? Entity)` is **unique to `CurrencyService`** (`ICurrencyService.cs:14-15`), added by `b8beb0d` for the API — sibling master interfaces (`ICostCenterService`, `IExpenseService`, `IIncomeService`) expose **no** Create/Update at all, so M2-B03's template must generalise the *delete-guard* tuple, not this one; (2) no `CanDelete…` method returns a bare `Task<bool>`; (3) there is **no domain-exception base type or marker interface** anywhere in `BusinessLayer/**`; (4) grepped `X-Correlation-Id`/`CorrelationId`/`TraceIdentifier` across `docs/`, `V.SMART.Api` and `tests/` before this task — two documentation hits, **zero implementation**. **TRAP for anyone copying the mapping:** `InvalidOperationException` is thrown 1,107 times in `BusinessLayer/**` and carries *both* meanings — a business refusal (`StockManagerService.cs:210` "No available stock to issue.") and an infrastructure fault rethrown from a `catch` (`StockManagerService.cs:345` "Failed to retrieve stock details.") — so no exception→status rule is safe; M2-A06 maps every escaping exception to `500`. Equally, the refusal tuple itself carries non-refusal meanings in two confirmed places (`CurrencyService.cs:197` not-found; `CurrencyService.cs:208-211` a caught fault) → **Q-34** | [KB-040 § Error contract](api/api-overview.md#error-contract-m2-a06), BR-SO-001, `V.SMART/V.SMART.Api/Middleware/ProblemResults.cs` | 2026-08-20 |
| INV-041 | How does `sort` reach a business service whose ordering is hardcoded, without rewriting 134 services? | Complete | `SearchWithDynamicFilterAsync` declared **134** times with `Task<` across `V.SMART/V.SMART.Shared/BusinessLayer/` (re-measured 2026-08-20, matches the 2026-08-12 count); **67** `public static class *FilterBuilder` across `V.SMART.Shared/`; `CurrencyService.cs:34-100` (search), `:206` (`_ => query`), `:180-209` (`CurrencyFilterBuilder`), `:279` (`OrderByDescending(x => x.CurrId)`), `:80-81` (`Skip`/`Take`); `CurrencyList.razor:344-348` (the only production caller, named arguments), `:111,130,136` (QuickGrid sorting one page), `:758-760` (the `Status` filter key), `Data/Master/Accounts_Module/Currency.cs:9-29` | **No service anywhere takes a sort parameter** (grepped `BusinessLayer/` for `sort`/`sortBy`/`sortColumn`/`orderBy` service parameters — **0 hits**); ordering is hardcoded per service. **Decision (M2-B02, recorded as [ADR-002 §2a](decisions/ADR-002-rest-api-layer.md)): an additive 4-argument overload** `SearchWithDynamicFilterAsync(int, int, Dictionary<string, object>?, string? sort)`, with the 3-argument member delegating to it — compiler-checked, and the only production call site uses 3 named arguments so it is untouched. The 133 other sites convert per module (KB-080 §10 step 06), not in one sweep. **Passing sort through the filter dictionary was rejected** because every `*FilterBuilder.ApplyFilter` ends in `_ => query` (`CurrencyService.cs:206`), so an unrecognised key is silently ignored and the request answers 200 while sorting nothing. **That failure mode is already live in production:** `CurrencyList.razor:760` sets a `Status` filter key that `CurrencyFilterBuilder` has no case for and the `Currency` entity has no column for, so that dropdown filters nothing, silently (→ Q-36). **Sorting in the controller after materialisation was rejected** because `Skip`/`Take` run first (`:80-81`), so it sorts one page — the wrong rows. **NEGATIVE RESULTS:** (1) the 134 declarations are signature-uniform — a scan for a 4th parameter after `filters` returns nothing, so one overload shape fits all; (2) `frontend/nexgen-web/src` has no `PagedResult`/`totalCount`/`pageNumber` — no existing client constrains the shape; (3) an unused, unreferenced `PagedResult<T>` already exists at `V.SMART.Shared/ViewModels/RejectionMasterVM.cs:33-40` (with `TotalPages`) — a namespace-collision hazard, not the API type; (4) **there was no legacy server-side sort to preserve** — `CurrencyList.razor:111` binds QuickGrid to `CurrencyVMs.AsQueryable()`, i.e. it sorts the current page only, so server-side sort is an improvement, not a regression risk | [ADR-002 §2a](decisions/ADR-002-rest-api-layer.md), [KB-011 § Business service conventions](architecture/backend-architecture.md#business-service-conventions-observed-consistent), [KB-040](api/api-overview.md) | 2026-08-20 |
| INV-025 | Delete-guard audit — every `(bool CanDelete, string Message)` guard for the R-08 copy-paste pattern | **Complete** | **Population (Confirmed, 2026-08-21):** `public async Task<(bool CanDelete, string Message)> <any name>` = **79** implementations across **61** files, all under `V.SMART/V.SMART.Shared/BusinessLayer/`; the `CanDelete`-prefixed subset is **64**; the wider `(bool <any>, string Message)` family is **93**. `bin/`/`obj/` excluded via `--exclude-dir` — raw counts differ without that. Read in full: all 79 bodies (two independent `awk` scanners plus manual re-reading of every method cited by line); `IBusinessService/**` (declarations); `Pages/**` and `V.SMART.Api/Controllers/**` (call sites); `db/stored-procedures/*.sql`; `Existing Store Procedures/`. **Negative results (all Confirmed):** **zero** guards outside `BusinessLayer/`; interface declarations map **1:1** to implementations (both set differences empty — no declared-but-unimplemented, none implemented-but-undeclared), the sole mismatch being a *commented-out* declaration at `IBusinessService/IOutSourcingService/ISubContractSCNService/ISubConSCNService.cs:50`; the near-neighbour name sweep found **zero** hits for `IsDeletable`, `CanBeDeleted`, `CheckDelete`, `AllowDelete`, `IsDeleteAllowed`, `EnsureDeletable`, `VerifyDelete`, `PreDelete`, `CanDrop`; **zero** guards return `(true, …)` from a `catch` (the fail-open shape does not exist); exactly **one** ternary-form guard and **zero** `switch`-form guards, so the detector's blind spots are closed; of 78 committed stored procedures only one contains `DELETE` and it is a report procedure. **Headline:** the R-08 defect class is **eradicated** — one surviving instance across all 93 guards (`MfgPoService.cs:613-615`), and it was already recorded in KB-060. **What the audit actually found:** 14 guards with no caller; **3** that can never refuse (a 4th inert after one check), of which the null-unsafe subset is recorded in KB-061 §5.3a; 29 service files with an unguarded delete; 77 of 79 running outside the delete transaction. | [KB-061](risks/delete-guard-audit.md) — full 79-row inventory including every `Correct` verdict. Risks **R-60…R-64** in [KB-060](risks/technical-debt-register.md); questions **Q-60…Q-64** in [KB-004](open-questions.md). | 2026-08-21 |
| INV-042 | What `role` means in `GET /api/v1/me`, and which `User`/`UserAuthority` data belongs in the bootstrap response | Complete | `V.SMART/V.SMART.Api/Auth/JwtTokenService.cs:29-35`; `V.SMART/V.SMART.Api/Controllers/AuthController.cs:32-37,60-67`; `V.SMART/V.SMART.Shared/Services/CurrentUserService.cs:68-75`; `V.SMART/V.SMART.Shared/Data/Enum/UserRole.cs:3-7`; `V.SMART/V.SMART.Shared/Layout/NavMenu.razor:36,148,462-464`; `V.SMART/V.SMART.Shared/Pages/Home.razor:240`; `V.SMART/V.SMART.Shared/Data/Master/Admin_Module/User.cs:34,36,66,68,75,124`; `V.SMART/V.SMART.Api/Authorization/ScreenRightSet.cs:4-10,46-61` | Role is the **JWT `ClaimTypes.Role` claim**, not a `User` row read and never `CurrentUserService.GetUserRoleAsync()` (R-18). `ERPAdmin` (R-31) is not propagated. `UserAuthority` and all four `User` UI flags are **deferred**. Full findings below | 2026-08-20 |
| INV-046 | What consumes `LogUserAction` today, is audit coverage uniform, and is the `#if ANDROID \|\| WINDOWS \|\| MACCATALYST` `_basePath` branch ever taken? | Complete | `V.SMART/V.SMART.Shared/Services/FileLoggingService.cs`, `ILoggingService.cs`, 494 `LogUserAction` call sites across 202 files, `V.SMART.Shared.csproj`, `dotnet msbuild -getProperty:DefineConstants` on both TFMs, `V.SMART/V.SMART.Api/Program.cs`, `Middleware/CorrelationId.cs`, `DependencyInjection/ServiceCollectionExtensions.cs:308` | [KB-113](architecture/observability.md); see the five findings below | 2026-08-21 |
| INV-050 | **How do the KB-011 business-service method conventions map onto REST verbs, and what happens to the cases that do not map one-to-one?** | Complete *(2026-08-24, M2-B03)* | [KB-011 §Business service conventions](architecture/backend-architecture.md#business-service-conventions-observed-consistent) (INV-002); `V.SMART/V.SMART.Api/Controllers/CurrencyController.cs` (126 lines, read in full); `V.SMART/V.SMART.Api/Controllers/AuthController.cs` (171 lines, read in full); `V.SMART/V.SMART.Api/Contracts/*.cs` (9 files); `V.SMART/.../IAccountsService/ICurrencyService.cs:32,35-36`; `V.SMART/.../ISalesService/IMfgPoService.cs:56,57,59,72,73,78`; `V.SMART/.../SalesService/MfgPoService.cs:346-420` (filter keys); `V.SMART/.../Pages/SalesAndLabour_pages/SalesPo_Pages/MfgPOUpsert.razor:2180,3294,3295,3414` | **The mapping is one-to-one except for three cases, all now decided (KB-114 §9).** (1) **`Upsert…Async(TVm)` — decided: `POST /` and `PUT /{id:int}` both call the one method**, distinguished at the controller: `POST` requires body id `0` (else 400) and answers 201 + `Location`; `PUT` requires route id == body id (else 400) and answers 200. Splitting the service was rejected because `Upsert…Async` is the **dominant** shape — **129 files** under `BusinessLayer/` match `public.*UpsertAsync\|Task<.*> Upsert` (measured 2026-08-24) — and all are live under Blazor Server. The `(bool, string, TVm?)` `CreateAsync`/`UpdateAsync` split `CurrencyController` uses is **unique to `CurrencyService`** (`ICurrencyService.cs:35-36`, INV-040) and is honoured where it exists. **Residual, recorded not guessed:** whether a given `Upsert…Async` *inserts* on an unknown non-zero id is per-service; KB-114 requires it be verified before that resource's `PUT` ships rather than adding a blanket pre-check. (2) **Typeahead `Search…Async(string)` — decided: its own `GET /{resource}/search?q=`, capped, never a mode of the list endpoint**, because the list response is the frozen `PagedResult<T>` and two response shapes on one operation is exactly the drift M2-B10 removes. (3) **`GetNext…NoAsync` is deliberately NOT an endpoint** — document numbering is allocated server-side inside the create transaction (M2-B12); exposing it would let a client allocate a number outside that transaction. Also settled: `CanDelete…Async` is consulted **before** `Delete…ByIdAsync` and a refusal is **409 carrying the message verbatim** (BR-SO-001). (4) **`IsDuplicate…Async` — corrected 2026-08-24, and this is the finding worth not re-deriving.** It is never its own endpoint **and never called from a controller**, but "it is a 409 out of create/update" is true of only *some* services: the family has **43 declarations under 16 distinct names** in `BusinessLayer/`, and enforcement sits in three incompatible places — (a) **inside the service as a refusal tuple**, `CurrencyService.cs:108,152` → clean 409; (b) **inside the service as a `throw`**, `LeadService.cs:206,209` `InvalidOperationException("Leads name already exists.")`, which the API deliberately does not map (`ProblemResults.cs:18-31`) and therefore surfaces as **500**; (c) **only in the Razor page**, `MfgPOUpsert.razor:3745-3753` refuses a duplicate PO while `MfgPoService.cs:985` `UpsertPoAsync` never calls `IsDuplicatePoAsync` (`:964`) — so a `POST` copied naively **creates the duplicate the live Blazor screen refuses**. KB-114 §9 therefore imposes a per-resource verification duty (grep the service *and* the upsert page) and forbids the controller from doing the check itself, because `IsDuplicatePoAsync` returns a bare `bool` and the only message that exists is UX text in the page (`MfgPOUpsert.razor:3750`) — composing a 409 title in the controller would breach ADR-002 §4. Shape (b)'s status mapping is **unresolved and owner-owned**: [Q-81](open-questions.md). **Negative results:** there is **no service convention for bulk create/update/delete**, so the template defines none; there is **no `PATCH`-shaped method** (no service takes a partial VM); there is **no soft-delete convention**; and idempotency keys are deferred to M2-B12-03, so no per-controller scheme is to be invented. **Stale-premise finding:** the "two different 400 body shapes in `CurrencyController`" that M2-B03's own task file names as its worked counter-example **no longer exists** — M2-A06 closed it (R-24 CLOSED 2026-08-20) and the controller now uses only `ProblemResults` helpers. The anti-pattern is documented by name in KB-114 §6 so it stays greppable. | [KB-114](api/controller-conventions.md) — Confirmed | 2026-08-24 |
| INV-051 | **Which TypeScript client generator, scored against the real committed \`api/openapi.json\`, and what does a \`decimal?\` become on the wire?** | Complete *(2026-08-24, M2-B10)* | \`api/openapi.json\` (18 operations, 6 controllers, produced by \`dotnet swagger tofile\`); \`frontend/nexgen-web/src/app/core/api/generated/\`; \`.github/workflows/ci.yml\` job \`api-contract\`; four generators installed and actually run against that document | **Chosen: \`ng-openapi-gen\` 1.0.5.** The only candidate emitting Angular \`HttpClient\` services, which is what puts every call through the SPA's auth interceptor: \`currencies.getCurrencies({ pageNumber: 1, pageSize: 20 })\` returns \`Observable<CurrencyVMPagedResult>\`, schema names survive verbatim (\`camelizeModelNames: false\`), \`ProblemDetails\` is a named interface with \`title?: string \| null\` so ADR-002 §4's verbatim 409 message reaches the UI, and output is byte-identical on repeated runs. **Rejected, with reasons: \`@hey-api/openapi-ts\` 0.99.0** — best error typing of the four (per-status unions) but fetch-only (\`@hey-api/client-angular\` does not exist, npm 404), so it bypasses Angular's interceptor chain; also pre-1.0. **\`openapi-typescript\` 7.13.0** — types only, no methods, so every call site would be hand-written against a path string. **\`@openapitools/openapi-generator-cli\`** — needs a JRE; \`java\` is not installed on this workstation (\`command not found\`), so it would add a Java dependency to every developer machine and to CI. **Costs of the choice, recorded not hidden:** every JSON operation is emitted twice (\`getCurrencies\` and \`getCurrencies$Plain\`), because MVC's formatters genuinely accept \`text/plain\`/\`text/json\` and the document says so truthfully; and \`HttpErrorResponse.error\` is \`any\`, to be narrowed once in the wrapper layer. **The decimal answer, measured with a temporary probe property and then reverted:** \`decimal?\` → \`{"type":"number","format":"double","nullable":true}\` → \`probeNullableMoney?: number \| null;\`; non-nullable \`decimal\` → \`probeMoney?: number;\` (optional, because Swashbuckle does not mark value types \`required\`). Money crosses the boundary as an IEEE-754 double — **flagged to [M2-C10](execution/tasks/M2-C10.md), deliberately not resolved here**. **Negative results:** \`dotnet swagger tofile\` DOES work in this repository (the yes/no the task asked for), but only with FOUR environment variables, and its failure without them is the misleading "a type named 'Startup' could not be found"; nothing in startup opens a database connection, so placeholders suffice; and before this task there was no \`api/\` directory, no \`.config/dotnet-tools.json\` and no generator dependency anywhere. | [KB-112](api/generated-client.md) — Confirmed | 2026-08-24 |
| INV-028 | Row-level scoping via `User.StateCodesCsv`, and the account gates it sits beside | Complete *(performed 2026-08-20 by M2-A08)* | `Data/Master/Admin_Module/User.cs`, `BusinessLayer/BusinessService/LeadService/LeadService.cs`, `.../ILeadService.cs`, `Pages/SalesAndLabour_pages/Leads_Pages/LeadsList.razor`, `.../LeadsUpsert.razor`, `Pages/Master_Module_pages/Identity_Pages/Login.razor`, `.../QrLogin.razor`, `.../RegisterUpsert.razor`, `Repository/MasterRepository/Admins/UserRepository.cs`, `BusinessLayer/.../UserService.cs`, `V.SMART.Api/**` — **plus four negative greps, listed below** | [KB-108](architecture/row-scope-and-account-gates.md) | 2026-08-20 |
| INV-052 | **Credential usage inventory — every location each exposed credential is consumed from, including the ones that are not files** | Complete | `V.SMART/V.SMART.Web/appsettings.json`, `V.SMART/V.SMART.Api/appsettings.json`, `V.SMART/V.SMART.Shared/Data/MigrationData/*`, `V.SMART/V.SMART/MauiProgram.cs`, `V.SMART/V.SMART.Shared/Data/TenantInfo.cs`, `V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantDbContextFactory.cs`, `V.SMART/V.SMART.Shared/Services/StartupConfigurationValidator.cs`, `V.SMART/V.SMART.Shared/E_Invoice/LicenseProductKey.cs`, `.../EInvoiceAPIService/{EinvoiceDatabaseService,EWayDatabaseService}.cs`, `V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs`, `docs/CONFIGURATION.md`, `.github/workflows/ci.yml`, `git grep`/`git log -S` over `HEAD` and history | [`docs/runbooks/credential-rotation.md`](../runbooks/credential-rotation.md) §2. **Headline: the exposure has moved out of `V.SMART/` and into `docs/kb/`.** Zero credential literals remain under `V.SMART/` (M0-03-01/-02), but at `HEAD` the SA password is in **5** KB files, the production password at `docs/kb/risks/technical-debt-register.md:44`, and the default `Jwt:Secret` in **4** KB files — so **the task file's recorded negative result, "the JWT signing secret is not present in committed history", is FALSE on current `master`** (M0-00 carried it into `docs/`). Raised as **Q-84**. History is **353** commits, not one: `git log -S` over `V.SMART/` gives `c12c5b2` (introduced), `a43e18d`, `e6e5295` (removed). **C-3 mechanism re-confirmed:** per-tenant connection strings live in the master database's `Tenants` table, not in any file — `TenantInfo.cs:8`, dereferenced without a null guard at `TenantDbContextFactory.cs:19`, so a botched rotation is an opaque 500. **C-5 Unknown answered:** the gateway username/password are AES-encrypted inside `Companies.APIEinvoiceLicenseKey` (`EinvoiceDatabaseService.cs:152`, `EWayDatabaseService.cs:85`; unwrapped by `LicenseProductKey.GetUserName()` `:113` / `GetUserNameEway()` `:127`), so rotating them is a per-tenant data change, not a config change — and a malformed key calls `Environment.Exit(1)` (`LicenseProductKey.cs:123,138`; `EinvoiceDatabaseService.cs:182`), i.e. it kills the host, not just e-Invoicing. **New credential C-7:** the AES key and IV protecting every licence key are hardcoded at `LicenseProductKey.cs:28-29`, tracked and public — vendor-owned, not rotatable by this project. **Machine guard, so the runbook cites enforcement rather than instruction:** `StartupConfigurationValidator.cs:40-64,66-71,73,132` refuses known-old connection strings and any JWT secret under 32 UTF-8 bytes, called from `V.SMART.Api/Program.cs:28`, `V.SMART.Web/Program.cs:187`, `Auth/JwtTokenService.cs:24`. **Negative results, recorded so nobody re-searches:** no `${{ secrets.* }}` in `.github/workflows/ci.yml` (only the comment at `:12`); no `.pubxml`/`PublishProfiles`/`web.config`/Docker/`.env` tracked; no Key Vault or secret-manager client under `V.SMART/`; both `appsettings.Development.json` hold a `Logging` section only; `V.SMART/V.SMART/appsettings.json` holds only `UpdateSettings:SetupPath`; `V.SMART.Api/wwwroot/config/tenant.json:2-5` is a name and a UNC path, not a credential. **Unknown, not guessed:** the `Tenants` row count and which login each embeds; how many tenants hold a licence key; whether the logins were used from hosts outside this repository (**Q-15**); whether the values were ever pushed from another clone. Each needs production access this session must not have. Repository visibility re-verified public 2026-08-25 by INV-034's method (unauthenticated REST → `200`); a plain `git ls-remote` remains an invalid test. Confidence: **Confirmed** except where marked Unknown. | 2026-08-25 |


## INV-046 (M2-B11, 2026-08-21) — the audit trail, and the dead `#if` branch

Five findings. All **Confirmed** unless stated.

**1. `LogUserAction` has 494 call sites across 202 files, all of them in `V.SMART.Shared`.**
Zero in `V.SMART.Web`, zero in `V.SMART.Api`. By top-level directory: `BusinessLayer` 88 files,
`Pages` 76, `Repository` 35, `Services` 2, `Components` 1. **Consequence for M2-B11 and for
every API task after it: no API endpoint writes an audit record**, because no code reachable
from `V.SMART.Api`'s request pipeline calls `LogUserAction`. Restructuring the sink does not
change that; only adding call sites would, and M2-B11 was explicitly forbidden from adding any.

**2. Negative result — audit coverage is patchy, not uniform.** Seven of the eighteen module
folders under `V.SMART/V.SMART.Shared/Pages/` contain **zero** `LogUserAction` call sites:
`CashFlow_Pages`, `Dashboard_Pages`, `Home_Pages`, `HumanResource_Pages`,
`InstantSearch_Pages`, `Inventory(Stock)_Module_Pages`, `Report_Module_Pages`.
`HumanResource_Pages` is **not** unaudited overall — its business services log instead
(`AppointmentLetterService.cs:106,141,491`; `AttendanceService.cs:163,415`;
`StaffLoanService.cs:592,763`; `OfferLetterService.cs:106,141,492`; `SalaryService.cs:267`).
Note `StaffLoanService.cs:650,727` are **commented out** — dead logging. The other six show no
logging on any path grepped. *Caveat, stated so it is not overclaimed:* this is "no
`LogUserAction` text in that folder tree", not an exhaustive call-graph proof.
**"Preserve the audit trail" therefore means "preserve what exists", which is less than a
reader of R-23 would assume.**

**3. `additionalInfo` is free text and can be multi-line.** e.g. `PaymentsService.cs:1360-1367`
builds `additionalInfo: $"Payments Id: {payment.PaymentId}\n{changes}"` — an interpolated diff.
It is the one audit field that could plausibly carry a pasted connection string, which is why
M2-B11 routes it through `SensitiveDataRedactor` and the other five fields not.

**4. The `#if ANDROID || WINDOWS || MACCATALYST` branch (`FileLoggingService.cs:11-16`) is DEAD
in every build of `V.SMART.Shared` — `_basePath` is never null through it.** This answers the
question M2-B11 asked to be settled either way, and it is now **Confirmed**, not Inferred:

```
dotnet msbuild V.SMART/V.SMART.Shared/V.SMART.Shared.csproj -getProperty:DefineConstants \
  -p:TargetFramework=net9.0-windows10.0.19041.0   →  TRACE;DEBUG
dotnet msbuild V.SMART/V.SMART.Shared/V.SMART.Shared.csproj -getProperty:DefineConstants \
  -p:TargetFramework=net9.0                       →  TRACE;DEBUG
```

Neither TFM defines `WINDOWS`, `ANDROID` or `MACCATALYST`. Those symbols come from
`Microsoft.NET.Sdk.Maui`, which activates on `<UseMaui>true</UseMaui>`;
`V.SMART.Shared.csproj:1` uses `Microsoft.NET.Sdk.Razor` and never sets it, and MSBuild
evaluates a `ProjectReference`'s properties from **its own** csproj, not the consuming MAUI
project's. So the `#else` branch always compiles and `_basePath` is always
`AppContext.BaseDirectory/App_Data`. **The MAUI host does log; it logs to the application
directory like the others.** *Negative result worth keeping:* `DefineConstants` appears in no
csproj in `V.SMART/` — grepped, zero hits.

**5. Negative result — no `Serilog`, `HealthCheck` or `AddHealthChecks` reference existed
anywhere under `V.SMART/`** before M2-B11 (grepped `.cs` and `.csproj`, 2026-08-21, reproducing
the 2026-08-12 result). Also confirmed: `ILoggingService` is registered in exactly **one** place
since M2-B07 — `V.SMART/V.SMART.Shared/DependencyInjection/ServiceCollectionExtensions.cs:308`
— not three per-host sites as older task text assumes.

**Two stale statuses found while checking prerequisites, recorded so the next session does not
re-derive them:** `docs/kb/execution/tasks/M2-A06.md:10` reads `status: Needs Review` and
`M2-B07.md:10` reads `status: Blocked`, while `git log master` shows both merged (`76eca5d`,
`e63716e`; `aaae3a0`, `ffbb1dd`). Task-file frontmatter is not status authority; the tracker and
`git log` are.
**M0-07 amendment to INV-023 (2026-08-17) — CI now exists in the repository, but has never
run.** `.github/workflows/ci.yml` runs hygiene guard → restore → build → analyzer warning gate
on every push and every PR to `master`, on `windows-latest`, gated against a committed
ratcheting baseline in `ci/warning-baseline.json` (`V.SMART.Api` 6,693 / `V.SMART.Web` 6,695
warnings, 0 errors, SDK `10.0.400`). No `-warnaserror` anywhere.
**The "no test project" finding is unchanged and remains true** — M0-07 added a commented
placeholder naming M0-12-01 and deliberately no `dotnet test`.
*Evidence:* `.github/workflows/ci.yml`; `ci/warning-baseline.json`;
[KB-087](execution/ci-pipeline.md) §7 (gate proven to fail on a deliberately introduced
`CS1030` and to pass with a ratchet notice below baseline — both observed locally).
*Not verified (KB-087 §8):* **no green run URL and no failed run URL exist** — an execution
session cannot push, so the workflow has never executed on a GitHub-hosted runner; the
baseline is marked `provisional` until the runner regenerates it; and no required status check
is configured on `master`. Confidence: **Confirmed** for what the files contain and for the
local gate behaviour; **Unknown** for runner behaviour.

### INV-008 amendment (2026-08-21, M2-B01) — the route surface moved to `/api/v1`

Re-verification, not a new investigation. The **shape** of the API surface is unchanged — still
2 controllers and 6 endpoints, still the same verbs, parameters, request bodies, response bodies
and status codes. Only the path prefix changed.

```yaml
Finding:        All six endpoints moved from /api/… to /api/v1/… (ADR-002 §6). The prefix is
                declared once, in V.SMART/V.SMART.Api/ApiRoutes.cs (public const string V1 =
                "api/v1"), and both controllers compose their route from it; no controller
                carries a literal "api/v1". The old paths were REMOVED, not aliased. No
                versioning package was added — V.SMART.Api.csproj still has exactly three
                PackageReference entries. Program.cs was NOT touched: MapControllers() is the
                only route mapping and carries no path.
                CALLER ENUMERATION (the one thing this task had to derive). Grepped the whole
                repository for "api/auth|api/currencies" across *.ts, *.cs, *.json, *.http,
                *.md, *.html, *.yml, *.yaml, *.ps1, excluding node_modules/, bin/ and obj/.
                The only RUNTIME callers of the old paths are the two Angular-pilot lines
                below; they are deliberately NOT updated (M2-C11 owns that tree).
                NEGATIVE RESULTS, each searched and each empty:
                  - no Postman collection anywhere in the repository;
                  - the one .http file, V.SMART/V.SMART.Api/V.SMART.Api.http, still contains
                    only the dotnet template's GET /weatherforecast/ and never referenced
                    either old path;
                  - no .ps1, .yml or .yaml file references either path (so CI calls neither);
                  - V.SMART.Web and V.SMART.Shared reference neither path — the Blazor host
                    calls its services in-process and does not go through the API at all;
                  - no hardcoded path literal survives anywhere in V.SMART.Api source: after
                    the change, `grep -rn "\"[^\"]*api/" V.SMART/V.SMART.Api/` (bin/obj
                    filtered) returns only the two composed [Route($"{ApiRoutes.V1}/…")]
                    attributes.
                The remaining hits are prose that records history (other task files, the
                failure log, ADR-002's own worked examples) and are left alone.
Evidence:       V.SMART/V.SMART.Api/ApiRoutes.cs:29 (const V1);
                V.SMART/V.SMART.Api/Controllers/AuthController.cs:12
                  [Route($"{ApiRoutes.V1}/auth")], action [HttpPost("login")] at :40;
                V.SMART/V.SMART.Api/Controllers/CurrencyController.cs:11
                  [Route($"{ApiRoutes.V1}/currencies")], actions at :43, :60, :69, :82, :95;
                V.SMART/V.SMART.Api/Program.cs:200 app.MapControllers() — sole mapping;
                BROKEN BY DESIGN, 2 call sites, not updated:
                  frontend/vsmart-erp/src/app/core/auth/auth.service.ts:54;
                  frontend/vsmart-erp/src/app/features/currency/currency.service.ts:18
Business rule:  n/a — this task changes URLs, not behaviour. No service call, validation,
                calculation, permission check or persistence path was touched.
Confidence:     Confirmed
Last verified:  2026-08-21
```

**Stale line numbers this amendment corrects** (the pre-M2-A06/M2-B02 figures still quoted in
several task files): the route attributes are at `AuthController.cs:12` and
`CurrencyController.cs:11`, not `:11` and `:9`; `Program.cs` is **202** lines, not 118, with
`SwaggerDoc("v1", …)` at `:38` and `MapControllers()` at `:200`.

### INV-006 amendment (2026-08-19, M2-C04-01) — the theme-persistence surface

Added by **M2-C04-01** while implementing the React token layer. It does not change any
earlier INV-006 finding; it fills the gap those findings left about how the existing UI
stores a theme. Every claim below was re-verified against current code on 2026-08-19, not
copied from the task file.

```yaml
Finding:        UserThemePreference stores a single bool IsDarkMode (default false) and
                cannot represent the 'system' preference KB-051 specifies. The whole entity
                is { Id, User, UserId, IsDarkMode } in a 22-line file — no enum, no nullable,
                no tri-state.
                NEGATIVE RESULT: IUserThemePreferenceService has NO HTTP surface anywhere.
                Grepping V.SMART.Api for "theme" (case-insensitive) returns exactly one hit,
                and it is a COMMENT. No theme endpoint, DTO or route exists. (Controller
                count corrected 2026-08-23: V.SMART.Api/Controllers/ held two when this was
                written and holds six now — Auth, Currency, CurrencyExcel, Files, Me,
                Reference. The negative result is unchanged: still one comment, still no code.)
                React therefore persists the preference locally (localStorage, key
                "nexgen.theme") until a settings endpoint exists — M3-3.
                ThemeStateService is 26 lines: one bool IsDarkMode with a private setter, one
                SetTheme(bool), one event Action OnChange. No persistence, no storage, no
                notion of "system". It is registered Scoped, i.e. per Blazor circuit, so it is
                per-tab rather than per-user; durability comes from localStorage plus the
                UserThemePreference row, read and written from MainLayout.razor.
                CONSEQUENCE, raised not decided: either a settings endpoint extends the entity
                to a tri-state, or 'system' is a client-only concept resolved to a boolean
                before persistence. Q-33 (KB-004). The schema was NOT changed.
                Second-order constraint for whoever builds that endpoint: the service cannot
                be lifted into the API as-is — it injects IJSRuntime for localStorage and is
                one of the seam-coupled registrations the API host cannot resolve (INV-039).
                Tenancy note: UserThemePreference is a DbSet on ApplicationDbContext, i.e. the
                PER-TENANT database, so one user across two tenants already has two rows.
Evidence:       V.SMART/V.SMART.Shared/Data/Master/MasterScreeenManagement_Module/UserThemePreference.cs:12-21 (bool at :20) ;
                V.SMART/V.SMART.Shared/Shared/ThemeStateService.cs:9-25 ;
                V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SettingsService/UserThemePreferenceService.cs ;
                V.SMART/V.SMART.Shared/Layout/MainLayout.razor:8 ;
                V.SMART/V.SMART.Shared/DependencyInjection/ServiceCollectionExtensions.cs:238,348 ;
                V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs:128 ;
                V.SMART/V.SMART.Api/Program.cs:221 (the single "theme" hit — a comment; it was :117
                when this amendment was written) ;
                git grep -ni "theme" -- V.SMART/V.SMART.Api  → 1 line, no code
Business rule:  n/a — this is presentation state, not ERP behaviour
Confidence:     Confirmed
Last verified:  2026-08-19
```

**Consequence recorded for the strangler period:** the React and Blazor theme preferences are
independent. A user switching theme in React sees no change in Blazor, and vice versa. That is
documented in `frontend/nexgen-web/README.md` and in
`frontend/nexgen-web/src/app/core/theme/README.md` so it is read as expected behaviour rather
than discovered as a bug. (Path corrected 2026-08-23: the React `src/shared/theme/` tree was
deleted by `M2-C01`; the Angular layer lives in `src/app/core/theme/`.)

### INV-006 amendment (2026-08-23, M2-C04-01) — how PrimeNG 22 consumes the token layer

Added by **M2-C04-01** while implementing the **Angular** token layer, and it is the answer to
**Q-67**. Every claim was verified against the installed packages and against observed output on
2026-08-23, not carried from the task file.

```yaml
Finding:        PrimeNG 22's preset system consumes the CSS-variable token layer DIRECTLY:
                a semantic token whose value is the string "var(--accent)" is emitted verbatim
                into the generated --p-* custom property. The preset is therefore a pure
                mapping onto src/styles/tokens.css with NO colour value duplicated into
                TypeScript. This is route A of the two the task offered; the documented
                fallback (overriding --p-* from styles.scss) was not needed.
                MECHANISM: @primeuix/styled's value emitter (getVariableValue) rewrites only
                brace references — {a.b} becomes var(--p-a-b) — and returns any other string
                unchanged. The same bundle exports VAR_REGEX = /var\([^)]+\)/g and uses it to
                blank out var(...) before testing for calc(), i.e. var() inside a token value
                is anticipated by the library, not accidental.
                OBSERVED, not inferred:
                  toVariables({primary:{color:'var(--accent)'}},{prefix:'p'}).declarations
                    === '--p-primary-color:var(--accent);'
                TRAP 1: Aura 3.0.0 has NO colorScheme.dark block. Its semantic values use the
                CSS light-dark() function, which is selected by the color-scheme PROPERTY, not
                by a selector — e.g. text.color is "light-dark({surface.700}, {surface.0})".
                Setting darkModeSelector alone would leave the two mechanisms out of step.
                RESOLUTION TAKEN: tokens.css sets color-scheme:light on :root and
                color-scheme:dark under [data-theme='dark'], and darkModeSelector is
                '[data-theme="dark"]'. Both mechanisms then hang off the ONE attribute
                ThemeService writes, so an un-overridden Aura token flips at the same moment
                as an overridden one.
                TRAP 2: PrimeNG's palette helpers hard-require a hex literal. mix() guards on
                /^#([0-9a-f]{3}|[0-9a-f]{6})$/i and returns its input unchanged otherwise, and
                palette(), shade() and tint() are built on it — so updatePrimaryPalette() and
                a generated 50..950 ramp CANNOT be driven from a CSS variable. Override the
                semantic LEAVES instead.
                TRAP 3: toUnit() appends px to a bare number except for opacity, z-index,
                line-height, font-weight, flex and order keys. Pass unit-bearing strings.
                CONFIRMED SHAPES: definePreset(base, ...extensions) is exported by
                @primeuix/themes 3.0.0; darkModeSelector is typed
                'system' | 'none' | (string & {}) and an attribute form resolves to
                :root[data-theme="dark"],:host[data-theme="dark"] — so the attribute MUST be
                on <html>, which is what the pre-paint script in index.html does.
Evidence:       frontend/nexgen-web/src/app/core/theme/theme.preset.ts ;
                frontend/nexgen-web/src/app/core/theme/tokens.spec.ts (the emitter assertion) ;
                frontend/nexgen-web/src/app/app.config.ts ;
                node_modules/@primeuix/themes/dist/index.d.mts:6 (definePreset) ;
                node_modules/@primeuix/styled/dist/index.d.mts:494 (darkModeSelector) ;
                node_modules/@primeuix/styled/dist/index.mjs:1 (getVariableValue, toVariables,
                  mix, ThemeUtils.getCommon — single-line bundle) ;
                node_modules/@primeuix/themes/dist/aura/base/index.mjs:1 (light-dark values) ;
                observed: npm run test:ci → 8 files, 47 tests passed, 2026-08-23
Business rule:  n/a — presentation only
Confidence:     Confirmed
Last verified:  2026-08-23
```

**Also re-confirmed on 2026-08-23, unchanged:** `UserThemePreference.cs:20` still reads
`public bool IsDarkMode { get; set; } = false;`. The entity was **not** modified. Q-33 stays
open.


### INV-006 amendment (2026-08-23, M2-C04-02) — the `DataAnnotations` → Angular validator mapping surface

Added by **M2-C04-02** while building the form layer. The attribute counts below were measured
with `git grep --untracked` over `V.SMART/V.SMART.Shared/ViewModels/` on 2026-08-23, not
carried from a task file.

```yaml
Finding:        ViewModel validation uses DataAnnotations, and the surface is WIDER than the
                five attributes KB-015 names. Measured across the 274 ViewModel files (150 of
                them carry at least one validation attribute):
                  [Required] 615, [StringLength] 506, [Range] 496, [MaxLength] 95,
                  [Display] 58, [MinLength] 43, [RegularExpression] 31, [DataType] 16,
                  [Phone] 12, [Key] 9, [EmailAddress] 9, [Column] 7, [NotMapped] 2,
                  [DisplayFormat] 2.
                KB-015 lists only [Required], [StringLength], [RegularExpression],
                [EmailAddress], [Phone] — it omits [Range], the THIRD most common.

                MAPPING (UX mirror only; the server stays authoritative):
                  [Required]            -> Validators.required, plus aria-required and the
                                           "*" marker rendered by app-form-field
                  [StringLength(n)]     -> Validators.maxLength(n)
                  [MaxLength(n)]        -> Validators.maxLength(n)
                  [MinLength(n)]        -> Validators.minLength(n)
                  [RegularExpression(p)]-> Validators.pattern(p) — CAVEAT: .NET anchors the
                                           pattern implicitly for IsMatch-style validation and
                                           JS RegExp does not, so the generated shape needs an
                                           explicit ^...$
                  [EmailAddress]        -> Validators.email — a DIFFERENT regex from .NET's;
                                           the divergence is acceptable for UX because the
                                           server re-validates
                  [Phone]               -> Validators.pattern; Angular has no built-in
                  [Range(min,max)]      -> Validators.min / Validators.max, EXCEPT on money and
                                           quantity fields, whose SPA value is the branded
                                           Money/Qty of M2-C10, not a number. Validators.min
                                           and .max coerce with Number() and cannot be used
                                           there; a decimal-aware comparator is needed and NO
                                           TASK OWNS IT YET.
                  [DataType(Date)]      -> presentational: selects app-date-picker. Not a
                                           validator.
                  [Display] [Key] [Column] [NotMapped] [DisplayFormat] -> not validators.

                ErrorMessage strings are per-attribute product UX copy and must be carried into
                the generated shapes, or the SPA shows generic messages where Blazor shows
                specific ones.

                CROSS-FIELD RULES ARE NOT EXPRESSIBLE AS FIELD VALIDATORS, and they live in TWO
                places, not one: (a) Razor @code — ValidateRowAsync, IsItemAlreadySelected,
                quantity-balance checks (KB-015); and (b) 29 ViewModels implementing
                IValidatableObject, i.e. cross-field validation on the ViewModel itself. Both
                are extracted server-side by each wave's -03 step. M2-C04-02 implements
                neither.
Evidence:       V.SMART/V.SMART.Shared/ViewModels/MasterViewModel/AccountsViewModel/CurrencyVM.cs:14
                  ("Please Enter Currency Name"), :23-24 (the currency-symbol
                  RegularExpression with its enumerated allowed symbols) ;
                V.SMART/V.SMART.Shared/ViewModels/MasterViewModel/AdminViewmodel/UserVM.cs:123
                  Validate(...), :126-139 — password required only when UserId == 0 ||
                  ChangePassword, length >= 6, plus a complexity regex ;
                further IValidatableObject implementations: MaterialIssNoteVM.cs:11,
                  SCNGenVM.cs:12, ToolCribReturnSubVM.cs:15, UserAuthorityVM.cs:10,
                  AssemblyDefVM.cs:11, ItemVM.cs:10, ExpInvSubVM.cs:12 ;
                KB-015 §Forms and validation (the incomplete five-attribute list)
Business rule:  n/a — no rule is created here; the mapping is a presentation contract
Confidence:     Confirmed (counts and file:line re-verified 2026-08-23)
Last verified:  2026-08-23
```

**Three further findings from the same pass, recorded because they contradict or extend what
`M2-C04-02.md` assumes.**

1. **`CustomerSelection.razor` is not a debounced async typeahead.** The task file
   (`M2-C04-02.md:177`) describes it as one. The source does not do that:
   `V.SMART/V.SMART.Shared/Components/CustomerSelection.razor:119-148` eagerly loads the entire
   active-customer list into an in-memory `List<CustomerVM>` on init, and `:150-160`
   `SearchCustomers` filters that list synchronously via `Task.FromResult`, ignoring the
   `CancellationToken`. `VendorSelection.razor` has the same shape
   (`GetAllActiveVendorsAsync` ~`:108`, `Task.FromResult` ~`:126`/`:138`). `app-combobox`'s
   debounced async loader is therefore a **new capability**, which is fine — but it must not be
   justified as preserving existing behaviour.
2. **`CustomerSelection.razor` is a routable page, not a control** — `@page "/Customer/Select"`
   and `"/Customer/Select/{IsExport:bool}/{IsMfgInv:bool}"` at `:1-2`, returning via a
   `ReturnUrl` query parameter with an `{id}` token substitution at `:282`, and choosing its
   loader by route flag at `:124-139`. That is a screen-navigation pattern for
   **M2-C06**'s `RecordPickerDialog`, not for a form control.
3. **A party-completeness gate lives in that page and has no owner.**
   `CustomerSelection.razor:168-289` `Proceed()` blocks the selection unless ~12 fields are
   present, and at `:207-229` requires a GST number of **exactly 15 characters** (`:215`)
   matching an Indian GST regex (`:222`) when `BusiType` is `Local` or `InterState`; otherwise,
   for exports, `:239-246` requires a currency rate for today or redirects to
   `/currency_today`. These are ERP rules, they are **client-side only** today, and nothing in
   the `M2-C0x` chain is assigned to carry them. Recorded as a risk in
   [KB-060](risks/technical-debt-register.md).

**Negative result, same pass:** the customer→currency/terms/consignee/cost-centre cascade is
**not** defined in `CustomerSelection.razor`. It is copy-pasted as a private
`ApplyCustomerSelectionAsync` in **23** files under `V.SMART/V.SMART.Shared/Pages/` (e.g.
`SubContractDCOutUpsert.razor:1471`, `SubConGRNUpsert.razor:2146`, `SubConSCNUpsert.razor:961`,
`EstimationUpsert.razor:1125`). Correctly out of scope for M2-C04-02; a sizing signal for the
`-03` extraction steps.

### INV-006 amendment (2026-08-23, M2-C04-03) — what the feedback layer binds, and three superseded Razor components

Added by **M2-C04-03** while building the overlay and feedback layers. The `ProblemDetails`
field names were the task's single new check; every citation below was re-read in the working
tree on 2026-08-23.

```yaml
Finding:        The SPA feedback layer binds the error state and the toast layer to the
                ProblemDetails members shipped by M2-A06: the human-readable message is
                `title` - NOT `detail` - and the correlation id is the RFC 7807 extension
                member `traceId`, flat in the JSON body.
                  * A 409 business-rule refusal carries the service's own sentence in
                    `title`, verbatim (ApiProblems.cs:47-53), so app-error-state.message and
                    app-inline-alert.message bind `title`; binding `detail` alone would
                    render an empty panel for exactly the case an operator most needs.
                  * `traceId` is set on every body at ApiProblems.cs:43, and again on the
                    403 at :102 and the 400 ValidationProblemDetails at :155. Its value is
                    Activity.Current?.Id ?? HttpContext.TraceIdentifier (CorrelationId.cs:22-33)
                    and it is also returned in the X-Correlation-Id header
                    (CorrelationId.cs:25).
                  * The 403 screen-right body carries the extensions `screen`
                    (ApiProblems.cs:100) and `right` (:101); those two map one-to-one onto
                    app-permission-denied-state's two inputs, which is why that component
                    needs no service.
                  * Clients branch on `type` (ProblemTypes.cs:17-45), never on `title`.
                BsModal.razor's optional-reason capability is reproduced by app-confirm-dialog
                over PrimeNG's ConfirmationService - with one deliberate behaviour change:
                the Razor original allows Confirm on an empty reason and answers with a
                toastr warning "Please enter a valid reason before confirming."
                (BsModal.razor:76-93), while app-confirm-dialog disables Confirm until the
                reason is non-empty after trim. PrimeNG's own Confirmation.accept/reject
                callbacks take no arguments (primeng/api), so the reason cannot travel back
                through them; it is held in ConfirmDialogService and read by the callback
                that resolves the promise.
                ProcessingOverlay.razor - a full-page overlay with an animated logo, no ARIA
                and no role (ProcessingOverlay.razor:5-33) - is replaced by a region-scoped
                app-busy-overlay over p-blockui, with a fullPage escape hatch, per KB-051's
                "saving never blocks the page".
                UnsavedChangesModal.razor has THREE outcomes - Continue/Discard/Cancel, with
                backdrop click mapped to Cancel (UnsavedChangesModal.razor:4, :43-52) - which
                app-confirm-dialog's two-outcome contract does not cover. The dirty-form
                guard (M2-C03/M2-C08) therefore composes app-modal, not app-confirm-dialog.
Evidence:       V.SMART/V.SMART.Shared/Components/BsModal.razor:4,21-23,57-74,76-93 ;
                V.SMART/V.SMART.Shared/Components/ProcessingOverlay.razor:5-33 ;
                V.SMART/V.SMART.Shared/Components/UnsavedChangesModal.razor:4,43-52 ;
                V.SMART/V.SMART.Api/Middleware/ApiProblems.cs:43,47-53,89-104,145-157 ;
                V.SMART/V.SMART.Api/Middleware/CorrelationId.cs:14-33 ;
                V.SMART/V.SMART.Api/Middleware/ProblemTypes.cs:17-45
Business rule:  BR-SO-003 (capability only; the rule stays server-side)
Confidence:     Confirmed
Last verified:  2026-08-24
```

**Line-citation correction to the INV-040 amendment below.** That entry cites the `403`
`traceId` at `ApiProblems.cs:86` and the `400` at `:131`. Current source has them at **`:102`**
and **`:155`**; `:43` is still correct. The drift is explained by `AccountGateRefused` (`:68`)
and `PayloadTooLarge` (`:111`) landing since. The substance of that amendment is unchanged.

**Negative results, same pass, all `git grep -n --untracked` over
`frontend/nexgen-web/src` on 2026-08-23:** `"An error occurred"` — **0 hits** (the server's
own constant is `"An unexpected error occurred."`, `ApiProblems.cs:139`, which arrives as
`title` and is rendered verbatim; the ban is on client-authored fallbacks);
`"core/auth"` under `src/app/shared/components` — **0 hits**; `MessageService` — hits in
`feedback/toast.service.ts` only.

**Measured PrimeNG 22.1.0 behaviour, found by test rather than assumed** — recorded so no
future session rediscovers it: `Drawer.onKeyDown` answers `Escape` with `hide(false)`, which
never clears `visible`; `Drawer` emits `onHide` only from `close()`, never for a programmatic
close; `Drawer`'s document escape fallback keys off the deprecated `event.which` and
`unbindDocumentEscapeListener()` calls itself; the drawer root is hard-coded
`role="complementary"`; the dialog and drawer close icons have no accessible name unless
`closeAriaLabel`/`ariaCloseLabel` is set, and `p-confirmdialog` does not forward
`closeAriaLabel` to the dialog it renders; `p-contextmenu` puts `aria-level`, `aria-setsize`
and `aria-posinset` on `role="menuitem"`; `p-progressbar` writes `aria-level="42%"` and turns
an unbound `value` into `aria-valuenow="NaN"`; `p-toast` items are `role="alert"
aria-live="assertive"`. Each is worked around through `[pt]` or a small wrapper, never a fork.

**Added 2026-08-24, and the most expensive of the set: `p-confirmdialog` never moves focus
into itself.** It hard-codes `[focusOnShow]="false"` on the `p-dialog` it renders and depends
on `pAutoFocus` sitting on *its own* accept/reject buttons; supplying a custom `#footer` —
which any wrapper needing a reason field must — removes those buttons, so focus never enters
the dialog and PrimeNG's focus trap holds nothing, because a trap only holds focus that is
already inside. `p-confirmdialog` emits no `(onShow)`, and `p-dialog` **moves** its wrapper to
`document.body` (`appendContainer()`) as the enter transition starts, blurring anything focused
before the move — so focusing from a view-query effect does not survive. The fix used is
`afterEveryRender`, which runs again after the move, with `focusFirstElementIn` a no-op once
focus is inside. Evidence:
`frontend/nexgen-web/src/app/shared/components/overlay/confirm-dialog.component.ts:82-118`,
asserted by `confirm-dialog.component.spec.ts:153-192` (focus-in + `Tab` trap, focus restore,
scroll lock), which fail against the pre-fix component — measured on 2026-08-24 by reverting
it. **Generalisation for future overlay wrappers:** a PrimeNG overlay's focus contract is not
inherited by a wrapper that replaces its templated buttons; assert it by test for every new
overlay, as `overlay-focus.ts:5-9` warns.

### INV-042 — `role` in `GET /api/v1/me`, and what else belongs in the bootstrap response

Produced by **M2-A07**, 2026-08-20. Every citation below was re-read in the working tree on
that date, not carried from the task file.

**Finding 1 — role is sourced from the JWT `ClaimTypes.Role` claim, not from the `User` row,
and never from `CurrentUserService.GetUserRoleAsync()`.**
`JwtTokenService.cs:34` mints `new Claim(ClaimTypes.Role, user.Role?.ToString() ?? string.Empty)`
and `AuthController.cs:67` returns **the identical expression** in `LoginResponse`, so claim and
row are the same value for the life of a token; reading the claim keeps `/me` and `/auth/login`
from disagreeing and costs no query. `CurrentUserService.cs:68-75` reads claim type `"role"`
while both providers write `ClaimTypes.Role`, so it always returns `""` (R-18) — it is not
called, and `MeController` takes no dependency that could reach it.
*Business rule:* BR-AUTH-002. *Confidence:* **Confirmed**. *Last verified:* 2026-08-20.

**Finding 2 — `ERPAdmin` is a dead string and is not propagated (R-31).** `UserRole.cs:3-7`
has exactly `Administrator` and `User`. `git grep -n ERPAdmin -- V.SMART` returns exactly three
hits — `NavMenu.razor:36`, `NavMenu.razor:148`, `Pages/Home.razor:240` — all
`<AuthorizeView Roles="Administrator,ERPAdmin,User">`. Because that list also names both real
enum members, the gate is effectively *"any authenticated user with a role"*, and `ERPAdmin`
changes nothing. The API model carries the role as an opaque string, so the name cannot enter it.
*Confidence:* **Confirmed**. *Last verified:* 2026-08-20.

**Finding 3 — `UserAuthority` is deferred to M3-4.** It is twelve `(bool, string level)` pairs
on one row (`Data/Master/Admin_Module/UserAuthority.cs`), navigated from `User.cs:124`. ADR-004
§4 enforces it **inside** the approval endpoints, the approval module is M3-4, no M2 consumer
references it, and KB-050's permission contract has no slot for it. Including it in `/me` would
publish approval policy to every client for no consumer.
*Confidence:* **Inferred** (from the absence of any M2 consumer). *Last verified:* 2026-08-20.

**Finding 4 — none of the four `User` UI flags belongs in `/me`, and two of the assumptions
about them in circulation are wrong.**
- `IsViewOnly` (`User.cs:66`) is **not** a runtime gate. At user creation it *materialises* a
  `UserRight` row for every screen with `CanView = true` and everything else false
  (`UserService.cs:442-467`); its only other reads are the admin screens that display or edit the
  field itself — `UserRights_Pages/UserRights.razor:288-292`, `Identity_Pages/UserList.razor:185-188,441`,
  `Identity_Pages/RegisterUpsert.razor:94-113`. Its effect is therefore already fully present in the rights map.
- `IsProductionBased` (`User.cs:75`) does **not** on its own "collapse the menu to a
  shop-floor view". The gate is `ShowProductionMenu = IsQrLogin && IsProductionUser`
  (`NavMenu.razor:462-464`), reached through `UserService.IsProductionLogEnabledAsync`
  (`UserService.cs:60-70`) from `Login.razor:432` / `QrLogin.razor:59`. **The API has no
  QR-login path at all**, so the flag alone changes nothing today.
- `HideManualJobOrderButton` (`User.cs:68`) is read only by
  `JobOrderService.IsManualyJobOrderHiden` (`JobOrderService.cs:1349-1359`), single caller
  `JobOrderUpsert_pages.razor:1253` — one button on one M3 screen.
- `LevelAuthorization` (`User.cs:36`) is an approval-authority input (`UserAuthorityservice.cs:178`).
*Confidence:* **Confirmed** for each citation; **Inferred** for the conclusion that none is
needed by the M2 client. *Last verified:* 2026-08-20.

**Finding 5 (negative result, and it matters) — the Blazor sidebar is not permission-filtered
at all.** Grepping `NavMenu.razor` (888 lines) for `UserRight`, `RightsHelper`, `CanView` or
`IsHide` returns **zero** hits. Its only authorization is the two `AuthorizeView` role gates
(`:36`, `:148`) and `ShowProductionMenu` (`:462-464`). `IsHide` is consumed **per page**, via
`RightsHelper.IsHidden` (`RightsHelper.cs:19-20`) through `BaseUserRightsComponent.cs:27`.
*Consequence:* M2-C03's permission-filtered sidebar has **no legacy behaviour to preserve** —
it is new behaviour built on this endpoint, so "port `NavMenu`" is the wrong instruction.
*Confidence:* **Confirmed**. *Last verified:* 2026-08-20.

**Finding 6 — the provider seam cannot carry `ScreenCode` or `IsPrintRequired`, so `/me` does
not.** `ScreenRightSet.cs:4-10` defines `ScreenRightEntry(ScreenName, CanView, CanCreate,
CanEdit, CanDelete, IsHide)` and nothing else; `UserRightsProvider.cs:116-122` projects exactly
those six values and drops any row whose `Screens` navigation is null (`:110-114`). `Screens`
does carry `Id`, `ScreenCode` and `IsPrintRequired`, but they never reach the seam. Since
M2-A07 must read through this seam and must not modify `V.SMART.Api/Authorization/**`, both
fields are omitted — which matches KB-050's client contract, written before this endpoint
existed and asking for exactly the five booleans. A future task needing `IsPrintRequired` should
extend `ScreenRightEntry` (one change, both readers) rather than adding a second query path.
*Confidence:* **Confirmed**. *Last verified:* 2026-08-20.

**Finding 7 — absent rows are omitted, and duplicates collapse first-match-wins.**
`UserRightsRepository.cs:22-28` returns only `r.UserId == userId` with no `OrderBy`, and
`RightsHelper.cs:7-20` reads `...FirstOrDefault(...)?.CanX ?? false` five times, which
`ScreenRightSet.Has` (`ScreenRightSet.cs:46-61`) reproduces including the `?? false`.
Deny-by-default is therefore already the semantic, so materialising 152 all-`false` rows would
add no information at triple the payload. Keying the response by `ScreenName` dissolves the
ordering question; the duplicate question it does not dissolve is answered by keeping the
**first** entry (`Dictionary.TryAdd`), which is the row the filter's `FirstOrDefault` decides
on. Q-27 (do duplicates exist in a live tenant?) remains **Unknown** and is unaffected by this:
whatever the answer, the map and the filter now agree.
*Business rule:* BR-AUTH-002. *Confidence:* **Confirmed**. *Last verified:* 2026-08-20.

---
### INV-028 (2026-08-20, M2-A08) — the negative results, and the INV-004 amendment

**Finding.** `User.StateCodesCsv` does **not** filter customers or vendors anywhere — a
negative result that contradicts Q-08's `Inferred` claim. The only real row scope is
`LeadService.GetAllLoadLeadsAsync` (`LeadService.cs:128-152`), on `Leads` only, in the service
layer, with **one** call site — while its unscoped sibling `GetAllLeadsAsync` (`:95-106`) has
**four** (`LeadsList.razor:398`, `:476`; `LeadsUpsert.razor:1275`, `:1287`). It filters **in
memory**, resolves the current user by **username string equality**, and **fails closed** on a
blank CSV (zero leads). Two facts nothing had recorded before: `LeadsList.razor:470-484`
exempts **`UserId == 1`** from scope entirely, and `LeadsList.razor:396-401` computes the
**paging total from the unscoped query** for every user.

*Negative greps, each re-run 2026-08-20 and each a finding in its own right:*

1. `git grep -nE "StateCodesCsv|StateCodes\b" -- V.SMART/`, excluding `Migrations/` — hits in
   **eight files only**, all listed in [KB-108](architecture/row-scope-and-account-gates.md)
   §2.1. **No customer, vendor, item, document or report query.**
2. `git grep -ni statecode -- V.SMART/V.SMART.Api/ tests/` — **nothing** before this task.
3. `grep -rli StateCodesCsv db/ "Existing Store Procedures"` — **nothing**. No stored procedure
   references it, so ADR-005's report path has **no scope predicate to preserve**; M2-B08 must
   invent one.
4. `git grep -nE "\.Where\([^)]*([Cc]urrent[Uu]ser|currentUserName|currentUserId)" -- V.SMART/V.SMART.Shared/BusinessLayer/`
   — **zero matches**. No business service filters rows by the current user inline.
5. `git grep -n "GetUserTrialAsync" -- V.SMART/` — **declaration and implementation only, zero
   call sites**. Dead code.

*Business rules:* BR-AUTH-001 (account gates). *Confidence:* **Confirmed** for every item
above; **Inferred (strong)** only for "`LeadService` is the only row-scoped service", which
rests on greps 1 and 4 rather than on an exhaustive read of 285 services.

**INV-004 amendment (2026-08-20, M2-A08).** INV-004 → [KB-013](architecture/auth-and-permissions.md)
describes the three authorization mechanisms and remains correct. It does **not** cover the
**account gates** — QR expiry, trial expiry, device binding — which are **not** in any service:
they live in `Login.razor` and `QrLogin.razor` `@code`. Nor does it cover **row scope**, which
is a fourth concern. Both are documented in KB-108 and summarised in KB-013.

### INV-031 — Test-harness feasibility: hosting `ApplicationDbContext` in a test process

Produced by **M0-12-01**, 2026-08-19. Every finding below was **executed**, not reasoned
about; each spike is retained as a permanent test in
`tests/V.SMART.Shared.Tests/DbFixtureTests.cs`, so if one of these facts changes, CI says so.

> **The pre-spike inference was wrong, and in the opposite direction.** The task specification
> inferred (high confidence) that `Microsoft.EntityFrameworkCore.InMemory` could **not** build
> this model because `OnModelCreating` calls the relational-only `ToView(null)` 65 times, and
> that `Microsoft.EntityFrameworkCore.Sqlite` **could**, being relational. Both halves are
> false. Do not re-derive this from the `ToView` count — it does not predict the outcome under
> EF Core 9.0.5.

**Finding 1 — the InMemory provider hosts the model. Sqlite does not.**
*Evidence:* executed 2026-08-19, EF Core 9.0.5, SDK 10.0.400, `net9.0`.
`new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(...).Options).Database.EnsureCreated()`
returns `true`. The same call over
`new SqliteConnection("DataSource=:memory:")` throws, verbatim:

```
Microsoft.Data.Sqlite.SqliteException: SQLite Error 1: 'near "MAX": syntax error'.
   at Microsoft.Data.Sqlite.SqliteException.ThrowExceptionForRC(Int32 rc, sqlite3 db)
```

*Business rule:* n/a. *Confidence:* **Confirmed**. *Last verified:* 2026-08-19.

**Finding 2 — the Sqlite failure has a specific, locatable cause, and it is production code
this task could not touch.** `Database.GenerateCreateScript()` under Sqlite emits an 8,523-line
script containing five columns typed `NVARCHAR(MAX)` / `nvarchar(max)` — a SQL-Server-only
type name that SQLite's parser rejects. They come from nine `[Column(TypeName = ...)]`
attributes, **not** from `ApplicationDbContext.OnModelCreating` (which is why grepping the
context for `HasColumnType` found nothing):
`V.SMART/V.SMART.Shared/Data/HumanResource/Attendance/Attendance.cs:27,30,42,45`;
`.../Data/Inspection/FinalInspection/FinalInspection.cs:87`;
`.../Data/Inspection/FinalInspection/FinalInspectionRef.cs:21`;
`.../Data/Inspection/IncomingInspection/IncomingInspectionRef.cs:23`;
`.../Data/Inspection/MasterInspection/InspectionRef.cs:24`;
`.../Data/Inspection/MasterInspection/MasterInspection.cs:47`.
*Consequence:* Sqlite becomes viable only if those attributes are removed or made
provider-conditional — a production-code change, out of scope here, and recorded as technical
debt rather than made. *Confidence:* **Confirmed**. *Last verified:* 2026-08-19.

**Finding 3 — `EnsureCreated()` DOES apply the `HasData` seeds under the InMemory provider.**
This is the answer M0-06 and M0-13 were waiting on. Observed counts immediately after
`EnsureCreated()`, with no manual seeding: `Screens` **152**, `Users` **1** (`UserName` =
`"Administrator"`), `Stores` **9**, `UOM` **49**, `Category` **16**. Note there are **ten**
`HasData` calls in `OnModelCreating`, not the two the task file named — `User` (:1136),
`Screens` (:1151), `InspectionSettings` (:1331), `ScreenManagement` (:1340), `Category`
(:1694), `Store` (:1715), `UOM` (:1729), `State` (:1783), `Currency` (:1828) and `StoreMap`
(:1835). *Confidence:* **Confirmed**. *Last verified:* 2026-08-19.

**Finding 4 — the foreign-key requirement for seeding `StockAdd`.** `StockAdd` declares three
foreign keys — `ItemId`→`Item`, `StoreId`→`Store`, `ScreenCode`→`Screens`
(`V.SMART/V.SMART.Shared/Data/Inventory(Stock)/StockAdd.cs:22-54`). Because of Finding 3,
`Store` and `Screens` **already exist** after `EnsureCreated()`, so only an `Item` must be
created. `Item` in turn needs a `MeasureUnit` (FK to `UOM`, `Item.cs:50-54`) and a
`CategoryCode` (`Item.cs:41-45`) — both satisfiable from seeded rows — **and** non-nullable
`HSNCode` / `SACCode` (`Item.cs:141-147`), which the InMemory provider *does* enforce:
omitting them throws
`Microsoft.EntityFrameworkCore.DbUpdateException : Required properties '{'HSNCode', 'SACCode'}' are missing for the instance of entity type 'Item'.`
`TestDbContextFactory.SeedMasterData(context)` does exactly this and returns the new `ItemId`.
*Confidence:* **Confirmed** (each fact observed as a test failure and then as a pass).
*Last verified:* 2026-08-19.

**Finding 5 — InMemory enforces required properties but NOT foreign keys.** A `StockAdd` with
a dangling `ItemId` saves without error. Seed the parents anyway, for parity with SQL Server
and so the test reads truthfully; do **not** rely on this harness to catch an FK violation.
*Confidence:* **Confirmed** (nullability error observed in Finding 4; no FK error ever raised).
*Last verified:* 2026-08-19.

**Finding 6 — the harness satisfies the `IAsyncQueryProvider` constraint.** `IRepository<T>.GetQueryable()`
results have EF async operators applied to them by callers — e.g.
`StockManagerService.cs:114-116` (`FirstOrDefaultAsync`) and `:205-207` (`ToListAsync`) — which
a `list.AsQueryable()` double cannot serve. The InMemory provider is a real EF Core provider,
so `await context.Screens.Where(...).ToListAsync()` works. **M0-13 is therefore NOT blocked.**
*Confidence:* **Confirmed** (executed). *Last verified:* 2026-08-19.

**Finding 7 — what this harness cannot do, stated so nobody assumes otherwise.** The InMemory
provider does not translate LINQ to SQL, so it **cannot** catch a "could not be translated"
regression, and it does not enforce relational constraints such as the
`OnDelete(DeleteBehavior.Restrict)` configured for `StockIssueTrack`→`StockAdd`
(`ApplicationDbContext.cs:509-512`). Anything depending on SQL semantics needs a real SQL
Server, which no test in this repository has. *Confidence:* **Confirmed** by the provider's
documented contract plus Finding 5. *Last verified:* 2026-08-19.

**Finding 8 — a plain `net9.0` test project references the multi-targeted, Razor-SDK
`V.SMART.Shared` cleanly.** No `SetTargetFramework` and no switch to `net9.0-windows` was
needed; the reference resolves to the `net9.0` leg automatically and the build succeeds
(2 warnings, both the pre-existing `NU1608`, 0 errors). Negative result worth recording — this
was an anticipated obstacle that did not materialise. *Confidence:* **Confirmed.**
*Last verified:* 2026-08-19.

**M0-07 amendment to INV-029 (2026-08-17) — runner-vs-local warning count: NOT YET COMPARED.**
This is an explicit negative result, recorded so no future session assumes it was done. The
runner has never built this repository, so no runner-side warning count exists. What *is*
newly Confirmed is a local-vs-local delta that every later comparison must account for:
splitting restore into its own step and building with `--no-restore --no-incremental -v normal`
yields **6,693** (Api) and **6,695** (Web) — 2 and 3 lower than KB-086's 6,695/6,698, because
the restore-time `NU1608` warnings no longer appear in the build log. The arithmetic reconciles
exactly against KB-086 §5; every other code matches count-for-count. When the workflow first
runs, the **runner's** numbers become the baseline and the runner-vs-local delta must be
recorded here.

**M0-15 amendment (2026-08-17), closing the solution-build gap INV-029 left open:** the
solution build **succeeds** — 0 errors, 13,367 warnings, ~4–4.5 min — but only reproducibly
**from a clean `obj`**; a dirty `obj` (stale artifacts from an unrelated earlier local MAUI
build attempt) produced 2 file-lock/permission errors, not compilation errors. Both a clean
run and the dirty-`obj` failure were independently reproduced. Whether it would succeed on a
workload-free CI runner remains **Unknown** — no workload-free environment was available to
test it in this session. SDK list drifted since 2026-08-12: now `10.0.300`/`10.0.400` (not
`10.0.302`), on the same machine, with no repository change — a root `global.json` was
created pinning `10.0.400` with `rollForward: latestFeature` to stop this recurring silently.
Warning baseline `6,695` (Api build) confirmed reproducible across two clean runs; its
dominant codes are the `CS86xx` nullable-reference family, **not** `MUD0002` as this row
previously implied — `MUD0002` is only 130 occurrences (1.94%). Full detail, evidence and
methodology: [KB-086](execution/M0-15-build-baseline.md).

**M0-03-02 amendment (2026-08-18) — the credential exposure INV-029 recorded was not only in
configuration files, and not only database credentials. Confirmed by direct reading of the
files and by `git grep --untracked`.**
- **Hardcoded in C#, now removed from the working tree.** The SA password, the production
  host `154.61.76.112,1533` and the `bspl` password were C# string literals at
  `V.SMART/V.SMART.Shared/Data/MigrationData/ApplicationDbContextFactory.cs:13-14`,
  `V.SMART/V.SMART.Shared/Data/MigrationData/MasterDbContextFactory.cs:11-12` and
  `V.SMART/V.SMART/MauiProgram.cs:228,231,235` (line numbers as they stood before M0-03-02;
  re-verified unchanged on 2026-08-18 immediately before editing). All three now read
  configuration — `ConnectionStrings:MasterDb`, and the new
  `ConnectionStrings:DesignTimeTenantDb` for the per-tenant design-time factory. Confidence:
  **Confirmed**.
- **A distinct, non-database credential.**
  `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/EInvoiceAPIService/EinvoiceDatabaseService.cs:1413-1414`
  and `…/EWayDatabaseService.cs:900-901` held trailing comments containing a **GST e-Invoice /
  e-Way gateway** user name and password — a different system, a different owner and a
  different rotation procedure from the SQL Server credentials. The running code already took
  both as parameters of `GetUserNameandPassword(string username, string password)`, so the
  literals were dead. The comments are deleted; the values still require their own rotation in
  M0-04. Confidence: **Confirmed**.
- **Negative result — do not re-derive.** The literal `bspl` outside the credential sites is
  **not** a credential: `V.SMART/V.SMART.Shared/Pages/Home.razor:198,316` (public contact
  email address), `V.SMART/V.SMART.Shared/V.SMART.Shared.csproj:19-20` and
  `V.SMART/V.SMART.Shared/Components/ProcessingOverlay.razor:31` (image file names),
  `Payments.razor:1412`, `Receipts.razor:1414`, `AdvanceAdjustment.razor:1359` (`upi://pay`
  sample strings), `V.SMART/V.SMART.Api/wwwroot/config/tenant.json:2,5` (tenant name / UNC
  path). **Correction to the earlier list:** `V.SMART/V.SMART.Web/Components/App.razor` and
  `V.SMART/V.SMART.Shared/Pages/Master_Module_pages/Identity_Pages/Login.razor` were named as
  `bspl` hits by KB-060 R-01 and by the M0-03-02 task file; `grep -ic bspl` returns **0** for
  both today. Confidence: **Confirmed**.
- **`dotnet ef` is installed here** (`dotnet ef --version` → `10.0.11`), and the design-time
  factories were smoke-tested with it: `dotnet ef dbcontext info --project
  V.SMART/V.SMART.Shared/V.SMART.Shared.csproj --startup-project
  V.SMART/V.SMART.Web/V.SMART.Web.csproj --context <MasterDbContext|ApplicationDbContext>
  --framework net9.0`. `--framework` is **required** (the library multi-targets), and
  `V.SMART.Api` cannot be the startup project — it does not reference
  `Microsoft.EntityFrameworkCore.Design`; `V.SMART.Web` does
  (`V.SMART/V.SMART.Web/V.SMART.Web.csproj:12`). Confidence: **Confirmed**.

**M0-03-03 amendment (2026-08-18) — startup configuration guards before this task. The third
and last amendment M0-03 makes to INV-029. Confirmed by direct reading of both composition
roots on 2026-08-18, and by running both hosts.**

```yaml
Finding:        Before M0-03-03 the only configuration guard in either web host was a null
                check on Jwt:Secret; empty, whitespace, too-short and known-default values
                all passed, and ConnectionStrings:MasterDb had no check at all in either
                host. A second, independent null-only copy of the Jwt:Secret guard sat in
                the token service itself.
Evidence:       V.SMART/V.SMART.Api/Program.cs:56-57 (the guard, pre-task)
                V.SMART/V.SMART.Api/Program.cs:82-83 (unchecked connection string, pre-task)
                V.SMART/V.SMART.Web/Program.cs:226-227 (unchecked connection string, pre-task)
                V.SMART/V.SMART.Api/Auth/JwtTokenService.cs:20-21 (the duplicate guard)
Business rule:  n/a
Confidence:     Confirmed
Last verified:  2026-08-18
```

- **Negative result — do not re-derive.** Grepped the whole tree for
  `StartupConfigurationValidator`, `ValidateConfiguration` and `ValidateStartup` before
  writing any code: the only hit was this task's own specification file. No configuration
  validation helper of any kind existed in the solution. Confidence: **Confirmed**.
- **Line numbers re-verified before editing (task file was accurate):**
  `V.SMART/V.SMART.Api/Program.cs` is **118** lines, not the 119 the task file states; `:20`,
  `:48-54`, `:56-57`, `:58`, `:60-74`, `:82-83`, `:103`, `:113-116` all held as cited.
  `V.SMART/V.SMART.Web/Program.cs` `:180`, `:192`, `:226-227` all held.
- **The pre-rotation plaintexts needed for the SHA-256 deny-list are recoverable from git
  history**, so no session ever needs to be given them: the connection strings from
  `c12c5b2` (`V.SMART.Web/appsettings.json`, both `MigrationData` factories,
  `MauiProgram.cs`) and `6dbf4b4`, and the JWT secret from `44314ed^`
  (`docs/kb/risks/technical-debt-register.md`, before that commit redacted it). Six distinct
  connection-string values and one secret. **Negative result:** the pre-M0-03-01
  `V.SMART/V.SMART.Api/appsettings.json` value is *not* recoverable — that file was never
  committed with a connection string (`git rev-list --all` over the path, checked for
  `Server=`, returns nothing). Confidence: **Confirmed**.
- **Neither host needs a reachable SQL Server to start**, which is what made the six manual
  startup cases testable here: `AddDbContext` does not open a connection at startup, so a
  host with valid-shaped but non-resolving configuration reaches "Application started".
  Confidence: **Confirmed** (observed, 2026-08-18).

**M0-14 amendment (2026-08-18) — negative result on `DetailedError` binding. Re-verified
after M0-03-01 and M0-03-03 both landed; do not re-derive.**

```yaml
Finding:        Grepped case-insensitively for "DetailedError" across V.SMART/; exactly two
                hits, no binding code anywhere in the solution. The appsettings.json key is
                read by nothing.
Evidence:       git grep -in "DetailedError" -- "V.SMART/" ->
                V.SMART/V.SMART.Web/Program.cs:198 (options.DetailedErrors = true; — the
                hardcoded literal, inside the AddServerSideBlazor block starting at line 196)
                V.SMART/V.SMART.Web/appsettings.json:14 ("DetailedError": true — dead key)
Business rule:  n/a
Confidence:     Confirmed (the two hits, and the absence of any binding code)
Last verified:  2026-08-18
```

Resolution taken: `Program.cs:198` now reads
`options.DetailedErrors = builder.Environment.IsDevelopment();`; the dead
`"DetailedError": true` key was deleted from `appsettings.json` rather than bound, since it
was proven dead. See KB-060 R-20 (resolved).

**M0-08 amendment (2026-08-17) — negative result, re-verified after M0-00. Do not re-derive
this.** No build output, IDE state or dependency directory is tracked by git. R-14's original
"committed" claim is contradicted by `git ls-files`: **2,451** tracked paths (2,162 at the
2026-08-12 audit; the increase is M0-00's `frontend/`, `docs/`, `.github/` plus the later
`V.SMART.Api` and M0-15 commits), and
`git ls-files | grep -E -i "(^|/)(bin|obj|dist|node_modules|\.angular|\.vs|out-tsc|bazel-out|packages)/|\.(user|suo|userosscache|rsuser)$|\.db-lock$|(^|/)TestResults/|\.vsidx$"`
produces **no output**. The named paths exist on disk and are ignored — originally via
`frontend/vsmart-erp/.gitignore:4,10,32` and `.gitignore:9,37`; as of M0-08 all of them also
resolve against the **root** `.gitignore` alone (`:381` `**/dist/`, `:382` `**/.angular/`,
`:286` `node_modules/`, `:37` `.vs/`, `:9` `*.user`, `:30,31` `[Bb]in/`/`[Oo]bj/`), verified
by moving the nested file aside and re-running `git check-ignore -v`. Piping all 2,451 tracked
paths through `git check-ignore -v --stdin` returns nothing, so no rule shadows a tracked
file. **No history rewrite is required for R-14.** Enforcement is now mechanical:
`tools/check-no-build-output.sh` (exit 0 clean, proven to exit 1 on a deliberately force-added
`V.SMART/dist/` file, which was then fully reverted). Confidence: **Confirmed**.
| INV-034 | Repository visibility of `ErpStore/NexERP_B` — timeline: reported public (flawed test) → corrected to private → **owner deliberately made it public again**, same day | Complete | `git -c credential.helper= ls-remote` (private: fails demanding auth; public: succeeds, exit 0) vs. plain `git ls-remote` (always silently succeeds via cached credentials — not a valid test on its own); unauthenticated `curl https://api.github.com/repos/ErpStore/NexERP_B` (private: 404; public: 200); `git config --system --get-all credential.helper` (`manager`) | [KB-085 §Repository visibility correction](execution/M0-00-baseline-decisions.md#repository-visibility-correction-inv-034) | 2026-08-12 |
| INV-027 | Stored-procedure DDL capture (all 94) | **Complete** (2026-08-13, M0-01-02 half B/C — DDL actually landed and was verified, not just the tooling) | Reference-vs-scripted reconciliation (M0-01-01, unchanged): 94 referenced, 13 declared, 11 `scripted`, 1 `case_mismatch`, 1 `unreferenced`, 82 `missing` — worklist `db/stored-procedures/manifest.csv`. **Live-database capture (M0-01-02, this update):** operator PavanKunar ran `db/tools/Export-StoredProcedures.ps1` against `NexGenErpDb` (`DESKTOP-FIIBE97\SQLEXPRESS`, SQL authentication) on 2026-08-13. **78 of 82** `missing` procedures captured cleanly into `db/stored-procedures/`; **4 negative results** (genuinely absent, not a tool defect — independently cross-checked against the source text): `Sp_BomAnalysis`, `Sp_Print_Estimation`, `Sp_Print_Receipts`, `Sp_Print_SingleProcessInspection` — escalated in `db/stored-procedures/CAPTURE-STATUS.md` for a human dead-code-vs-latent-defect decision, per procedure. `db/tools/verify-capture.sh` exits 0 (0 hard failures, 4 recorded warnings for the negative results). **Provenance is not a clean single-tenant capture — read before reusing this finding:** `NexGenErpDb` was empty of procedures until the operator manually deployed a script (`AllSp.sql`, local, not committed) originally scripted from a *different* database, `IQSMARTDEMO_DB_2025-26` (a demo tenant reachable via the connection string already commented out in `ApplicationDbContextFactory.cs`). The DDL's actual origin is `IQSMARTDEMO_DB_2025-26`, relayed through `NexGenErpDb`, not a capture directly from a nominated production tenant. Full detail in `db/stored-procedures/CAPTURE-STATUS.md`, "Provenance caveat". **Tooling fix recorded the same day:** 3 of the 78 initially failed ("unrecognized leading statement") because their deployed definitions carry a leading `-- ====...` comment before `CREATE PROCEDURE` — `Export-StoredProcedures.ps1`'s regex didn't tolerate a leading comment even though `verify-capture.sh`'s own spec already does; fixed, re-captured cleanly (see git history). **What this leaves open, explicitly not this investigation's job:** whether `IQSMARTDEMO_DB_2025-26` is representative of a real production tenant is Q-14 (open-questions.md), owned by M0-02 — this capture is now that task's input, not its answer. | [KB-102](architecture/stored-procedure-inventory.md), [db/stored-procedures/CAPTURE-STATUS.md](../../db/stored-procedures/CAPTURE-STATUS.md) | 2026-08-13 |
| INV-037 | `UserRight`/`Screens` uniqueness, duplicate-row risk, and rights write sites | Complete | `Data/ApplicationDbContext.cs` (`HasIndex`/`HasAlternateKey` grep — 5 hits, **none** on either entity; `HasData` seed at `:1151`), `Migrations/ApplicationDbContextModelSnapshot.cs:4676-4680,9127-9145,25933-25945`, `Migrations/20260217110637_InitialCreate.cs:563-576,7204-7237,9796-9803`, `Data/Master/Admin_Module/UserRight.cs`, `Data/Master/MasterScreeenManagement_Module/Screens.cs`, `BusinessLayer/…/AdminService/UserRightService.cs`, `BusinessLayer/…/AdminService/UserService.cs:442-464`, `BusinessLayer/…/HRMasterService/EmployeeService.cs:185-191`, `Pages/Master_Module_pages/UserRights_Pages/UserRights.razor:299,446,462`, `Pages/Master_Module_pages/Employee_Pages/EmployeeUpsert.razor:918,921`, `Pages/Master_Module_pages/Identity_Pages/Login.razor:345-349`, `V.SMART.Api/Controllers/AuthController.cs`, `V.SMART.Api/Program.cs`, `V.SMART.Api/Auth/JwtTokenService.cs`, `Services/CurrentUserService.cs:50-66`, `Services/MultiCompanyService/TenantProvider.cs:25-58` | [KB-105](architecture/server-side-authorization-spec.md). **Negative results:** no unique constraint or alternate key on `UserRight` — only non-unique FK indexes; no index of any kind on `Screens` beyond its PK, and `ScreenName` is `nvarchar(max)` so it cannot be one; no collation configured anywhere in the model or migrations; **no code path writes a `Screens` row**. **Positive:** all 152 seeded `ScreenName`s unique (also case-insensitively), `Id == ScreenCode` throughout; 5 `UserRight` write sites, **all in the Blazor host, none in the API**. Whether duplicates exist in live tenant DBs stays **Unknown** — Q-27 | 2026-08-18 |

**M2-A01-02 amendment to INV-037 (2026-08-20).** `M2-A01-02`'s independent validation
confirmed the 152-name uniqueness finding above by direct comparison rather than by trusting
it: `V.SMART/V.SMART.Api/Authorization/ScreenCatalogue.cs`'s compile-time copy of the seeded
screen names diffs **identical** against the live `Screens` seed at
`V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs:1152-1327`, including the canonical seed
typos. Q-27 (duplicate `(UserId, ScreenId)` rows in a live tenant database) remains
**Unknown** — the dev tenant reachable in that session was not queried for it.

**M2-A01-03 amendment to INV-037 (2026-08-20) — the `UserRight` write-site enumeration,
re-verified for cache invalidation.** Re-run against current code rather than trusted:
`git grep --untracked -n "UserRights\." -- V.SMART` returns exactly **six write statements
across five sites**, unchanged from 2026-08-18.

```yaml
Finding:        5 call sites write UserRight rows (6 statements). 0 run in the API process
                and are invalidated explicitly; 5 run only in the Blazor host
                (V.SMART.Web) and are bounded by the cache TTL alone.
                IUserRightsProvider.Invalidate(tenantId, userId) exists and is correct, but
                has NO caller that can ever fire today — it is infrastructure for a future
                API-side writer.
Evidence:       V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/MasterService/AdminService/UserRightService.cs:77   (CreateRangeAsync, in SyncRightsForUserAsync:32; sole caller Pages/Master_Module_pages/Identity_Pages/Login.razor:348 — Blazor)
                V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/MasterService/AdminService/UserService.cs:464       (CreateAsync — Blazor)
                V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/MasterService/HRMasterService/EmployeeService.cs:191 (DeleteAsync — Blazor)
                V.SMART/V.SMART.Shared/Pages/Master_Module_pages/Employee_Pages/EmployeeUpsert.razor:921                  (DeleteAsync — Blazor)
                V.SMART/V.SMART.Shared/Pages/Master_Module_pages/UserRights_Pages/UserRights.razor:446, :462              (UpdateAsync / CreateAsync — Blazor)
Business rule:  BR-AUTH-002
Confidence:     Confirmed
Last verified:  2026-08-20
```

**Negative results, all Confirmed, all recorded so no future session repeats them:**

- **No `UserRight` write exists anywhere in `V.SMART.Api`.**
  `git grep --untracked -n "UserRightService\|IUserRightService\|EmployeeService\|UserService" -- V.SMART/V.SMART.Api`
  matches only prose in comments (`Auth/ApiAuthStateProvider.cs:8`;
  `Authorization/ScreenRightAuthorizationFilter.cs:74,75,165`; `Program.cs:132`). This is why
  the invalidation count above is 0, not a wiring omission.
- **No `UserRight` write bypasses the repository.**
  `git grep --untracked -n "Set<UserRight>\|UserRight>(" -- V.SMART` (excluding `Migrations/`)
  returns only the `DbSet` (`Data/ApplicationDbContext.cs:148`), the navigation property
  (`Data/Master/Admin_Module/User.cs:123`), the AutoMapper profile
  (`Mappings/MasterMapping/AdminProfile/UserRightsMapping.cs:24`) and the read
  (`Repository/MasterRepository/Admins/UserRightsRepository.cs:24`).
- **No stored procedure touches `UserRight`.** `grep -ril userright` across
  `db/stored-procedures` and `Existing Store Procedures` returns nothing — there is no
  out-of-band SQL writer that would evade both the inventory and any invalidation.

Consequence, and it must not be softened: **the 60-second TTL is the only staleness bound
that exists** (KB-105 §8.4, §8.6; F-7; Q-29).

> **Context note, 2026-08-20:** the npm-registry measurements in this amendment (Vite 8,
> `@mantine/core` 9.5.1, react-router 8.3.0 …) were **true when measured** and are kept as
> measurements. **They are no longer the plan** — [ADR-007](decisions/ADR-007-angular-stack.md)
> replaced React with Angular + PrimeNG. What still transfers is the *method*, which is why the
> block is worth keeping: `npm create <tool>@latest` emits whatever is current, and pinning a
> stack means measuring the registry rather than trusting a task file written days earlier. The
> `.gitignore` findings below are framework-independent and still hold — note `**/.angular/`
> was already covered, which now matters more than it did.

**M2-C01 amendment to INV-029 (2026-08-19) — the post-M0 `.gitignore` / `.github/workflows/`
state, and the repository's first React project.** M2-C01's task file was written on
2026-08-12 and asserts that `.github/workflows/` is empty and that `.gitignore` ignores only
`node_modules/`. **Both are false now**; M0-07 and M0-08 landed in between. Confirmed by
direct inspection on 2026-08-19 before any file was written.

```yaml
Finding:        The React application project frontend/nexgen-web/ now exists, and npm ci,
                typecheck, lint, format:check, test, coverage, build and e2e are verified
                working commands on this workstation (node v24.19.0, npm 11.17.0). CI gained
                a blocking `frontend` job and a non-blocking `frontend-e2e` job. Neither has
                ever run on a GitHub-hosted runner.
Evidence:       frontend/nexgen-web/package.json, frontend/nexgen-web/package-lock.json,
                .github/workflows/ci.yml (jobs: build, frontend, frontend-e2e),
                docs/kb/execution/prompt-template.md "Verified frontend commands"
Business rule:  n/a
Confidence:     Confirmed
Last verified:  2026-08-19
```

- **Pre-existing state, re-verified rather than assumed (Confirmed).** `.github/workflows/ci.yml`
  exists (M0-07), 213 lines, one `build` job on `windows-latest` — so M2-C01's "if ci.yml does
  not exist, stop and report Blocked on M0-07" branch did **not** fire. `.gitignore` was 385
  lines: M0-08's block at `:381-385` already covers `**/dist/`, `**/.angular/`, `**/out-tsc/`,
  `**/coverage/`, and `:286` covers `node_modules/` — i.e. most of what a Vite/Vitest tree
  produces was already ignored.
- **Negative result — do not re-derive.** Grepped `.gitignore` for `env`, `playwright`,
  `test-results` and `.vite`: **no rule for any of them**, and in particular **no `.env` rule
  anywhere in the file**. In a public repository (R-01) that meant a developer's `.env` was
  committable. M2-C01 added `.env`, `.env.*`, `!.env.example`, `**/playwright-report/`,
  `**/test-results/`, `**/.vite/`. Confidence: **Confirmed**.
- **Negative result — `tools/check-no-build-output.sh` does not cover any of them.** Its
  pattern (`tools/check-no-build-output.sh:29`) matches `TestResults/` — the unhyphenated .NET
  spelling — and not `test-results/`, `playwright-report/`, `coverage/` or `.vite/`. So for
  those four paths `.gitignore` is the only defence; the CI hygiene guard is not a backstop.
  Confidence: **Confirmed**.
- **Toolchain drift worth recording (Confirmed, 2026-08-19).** ADR-003's pinned majors are now
  well behind the npm registry, and one pin is unsatisfiable via the task file's own scaffold
  command: `npm create vite@latest` emits **Vite 8** with `@vitejs/plugin-react@6`, which
  peer-requires `vite ^8`. M2-C01 therefore did **not** run that command; it wrote
  `package.json` directly against the ADR-003 majors (`vite@6` + `@vitejs/plugin-react@4.7.0`,
  whose peer range includes Vite 6). Registry `latest` measured the same day: vite 8.2.1,
  @mantine/core 9.5.1, react-router 8.3.0, typescript 7.0.2, eslint 10.8.1, vitest 4.1.11 —
  against ADR-003's Vite 6 / Mantine 7 / Router 7 / TypeScript 5. All the ADR-003 majors remain
  installable today; that will not stay true indefinitely.
- **ESLint 9, not 10 (Confirmed).** `eslint-plugin-jsx-a11y@6.10.2` peers `eslint ^3 … ^9` —
  it does not yet declare ESLint 10 support. `typescript-eslint@8` peers `typescript >=4.8.4
  <6.1.0`, which is a second argument for honouring ADR-003's TypeScript 5 pin rather than
  taking `typescript@latest` (7.0.2).

### INV-029 amendment (2026-08-21, M2-C01 re-scoped) — the Angular toolchain, measured

Supersedes the React block above **as the plan**, not as a measurement: those numbers were true
when taken. This amendment records what the *Angular* scaffold actually installed and ran.

```yaml
Finding:        The Angular application workspace frontend/nexgen-web/ exists, replacing the
                React tree at the same path (deleted in the same commit). npm ci, typecheck,
                lint, format:check, test:ci, build and e2e are verified working commands.
                Angular CLI 22.1.5, @angular/core 22.1.3, primeng 22.1.0, typescript 6.0.3,
                Node v24.19.0, npm 11.17.0 observed on this Windows workstation. CI's
                frontend and frontend-e2e jobs are re-pointed at the Angular commands;
                neither has ever run on a GitHub-hosted runner.
Evidence:       frontend/nexgen-web/package.json, frontend/nexgen-web/angular.json,
                .github/workflows/ci.yml (jobs: build, frontend, frontend-e2e),
                docs/kb/execution/prompt-template.md "Verified frontend commands"
Business rule:  n/a
Confidence:     Confirmed
Last verified:  2026-08-21
```

**What `ng new` actually generates at CLI 22.1.5 (Confirmed — the narrow new investigation this
task was asked to do):**

- `--test-runner` offers `karma` and `vitest` and **defaults to `vitest`**. Karma is opt-in.
  M2-C01's implementation step 10 ("Replace Karma") was obsolete before it was read: nothing
  generated Karma. Vitest runs through the `@angular/build:unit-test` builder — there is no
  separate `vitest.config.ts` and no hand-written transform.
- `--file-name-style-guide` defaults to **`2025`**, which generates `src/app/app.ts` /
  `app.html`, not `app.component.ts`. `2016` was passed here so the tree matches KB-050.
- `--standalone` and `--strict` both default to **true**, so both flags are no-ops. The generated
  workspace is **zoneless**: no `zone.js` dependency and no `polyfills` entry in `angular.json`.
- Generated `tsconfig.json` sets `noImplicitOverride`, `noFallthroughCasesInSwitch`,
  `noPropertyAccessFromIndexSignature` and `noImplicitReturns` but **not** `strict`,
  `strictTemplates` or `noUncheckedIndexedAccess` — all three were added by hand.
- **`baseUrl` is now an error, not a warning.** TypeScript 6.0.3 emits `TS5101: Option 'baseUrl'
  is deprecated and will stop functioning in TypeScript 7.0`, which fails `tsc`. The `@/*` path
  alias is therefore declared with `paths` alone, resolved relative to the tsconfig. Negative
  result worth keeping: the familiar `baseUrl` + `paths` pair no longer compiles.
- No `src/environments/`, no ESLint config and no Playwright config are generated;
  `ng add angular-eslint` (22.1.0) writes `eslint.config.js` and the `lint` architect target.

**Registry facts that contradicted M2-C01's own dependency table (all Confirmed, 2026-08-21):**

- **`@primeng/themes` has no version 22** — its latest is 21.0.4 and `primeng@22.1.0` does not
  depend on it. The v22 preset package is **`@primeuix/themes@3.0.0`**, which is what was
  installed. ADR-007 never names `@primeng/themes`; the error was the task file's alone.
- **`primeng@22.1.0` peer-requires `@angular/cdk ^22.1.0`**, which the table omits. `@angular/cdk`
  is a behaviour-primitives package, **not** `@angular/material`, so installing it does not breach
  the one-component-library rule.
- **`@angular/compiler-cli@22.1.3` peers `typescript >=6.0 <6.1`** while registry `latest` is
  7.0.2. `typescript@latest` breaks the build; `~6.0.2` is correct. Same class of trap as Q-30.
- `vite` and `zod` remain in `package-lock.json` **as transitive dependencies of Angular itself** —
  `vite` under `@angular/build` and `@vitest/mocker`, `zod` under `@angular/cli` and
  `@angular/forms`. Neither is a React remnant and neither can be removed.

**Negative result — `.gitignore` needed no change.** `**/dist/` (`:381`), `**/.angular/` (`:382`),
`**/out-tsc/` (`:383`), `**/coverage/` (`:384`), `**/playwright-report/` (`:395`),
`**/test-results/` (`:396`) and `node_modules/` (`:286`) already cover everything an Angular build
emits — the React-era block M2-C01 added in August turns out to be framework-independent.
`git status --porcelain` after a production build showed no untracked output and
`tools/check-no-build-output.sh` exited 0. The only now-dead pattern is `**/.vite/` (`:397`), left
in place as harmless rather than churned.

### INV-040 amendment (2026-08-20, M2-C00) — the shipped error contract, read from source for the frontend

`M2-C00` re-read `V.SMART/V.SMART.Api/Middleware/ApiProblems.cs` and `ProblemTypes.cs` end to end
so that KB-050's frontend error-handling section could be written against the **shipped** contract
rather than against ADR-002's table alone. No new investigation id: this is the same subject as
INV-040, seen from the client side. **All Confirmed, 2026-08-20:**

- `ApiProblems` is *"the one place that builds every error body this API returns"*
  (`ApiProblems.cs:7-13`); media type `application/problem+json` (`:16`); every body carries
  `traceId` (`:43`, plus `:86` for the `403` and `:131` for the `400`) and `instance` = request path
  (`:40`).
- 409 business-rule refusal carries the service's message into `title` **verbatim — not reworded,
  prefixed or truncated** (`:47-53`). 403 is a frozen shape whose `detail` is composed from the
  *required* screen and right only, with `screen`/`right` extensions (`:66-88`). 400 validation is
  an `errors` dictionary keyed by field with the `DataAnnotations` messages verbatim (`:117-133`).
  401 is deliberately uninformative (`:59-64`); 404 minimal (`:55-57`); 500 carries `traceId` and a
  constant title only, in **every** environment (`:104-115`); tenant-unresolved never leaks a
  connection string or host name (`:90-102`).
- `type` values are stable identifier URIs under `https://api.v-smart.local/problems/`
  (`ProblemTypes.cs:17-41`), **not** dereferenceable URLs — so a client must branch on `type`,
  never on `title`. Recorded in [KB-050](frontend-new/react-architecture.md) §*Error handling*.

**INV-021 re-verification (2026-08-20, M2-C00), pilot facts only — no re-derivation of its file
inventory.** Confirmed still true: `localStorage` JWT (`frontend/vsmart-erp/src/app/core/auth/auth.service.ts:29-35,60-61,66-72`);
`http://localhost:5144` hardcoded in **both** `src/environments/environment.ts:1-5` and
`environment.prod.ts:1-4`, i.e. the production build points at localhost; all four routed
components imported eagerly (`src/app/app.routes.ts:3-6`); Karma + Jasmine still in `package.json`.
**Negative result (Confirmed):** the pilot contains **no** permission gating of any kind — no
permission service, no rights directive, and its only guard checks authentication alone
(`src/app/core/auth/auth.guard.ts:11-20`). The 152 × 5 matrix has to be built from nothing in
`M2-C02`; there is no pilot code to adopt for it.

### INV-035 — Default administrator seed: removal mechanics and per-tenant impact

Produced by **M0-06**, 2026-08-19. Questions 1 and 2 below were **executed with the EF tooling**,
not reasoned about; the exact output is quoted. Three probe migrations were generated and all
three were deleted again, with `ApplicationDbContextModelSnapshot.cs` restored from git each
time.

**Tooling used (previously unverified in this repository — it works):**

```bash
export ConnectionStrings__DesignTimeTenantDb="<any value; no connection is opened>"
dotnet ef migrations add <Name> \
  --project V.SMART/V.SMART.Shared/V.SMART.Shared.csproj \
  --framework net9.0 --context ApplicationDbContext
```

Both switches are required: `--framework net9.0` because `V.SMART.Shared` multi-targets, and
`--context ApplicationDbContext` because the project has more than one `DbContext` (without it
the tool exits with *"More than one DbContext was found. Specify which one to use."*).
`dotnet-ef` 10.0.11 on SDK 10.0.400 drove the EF Core 9.0.5 model without complaint. **The
connection string is never opened** by `migrations add`, so no credential is needed and none was
committed — `Data/MigrationData/DesignTimeConnectionString.cs:23-62` merely requires the
variable to be *set*.

#### Finding 1 (Confirmed, executed) — removing `HasData` scaffolds a `DELETE`, and the `Down()` re-commits the hash

With the seed block deleted, `migrations add RemoveDefaultAdministratorSeed` printed
*"An operation was scaffolded that may result in the loss of data."* and generated, verbatim:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DeleteData(
        table: "Users",
        keyColumn: "UserId",
        keyValue: 1);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.InsertData(
        table: "Users",
        columns: new[] { "UserId", ... 35 columns ... },
        values: new object[] { 1, ..., "Administrator", "<the published PBKDF2 hash>" });
}
```

Two consequences. The `Down()` would have re-committed the very hash the task exists to remove,
and the `Up()` is not safe to ship (Finding 2). Both halves were therefore replaced by hand with
documented no-ops; the migration is retained **only** to carry the updated model snapshot, so
that the next unrelated `migrations add` does not silently scaffold that `DeleteData` into
someone else's migration.

#### Finding 2 (Confirmed) — the delete would NOT be blocked; it cascades silently. The task file's premise was wrong

M0-06's task file (*Investigation Requirements* item 1, *Existing Behavior to Preserve* item 6)
asserted that the global `DeleteBehavior.Restrict` loop would make `UserRight`/`UserAuthority`
rows **block** the delete. The source says otherwise. Exactly three foreign keys reference
`User` in the whole model, and all three are `Cascade`:

| Relationship | Evidence (post-M0-06 snapshot) |
|---|---|
| `UserRight.User` | `ApplicationDbContextModelSnapshot.cs:25924-25928` — `OnDelete(DeleteBehavior.Cascade)`, `IsRequired()` |
| `UserAuthority.User` | `:25937-25941` |
| `UserThemePreference.User` | `:26422-26426` |

The physical DDL agrees: `Migrations/20260217110637_InitialCreate.cs:7196-7200`
(`FK_UserAuthority_Users_UserId`, `onDelete: ReferentialAction.Cascade`) and `:7232-7236`
(`FK_UserRights_Users_UserId`, Cascade). The reason the global loop does not catch them is that
it rewrites a relationship **only** when its behaviour is not already
`Cascade`/`NoAction`/`Restrict`, and EF defaults a **required** FK to `Cascade`.

**The failure mode is therefore silent data loss, not a loud abort** — the opposite of what the
task anticipated, and worse. This is why KB-104's removal step deletes the dependants
explicitly, inside a transaction, with the row counts reviewed before commit.

#### Finding 3 (Confirmed, executed) — a non-constant `HasData` value is accepted but useless

`UserPassword` was temporarily set to `System.Guid.NewGuid().ToString()` and `migrations add`
was run **twice with no source change in between**. Neither run errored. The two generated
migrations were:

```csharp
// M0_06_ProbeB_1
migrationBuilder.UpdateData(table: "Users", keyColumn: "UserId", keyValue: 1,
    column: "UserPassword", value: "91324d66-7aea-4e31-b2a0-e9630d0afbd7");

// M0_06_ProbeB_2 — no source change between the two runs
migrationBuilder.UpdateData(table: "Users", keyColumn: "UserId", keyValue: 1,
    column: "UserPassword", value: "8c121da5-0005-4fd5-9a40-32a73fda66a4");
```

So EF Core 9 does **not** reject a non-deterministic `HasData` value, but the value is evaluated
**once, at scaffold time**, and baked into the migration and the snapshot as a literal. Every
deployment gets the *same* "random" password, it is committed to source, and every subsequent
`migrations add` produces a spurious `UpdateData`. **"Random per-deployment password" is
therefore not achievable through `HasData`.** It must be a runtime bootstrap component — which
is Option A, and which M0-06 deferred rather than half-building.

#### Finding 4 (Confirmed) — no must-change-password mechanism exists

`Data/Master/Admin_Module/User.cs` has no `MustChangePassword`, no `PasswordChangedDate` and no
equivalent. Option B ("keep the row, force a change at login") therefore needs a schema change
plus a Blazor login-flow change, and was rejected on cost inside a 1-day task.

#### Finding 5 (Confirmed) — `UserId == 1` is an undeclared superuser, and that is a second lockout route

Recorded in full as **R-40** in `risks/technical-debt-register.md`, and as Trap 2 in
[KB-104](security/default-admin-removal-runbook.md). Summary: `Login.razor:345-349` auto-grants
`UserId 1` all 152 screen rights on every login via `UserRightService.cs:32-87`; no `UserRight`
rows are seeded by `HasData` at all; rights are deny-by-default (`RightsHelper.cs:7-20`,
`?? false`). A replacement administrator with a different `UserId` therefore authenticates and
sees nothing, and cannot grant rights to itself (`UserRights.razor:215`).

#### Negative results — recorded so nobody greps for these again

| Searched for | Result |
|---|---|
| The committed PBKDF2 hash, across the whole repository (`git grep --untracked`) | **110 files before M0-06; 109 after** (measured with `git grep --untracked -l`). All 109 remaining are pre-existing historical migration files under `V.SMART/V.SMART.Shared/Migrations/` — 108 `*.Designer.cs` model snapshots plus the `InsertData` at `20260217110637_InitialCreate.cs:7562` (which also carries `admin@example.com`, `9999999999` and `Administrator` on that one line). History is never edited; **M0-05** owns removing it from git history. The single non-migration occurrence, `ApplicationDbContext.cs:1142`, is the one M0-06 deleted. |
| Any other seeded credential — `AQAAAAIA` (the ASP.NET Identity v3 hash prefix) in `*.cs`/`*.razor`/`*.json`, excluding `Migrations/` and `ApplicationDbContext.cs` | **Zero hits.** There is no second seeded credential anywhere. |
| `Entity<User>()` outside `Migrations/` | **Exactly one hit** — the block M0-06 removed. No second `User` seed exists, including in `MasterDbContext`. |
| `admin@example.com` / `9999999999` outside `Migrations/` | One hit each, both in the removed block. (Other `999999999` matches are unrelated `[Range(0, 999999999999.999)]` attributes in `AssemblyDefVM.cs` / `AssemblyDefLabourVM.cs`.) |
| The literal `"Administrator"` outside `Migrations/` | Six hits, **none of them a username lookup**: the removed seed (2), the `UserRole` enum member (`Data/Enum/UserRole.cs:5`), a filter-dropdown status string (`AdminService/UserService.cs:208-209`), and two `<AuthorizeView Roles="Administrator,…">` in `Layout/NavMenu.razor:36,148`. **No code anywhere queries `UserName == "Administrator"`.** Removing the row breaks nothing that matches on the name; the numeric `UserId == 1` checks (Finding 5) are the real dependency. |
| `.Migrate()`, `MigrateAsync`, `EnsureCreated` across all of `V.SMART/` | **Zero hits.** Nothing applies migrations at startup. Independently re-verifies M0-01's finding and Q-02: a new migration reaches no tenant by itself. |
| `SyncRightsForUserAsync` across `V.SMART/` | **Exactly three hits** — `IUserRightService.cs:11`, `UserRightService.cs:32`, `Login.razor:348`. **None in `V.SMART.Api`**: `AuthController.cs:47` authenticates without the rights-bootstrap hook. |
| `DeleteData` against table `"Users"` anywhere in `Migrations/` | **Zero hits** — there was no precedent for deleting the admin row. (There *is* precedent for seed deletion generally: `20260228134840_…cs:128-136` and `20260304055613_…cs:34-42` each delete a `UserRights` seed row before the `Screens` row it depended on.) |

#### Unknowns this investigation could not close

- **How many real users each tenant has, and whether any tenant's only administrator is the
  seeded account.** Requires database access. KB-104 section 3 gives the read-only query; the
  deployment owner must run it. Raised as **Q-25**.
- **Whether `UserId 1` has ever logged in in a given tenant** — decides whether the cascade
  would destroy real rows. Same query, columns `RightsRowsForUser1` etc.
- **Q-02** (per-tenant migration rollout) and **Q-12** (production tenant list) remain open.


## Partial

| ID | Topic | Status | Gap | Doc |
|---|---|---|---|---|
| INV-011 | Business rules — cross-module sweep | **Partial** | 12 rules extracted with evidence (calculation, FIFO stock, sales-order lifecycle, auth, approval, reporting, tenancy). Per-module extraction pending — see below. **2026-08-19 (M0-13): BR-STK-001 and BR-STK-002 are now pinned by 25 executable characterisation tests** in `tests/V.SMART.Shared.Tests/Services/StockManagerServiceCharacterisationTests.cs` (suite 36/36 green), covering FIFO order, `RcSubID`/`StoreId` discrimination, re-issue reversal, `AddOrUpdateStockAsync` arithmetic, both delete guards, all five user-facing exception strings, and the R-07 drift asserted numerically. Every line citation in KB-030 for those two rules was re-verified against the working tree on that date and is correct. **2026-08-19 (M0-12-02): BR-CALC-001 and BR-CALC-002 are now pinned by 37 executable characterisation tests** — `tests/V.SMART.Shared.Tests/Services/CalculationServiceCharacterisationTests.cs` (30) and `tests/V.SMART.Shared.Tests/Services/CommonConstantsGstRateTests.cs` (7); suite 73/73 green. They cover all nine algorithm steps, both tax branches (item-wise exercised with three lines at three rate shapes), the three silent early returns, the divide-by-zero guard, the negative-basic boundary, the tax-inclusive TCS base, `MidpointRounding.AwayFromZero` on two distinct midpoints, the signed `RoundOff`, the absence of intermediate rounding, and the R-15 zero-coercion. **Every line citation in KB-030 for those two rules was re-verified against the working tree on that date and is correct** — nothing had moved since M0-13's correction of the `:12-118` → `:12-114` range. **Negative results (Confirmed):** decimal **scale** could *not* be pinned (xUnit's `Assert.Equal(decimal, decimal)` is numerically equal across scales) — raised as **Q-24**; and `GetIGST`'s "not found" answer is indistinguishable from the listed `0.000m` rate, so R-15 can be pinned by value but not by that distinction. **2026-08-19 (M0-09): BR-SO-002 is now FIXED and pinned.** The two unreachable guards in `MfgPoService.CanDeleteSalesOrderAsync` (`:504` tested `hasInvoice` where it computed `hasExpInvoice`; `:525` tested `hasRc` where it computed `hasCR`) were corrected on branch `migration/M0-09-delete-guard-fix`. Both defects were re-verified present at those exact lines immediately before the fix, and the two new tests in `tests/V.SMART.Shared.Tests/Services/MfgPoServiceDeleteGuardTests.cs` were **observed to fail** against the unfixed service, returning `(True, "Sales Order can be safely deleted.")` — the proof the guards were unreachable. BR-SO-001's `MfgPoService.cs:465-565` span and its message strings were re-verified unchanged on that date. **The wider `CanDelete…` audit is NOT part of this** — it remains INV-025 / task M0-10, untouched. **Stays `Partial`** — the other ten rules are unpinned and per-module extraction is still pending. **Negative result (Confirmed):** FIFO tie-breaking on equal `AddDate` could **not** be pinned — `StockManagerService.cs:206` declares no secondary sort key and the InMemory harness (INV-031) sorts stably while SQL Server does not; the suite uses distinct `AddDate` values and asserts nothing about ties. Same for whether SQL Server agrees with InMemory on `RcSubID == null` matching, and for `[Precision]` rounding — all three need a real SQL Server instance. | [KB-030](business-rules/business-rule-inventory.md) |
| INV-047 | Which of `M2-B04`'s 16 `V.SMART.Shared.Pages`-reference directives are safe to delete vs. genuinely load-bearing | **Partial** (2026-08-21, M2-B04, read-only investigation + a since-crashed implementation attempt) | **15 of 16 directives are dead `using` statements, byte-verified against the task file's table, no drift.** One is genuinely load-bearing: `FundTransFilterVM.cs:27` typed `public Bank? Bank { get; set; }` against the **Razor component** `V.SMART.Shared.Pages.Master_Module_pages.Bank_Pages.Bank`, not the EF entity `Banks` (`Data/Master/Accounts_Module/Banks.cs`) — confirmed by `dotnet build` on the unmodified tree failing `CS0246` at that exact line. The predicted `GridMode` case is a false alarm — both enums live inside `@code`, so nothing needs to move. `IApprovalService.cs`/`ApprovalService.cs`'s `using static …Authorization` imports **nothing** (the class declares zero `static` members). `Authorization.razor` itself needs no change — it is the file being decoupled *from*, and its only cross-referenced members are the nested `enum`s already inside `@code`. **Confirmed by build, not just inferred:** an implementation attempt on `migration/M2-B04-decouple-pages-references` removed the 14 dead `using`s, retyped `FundTransFilterVM.Bank` to `Banks` (with a `using V.SMART.Shared.Data.Master.Accounts_Module;` addition), and `dotnet build V.SMART.Api` returned **`0 Error(s)`** — closing the "does the compiler agree" unknown this investigation originally left open. **Both of attempt 1's loose ends are now closed (2026-08-21, M2-B04 attempt 2).** (a) The guard exists: `tests/V.SMART.Shared.Tests/Architecture/NoPagesReferenceFromDomainTests.cs`, two complementary tests (reflection over the compiled assembly, which cannot see an unused `using`; plus a source-text scan, which can). Both were **observed to fail** on a seeded violation — a temporary domain-namespace class carrying the directive *and* a `Bank`-typed property — naming all four reflection violations and the one source violation, and both pass after its removal. Shared suite **86/86 green**, API suite **117/117 green**. (b) **The warning count was noise, not a regression — settled with evidence.** `6695` is the *plain* `dotnet build` figure, which includes the two `NU1608` restore warnings; KB-083's plain-build baseline is also **6,695**. On this branch: `--no-incremental` plain build → **0 errors, 6,694 warnings** (equal to KB-083's `--no-incremental` baseline), and the CI form (`dotnet restore` then `--no-restore --no-incremental -v normal`) → **6,693**, which `tools/compare-warnings.sh … V.SMART.Api` reports as *"Gate: PASSED (equal to baseline)"* against the committed `ci/warning-baseline.json`. Per-file confirmation: **no warning in the build log originates on any line this diff added** — the sole warning in `FundTransFilterVM.cs` is a pre-existing `CS8618` on `TransNo` at `:16`. `V.SMART.Web` `--no-incremental`: **0 errors, 6,697 warnings**, equal to its baseline. **Why this stays `Partial`:** two things remain genuinely unverified, neither closable from this workstation — whether the **MAUI head** still compiles (KB-083: not build-verified here; not attempted), and whether `FundTransFilter.Bank` was ever meant to carry the `Banks` entity or is dead scaffolding, now filed as **Q-55** (Confirmed negative result: the property is referenced by *nothing* outside its own declaration, so the retype is behaviour-neutral, but intent is not recoverable from source). **The approval-workflow manual regression (BR-APPR-001) was also not run** — it needs a running Blazor app with `UserAuthority` rows. | [`tasks/M2-B04.md`](execution/tasks/M2-B04.md#execution-record-2026-08-21), [`task-tracker.md`](execution/task-tracker.md) footnote ²⁸ |
| INV-030 | Stored-procedure drift across tenant databases (Q-14) | **Partial** (2026-08-17, M0-02 tooling half) | **The question is not answered — it is undecided, which is not the same as "no drift".** Method and database-free tooling delivered: `db/tools/list-deployed-procedures.sql` extended with *Query B* (`FINGERPRINT_QUERY_VERSION 2`) emitting `schema_name`, `procedure_name`, `create_date`, `modify_date`, `definition_length`, **`hash_raw`** and **`hash_normalised`** — the pre-existing single `DefinitionSha256Hex` (CR/TAB-stripped only) was neither, a Confirmed gap found by this task; plus `db/tools/compare-tenant-fingerprints.sh` (classifies `identical`/`cosmetic`/`divergent`/`missing_in_tenant`/`extra_in_tenant`, prints the arithmetic, aborts loudly on header mismatch, malformed row, `NULL` hash or duplicate row — each of those failure modes exercised against synthetic fixtures on 2026-08-17, no database, no fabricated CSV in `db/drift/`), `db/RUNBOOK-tenant-drift-check.md` and `db/drift/README.md`. **Negative result, Confirmed:** `db/drift/` contains no tenant fingerprint, so zero tenants have been compared. **Blocker:** a DBA holding `VIEW DEFINITION` on **≥2** tenant databases, plus a working tenant list (Q-12 unanswered); a session may not acquire or reuse a credential. **Owner:** DBA — first candidate operator **PavanKunar** (ran the M0-01-02 capture), with the migration lead to resolve which database the "baseline" label denotes given the `IQSMARTDEMO_DB_2025-26` → `NexGenErpDb` provenance caveat in `db/stored-procedures/CAPTURE-STATUS.md`. Do **not** re-derive the tooling; re-open only to run the comparison once CSVs land. **Status of the question, 2026-08-18: Q-14 EXPLICITLY DEFERRED by Vivek** (repository owner / migration lead), the named owner, closing M0-02 via [KB-080 §7](execution/README.md)'s "answered **or** explicitly deferred with reason" path. **This row stays `Partial`, deliberately** — deferring the question does not complete the investigation: zero tenants have been fingerprinted and zero compared, so the drift classification is *undecided*, never "none". Reopen on any CSV landing in `db/drift/`, or on any per-tenant report / statutory-document surprise; the tooling is complete and must not be re-derived. | [KB-103](architecture/stored-procedure-drift.md) |

**M0-10 amendment to INV-011 (2026-08-21) — a negative result, deliberately not a new rule.**
The delete-guard audit ([KB-061](risks/delete-guard-audit.md)) read 79 guards and extracted
**no new `BR-` rule**, which is itself the finding. BR-SO-001 says a Sales Order cannot be
deleted once a downstream document exists; the audit confirms the **converse is not
enforced** — the Sales DC, Tax Invoice and Export Invoice that block the order's deletion can
each be deleted with no guard at all (`MfgDcService.cs:322`, `MfgInvService.cs:936`,
`ExpInvService`). Three further guards can never refuse at all, and a fourth refuses on only
one condition (R-62). Whether those are
intended permissions or missing rules **cannot be decided from code** — raised as **Q-62** and
**Q-64** rather than written up as rules. **Do not infer a rule from a guard's absence**: for
the per-module sweep, the 29 unguarded delete paths in KB-061 §5.1 are open questions, not
"deletion is always allowed".

## Scheduled — run one module ahead of its migration

Deliberately deferred: extracting all rules now would produce documentation that goes stale
before it is used.

| ID | Topic | Primary sources | Scheduled for |
|---|---|---|---|
| INV-012 | Document numbering + financial-year suffixes | ~20 `SELECT TOP 1 …` repositories, `DcRunningNumber`, `InvoiceAutoRunningNumber`, `FinancialYearHelper.cs` | Phase 2 (blocks R-12) |
| INV-013 | Balance-quantity derivation across `Ref*SubId` chains | services + `@code` | Phase 3.5 |
| INV-014 | Payroll calculation | `HumanResourceService/PayrollService/SalaryService.cs` | Phase 4.9 |
| INV-015 | e-Invoice / e-Way payload construction and error handling | `E_Invoice/**`, `EinvoiceDatabaseService.cs` (2,136 LOC) | Phase 4.5 |
| INV-016 | Costing / labour-cost rules | `CostingService.cs`, `AssemblyDefLabourService.cs` (1,839 LOC) | Phase 3.2 |
| INV-017 | Route-card operation sequencing and WIP | `PlanningService/RouteCardService.cs` (1,934 LOC) | Phase 4.3 |
| INV-018 | Subcontract material reconciliation (`SubConGRNTrack`) | `SubConGRNService.cs` (5,631 LOC) | Phase 4.6 |
| INV-019 | Labour DC outgoing rules | `LabourDcOutgoingService.cs` (6,112 LOC) + 6,528-LOC page | Phase 4.7 |
| INV-020 | TDS and advance adjustment | `AccountsService/**` | Phase 4.8 |
| INV-024 | `@code` triage per module (presentation / data / business) | `Pages/**` | one module ahead of each migration wave |
| INV-026 | Live database index inventory vs the EF model | production tenant DB | Phase 2 (blocks R-13) |

> **INV-025 left this table on 2026-08-21**, completed by task **M0-10**; its row is now in
> *Completed investigations*. Its published scope — *"all ~40 `CanDelete…Async`"* — was wrong
> in three ways, recorded here so the mistake is not repeated for a sibling investigation:
> the count was **79**, not ~40; the `Async` suffix misses three guards; and the `CanDelete`
> prefix that KB-060 recommended as the fix still misses **fifteen** more. Scope guard work by
> **return shape**. Output: [KB-061](risks/delete-guard-audit.md).

## Reserved ids — allocate from here

**This table is the sole authority on INV id ownership.** Where a task file names a
different id, the table wins. Three independent sessions claimed INV-030 simultaneously on
2026-08-12; pre-allocation exists so that cannot recur.

| ID | Reserved for | Task | Status |
|---|---|---|---|
| INV-030 | Stored-procedure drift across tenant databases (Q-14) | M0-02 | **Allocated and in use — `Partial`, see the Partial table above; Q-14 deferred 2026-08-18 (owner Vivek), row stays `Partial`** |
| INV-031 | Test-harness feasibility — can `ApplicationDbContext` be hosted in a test process, and under which EF provider? (`ToView(null)` × 65 makes InMemory doubtful) | M0-12-01 | **Allocated and used — `Complete`, see the Completed table above (M0-12-01, 2026-08-19). The parenthesised doubt was wrong: InMemory works, Sqlite fails.** |
| INV-032 | Decimal representation across the HTTP wire — format, precision source, rounding mode | M2-C10 | Reserved |
| INV-033 | Screen-name → route mapping for permission-filtered navigation | M2-C03 | Reserved |
| INV-034 | Repository visibility correction (moved to Completed table above) | M0-00 | Completed |
| INV-036 | Testing an EF-backed business service through `IUnitOfWork` (moved to Completed table above) | M0-13 | Completed |
| INV-037 | `UserRight`/`Screens` uniqueness, duplicate-row risk, and rights write sites | M2-A01-01 | **Claimed and complete 2026-08-18 — moved to the Completed table above** |
| INV-039 | DI composition drift across the three hosts' `Program.cs`/`MauiProgram.cs` | M2-B07 | **Claimed and complete 2026-08-19 — moved to the Completed table above** |
| INV-035 | Default administrator seed: removal mechanics and per-tenant impact | M0-06 | **Claimed and `Complete` 2026-08-19 — moved to the Completed table above** (merged to `master` 2026-08-25). It disproved the task file's `Restrict` premise: all three FKs to `Users` are `Cascade`. |
| **INV-038** | **no longer reserved** — `M0-06` claimed only INV-035; INV-038 was never used and is free | — | Free |
| INV-040 | Business-rule refusal signalling across the service layer, and how a `409` is produced | M2-A06 | **Claimed and complete 2026-08-20 — moved to the Completed table above** |
| INV-041 | Sort delivery to services whose ordering is hardcoded | M2-B02 | **Claimed and complete 2026-08-20 — moved to the Completed table above** |
| INV-042 | What `role` means in `GET /api/v1/me`, and which user data belongs in the bootstrap response | M2-A07 | **Claimed and complete 2026-08-20 — moved to the Completed table above** |
| INV-044 | **How is `screenCode` actually supplied to stock movements, and where are the real magic numbers?** | M2-B05 | **Claimed and `Complete` 2026-08-21.** **Answer: it is never a literal.** The screen code is resolved at runtime from the database by screen name — `GetScreenCodeByScreenNameAsync`, **166** call sites across **61** Razor pages. Of **244** stock-call expressions inspected, **0** pass an integer literal in the `screenCode` position; the sole `screenCode = <integer>` assignment in the repository is commented out (`SalaryDetails.razor:252`); every `GetQtyBalQtyByStockAddAsync` call passes the variable. **This falsifies R-10 as written and blocks M2-B05 pending re-specification.** The real literals are in the **`storeId`** parameter — **55** sites passing a bare `6`/`7`, confirmed as `REJECTION STORE`/`REWORK STORE` against both a rebuilt and the live database, all 9 `Stores` rows migration-seeded and identical between them → **R-66**. Also re-verified the seed's own assertions: 152 seeded rows, `Id == ScreenCode` for every one, no duplicate `ScreenCode` or `ScreenName` — but only **150** survive to a live database (**R-65**). |
| INV-045 | **How does file handling work today, and why can no part of it be called over HTTP?** | M2-B06 | **Claimed and `Complete` 2026-08-21.** **Answer: both abstractions are Blazor-shaped, and `V.SMART.Api` implements neither.** Upload takes `IBrowserFile` (`V.SMART.Shared/Services/IFileUploadService.cs:1,9`) — a type produced by Blazor's `InputFile`; an HTTP request produces `IFormFile` and there is no adapter, the shapes simply differ. Download is JS interop (`V.SMART.Web/Services/WebFileOpener.cs:31,53`), base64 over a SignalR circuit, which has no HTTP equivalent and needs none. Both are registered only in the two Blazor-ish hosts (`V.SMART.Web/Program.cs:260-261`, `V.SMART/MauiProgram.cs:261-262`); `AddVSmartDomain()` deliberately omits them as host-specific (M2-B07). **Storage layout:** four `baseFolder` cases under `WebRootPath` — `uploads/Logos`, `uploads/IsoLogos` (`WebFileUploadService.cs:40-45`), `uploads/drawings`, `uploads/correspondences` (`:86-90`) — each segmented per tenant by `tenant.Hostname`, sanitised by stripping `Path.GetInvalidFileNameChars()` and lowercasing (`:36-37`, `:80-81`), with a `Guid.NewGuid()` filename prefix (`:50`, `:84`) and a 20 MB `OpenReadStream` ceiling (`:101`). **`SaveFileAsync` returns a pipe-delimited composite** `"{webPath}\|{filePath}"` (`:56-57`) that callers must split; the only caller that splits it is `CompanyService.cs:145` (one call site, `CompanyUpsert.razor:1105`). **Defect confirmed:** `SaveCorresFileAsync` creates the file and leaves the stream copy commented out (`:100-104`), so every Blazor correspondence/drawing upload is **0 bytes** while reporting success → **R-67**. The bytes survive only because `Correspondence.Image` holds a second copy (`Correspondence.cs:14`, `CorrespondenceUpload.razor:306-309`), and the two download screens disagree about which copy to read (`CorrespondenceListByReference.razor:357-363` uses the column; `CorrespondanceList.razor:319-321` opens the empty file). **`IBrowserFile` consumer inventory — 12 files:** 1 interface declaration, 1 business service (`ICompanyService`/`CompanyService`, the only one an endpoint had to call, decoupled by this task), 3 host implementations, 1 commented-out signature (`IExcelTemplateService.cs:27`), the rest Razor pages left alone as UI. **Negative results, all recorded:** grep for `BlobServiceClient`, `S3`, `Azure.Storage` → **no hits**; there is no blob storage, no CDN and no background virus scanning anywhere in the repository. The only extension allow-list that exists is inside a Razor page (`CorrespondenceUpload.razor:213-220`, enforced `:284`) — i.e. client-side, with no server-side equivalent until this task. **Unknown, not guessed:** whether `WebRootPath` is durable in the target deployment — that is **Q-16**, and this task deliberately designed no blob-storage migration. Note also that `WebRootPath` resolves to a *different directory* per host, so the API takes a configurable `FileStorage:Root` to let both hosts share one store. |
| INV-046 | **What consumes `LogUserAction`, is audit coverage uniform, and is the `#if ANDROID \|\| WINDOWS \|\| MACCATALYST` branch ever taken?** | M2-B11 | **Claimed and `Complete` 2026-08-21.** **494 call sites across 202 files, all in `V.SMART.Shared`; zero in `V.SMART.Web` and zero in `V.SMART.Api`** — so no API endpoint writes an audit record today. **Coverage is patchy:** 7 of 18 `Pages/` module folders have zero call sites, and only `HumanResource` is covered elsewhere (by its services). **The `#if` branch is dead:** `DefineConstants` is `TRACE;DEBUG` on *both* TFMs (`-getProperty:DefineConstants`), so `_basePath` is never null — Confirmed, upgrading the previous Inferred. **Negative result:** no `Serilog`/`HealthCheck`/`AddHealthChecks` anywhere under `V.SMART/` before this task. Full detail in the INV-046 section above and in [KB-113](architecture/observability.md). |
| INV-047 | Which of `M2-B04`'s 16 `Pages`-reference directives are safe to delete vs. load-bearing | M2-B04 (renumbered at merge — M2-A07 had already claimed INV-042) | **Claimed 2026-08-21 — `Partial`, see the Partial table above. The directive analysis and the guard test are done; it stays `Partial` for the MAUI-head build (unverifiable here) and Q-55.** |
| INV-048 | Whether a prior investigation covers R-69's confirm-dialog bundle-deferral fix | M2-C13 | **Claimed and `Complete` 2026-08-24.** **Negative result:** grepped `investigation-registry.md` for `confirm-dialog`, `app.component`, `bundle`, `R-69` — only incidental mentions in unrelated entries (e.g. INV-006 focus-defect rows); no prior investigation of the deferral itself. Investigated fresh as part of `M2-C13` rather than reused. One secondary unknown surfaced and left open, not closable read-only: whether PrimeNG's `ConfirmationService.requireConfirmation$` is in fact a plain rxjs `Subject` (vs `ReplaySubject`/`BehaviorSubject`) was not independently re-verified against `node_modules/primeng` source at investigation time — carried as Inferred from the task file's own claim (`M2-C13.md:78-80`). The implementation and its independent validation subsequently exercised this empirically (the mutation test in `tasks/M2-C13.md` § Execution Record proves the queue is load-bearing), which is consistent with the Subject claim but is not the same as reading the PrimeNG source. |
| INV-049 | **Permission-matrix harness: the endpoint-discovery mechanism, the exempt allow-list policy, the rights-fixture/cache-reset mechanism, the case count and the suite runtime** | M2-A03 | **Claimed and `Complete` 2026-08-24.** **Discovery:** reflection over the real `V.SMART.Api` assembly (`tests/V.SMART.Api.Tests/PermissionMatrix/ApiEndpointDiscovery.cs`) — public, declared, non-`[NonAction]` instance methods on every `ControllerBase`, with metadata composed controller-attributes-then-action-attributes, MVC's own order, because both `ScreenRightAuthorizationFilter.cs:55-56` and `ScreenRightStartupValidator.cs:70-71` take `LastOrDefault()`; a `MethodInfo`-only read would find no controller-level `[RequireScreen]` and pass the API vacuously. **Surface measured on `master` tip `13ee72a`: 6 controllers, 18 actions — 10 gated, 7 `[NoScreenRight]`-exempt, 1 `[AllowAnonymous]`** (the task file's "2 controllers / 6 endpoints" was stale by 3x). **Allow-list policy:** `ExemptEndpointAllowList.cs`, two lists (1 anonymous — `POST /api/v1/auth/login`; 7 screen-right-exempt), compared against the assembly in **both directions**, so exemption by omission is impossible in either direction. **Cache reset:** M2-A01-03's documented zero-TTL bypass, per case, over the real `UserRightsProvider` and a per-case `MemoryCache`; every case asserts exactly one repository call and that no cache entry was written. Driven **400 alternating flips** in one run without a leak (`CacheIsolationTests`), which is the load question M2-A03 required be answered rather than assumed; the enabled-cache control case shows the same flip *is* stale at TTL 60, which is why the bypass is required. **Case count: 60** (10 gated x 6 fixtures), **106 harness tests** in total. **Runtime: whole suite 470 tests / 470 passed / ~6 s; the harness alone 106 tests / ~4-5 s** (measured 2026-08-24 on `migration/M2-A03-permission-matrix-harness`, SDK 10.0.400, warm). **Negative results:** `IDesignTimeModel` is absent from the EF Core assemblies this project references and `db.Model...GetSeedData()` throws on the read-optimised runtime model — the seed is therefore read by `EnsureCreated()` on InMemory and querying `db.Screens`. **Limit recorded, not hidden:** a real-but-wrong screen name (`"Currency Today"`) passes the catalogue check by construction; and the production fail-open direction (`ScreenRightAuthorizationFilter.cs:69-72`, `ScreenRightStartupValidator.cs:83-88`) is unchanged — **Q-71** — so the harness, not the host, is what refuses an unannotated controller today. Evidence: `tests/V.SMART.Api.Tests/PermissionMatrix/*`; `.github/workflows/ci.yml:213-219`; `V.SMART/V.SMART.Api/Authorization/**`. Business rule: BR-AUTH-002. Confidence: **Confirmed**. Full detail: [KB-105 §13](architecture/server-side-authorization-spec.md). |
| INV-050 | **How do the KB-011 service-method conventions map onto REST verbs?** | M2-B03 | **Claimed and `Complete` 2026-08-24 — moved to the Completed table above.** Headline: one-to-one except `Upsert…Async` (one method, two verbs — resolved: both, distinguished by the id at the controller), typeahead `Search…Async` (resolved: its own `GET /search?q=`) and `GetNext…NoAsync` (not an endpoint). Negative result: no bulk convention exists, so the template defines none. Output: [KB-114](api/controller-conventions.md). |
| INV-051 | **Which TypeScript client generator, and what does \`decimal?\` become?** | M2-B10 | **Claimed and \`Complete\` 2026-08-24 — moved to the Completed table above.** Headline: \`ng-openapi-gen\` 1.0.5, the only candidate emitting Angular \`HttpClient\` services so calls flow through the auth interceptor; rejected \`@hey-api/openapi-ts\` (fetch-only, pre-1.0), \`openapi-typescript\` (types only) and \`openapi-generator-cli\` (needs a JRE, not installed here). \`decimal?\` → \`number \| null\`, flagged to M2-C10. Output: [KB-112](api/generated-client.md). |
| **INV-053 +** | **next free** *(INV-052 was claimed by M0-04 on 2026-08-25 — credential usage inventory, moved to the Completed table above. INV-050 was claimed by M2-B03 on 2026-08-24. INV-049 was claimed by M2-A03 on 2026-08-24. INV-042 was double-claimed by `M2-A07` and `M2-B04`; resolved at merge 2026-08-21 — M2-A07 keeps INV-042, M2-B04 became INV-047. INV-045 was claimed by M2-B06, INV-046 by M2-B11, INV-048 by M2-C13. Check `git branch --no-merged master` before claiming)* | — | — |

Before claiming an id: `grep -rn "INV-0[0-9][0-9]" docs/` across **both** `docs/kb/` and
`docs/kb/execution/tasks/`, then add the row here in the same change that uses it. Never
reuse or renumber an id.

## Adding an entry

1. Assign the next `INV-0xx`.
2. Create the document with the standard frontmatter (see any file in `architecture/`).
3. Record every path examined under **Evidence**, including negative results —
   "grepped X, found nothing" is a finding worth not repeating.
4. Tag each claim Confirmed / Inferred / Unknown.
5. Add the row here.
6. Add unresolved questions to [`open-questions.md`](open-questions.md) with a `Q-xx` id.
