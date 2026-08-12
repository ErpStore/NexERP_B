---
doc_id: KB-003
title: Investigation Registry
module: meta
status: active
confidence: n/a
last_verified: 2026-08-12
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
| INV-004 | Authentication, screen rights, approval authority | Complete | `Authentication/Custom AuthenticationStateProvider.cs`, `Repository/MasterRepository/Admins/UserRepository.cs`, `Shared/BaseUserRightsComponent.cs`, `Shared/RightsHelper.cs`, `Data/Master/Admin_Module/*`, `V.SMART.Api/Auth/*` | [KB-013](architecture/auth-and-permissions.md) | 2026-08-12 |
| INV-005 | Multi-tenancy resolution and isolation | Complete | `Services/MultiCompanyService/*`, `Data/TenantInfo.cs`, `wwwroot/config/tenant.json` | [KB-014](architecture/multi-tenancy.md) | 2026-08-12 |
| INV-006 | Existing UI: routing, layout, components, `@code` density | Complete | `Routes.razor`, `Layout/NavMenu.razor` (888 LOC), `Components/` (22), `Pages/` (333 files, 440 routes), measured `@code` share | [KB-015](architecture/frontend-architecture-existing.md) | 2026-08-12 |
| INV-007 | Module inventory and inter-module dependency graph | Complete | `NavMenu.razor`, `Data/` folders, `BusinessLayer/` folders, `Ref*SubId` FK scan | [KB-020](modules/module-inventory.md) | 2026-08-12 |
| INV-008 | Existing API surface | Complete | `V.SMART.Api/**` (2 controllers, 6 endpoints) | [KB-040](api/api-overview.md) | 2026-08-12 |
| INV-009 | Reporting: FastReport + stored procedures | Complete *(amended 2026-08-12)* | `Services/ReportViewer/ReportService.cs`, `ReportExecutor.cs`, `wwwroot/templates/` (104 `.frx`), `Existing Store Procedures/` (13 `.sql`, of which only **12** are called → gap is **82**, not 81). **Scoped** name-extraction command, since the unscoped one now returns 111 by matching this KB's own prose: `grep -rhoE "Sp_[A-Za-z0-9_]+" --include=*.cs --include=*.razor --exclude-dir=obj --exclude-dir=bin V.SMART \| sort -u` | [KB-011](architecture/backend-architecture.md#reporting-subsystem), [ADR-005](decisions/ADR-005-reporting-and-printing.md), R-04 | 2026-08-12 |
| INV-010 | External integrations (e-Invoice, e-Way, IFSC, SMTP, biometric) | Complete | `E_Invoice/**`, `EinvoiceDatabaseService.cs`, `EWayDatabaseService.cs`, `BankService.cs`, URL scan | [KB-011](architecture/backend-architecture.md#integrations-with-external-systems) | 2026-08-12 |
| INV-021 | Angular pilot: scope and value | Complete | `frontend/vsmart-erp/src/**`, `package.json` | [KB-015](architecture/frontend-architecture-existing.md#the-angular-19-pilot-frontendvsmart-erp) | 2026-08-12 |
| INV-022 | Background jobs / scheduled tasks | Complete | grep for `IHostedService`, `BackgroundService`, `PeriodicTimer`, Hangfire, Quartz — **none exist** | [KB-010](architecture/system-overview.md#background-processing) | 2026-08-12 |
| INV-023 | Testing and CI | Complete | no test project in `.sln`; `.github/` has no workflows | [KB-010](architecture/system-overview.md#testing), R-05 | 2026-08-12 |
| INV-029 | Version-control state, repository visibility, and toolchain/build baseline | Complete | `git ls-remote`, `git log`, `git status --porcelain`, `git grep -l "<secret>" HEAD`, `dotnet --list-sdks`, `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` | [KB-080 §6](execution/README.md#findings-from-this-planning-pass-that-changed-m0), [KB-083](execution/prompt-template.md#verified-repository-commands) | 2026-08-12 |

## Partial

| ID | Topic | Status | Gap | Doc |
|---|---|---|---|---|
| INV-011 | Business rules — cross-module sweep | **Partial** | 12 rules extracted with evidence (calculation, FIFO stock, sales-order lifecycle, auth, approval, reporting, tenancy). Per-module extraction pending — see below | [KB-030](business-rules/business-rule-inventory.md) |

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
| INV-025 | Delete-guard audit — all ~40 `CanDelete…Async` for the R-08 copy-paste pattern | `BusinessLayer/**` | Phase 0 |
| INV-026 | Live database index inventory vs the EF model | production tenant DB | Phase 2 (blocks R-13) |
| INV-027 | Stored-procedure DDL capture (all 94) | live tenant DB | **Phase 0 — highest priority** |
| INV-028 | Row-level scoping via `User.StateCodesCsv` | grep `StateCodes` across `Pages/` and services | Phase 2 (blocks Q-08) |

## Reserved ids — allocate from here

**This table is the sole authority on INV id ownership.** Where a task file names a
different id, the table wins. Three independent sessions claimed INV-030 simultaneously on
2026-08-12; pre-allocation exists so that cannot recur.

| ID | Reserved for | Task | Status |
|---|---|---|---|
| INV-030 | Stored-procedure drift across tenant databases (Q-14) | M0-02 | Reserved |
| INV-031 | Test-harness feasibility — can `ApplicationDbContext` be hosted in a test process, and under which EF provider? (`ToView(null)` × 65 makes InMemory doubtful) | M0-12-01 | Reserved |
| INV-032 | Decimal representation across the HTTP wire — format, precision source, rounding mode | M2-C10 | Reserved |
| INV-033 | Screen-name → route mapping for permission-filtered navigation | M2-C03 | Reserved |
| **INV-034 +** | **next free** | — | — |

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
