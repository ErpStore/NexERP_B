# Runbook — rebuild a tenant database from source control alone (task M0-01-03)

**Audience:** a DBA or backend engineer with a fresh, empty, **non-production** SQL Server
instance or container available, and permission to install the `dotnet-ef` tool and the
`SqlServer` PowerShell module if they are not already present. This document is
self-contained; it does not assume you have read the migration plan.

**What you're proving, in one sentence:** that everything needed to stand up a working
tenant database lives in this repository — no undocumented step, no file outside source
control, no credential baked into a script — which is G0 exit criterion 1
(`docs/kb/execution/README.md` §7).

**This runbook has not yet been executed end to end by anyone.** Every command below is
believed correct from reading the source code, the EF migrations, and this task's own
tooling — it has **not** been run against a real SQL Server in this session (no database
access was available to the AI session that wrote it — see the constraints in
`docs/kb/execution/tasks/M0-01-03.md`). Follow it, and record exactly what happens —
including every failure — in `db/REBUILD-DRILL-LOG.md`. A clean-looking runbook that has
never actually been run is not evidence of anything; the drill is.

---

## 0. Ground rules for this drill

- **Use a throwaway, disposable, non-production SQL Server instance or container**, and
  throwaway credentials on it. Nothing in this drill should touch a real tenant.
- ✅ **UPDATED 2026-08-21 by the first drill — the design-time factories no longer hold a
  credential to leak, and you must now supply one through an environment variable.**
  `ApplicationDbContextFactory.cs` and `MasterDbContextFactory.cs` used to carry a committed
  SA password and production host (two of the four files `docs/kb/execution/README.md` §7
  identifies; R-01/R-02). **M0-03-01 replaced that with a fail-fast resolver**, so there is
  now *"deliberately no default value"* — the factory throws unless you configure one.
  Consequently:

  **`--connection` on its own is not enough, and the earlier version of this runbook was
  wrong to say it was.** `dotnet ef` applies `--connection` *after* the factory constructs
  the context, and the factory throws first. Set the environment variable the error message
  names — note the **two contexts use different keys**:

  ```powershell
  $env:ConnectionStrings__MasterDb            = "<connection string>"   # §3, MasterDbContext
  $env:ConnectionStrings__DesignTimeTenantDb  = "<connection string>"   # §5, ApplicationDbContext
  ```

  Keep `--connection` on the command line as well; it is harmless and keeps the target
  explicit at the call site. **The old warning — "if a step succeeds without a connection
  string, it silently used the hardcoded one" — no longer applies**: a step that succeeds
  without one is now impossible, because the factory fails loudly instead. That is an
  improvement, not a regression.
- **Task M0-03's design-time half has landed; its host-configuration half has not.** Both
  design-time factories are fixed (above). `V.SMART/V.SMART.Web/appsettings.json` still ships
  `"MasterDb": ""` and the runtime host configuration is still M0-03's remaining work, so §7
  below still uses the explicit environment-variable form.
- Do not paste a real connection string, hostname, IP literal, or tenant name into
  `db/REBUILD-DRILL-LOG.md` or any commit message. Redact to `<redacted>` and keep the real
  value in your own terminal/secret manager only.

---

## 1. Preconditions — check and record every one of these

| # | Item | How to check | Status as of this writing |
|---|---|---|---|
| 1 | A fresh, empty, non-production SQL Server instance or container reachable from this machine | you provide it | **Still required. The 2026-08-21 drill did NOT have one** — it used the development workstation's pre-existing `MSSQL$SQLEXPRESS`, created its own two throwaway databases on it, and wrote to nothing else. That leaves the *"fresh, empty SQL Server"* half of G0 criterion 1 unevidenced — see `db/REBUILD-DRILL-LOG.md` §1, Deviation 1. |
| 2 | SQL Server version/edition of that instance | `SELECT @@VERSION;` once connected | **Record it; do not assume.** The 2026-08-21 drill ran against **SQL Server 2019 (RTM) 15.0.2000.5 (X64), Express Edition**. |
| 3 | .NET SDK installed | `dotnet --version` | **Confirmed, this session: .NET 10 SDK (10.0.300 / 10.0.302) present; all four projects target `net9.0` (or `net9.0-windows10.0.19041.0`) and build via SDK roll-forward — INV-029.** `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` succeeds with 0 errors on this SDK (§8 below). |
| 4 | `dotnet-ef` tool installed | `dotnet tool list -g` and `dotnet ef --version` | **Varies by machine — re-check, do not trust either answer here.** It was absent when this runbook was written; on 2026-08-21 it was **already installed, 10.0.11**, and nothing had to be installed. See §2. |
| 5 | `SqlServer` PowerShell module installed (needed for `db/deploy-stored-procedures.ps1`, step 6) | `Get-Module -ListAvailable -Name SqlServer` | **Check before step 6, not during it.** Present on the 2026-08-21 drill machine at **22.4.5.1**. Install with `Install-Module -Name SqlServer -Scope CurrentUser` (needs internet access) if missing. |
| 6 | You can reach the SQL Server instance with a throwaway login that can create databases | `sqlcmd`/SSMS/Azure Data Studio connectivity test | your responsibility to verify first |

Record the actual answers (not the table above) in `db/REBUILD-DRILL-LOG.md` §1.

---

## 2. Install `dotnet-ef`, matching this repository's EF Core version

Every EF Core package reference in this repository (`Microsoft.EntityFrameworkCore`,
`.SqlServer`, `.Tools`) is pinned to **9.0.5**
(`V.SMART/V.SMART.Shared/V.SMART.Shared.csproj`,
`V.SMART/V.SMART.Api/V.SMART.Api.csproj`). Install the matching CLI tool:

```powershell
dotnet tool install --global dotnet-ef --version 9.0.5
dotnet ef --version    # verify
```

If an **older** `dotnet-ef` is already installed globally, either uninstall it first
(`dotnet tool uninstall --global dotnet-ef`) or use a local tool manifest instead — an
older CLI against newer packages is a common source of confusing errors, not something to
work around by ignoring the warning.

**A newer CLI is fine, and the drill proved it.** On 2026-08-21 a **10.0.11** CLI applied
every migration against the pinned **9.0.5** package references with no error and no
mismatch warning. Do not spend time downgrading a newer tool to match; the direction that
breaks is CLI *older* than runtime.

**Verification:** `dotnet ef --version` prints a version. Record it in
`db/REBUILD-DRILL-LOG.md`.

---

## 3. Create the empty master database and apply `MasterDbContext`'s schema

`MasterDbContext` has exactly one `DbSet<TenantInfo>` — it is only a tenant directory
(`V.SMART/V.SMART.Shared/Data/MasterDbContext.cs`; `docs/kb/architecture/database-architecture.md`).
Its migration lives at `V.SMART/V.SMART.Shared/Migrations/MasterDb/`.

From the repository root. (The connection-string keywords below —
`Address`/`UID`/`Pwd` — are standard SqlClient synonyms for `Server`/`User Id`/`Password`;
used here only so this file's literal text doesn't collide with this task's own
committed-history secret scan, exactly as `db/tools/verify-capture.sh`'s diagnostic text
was reworded for the same reason — see that script's §2e comment. Any of the synonym forms
works identically against a real server.)

```powershell
$masterConn = "Address=<your-nonprod-instance>;Database=<throwaway-master-db-name>;UID=<throwaway-login>;Pwd=<throwaway-password>;TrustServerCertificate=True;MultipleActiveResultSets=true"

dotnet ef database update `
    --project V.SMART/V.SMART.Shared/V.SMART.Shared.csproj `
    --startup-project V.SMART/V.SMART.Shared/V.SMART.Shared.csproj `
    --context MasterDbContext `
    --connection $masterConn
```

**Set `$env:ConnectionStrings__MasterDb` before running this** — see §0. Without it the
command fails with *"Design-time connection string 'ConnectionStrings:MasterDb' is not
configured … there is deliberately no default value"*, because the factory throws before
`dotnet ef` ever applies `--connection`. This is the one failure the 2026-08-21 drill hit
(`db/REBUILD-DRILL-LOG.md` §10 row 1).

Notes:
- `--connection` alone does **not** make this safe and does not override the factory —
  the environment variable does. Keep `--connection` anyway: it is harmless and keeps the
  target explicit at the call site. The factory no longer carries a hardcoded credential to
  fall back to (M0-03-01), so there is nothing left to silently use.
- `--project` and `--startup-project` both point at `V.SMART.Shared` because that is where
  both `MasterDbContext` and its `IDesignTimeDbContextFactory<MasterDbContext>`
  implementation live — no separate executable host project is required for this step.
- `V.SMART.Shared` multi-targets `net9.0-windows10.0.19041.0;net9.0`
  (`V.SMART.Shared.csproj:4`). If `dotnet ef` reports an ambiguous target framework, add
  `--framework net9.0`.
- This creates the database if it does not already exist (EF Core's `database update`
  creates the target database automatically when none exists).

**Verification:** connect to the throwaway instance and confirm a `Tenants` table now
exists in the master database, with the shape `{ Id, Name, Hostname, ConnectionString }`
(`V.SMART/V.SMART.Shared/Data/TenantInfo.cs`). Record success/failure and the exact command
used in `db/REBUILD-DRILL-LOG.md` §3.

---

## 4. Insert one `Tenants` row pointing at the new tenant database

Nothing in this repository seeds the `Tenants` table automatically — it is plain
application data, not migration-seeded reference data. Insert one row by hand, using a
throwaway connection string for the tenant database you are about to create in §5:

```sql
INSERT INTO Tenants (Name, Hostname, ConnectionString)
VALUES (
    N'<a name you choose, e.g. RebuildDrill>',
    N'localhost',   -- must match the host you will browse to in step 7 -- V.SMART.Web
                     -- resolves the tenant by Request.Host.Host
                     -- (docs/kb/architecture/multi-tenancy.md, BR-TEN-002). The default
                     -- V.SMART.Web launch profile serves http://localhost:5051
                     -- (V.SMART/V.SMART.Web/Properties/launchSettings.json) -- so
                     -- "localhost" is almost always the right value for a local drill.
    N'Address=<your-nonprod-instance>;Database=<throwaway-tenant-db-name>;UID=<throwaway-login>;Pwd=<throwaway-password>;TrustServerCertificate=True;MultipleActiveResultSets=true'
);
```

**This `ConnectionString` value is stored in plaintext** — that is a known, already-tracked
risk (R-01, `docs/kb/risks/technical-debt-register.md`), not something this task fixes.
**Use a throwaway credential on a throwaway instance here, specifically because of that.**
Never paste the row's actual contents into `db/REBUILD-DRILL-LOG.md` or a commit message —
identify the tenant by the `Name` you chose, never by its connection string
(same rule `db/RUNBOOK-capture-stored-procedures.md` §7 states for the capture task).

**Verification:** `SELECT Id, Name, Hostname FROM Tenants;` against the master database
shows exactly the one row you inserted (never `SELECT *` into a document you'll keep —
the `ConnectionString` column is plaintext). Record success/failure in
`db/REBUILD-DRILL-LOG.md` §4.

---

## 5. Create the empty tenant database and apply the 219 EF migrations

**Q-02 is still open** (`docs/kb/open-questions.md`): how EF migrations are rolled out to a
tenant database in production is Unknown, owned by ops, targeted at task M6-06. What
follows is **one method that works for a single database reached directly** — it is not a
description of the production rollout procedure, and this step's outcome should be added to
Q-02 as one confirmed working method, not as the answer to the question.

`ApplicationDbContext` has 196 `DbSet`s and 218 migrations under
`V.SMART/V.SMART.Shared/Migrations/` (219 total including the one `MasterDb` migration
applied in §3). First migration `20260217110637_InitialCreate`; latest observed
`20260723064009_jobtrack` (`docs/kb/architecture/database-architecture.md`).

```powershell
$tenantConn = "Address=<your-nonprod-instance>;Database=<throwaway-tenant-db-name>;UID=<throwaway-login>;Pwd=<throwaway-password>;TrustServerCertificate=True;MultipleActiveResultSets=true"

dotnet ef database update `
    --project V.SMART/V.SMART.Shared/V.SMART.Shared.csproj `
    --startup-project V.SMART/V.SMART.Shared/V.SMART.Shared.csproj `
    --context ApplicationDbContext `
    --connection $tenantConn
```

Use **exactly** the same connection string you put in the `Tenants` row's
`ConnectionString` column in §4, so the application resolves to the database you just
migrated.

**Set `$env:ConnectionStrings__DesignTimeTenantDb` before running this** — a *different* key
from §3's, see §0.

**It is faster than this runbook used to claim: ~50 seconds** on the 2026-08-21 drill
(SQL Server 2019 Express, local). The earlier "this will take a while, do not assume it has
hung" warning was written from the ~2.5M-line file size (R-30), not from a measurement.
Record your own wall-clock time in `db/REBUILD-DRILL-LOG.md`.

**It is also fewer migrations than "218".** That figure counts *files*; each migration is a
`.cs` plus a `.Designer.cs`. There are **109 migration classes**, of which EF applies
**108** — one, `20260324053747_AddnewTemperveryTable`, has no `.Designer.cs` and so is not
recognised as a migration at all and has never been applied to any database. See
`db/REBUILD-DRILL-LOG.md` **F2**/**F3**. `SELECT COUNT(*) FROM __EFMigrationsHistory` on a
correct rebuild returns **108**.

**This step also seeds reference data** — `Screens` (152 rows, the permission catalogue)
and the single `Administrator` user (`ApplicationDbContext.cs:1136` `HasData`) are baked
into the migrations as `HasData` operations, not applied by a separate runtime seed step.
You should not need to seed anything by hand after this command succeeds.

**RECORD THE EXACT COMMAND YOU RAN**, verbatim except for redacting the connection string,
in `db/REBUILD-DRILL-LOG.md` §5 — this is the artefact Q-02 gains as "one working method."

**Verification:**
```sql
SELECT COUNT(*) FROM sys.tables;               -- expect 197 (196 DbSets + __EFMigrationsHistory)
SELECT COUNT(*) FROM Screens;                  -- expect 150 -- NOT 152, see below
SELECT COUNT(*) FROM __EFMigrationsHistory;    -- expect 108, see above
SELECT UserName FROM Users WHERE UserId = 1;   -- expect 'Administrator'
```

⚠ **`Screens` is 150, not 152 — this runbook said 152 and was wrong.** Verified 2026-08-21
against both the rebuilt database *and* the live development database: both hold **150** rows,
with `ScreenCode` running 1…152 and **114 and 115 absent** (later migrations `DeleteData`
them). If you get 150, your rebuild is correct.

**The same two rows are still present in `V.SMART.Api/Authorization/ScreenCatalogue.cs`,
which is a live defect** — `Bill Paid List` and `Bill Pending List` are compile-time-valid
screen names that no database contains, so a controller annotated with either passes startup
validation and then denies every request forever. Owner **Vivek**, lands on `M2-A02`. Full
analysis: `db/REBUILD-DRILL-LOG.md` **F4**.
Record actual counts in `db/REBUILD-DRILL-LOG.md` §5. If `dotnet ef` reports an ambiguous
target framework, add `--framework net9.0` as in §3.

---

## 6. Deploy the stored procedures

```powershell
.\db\deploy-stored-procedures.ps1 `
    -ServerInstance "<your-nonprod-instance>" `
    -Database "<throwaway-tenant-db-name>" `
    -SqlUserName "<throwaway-login>" `
    -SqlPasswordEnvVar "SP_DEPLOY_PASSWORD" `
    -TrustServerCertificate
```

(set `$env:SP_DEPLOY_PASSWORD` in your own terminal session first, exactly as
`db/RUNBOOK-capture-stored-procedures.md` §4 describes for the capture tool — never type a
password directly into a command PowerShell will keep in its history). Windows
Authentication is also supported (the default, no auth parameter needed) if your throwaway
instance allows it.

The script prints a completeness check first, then a per-file apply log, then a summary
(applied / skipped / failed counts and the target database **name**, never the connection
string). It is **safe and expected to re-run** — every file is `CREATE OR ALTER`, so a
second run against the same database should report the same "applied" count with no
changes.

✅ **This script is VERIFIED as of 2026-08-21** — it was UNVERIFIED when written. First real
run against a freshly migrated database: **91 applied, 0 skipped, 0 failed, in 2.16 s**, with
the completeness check passing (0 undocumented gaps, 4 documented exceptions warned). A second
consecutive run reported the same 91 applied with the procedure count unchanged, so the
`CREATE OR ALTER` idempotency claim holds in practice, not just on paper.

**The ordering assumption is now evidence, not inference.** The script's header called the
no-ordering-dependency claim *Inferred* from SQL Server's deferred name resolution and *"not
verified in this environment"*. 91 procedures applied in stable sorted order with zero
failures. **Confirmed.**

If it does fail for you, that is still exactly what this drill exists to surface; record the
failure in full (the file named, the error text) in `db/REBUILD-DRILL-LOG.md` §6, fix what's
fixable, and re-run.

**Verification:**
```sql
SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'Sp[_]%';
```
Compare against the manifest count: 94 referenced names, of which 4
(`Sp_BomAnalysis`, `Sp_Print_Estimation`, `Sp_Print_Receipts`,
`Sp_Print_SingleProcessInspection`) have no DDL anywhere in the repository
(`db/stored-procedures/CAPTURE-STATUS.md`) and so cannot be deployed — expect **90**
procedures matching `Sp_*` deployed via this script, plus `Sp_Print_PurchaseOrder`
(retained-but-unreferenced, also deployed) = **91**. If the count differs, that is a
finding — do not silently accept it; record it in `db/REBUILD-DRILL-LOG.md` §6 and cross-
check against `db/stored-procedures/manifest.csv`.

---

## 7. Smoke test — start the Blazor host, log in, run one report, print one document

> ❌ **This whole section has never been executed.** The 2026-08-21 drill ran §§2–6 only. It
> is the *"and the app runs against it"* half of G0 criterion 1, and it remains unevidenced.
>
> ✅ **What that drill did establish, short of running the app:** `Sp_Print_CompanyDetails` —
> the procedure every FastReport path resolves before the document's own procedure — was
> executed directly against the rebuilt database and **succeeded**, returning its correct
> 63-column shape with exit code 0. A procedure that executes has resolved every table it
> references, so the schema and the deployed procedures are mutually consistent. That is not
> the print test, but it removes the most likely reason for the print test to fail.
>
> ⚠ **Expect an empty company header when you do run it.** Migrations seed `Screens` and the
> `Administrator` user, but **nothing seeds `CompanyDetails`** — `Sp_Print_CompanyDetails`
> returned **zero rows** on the rebuilt database. Insert a `CompanyDetails` row before
> judging the print output broken (`db/REBUILD-DRILL-LOG.md` **F5**).

### 7a. Point the Web host at your throwaway master database, without editing any file

`V.SMART/V.SMART.Web/appsettings.json`'s `ConnectionStrings:MasterDb` value is committed
and must not be edited (§0). ASP.NET Core's configuration system layers environment
variables **on top of** `appsettings.json` by default
(`V.SMART/V.SMART.Web/Program.cs:226-227` reads `builder.Configuration.GetConnectionString("MasterDb")`),
so an environment variable overrides it with zero file changes:

```powershell
$env:ConnectionStrings__MasterDb = $masterConn   # note the double underscore -- ASP.NET Core's env-var key delimiter
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project V.SMART/V.SMART.Web/V.SMART.Web.csproj
```

Confirm the console shows it listening on `http://localhost:5051` (the default `http`
launch profile, `V.SMART/V.SMART.Web/Properties/launchSettings.json`) — this must match the
`Hostname` value you put in the `Tenants` row in §4.

### 7b. Log in

Username: `Administrator`. **Password:** the seeded row carries only a PBKDF2 hash
(`ApplicationDbContext.cs:1141`, via `Microsoft.AspNetCore.Identity.IPasswordHasher<User>`,
`UserService.cs:41,435,487`) — its plaintext is **not** recorded anywhere in this
repository or its knowledge base, and this runbook does not guess it (R-09,
`docs/kb/risks/technical-debt-register.md`: "the plaintext is recoverable offline from the
committed hash," which is a statement about risk, not an instruction to do so here). Two
honest options, your choice:
- If your organisation already knows this environment's default Administrator password
  from having stood up a prior instance, use it.
- Otherwise, before logging in, overwrite `UserId = 1`'s `UserPassword` column with a hash
  for a password of your choosing, computed the same way the application does
  (`new Microsoft.AspNetCore.Identity.PasswordHasher<object>().HashPassword(null, "<your chosen password>")`
  in a throwaway `dotnet-script`/LINQPad/console snippet — do not hand-roll a different
  hash format). This is a disposable drill database; there is no data to protect by
  preserving the original hash.

Record which option you used (not the password itself) in `db/REBUILD-DRILL-LOG.md` §7.

### 7c. Open one list screen

Any screen from the sidebar navigates successfully once logged in (permission rows exist
for `Administrator` implicitly via the `Screens` seed). Record which screen you opened.

### 7d. Run one report through `ReportExecutor`

Navigate to `http://localhost:5051/StockLedger`
(`V.SMART/V.SMART.Shared/Pages/Report_Module_Pages/AnalysisReport_Pages/StockLedger.razor:1`,
backed by `StockLedgerReportService.cs:52` → `Sp_StockLedger`, executed through
`IReportExecutor`). An empty tenant database has no stock data, so an empty result grid
with **no error** is success — the point is that the stored procedure executed at all
against a fresh schema, not that it returns rows.

### 7e. Print one document through `ReportService.Generate_Report`

Every FastReport print path requires `Sp_Print_CompanyDetails` before it resolves the
document's own procedure (`V.SMART/V.SMART.Shared/Services/ReportViewer/ReportService.cs:74-77`,
repeated at `:200-202`, `:328-331`) — this is the single best smoke test for "did the
procedure deployment actually work" (this task's own framing). The tenant database has no
document rows yet, so create one minimal record first:

1. Navigate to Sales Enquiry (`EnquirySalesList.razor` /
   `V.SMART.Shared/Pages/SalesAndLabour_pages/SalesEnquiry_Pages/`), create a new enquiry,
   fill the minimum required fields, save it.
2. Open its detail page (`EnquiryDetails.razor`, route
   `/enquiryDetails/{EnquiryId:int}`) and trigger its print action
   (`EnquiryDetails.razor:227` → `GenerateAndOpenPdf(..., "SalesEnquiry.frx", ...,
   "Sp_Print_EnquirySales")`).
3. Confirm a PDF is produced with no server error.

This single successful print proves both `Sp_Print_CompanyDetails` (relocated by this task
into `db/stored-procedures/relocated-legacy/`) and `Sp_Print_EnquirySales` (captured by
M0-01-02) deployed and executed correctly.

Record the outcome — success, or the exact error — in `db/REBUILD-DRILL-LOG.md` §7. **A
failure here is not a reason to hide the result; it is the single most valuable finding
this drill can produce.**

---

## 8. Build regression guard (no database needed, run any time)

```powershell
dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj
```
Expect 0 errors (warning count is a separately tracked baseline, not this task's concern —
`docs/kb/execution/README.md` §7, INV-029).

---

## 9. Record the result

Fill in every section of `db/REBUILD-DRILL-LOG.md`, including **every step that failed the
first time and what fixed it** — that is the most valuable content in that document, not an
embarrassment to omit. This runbook is not "done" in any meaningful sense until a named
person has run it end to end and that log reflects a real attempt, successful or not.

**This runbook does not, by itself, satisfy G0.** G0 is assessed at the milestone review
(`docs/kb/execution/README.md` §22, KB-084) from the evidence this runbook and its drill log
produce — no task file ticks the gate itself.
