---
doc_id: KB-003
title: Investigation Registry
module: meta
status: active
confidence: n/a
last_verified: 2026-08-13
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
| INV-004 | Authentication, screen rights, approval authority | Complete *(extended 2026-08-13 by INV-036 — screen-rights enforcement detail for M2-A01-01; INV-004 itself unchanged and still authoritative for the as-is model)* | `Authentication/Custom AuthenticationStateProvider.cs`, `Repository/MasterRepository/Admins/UserRepository.cs`, `Shared/BaseUserRightsComponent.cs`, `Shared/RightsHelper.cs`, `Data/Master/Admin_Module/*`, `V.SMART.Api/Auth/*` | [KB-013](architecture/auth-and-permissions.md) | 2026-08-12 |
| INV-036 | Screen-rights enforcement mechanics — uniqueness, constraints, and the `UserRight` write surface (the three gaps KB-013 does not answer) | Complete | **F-1:** all 152 seeded `Screens.ScreenName` values unique, including case-insensitively; `ScreenCode == Id` for every row; Ids contiguous 1..152; no leading/trailing whitespace; two names contain `&` (`ApplicationDbContext.cs:1150-1151`, extracted mechanically). **F-2 (negative result):** no `HasIndex`, `HasAlternateKey` or unique constraint exists on `UserRight` or `Screens` — the only 5 `HasIndex` calls in `ApplicationDbContext.cs` target unrelated entities (`:581,586,589,594,617`); `Screens.cs:14-15` is `[Required]` only. So F-1's uniqueness is a property of the seed, enforced by nothing. **F-3:** 7 `UserRight` write sites — `UserRightService.cs:77`, `UserService.cs:464`, `EmployeeService.cs:191`, `UserRights.razor:446,462`, `EmployeeUpsert.razor:921`, `Login.razor:348` (indirect). **Also confirmed:** `RightsHelper` matches on a materialised `List<UserRight>` (`UserRightsRepository.cs:22-28` filters by `UserId` only, then `.ToListAsync()`), so screen-name comparison is **ordinal/case-sensitive .NET string equality, not a DB collation question**; and `UserId == 1` receives real full-rights rows via `SyncRightsForUserAsync` (`Login.razor:345-348` → `UserRightService.cs:62-77`) rather than any bypass. **Unknown:** live-tenant duplicate rows (Q-20); whether the API login path syncs rights (Q-21) — `V.SMART/V.SMART.Api/` is absent from the working tree and index | [KB-103](architecture/server-side-authorization-spec.md), [ADR-004](decisions/ADR-004-server-side-authorization.md), R-03 | 2026-08-13 |
| INV-005 | Multi-tenancy resolution and isolation | Complete | `Services/MultiCompanyService/*`, `Data/TenantInfo.cs`, `wwwroot/config/tenant.json` | [KB-014](architecture/multi-tenancy.md) | 2026-08-12 |
| INV-006 | Existing UI: routing, layout, components, `@code` density | Complete | `Routes.razor`, `Layout/NavMenu.razor` (888 LOC), `Components/` (22), `Pages/` (333 files, 440 routes), measured `@code` share | [KB-015](architecture/frontend-architecture-existing.md) | 2026-08-12 |
| INV-007 | Module inventory and inter-module dependency graph | Complete | `NavMenu.razor`, `Data/` folders, `BusinessLayer/` folders, `Ref*SubId` FK scan | [KB-020](modules/module-inventory.md) | 2026-08-12 |
| INV-008 | Existing API surface | Complete | `V.SMART.Api/**` (2 controllers, 6 endpoints) | [KB-040](api/api-overview.md) | 2026-08-12 |
| INV-009 | Reporting: FastReport + stored procedures | Complete *(amended 2026-08-12)* | `Services/ReportViewer/ReportService.cs`, `ReportExecutor.cs`, `wwwroot/templates/` (104 `.frx`), `Existing Store Procedures/` (13 `.sql`, of which only **12** are called → gap is **82**, not 81). **Scoped** name-extraction command, since the unscoped one now returns 111 by matching this KB's own prose: `grep -rhoE "Sp_[A-Za-z0-9_]+" --include=*.cs --include=*.razor --exclude-dir=obj --exclude-dir=bin V.SMART \| sort -u` | [KB-011](architecture/backend-architecture.md#reporting-subsystem), [ADR-005](decisions/ADR-005-reporting-and-printing.md), R-04 | 2026-08-12 |
| INV-010 | External integrations (e-Invoice, e-Way, IFSC, SMTP, biometric) | Complete | `E_Invoice/**`, `EinvoiceDatabaseService.cs`, `EWayDatabaseService.cs`, `BankService.cs`, URL scan | [KB-011](architecture/backend-architecture.md#integrations-with-external-systems) | 2026-08-12 |
| INV-021 | Angular pilot: scope and value | Complete | `frontend/vsmart-erp/src/**`, `package.json` | [KB-015](architecture/frontend-architecture-existing.md#the-angular-19-pilot-frontendvsmart-erp) | 2026-08-12 |
| INV-022 | Background jobs / scheduled tasks | Complete | grep for `IHostedService`, `BackgroundService`, `PeriodicTimer`, Hangfire, Quartz — **none exist** | [KB-010](architecture/system-overview.md#background-processing) | 2026-08-12 |
| INV-023 | Testing and CI | Complete | no test project in `.sln`; `.github/` has no workflows | [KB-010](architecture/system-overview.md#testing), R-05 | 2026-08-12 |
| INV-029 | Version-control state, repository visibility, and toolchain/build baseline | Complete *(visibility finding corrected 2026-08-12 by INV-034 — see below; do not cite INV-029 alone for visibility. Amended 2026-08-13 by M0-08 with a negative result on build-output tracking — see next sentence.)* **Negative result (M0-08, 2026-08-13):** re-ran the tracked-set audit after M0-00 first committed `docs/`, `frontend/` and `.github/` (2,335 tracked paths, up from 2,162) — no build output, IDE state, or dependency directory is tracked. R-14's original "committed" claim is contradicted by `git ls-files`; the named paths (`frontend/vsmart-erp/dist`, `.../node_modules`, `.../\.angular/cache`, `.vs/`, `*.csproj.user`) exist in the working tree and are ignored by `frontend/vsmart-erp/.gitignore:4,10,32` and root `.gitignore:9,37`. No history rewrite is required for R-14; see [KB-060 R-14](risks/technical-debt-register.md) for the closure and the CI guard (`tools/check-no-build-output.sh`) M0-08 added. **Amended again 2026-08-13 by M0-15 half A — the solution-build question INV-029 left open is now partly answered:** `NexGen-ERP---2025-master.sln:6,8,10,12` declares **four** projects while source control holds **three** — `V.SMART\V.SMART.Api\V.SMART.Api.csproj` (`:12`) is absent from the working tree *and* the index and is **not** gitignored (M0-00 G2 deferred it to M0-03-01). So on a **fresh clone** the solution build fails before compilation, structurally, and no amount of MAUI-workload installation fixes it; on the original developer machine, where the untracked directory persists, it loads and the separate workload question applies. This is a **G0 blocker** ("rebuild from source control alone"), owned by M0-03-01. Target frameworks re-confirmed by line (`V.SMART.Shared.csproj:4` multi-targets `net9.0-windows10.0.19041.0;net9.0`; `V.SMART.Web.csproj:4` single `net9.0`; `V.SMART.csproj:4-5` three TFMs plus a Windows-conditional fourth). Also re-confirmed absent at the root: `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `NuGet.config`, `.editorconfig`. **Still Unknown — needs a .NET SDK, which the 2026-08-13 session did not have:** all build timings, error/warning counts and the `MUD0002` share; `tools/measure-build-baseline.ps1` was written to collect them. See [KB-104](execution/M0-15-build-baseline.md). | `git ls-remote`, `git log`, `git status --porcelain`, `git grep -l "<secret>" HEAD`, `dotnet --list-sdks`, `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj`, `git ls-files \| grep -E -i "(^\|/)(bin\|obj\|dist\|node_modules\|\.angular\|\.vs\|out-tsc\|bazel-out\|packages)/\|\.(user\|suo\|userosscache\|rsuser)$\|\.db-lock$\|(^\|/)TestResults/\|\.vsidx$"` | [KB-080 §6](execution/README.md#findings-from-this-planning-pass-that-changed-m0), [KB-083](execution/prompt-template.md#verified-repository-commands), [KB-060 R-14](risks/technical-debt-register.md) | 2026-08-13 |
| INV-034 | Repository visibility of `ErpStore/NexERP_B` — timeline: reported public (flawed test) → corrected to private → **owner deliberately made it public again**, same day | Complete | `git -c credential.helper= ls-remote` (private: fails demanding auth; public: succeeds, exit 0) vs. plain `git ls-remote` (always silently succeeds via cached credentials — not a valid test on its own); unauthenticated `curl https://api.github.com/repos/ErpStore/NexERP_B` (private: 404; public: 200); `git config --system --get-all credential.helper` (`manager`) | [KB-085 §Repository visibility correction](execution/M0-00-baseline-decisions.md#repository-visibility-correction-inv-034) | 2026-08-12 |
| INV-035 | Credential usage inventory — every location each exposed credential (R-01, R-02, R-39) is consumed from | Complete *(repository-side; rotation itself is M0-04's operational half, not this investigation's)* | file:line citations for C-1 through C-7 in [`docs/runbooks/credential-rotation.md`](../runbooks/credential-rotation.md); a broader credential-shaped-literal sweep across `.cs`/`.json` outside the known files — negative result, none found; SMTP/biometric — negative result; the one other external HTTP integration (`BankService.cs:501`, IFSC lookup) is a public keyless API, not a credential. **New finding, not in the original inventory:** traced the e-Invoice/e-Way gateway credential's actual runtime source to `Companies.APIEinvoiceLicenseKey` (encrypted, per tenant), decrypted at request time with a **hardcoded AES key/IV** committed at `LicenseProductKey.cs:28-29` — recorded as **R-39**, since resetting the gateway password alone does not close this exposure. Unresolved gaps recorded as Unknown, not guessed: how many `Tenants` rows exist and which logins they embed (needs DB access); CI/CD variables, IIS config, `.pubxml` (none found in-repo; unknown outside it); developer `user-secrets` stores; SQL Agent jobs (pre-existing Q-15, not re-derived) | [`docs/runbooks/credential-rotation.md`](../runbooks/credential-rotation.md), [KB-060 R-01, R-02, R-39](risks/technical-debt-register.md), [M0-04](execution/tasks/M0-04.md) | 2026-08-13 |

## Partial

| ID | Topic | Status | Gap | Doc |
|---|---|---|---|---|
| INV-011 | Business rules — cross-module sweep | **Partial** | 12 rules extracted with evidence (calculation, FIFO stock, sales-order lifecycle, auth, approval, reporting, tenancy). Per-module extraction pending — see below | [KB-030](business-rules/business-rule-inventory.md) |
| INV-027 | Stored-procedure DDL capture (all 94) | **Partial** *(repository half complete 2026-08-13, M0-01-01; capture tooling + runbook complete 2026-08-13, M0-01-02 half A)* | Repository-side reconciliation is Complete: 94 referenced names classified against the 13 declared (11 `scripted`, 1 `case_mismatch`, 82 `missing`, 1 `unreferenced`), with `file:line` evidence for every referenced name and a re-runnable generator (`db/tools/sp-inventory.sh`). M0-01-02 half A delivered `db/tools/Export-StoredProcedures.ps1` (UNVERIFIED — never run against a database), `db/tools/list-deployed-procedures.sql`, `db/tools/verify-capture.sh` (tested against 7 synthetic failure modes plus a synthetic pass, all correct), `db/RUNBOOK-capture-stored-procedures.md`, and a pre-filled `db/stored-procedures/CAPTURE-STATUS.md`. **Gap, still open:** the actual DDL for the 82 `missing` procedures has **not** been captured — that requires a named DBA with `VIEW DEFINITION` on a nominated live tenant database (M0-01-02 half B), which an AI session cannot perform. `db/stored-procedures/` contains 0 `.sql` files as of this entry. Negative/structural results recorded: `Sp_Print_PurchaseOrder.sql` is `unreferenced` dead DDL; no `.cs`/`.razor` grep can see FastReport `.frx` data-source bindings, procedure-to-procedure calls, or names composed at run time (checked with a bounded heuristic for the last of these — no output, but not exhaustive) | [KB-102](architecture/stored-procedure-inventory.md), R-04, [M0-01-02](execution/tasks/M0-01-02.md) |

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
| INV-034 | Repository visibility correction (moved to Completed table above) | M0-00 | Completed |
| INV-035 | Credential usage inventory (moved to Completed table above) | M0-04 | Completed |
| INV-036 | Screen-rights enforcement mechanics (moved to Completed table above) | M2-A01-01 | Completed |
| **INV-037 +** | **next free** | — | — |

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
