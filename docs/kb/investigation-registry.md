---
doc_id: KB-003
title: Investigation Registry
module: meta
status: active
confidence: n/a
last_verified: 2026-08-18
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
| INV-029 | Version-control state, repository visibility, and toolchain/build baseline | Complete *(visibility finding corrected 2026-08-12 by INV-034 — see below; do not cite INV-029 alone for visibility. **Solution-build gap closed 2026-08-17 by M0-15** — see below.)* | `git ls-remote`, `git log`, `git status --porcelain`, `git grep -l "<secret>" HEAD`, `dotnet --list-sdks`, `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj`, `dotnet build V.SMART/V.SMART.Web/V.SMART.Web.csproj`, `dotnet build NexGen-ERP---2025-master.sln`, `dotnet workload list` | [KB-080 §6](execution/README.md#findings-from-this-planning-pass-that-changed-m0), [KB-083](execution/prompt-template.md#verified-repository-commands), [KB-086](execution/M0-15-build-baseline.md) | 2026-08-18 |

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

## Partial

| ID | Topic | Status | Gap | Doc |
|---|---|---|---|---|
| INV-011 | Business rules — cross-module sweep | **Partial** | 12 rules extracted with evidence (calculation, FIFO stock, sales-order lifecycle, auth, approval, reporting, tenancy). Per-module extraction pending — see below | [KB-030](business-rules/business-rule-inventory.md) |
| INV-030 | Stored-procedure drift across tenant databases (Q-14) | **Partial** (2026-08-17, M0-02 tooling half) | **The question is not answered — it is undecided, which is not the same as "no drift".** Method and database-free tooling delivered: `db/tools/list-deployed-procedures.sql` extended with *Query B* (`FINGERPRINT_QUERY_VERSION 2`) emitting `schema_name`, `procedure_name`, `create_date`, `modify_date`, `definition_length`, **`hash_raw`** and **`hash_normalised`** — the pre-existing single `DefinitionSha256Hex` (CR/TAB-stripped only) was neither, a Confirmed gap found by this task; plus `db/tools/compare-tenant-fingerprints.sh` (classifies `identical`/`cosmetic`/`divergent`/`missing_in_tenant`/`extra_in_tenant`, prints the arithmetic, aborts loudly on header mismatch, malformed row, `NULL` hash or duplicate row — each of those failure modes exercised against synthetic fixtures on 2026-08-17, no database, no fabricated CSV in `db/drift/`), `db/RUNBOOK-tenant-drift-check.md` and `db/drift/README.md`. **Negative result, Confirmed:** `db/drift/` contains no tenant fingerprint, so zero tenants have been compared. **Blocker:** a DBA holding `VIEW DEFINITION` on **≥2** tenant databases, plus a working tenant list (Q-12 unanswered); a session may not acquire or reuse a credential. **Owner:** DBA — first candidate operator **PavanKunar** (ran the M0-01-02 capture), with the migration lead to resolve which database the "baseline" label denotes given the `IQSMARTDEMO_DB_2025-26` → `NexGenErpDb` provenance caveat in `db/stored-procedures/CAPTURE-STATUS.md`. Do **not** re-derive the tooling; re-open only to run the comparison once CSVs land. **Status of the question, 2026-08-18: Q-14 EXPLICITLY DEFERRED by Vivek** (repository owner / migration lead), the named owner, closing M0-02 via [KB-080 §7](execution/README.md)'s "answered **or** explicitly deferred with reason" path. **This row stays `Partial`, deliberately** — deferring the question does not complete the investigation: zero tenants have been fingerprinted and zero compared, so the drift classification is *undecided*, never "none". Reopen on any CSV landing in `db/drift/`, or on any per-tenant report / statutory-document surprise; the tooling is complete and must not be re-derived. | [KB-103](architecture/stored-procedure-drift.md) |

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
| INV-030 | Stored-procedure drift across tenant databases (Q-14) | M0-02 | **Allocated and in use — `Partial`, see the Partial table above; Q-14 deferred 2026-08-18 (owner Vivek), row stays `Partial`** |
| INV-031 | Test-harness feasibility — can `ApplicationDbContext` be hosted in a test process, and under which EF provider? (`ToView(null)` × 65 makes InMemory doubtful) | M0-12-01 | Reserved |
| INV-032 | Decimal representation across the HTTP wire — format, precision source, rounding mode | M2-C10 | Reserved |
| INV-033 | Screen-name → route mapping for permission-filtered navigation | M2-C03 | Reserved |
| INV-034 | Repository visibility correction (moved to Completed table above) | M0-00 | Completed |
| **INV-035 +** | **next free** | — | — |

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
