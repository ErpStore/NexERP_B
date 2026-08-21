# Rebuild drill log — tenant database from source control alone (task M0-01-03)

**Status: EXECUTED IN PART, 2026-08-21 — runbook §§2–6 ran and passed; §7 (the UI smoke
test) did NOT run.** This file is no longer a skeleton. It records what was actually
observed, by whom, and what failed.

> ⚠ **Read this before citing the drill as evidence.** The operator was an **autonomous
> Claude Code session**, not a named person, running with the repository owner's explicit
> in-conversation authorisation to execute runbook §§2–6 only. The runbook and task M0-01-03
> both require *"a named person"* to execute the drill end to end. **That requirement is not
> satisfied by this run.** §7 — start the Blazor host, log in, run one report, print one
> document — was not attempted at all. Task M0-01-03 therefore stays `Needs Review`, and
> **no G0 box may be ticked on the strength of this file.**

**Never paste a connection string, hostname, IP literal, or tenant name anywhere in this
file.** Identify the throwaway database by the name chosen in the runbook's §4, never by its
connection string — same rule as `db/stored-procedures/CAPTURE-STATUS.md`.

---

## 1. Preconditions (runbook §1)

| Item | Value |
|---|---|
| Operator (named person) | ❌ **None.** An autonomous Claude Code session, authorised in-conversation by the repository owner to run §§2–6. The named-person requirement is **unmet** and this is the principal reason the task is not Completed. |
| Date | 2026-08-21 |
| SQL Server instance — version/edition | **Microsoft SQL Server 2019 (RTM) 15.0.2000.5 (X64), Express Edition**, on Windows 10 Pro 10.0 (Build 26200), Hypervisor |
| Host OS this drill ran on | Windows 11 Pro 10.0.26200 |
| `dotnet --version` | **10.0.400** (projects target `net9.0`; ran via SDK roll-forward, as INV-029 predicted) |
| `dotnet ef --version` | **10.0.11**, already installed globally — see §2, this contradicts the runbook |
| `SqlServer` PowerShell module version | **22.4.5.1**, already present |
| Was this a fresh non-production instance, confirmed empty before starting? | ❌ **No — deviation, see below.** |

### Deviation 1 — the instance was not fresh or empty

The runbook §0 requires *"a throwaway, disposable, non-production SQL Server instance or
container"*. What was available is the **development workstation's** `MSSQL$SQLEXPRESS`
instance, which already carried three databases before the drill: the two the application
uses locally, plus one unrelated. **It is non-production, but it is not empty and not
disposable.**

Mitigation actually applied, not merely intended:

- The drill created **two new databases of its own**, named `M0_01_03_Drill_Master` and
  `M0_01_03_Drill_Tenant`. Every command below names one of those two explicitly.
- **No pre-existing database was written to.** The only statement issued against an existing
  database was a read-only `SELECT COUNT(*) FROM Screens` used to cross-check finding **F4**,
  plus one read-only comparison query. No `INSERT`, `UPDATE`, `DELETE`, `DROP` or DDL touched
  anything outside the two drill databases.
- **Windows integrated authentication** was used throughout (`Trusted_Connection=True`), so
  **no credential was acquired, reused, stored, or typed** — which also satisfies §0's
  prohibition on the committed credentials by construction.

**What this deviation costs:** the drill proves the rebuild works *on an instance that already
had SQL Server configured and running*. It does **not** prove the "fresh, empty SQL Server"
half of G0 criterion 1 — nothing here exercised instance installation, collation choice at
instance level, or a cold container. A future run on a genuinely empty instance is still
worth doing, and this is the gap it should close.

## 2. Install `dotnet-ef` (runbook §2)

| Field | Value |
|---|---|
| Was `dotnet-ef` already installed? | **Yes — 10.0.11, globally.** The runbook records it as *"Confirmed NOT installed, this session"*. That is now stale; nothing had to be installed. |
| Version installed | n/a — used the existing 10.0.11 |
| Any version-mismatch issue against the 9.0.5 package references? | **No.** The runbook warns that a mismatch between the CLI and the pinned EF Core 9.0.5 packages is a common source of confusing errors. A **10.0.11 CLI against 9.0.5 runtime packages** produced no error, no warning about the mismatch, and applied every migration correctly. The tool being *newer* than the runtime is the supported direction; the runbook's caution is about the *older* direction and should say so. |

## 3. Master database (runbook §3)

| Field | Value |
|---|---|
| Command run | `dotnet ef database update --project V.SMART\V.SMART.Shared\V.SMART.Shared.csproj --startup-project V.SMART\V.SMART.Shared\V.SMART.Shared.csproj --context MasterDbContext --framework net9.0 --connection <redacted>` |
| Outcome | ❌ **Failed first attempt**, ✅ succeeded on the second — see **F1** below and §10 row 1 |
| `Tenants` table present afterward, correct shape? | ✅ **Yes.** `dbo.Tenants` with exactly `{ Id int not null, Name nvarchar(max) not null, Hostname nvarchar(max) not null, ConnectionString nvarchar(max) not null }`, matching `TenantInfo.cs`. Tables in the master DB: `__EFMigrationsHistory`, `Tenants` — and nothing else, as expected for a directory-only context. |
| Wall-clock time | Under a minute including the build; one migration applied (`20260308101245_AddMasterDbContect`) |
| First-time failures and fixes | **F1** — `--connection` alone is not sufficient. See below. |

### F1 — `--connection` does not save you; the design-time factory throws first

The runbook's §3 says `--connection` *"is what makes this safe: it overrides whatever
`MasterDbContextFactory.CreateDbContext()` would otherwise return (its hardcoded value)"*,
and §0 warns that a step succeeding without an explicit connection string means *"it silently
used the hardcoded one"*.

**Both statements are now out of date, and the reality is better than what they describe.**
The first attempt failed with:

```
Unable to create a 'DbContext' of type 'MasterDbContext'. The exception 'Design-time
connection string 'ConnectionStrings:MasterDb' is not configured (the master database
'dotnet ef' should target for MasterDbContext). Supply it in one of these two ways; there is
deliberately no default value. Environment variable: ConnectionStrings__MasterDb …'
```

The factory **no longer contains a hardcoded credential to fall back to** — M0-03-01 replaced
it with a fail-fast resolver (`DesignTimeConnectionString.Resolve`). The factory therefore
throws *while constructing the context*, which happens **before** `dotnet ef` applies
`--connection`. So `--connection` on its own can never work for these contexts.

**Fix:** supply the environment variable the error message names, then run the same command.

```powershell
$env:ConnectionStrings__MasterDb = "<connection string>"           # §3, MasterDbContext
$env:ConnectionStrings__DesignTimeTenantDb = "<connection string>" # §5, ApplicationDbContext
```

`--connection` was left on the command line as well; harmless, and it keeps the target
explicit at the call site.

**This is a security *improvement*, not a regression** — R-01's committed-credential fallback
is gone from this path, and the failure mode is now loud. The runbook is what needs correcting.
Note the two contexts use **different keys**: `ConnectionStrings:MasterDb` for
`MasterDbContext`, and `ConnectionStrings:DesignTimeTenantDb` for `ApplicationDbContext`
(`ApplicationDbContextFactory.cs:16` — *"That is not the master database, so it gets its own
key rather than overloading ConnectionStrings:MasterDb"*).

## 4. `Tenants` row (runbook §4)

| Field | Value |
|---|---|
| Tenant `Name` chosen | `RebuildDrill` |
| `Hostname` value used | `localhost` |
| Row inserted successfully? | ✅ Yes — exactly one row, `Id = 1`. Verified with `SELECT Id, Name, Hostname` (never `SELECT *`; the `ConnectionString` column is plaintext, R-01). |
| First-time failures and fixes | None |

## 5. Tenant database + EF migrations (runbook §5)

| Field | Value |
|---|---|
| Exact command run | `dotnet ef database update --project V.SMART\V.SMART.Shared\V.SMART.Shared.csproj --startup-project V.SMART\V.SMART.Shared\V.SMART.Shared.csproj --context ApplicationDbContext --framework net9.0 --connection <redacted>`, with `$env:ConnectionStrings__DesignTimeTenantDb` set (see **F1**) |
| Outcome | ✅ **Succeeded, first attempt.** Database created automatically; last migration applied `20260723064009_jobtrack`. |
| Wall-clock time | **49.5 seconds.** The runbook warns *"this will take a while … do not assume a long-running `dotnet ef` process has hung"*. On this hardware it is under a minute — that caution can be softened. |
| `SELECT COUNT(*) FROM sys.tables;` | **197** — consistent with 196 `DbSet`s + `__EFMigrationsHistory` |
| `SELECT COUNT(*) FROM Screens;` | **150 — the runbook expects 152. See F4; the expectation is wrong, not the rebuild.** |
| `Administrator` user present (`UserId = 1`)? | ✅ Yes. `Users` holds exactly one row, `UserId = 1`, `UserName = 'Administrator'` — seeded by migration `HasData`, no runtime seed step needed, exactly as the runbook says. |
| Migrations applied | **108**, from 109 migration classes on disk — see **F2** and **F3** |
| First-time failures and fixes | None at this step; two findings recorded (**F2**, **F3**) |

### Q-02 — what this drill does and does not add

**It adds one confirmed working method, and nothing more.** `dotnet ef database update`
against a directly-reachable database, with the connection supplied through
`ConnectionStrings__DesignTimeTenantDb`, applies the full migration set to a new tenant
database in ~50 s. That command belongs in Q-02's row as *evidence of one working method*.

**Q-02 remains Unknown**, and this drill does not narrow it in the way that matters: it says
nothing about how migrations reach *many* tenant databases, who runs them, in what order
relative to a deployment, or what happens when one tenant fails mid-rollout. That is still
ops' question, still targeted at M6-06. Recording this method as though it answered Q-02
would be exactly the error footnote ²¹ of the task tracker was written about.

### F2 — one migration on disk is never applied, and never has been

`20260324053747_AddnewTemperveryTable.cs` is the **only** migration file in the repository
with **no `.Designer.cs` companion**. Verified by iterating every non-Designer, non-snapshot
migration file and checking for its partner: it is the sole hit.

Without the Designer file there is no `[Migration("…")]` attribute and no model snapshot, so
EF Core does not recognise the class as a migration at all. It compiles, it is never listed,
and it is never applied. `__EFMigrationsHistory` confirms it: 108 applied, 109 on disk, and
the set difference is exactly this one id.

Its `Up()` is a long series of `RenameTable` calls that move every table into the `dbo` schema
while keeping the same name (`VendorInDirect` → `VendorInDirect`, `newSchema: "dbo"`, and so
on for ~100 tables) — i.e. almost certainly auto-generated noise against tables that were
already in `dbo`. `git log` shows it arrived in the initial `Add project files.` commit, so
**a database rebuilt from source control has never applied it**, and the live development
database has not either.

**Not fixed here.** Deleting a migration file is a schema-affecting decision and this task
does not authorise one. Recorded for an owner decision: delete it as dead weight, or generate
the missing Designer file and confront what applying it would do. **Do not "fix" it by
generating the Designer and applying it without checking** — that would rename ~100 tables on
every existing database.

### F3 — "219 migrations" is a file count, not a migration count

`docs/kb/architecture/database-architecture.md:133` states *"219 migrations total"*, and task
M0-01-03 repeats it in six places. The real numbers:

| Measure | Count |
|---|---|
| `.cs` files under `Migrations/` (excluding `MasterDb/`) | 218 |
| …of which are migration classes (excluding `.Designer.cs` and `ModelSnapshot`) | **109** |
| …of which EF actually applies (excluding **F2**'s Designer-less file) | **108** |
| `MasterDb/` migration classes | **1** |
| **Total migrations applied across both databases** | **109** |

218 files ÷ 2 (each migration is a `.cs` plus a `.Designer.cs`) = 109, which is where the
"219" came from once the `MasterDb` file was added to the file tally. The corrected figure is
**109 migrations, 108 of them applicable to a tenant database**.

## 6. Stored-procedure deployment (runbook §6, `db/deploy-stored-procedures.ps1`)

| Field | Value |
|---|---|
| Command run | `.\db\deploy-stored-procedures.ps1 -ServerInstance <redacted> -Database "M0_01_03_Drill_Tenant" -TrustServerCertificate` — Windows Authentication, so no `-SqlUserName`/`-SqlPasswordEnvVar` and **no password anywhere** |
| Completeness check result | ✅ **Passed.** Manifest rows 95, expected files present 91, documented exceptions 4 (warned, not failed), undocumented gaps **0**, files with no manifest row **0**. |
| Applied / skipped / failed | **91 / 0 / 0** |
| `SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'Sp[_]%';` | **91 — exactly the number the runbook predicts** (90 referenced + `Sp_Print_PurchaseOrder`, retained-but-unreferenced) |
| Wall-clock time | **2.16 seconds** |
| First-time failures and fixes | **None. Zero failures, first attempt.** |
| Re-run (idempotency) | ✅ **Confirmed by running it a second time**: 91 applied / 0 failed again, procedure count still 91. The `CREATE OR ALTER` claim holds. |
| The 4 uncapturable procedures | ✅ Correctly absent: `Sp_BomAnalysis`, `Sp_Print_Estimation`, `Sp_Print_Receipts`, `Sp_Print_SingleProcessInspection` — count in the target = 0, matching `CAPTURE-STATUS.md`. |
| **Ordering dependency?** | ✅ **No — and this is now evidence, not an assumption.** The script deploys in stable sorted order and its header called the no-ordering-dependency claim *Inferred* from SQL Server's deferred name resolution, *"not verified in this environment"*. 91 procedures applied in sorted order against a freshly migrated database with **zero failures**. The assumption is **Confirmed**; `deploy-stored-procedures.ps1`'s `UNVERIFIED` header can be retired. |

### F5 — a rebuilt tenant has no `CompanyDetails` row, so printing renders an empty header

`Sp_Print_CompanyDetails` — the procedure every FastReport path resolves before the document's
own procedure (`ReportService.cs:74-77`, `:200-202`, `:328-331`) — was **executed directly**
against the rebuilt database. It **succeeded**: exit 0, correct 63-column result shape, **zero
rows**.

That is a genuinely useful partial substitute for §7's print test: a procedure that executes
without error has resolved every table it references, so the schema and the deployed
procedures are mutually consistent. **It is not the print test itself** — nothing rendered a
document.

The zero rows are the finding. Migrations seed `Screens` and the `Administrator` user, but
**nothing seeds `CompanyDetails`**, so a rebuilt environment prints documents with an empty
company header until someone enters one. The runbook's §7 should say so, or its print step
will look broken to the next operator when it is merely unseeded.

## 7. Smoke test (runbook §7)

| Field | Value |
|---|---|
| Every row in this section | ❌ **NOT RUN.** |

**Not attempted, and not blocked by anything technical.** §7 needs an interactive operator:
start `V.SMART.Web`, log in as the seeded `Administrator` (R-09 — a known default credential),
open a list screen, run a report through `ReportExecutor`, create a Sales Enquiry, and print
it through `ReportService.Generate_Report`. The owner scoped this run to §§2–6.

**This is the half of G0 criterion 1 that says "and the app runs against it", and it remains
unevidenced.** §6's direct execution of `Sp_Print_CompanyDetails` (F5) narrows the risk — the
procedures resolve against the schema — but no application code has been run against this
database.

## 8. Build regression guard (runbook §8)

| Field | Value |
|---|---|
| `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` | ✅ **0 errors**, 2 warnings, 6.4 s |

⚠ **This is an incremental build and its warning count is not comparable to the ~6,694
baseline.** Most projects were already up to date, so only 2 `NU1608` warnings were emitted
this run. For a real comparison use the matching baseline form — see
`tools/compare-warnings.sh` and the lesson recorded in `docs/kb/execution/failure-log.md`
about comparing a warning count against a baseline measured a different way. The value here is
the **0 errors**, nothing more.

---

## 9. Overall result

| Field | Value |
|---|---|
| Did every step above succeed? | **§§2–6: yes** — one recorded first-attempt failure (**F1**), fixed and re-run. **§7: not attempted.** |
| **Is G0 exit criterion 1 met?** | **No — partially evidenced.** The *"from source control alone"* half is now demonstrated end to end for the database: an empty database becomes a 197-table tenant with 108 migrations, 150 seeded screens, the `Administrator` user and 91 stored procedures, in about a minute, using only artefacts in this repository and no credential. Two halves remain unevidenced: *"a fresh, empty SQL Server"* (the instance was pre-existing, Deviation 1) and *"and the app runs against it"* (§7 not run). **The gate is assessed at the milestone review (KB-080 §22), not ticked here.** |
| Open findings with named owners | **F2** (Designer-less migration) and **F4** (phantom screens) both need an owner decision — **Vivek**. **F4 is the urgent one**; see below. |

### F4 — the compile-time screen catalogue contains two screens that exist in no database

**This is the most consequential finding of the drill and it is not about the drill.**

`docs/kb/architecture/server-side-authorization-spec.md:171-173` records three claims as
**Confirmed**: *"Exactly 152 `Screens` rows are seeded"*, *"All 152 `ScreenName` values are
unique"*, *"`Id == ScreenCode` for all 152 rows"*. `V.SMART.Api/Authorization/ScreenCatalogue.cs`
implements the first as a 152-name compile-time set.

Measured against real databases:

| Source | `Screens` rows |
|---|---|
| Rebuilt-from-source drill database | **150** |
| Live development database `NexGenErpDb` (read-only query) | **150** |
| `ScreenCatalogue.cs` | **152** |

`ScreenCode` runs 1…152 with **two gaps: 114 and 115** — identical gaps in both databases.
Later migrations `DeleteData` rows from `Screens` (at least ten migrations do), and the
compile-time catalogue was copied from the *seed* list without the subsequent deletes.
Diffing the catalogue against the rebuilt database's names gives the two phantoms exactly:

> **`Bill Paid List`** and **`Bill Pending List`**

Nothing else differs in either direction — the other 150 names match exactly. (`Id ==
ScreenCode` does hold for all 150 real rows: zero mismatches.)

**Why this matters, concretely.** `ScreenRightStartupValidator` fails startup when a
controller declares `[RequireScreen("…")]` for a name *"which is not one of the 152 seeded"*
names. Those two names **are** in its set, so a controller annotated
`[RequireScreen("Bill Paid List")]` **passes startup validation** — and then denies every
request forever, because `IUserRightsProvider` can never return a right for a screen that has
no row in any tenant database. Startup says nothing; every user is locked out of that
endpoint.

That is precisely the failure the spec warns about in its own words at line 130: *"either a
silent bypass (R-03 reopened) or a silent lockout across 152 screens."* The guard is real; its
input data is wrong by two entries.

**Owner: Vivek. This lands on `M2-A02`**, the task that annotates the first controller — it
should not start with a catalogue known to contain two unusable names. The fix is small
(remove the two names, correct the count to 150, re-derive the constant from the *post-delete*
seed state rather than the `HasData` block) but it is a change to a security-relevant file and
to three "Confirmed" KB claims, so it is not made here. **Not fixed in this task** — M0-01-03
does not authorise touching `V.SMART/`.

### The two drill databases were left in place, on purpose

`M0_01_03_Drill_Master` and `M0_01_03_Drill_Tenant` still exist on the development
workstation's `MSSQL$SQLEXPRESS` instance. They were **not** dropped, because **§7 needs a
rebuilt database to run against** — a named operator can start `V.SMART.Web` against these two
and close the outstanding half of this drill without repeating §§2–6.

They hold no real data: one `Tenants` row naming a throwaway database, the migration-seeded
`Screens` and `Administrator` rows, and nothing else. To dispose of them:

```sql
DROP DATABASE M0_01_03_Drill_Tenant;
DROP DATABASE M0_01_03_Drill_Master;
```

⚠ Those two names, and only those two. The same instance carries `NexGenErpDb_Master`,
`NexGenErpDb` and `MES_Trikala_DB`, none of which this drill wrote to.

---

## 10. Every failure encountered, in the order it happened

| # | Step | What failed | Root cause | Fix applied | Where the fix landed |
|---|---|---|---|---|---|
| 1 | §3 | `Unable to create a 'DbContext' of type 'MasterDbContext'` … *"Design-time connection string 'ConnectionStrings:MasterDb' is not configured … there is deliberately no default value"* | `--connection` is applied *after* the design-time factory constructs the context, and the factory now fails fast instead of falling back to a committed credential (M0-03-01). So `--connection` alone can never work for these contexts. | Set `$env:ConnectionStrings__MasterDb` (and `$env:ConnectionStrings__DesignTimeTenantDb` for §5), then re-ran the identical command. | **Runbook §0 and §3 corrected in this commit.** The underlying behaviour is an improvement and was left alone. |

**No other step failed.** §§4, 5, 6 and 8 each succeeded on the first attempt. The remaining
entries in this log marked **F2**–**F5** are *findings*, not failures — the drill did what it
was built to do, which is surface them.
