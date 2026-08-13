---
doc_id: KB-060
title: Technical Debt and Risk Register
module: risks
source_files:
  - V.SMART/V.SMART.Web/appsettings.json
  - V.SMART/V.SMART.Api/appsettings.json
  - V.SMART/V.SMART.Shared/Shared/BaseUserRightsComponent.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/InventoryService/StockManagerService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgPoService.cs
  - V.SMART/V.SMART.Shared/Services/SessionTimeoutService.cs
  - V.SMART/V.SMART.Shared/Services/CurrentUserService.cs
  - V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs
  - V.SMART/V.SMART.Shared/Repository/MasterRepository/Admins/UserRepository.cs
status: complete
confidence: mixed
last_verified: 2026-08-13
dependencies: [KB-011, KB-012, KB-013, KB-040]
---

# Technical Debt and Risk Register

Severity: **Critical** (fix before anything ships) · **High** (fix during Phase 2–3) ·
**Medium** (schedule) · **Low** (opportunistic).

Confidence: **Confirmed** (traced in code) · **Inferred** · **Unknown**.

---

## Critical

### R-01 — Live database credentials committed to source control
**Confirmed.** `V.SMART.Web/appsettings.json` and `V.SMART.Api/appsettings.json` contain
`Server=DESKTOP-FIIBE97\SQLEXPRESS;…User Id=sa;Password=aDMIN@123`. A **production**
connection is present as a commented line:
`Server=154.61.76.112,1533;Database=IQSmartDb_Master;User Id=bspl;Password=U^b1p7j61`.
Per-tenant connection strings (with credentials) are additionally stored in plaintext in
the `Tenants` table.
**Impact.** Anyone with repository access has database credentials, including for what
appears to be a live internet-reachable host.

> **Escalation, 2026-08-12 (INV-029, Confirmed), item 1 CORRECTED then SUPERSEDED same
> day (INV-034 + owner decision):**
>
> 1. ~~**The repository is public.**~~ ~~**CORRECTED: the repository is PRIVATE.**~~
>    **SUPERSEDED: the repository is public again, by deliberate owner decision, 2026-08-12.**
>    Timeline: the original claim ("`git ls-remote` succeeds with no authentication → public")
>    was an artefact of Windows Git Credential Manager silently authenticating with the
>    owner's cached credentials — anonymous access was never actually tested. Retesting with
>    the credential helper explicitly disabled showed the repository was in fact **private**
>    at that point. The repo owner (Kumar) then **deliberately set the repository to
>    public**, re-verified the same rigorous way (credential-helper-disabled `git ls-remote`
>    succeeds; unauthenticated REST call returns `200`, not `404`). **As of this decision,
>    treat the SA password, the production host, the `bspl` credential, and the JWT secret
>    (R-02) as genuinely reachable by anyone on the internet.** See the resolved **Q-19** and
>    [KB-085](../execution/M0-00-baseline-decisions.md#repository-visibility-correction-inv-034).
> 2. **The credentials are hardcoded in C#, not only in configuration.** The SA password,
>    the host `154.61.76.112`, and the `bspl` password are all present in committed
>    commit `c12c5b2` in **four** files:
>    `V.SMART/V.SMART.Web/appsettings.json`,
>    `V.SMART/V.SMART.Shared/Data/MigrationData/ApplicationDbContextFactory.cs`,
>    `V.SMART/V.SMART.Shared/Data/MigrationData/MasterDbContextFactory.cs`,
>    `V.SMART/V.SMART/MauiProgram.cs`.
>    The literal `bspl` additionally appears in
>    `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/EInvoiceAPIService/EinvoiceDatabaseService.cs`
>    and `EWayDatabaseService.cs`.
>
> 3. **A third database host is exposed**, not previously recorded: a commented
>    `VK-7-HP\SQLEXPRESS` connection string at `V.SMART/V.SMART/MauiProgram.cs:235`,
>    alongside the production one at `:228` and the active SA one at `:231`.
> 4. **A second, different class of credential is exposed — GST e-Invoicing gateway API
>    credentials.** `EinvoiceDatabaseService.cs:1413-1414` and `EWayDatabaseService.cs:900-901`
>    carry commented literals `API_Bhargavispl` / `$Winbspl789`. These are **not** database
>    credentials: they authenticate to the statutory e-Invoice / e-Way Bill gateway. Exposure
>    risk is filing or cancelling invoices against the company's GSTIN — a compliance
>    incident, not just a data-breach one. Different owner, different rotation path, and
>    **not covered by R-02 or by any connection-string remediation.**
>
> The action item below therefore **understated the work** twice over: moving configuration
> to environment variables does not remove a connection string compiled into a `.cs` file,
> and none of it touches the gateway credentials. Execution task **M0-03-02** closes the
> hardcoded-C# gap; **M0-04** must treat the gateway credentials as a separate rotation with
> its own owner.

**Action.** Rotate both credential sets **now** — before, and independently of, any
repository change. Move configuration to environment variables / Key Vault (M0-03-01) **and
remove the hardcoded connection strings from C# source** (M0-03-02). Purge from git history
(`git filter-repo`, M0-05) — this is damage limitation, not a remedy; rotation is the remedy.
Encrypt the `Tenants` connection-string column. Use least-privilege SQL logins, not `sa`.

### R-02 — JWT signing secret committed
**Confirmed.** `V.SMART.Api/appsettings.json` `Jwt:Secret` holds a hardcoded default value
containing the literal words "Change In Production" — i.e. it was never rotated.
**Impact.** Anyone with the repo can forge tokens for any user and any `TenantId` —
complete cross-tenant compromise once the API is live.
**Action.** Move to secret storage; rotate; fail startup if the secret is missing or is
the known default (M0-03-01, M0-03-03).

> **Note, updated 2026-08-13 (M0-00 incident; visibility corrected to private, then
> SUPERSEDED by an owner decision to make the repo public again, all 2026-08-12).** The
> 2026-08-12 note below was itself wrong in a way that caused harm: it quoted the secret's
> **literal value** as evidence, and that value was carried into `HEAD` on
> `migration/M0-00-vcs-baseline` (which was then merged to `master`) when M0-00 committed
> `docs/` as group G7, because the value lived in *this KB document*, not in
> `V.SMART.Api/appsettings.json` — which was correctly never committed. The repository was
> briefly confirmed **private** (INV-034), then the owner **deliberately made it public**
> the same day, re-verified rigorously (see R-01 above). **Treat this JWT secret as
> published on the internet, not merely exposed to collaborators, and rotate it regardless
> of the `appsettings.json` tracking state.** The value has been redacted from this document
> as of this commit; it remains in the git history of `master` until M0-05's purge, and
> rotation (M0-04/M0-03-03) must not be deprioritized. See
> [KB-085 §Unexpected finding](../execution/M0-00-baseline-decisions.md#unexpected-finding-jwt-secret-value-exposed-via-this-kb-document-not-via-appsettingsjson).
>
> **Original note, 2026-08-12 (INV-029, Confirmed), preserved for the record but
> superseded by the above:** "Unlike the database credentials (R-01), this string was not
> found in committed history — `git grep -l "<the secret string>" HEAD` returns nothing,
> so `V.SMART/V.SMART.Api/appsettings.json` appears to be untracked or uncommitted. It is
> therefore exposed locally but not published. Rotate it anyway, and confirm the file's
> tracked status as part of M0-00 before assuming this holds." The instruction to
> "confirm... as part of M0-00" was followed for `appsettings.json` itself (correctly
> deferred, never committed) but not for *this document quoting the value*, which is the
> gap that caused the exposure.

### R-03 — Authorization enforced only in the UI layer
**Confirmed.** `BaseUserRightsComponent` + `RightsHelper` are the only permission checks;
no service or repository checks rights. `CurrencyController` carries a bare `[Authorize]`
with no screen-right check.
**Impact.** Every REST endpoint is accessible to any authenticated user regardless of their
`UserRight` rows. Blocks any production API rollout.
**Action.** [ADR-004](../decisions/ADR-004-server-side-authorization.md). Must land before
the second controller is written.

### R-38 — Account-level login gates are enforced only in Blazor `@code`; the API bypasses all of them
**Confirmed, added 2026-08-12.** R-03 established that *authorization* (screen rights) is
UI-only. This is a distinct, previously unrecorded category: **authentication-time account
gates** are also UI-only, and the existing API already skips them.

`V.SMART.Api/Controllers/AuthController.cs:39-59` is the entire API auth surface — resolve
tenant → `LoginAsync` → issue a JWT. Nothing else. Meanwhile the Blazor login enforces three
gates that the API does not:

| Gate | Enforced at | Notes |
|---|---|---|
| **Trial / expiry** (Q-06) | `Login.razor:271-275` | Three carve-outs: `!IsDesktop`, `UserId > 1`, and `TrialDays > 0` — so a user with a past `ExpiryDate` but `TrialDays == 0` is **exempt**. Contains a dead `user != null` check evaluated *after* `user.UserId` is dereferenced. `GetUserTrialAsync` (`IUserRepository.cs:14`, `UserRepository.cs:63-72`) has **zero call sites** — dead code |
| **Device binding** (Q-07) | `Login.razor:277-322` | Only when `UserId > 1 && (IsMobile \|\| IsDesktop)`; trust-on-first-use; **device identity is client-asserted** via `deviceHelper.getDeviceId`/`isMobile`. `UserService.UpdateUserDeviceAsync:713-757` only **records** — it never compares, never refuses, and never writes `IsMobile`/`IsDesktop`. It calls `IJSRuntime` directly, so it **cannot run inside an API request at all** |
| **QR token expiry** (Q-05, R-16) | `QrLogin.razor:50-56`, `Login.razor:422-429` | Enforced *post-query*, in **two duplicated copies**. `UserRepository.GetUserByQrToken:52-60` still **returns expired users**, so correctness depends entirely on which caller checks |

**Impact.** Every React login bypasses three live gates. Trial enforcement, device binding
and QR expiry silently cease to exist the moment the SPA becomes the front door — and
because two of the three are client-asserted or duplicated in the UI, they cannot simply be
lifted as-is.

**Action.** Task **M2-A08**. Each gate needs a product decision before it is ported: turning
on an enforcement that was previously bypassable *can lock existing users out*. Device
binding in particular cannot be ported unchanged — a server-side API cannot call
`IJSRuntime`, and client-asserted device identity is not a security control.

### R-04 — **82** of 94 stored procedures have no DDL in source control
**Confirmed.** 94 distinct `Sp_*` names referenced from C#/Razor; 13 `.sql` files in
`Existing Store Procedures/StoredProcedures/`.

> **Corrected 2026-08-12 → the gap is 82, not 81.** Only **12** of the 13 scripted files map
> to a name the application actually calls:
>
> - `Sp_Print_PurchaseOrder.sql:1` declares `[dbo].[Sp_Print_PurchaseOrder]` — a name
>   referenced **nowhere** in C#/Razor. It is dead DDL.
> - The application calls **`Sp_Print_PurchasePo`** (`PurchasePoDetails.razor:306`,
>   `PurchPOUpsert.razor:4596`, `Authorization.razor:723`), which has **no DDL at all**.
>
> So one scripted file is dead and one live print path is unscripted. This is exactly the
> "referenced but never deployed" class the reconciliation was meant to surface.
> *(Confirmed.)*
>
> A second, softer mismatch: `Sp_Print_MFGDC.sql` declares `[dbo].[Sp_Print_MFGDC]` while
> code calls `Sp_Print_MfgDC` (`MfgDcDetails.razor:395`, `MfgDcUpsert.razor:3351`).
> Case-only, and resolves under a default case-insensitive collation — **Inferred**, because
> no live tenant collation has been observed. Task **M0-01-01** must confirm it.
>
> **Reproducing the 94.** INV-009's recorded command (`grep -rhoE "Sp_[A-Za-z0-9_]+" | sort -u`)
> is unscoped and now returns **111**, because it matches the `.sql` files and this knowledge
> base's own prose — the KB has begun contaminating its own evidence. The correct, scoped
> form is:
> ```bash
> grep -rhoE "Sp_[A-Za-z0-9_]+" --include=*.cs --include=*.razor --exclude-dir=obj --exclude-dir=bin V.SMART | sort -u
> ```
> which returns exactly 94. Do not "correct" 94 upward on the strength of the unscoped count.
>
> **M0-01-01 complete, 2026-08-13.** The full systematic reconciliation — every referenced
> name classified `scripted` / `case_mismatch` / `missing` / `unreferenced` with `file:line`
> evidence for each — is in
> [KB-102](../architecture/stored-procedure-inventory.md) and
> [`db/stored-procedures/manifest.csv`](../../../db/stored-procedures/manifest.csv). Counts
> confirmed: 11 `scripted`, 1 `case_mismatch`, 82 `missing`, 1 `unreferenced` (13 declared,
> 94 referenced — arithmetic closes both ways, see KB-102). The 82 `missing` rows are
> M0-01-02's worklist.
**Impact.** A tenant database cannot be rebuilt from the repository. Reports and the entire
`ReportExecutor` path break in any fresh environment. No review, no versioning, no rollback
for procedure changes.
**Action.** Script all 82 `missing` procedures (per KB-102's manifest) from a live tenant
database into `db/stored-procedures/`, one file each, following the conventions in
[`db/stored-procedures/README.md`](../../../db/stored-procedures/README.md), and add a
deployment step (M0-01-03). **Do this before any other work** — it is cheap and it is
currently a single-point-of-failure for the product.

### R-05 — No automated tests, no CI
**Confirmed.** No test project; `.github/` contains no workflows.
**Impact.** ~250k LOC of business logic with no regression safety net, about to undergo the
largest change in its life. Every refactor is unverifiable.
**Action.** Stand up CI (build + analyzers) immediately (M0-07). Add characterisation tests
for `ICalculationService` and `IStockManagerService` **before** touching them (M0-12, M0-13)
— these two are the highest-consequence code in the system.

> **Test-harness constraint discovered 2026-08-12 (Confirmed).** The two services need
> *different* harnesses, and the difference is not a matter of taste:
>
> - `CalculationService` has **no constructor dependencies** — it is testable as a pure unit,
>   with no database at all.
> - `StockManagerService` and `MfgPoService` apply **EF Core async operators** to
>   `IRepository<T>.GetQueryable()` results, so a collection-backed repository stub throws at
>   runtime. They need a real EF provider.
> - `ApplicationDbContext.OnModelCreating` calls the **relational-only `ToView(null)` 65
>   times**, so the EF Core **InMemory** provider probably cannot build the model at all;
>   **SQLite in-memory** probably can. *(Inferred — neither has been executed.)* Task
>   **M0-12-01** must spike both and record the exact exception text before M0-13, M0-09 and
>   M0-06 can proceed; each carries an explicit Blocked condition if no fixture materialises.

---

## High

### R-06 — ~184,000 LOC of logic inside Razor `@code` blocks
**Confirmed.** 57% of 321,661 Razor LOC. Traced in `MfgPOUpsert.razor`: validation,
quantity balancing, cancellation, short-close, and cascade rules all live in the page.
**Impact.** Deleting the Blazor UI without extraction destroys real ERP behaviour. Drives
every "Very High" complexity rating in the feature map.
**Action.** Per-module triage into three buckets — presentation (discard), data loading
(becomes API calls), business logic (extract to service). Extraction happens **before** the
corresponding React screen is built, and is validated against the still-running Blazor app.

### R-07 — FIFO stock issue silently under-allocates
**Confirmed.** `StockManagerService.cs:209-233` (re-verified 2026-08-12; previously cited as
`:203-231`) — `TrackStockUsageAsync` (declared `:177`) throws only when
**no** batch has `BalQty > 0`. If batches exist but total balance is short, the loop
finishes with `remainingQty > 0` and returns normally: `StockIssue.IssueQty` records the
full quantity while `StockIssueTrack` accounts for less.
**Impact.** Stock ledger drifts out of balance without any error. Affects every SCN/MIN
path.
**Action.** Confirm intent (Q-01). If unintended, add a post-loop check and fix in the
service so both UIs benefit. Add tests first.

### R-08 — Copy-paste defects in delete guards
**Confirmed.** `MfgPoService.cs:504` tests `hasInvoice` where it computes `hasExpInvoice`;
`:525` tests `hasRc` where it computes `hasCR`. Two guards are unreachable, so a Sales
Order with only an export invoice, or only a contract review, can be deleted.
**Impact.** Referential-integrity violation → orphaned downstream documents.
**Action.** Fix both; then audit **every** `CanDelete…` method for the same pattern
(INV-025, task M0-10). An API makes these branches far easier to reach than the current UI
does.

> **Scope correction, 2026-08-12 (Confirmed).** "~40 methods" understated the audit by more
> than half. A scoped grep over `V.SMART/V.SMART.Shared/BusinessLayer/` returns **63**
> implementations. **Three of them are not `Async`-suffixed**, so an audit scoped by the
> `Async` suffix silently misses them:
>
> - `OutSourcingService/PurchOrSubConQuoteService/PurchaseQuoteService.cs:864` — `CanDeletePurchaseQuote`
> - `OutSourcingService/SubContractDcOutService/SubConDcOutService.cs:2262` — `CanDeleteSubConDcOutgoing`
> - `ProductionService/ProductionLogService.cs:192` — `CanDeleteProductionLog`
>
> All three return the same `(bool CanDelete, string Message)` tuple as their `Async`-suffixed
> peers. M0-10 establishes the canonical count and must search by the `CanDelete` prefix, not
> the `Async` suffix.

### R-09 — Default administrator account with a committed password hash
**Confirmed.** `ApplicationDbContext.cs:1136` seeds `UserName = "Administrator"` with a
fixed PBKDF2 hash, `Role = Administrator`, `IsActive = true`, in **every** tenant database.
**Impact.** A known default credential across all tenants. The plaintext is recoverable
offline from the committed hash.
**Action.** Force a password change on first login; or seed with a random per-deployment
password; or disable the account after real users exist.

### R-10 — `ScreenCode` magic numbers with no typed definition
**Confirmed.** `StockManagerService.AddOrUpdateStockAsync/IssueOrUpdateStockAsync` take
`int screenCode`, which callers pass as literals. `StockAdd.ScreenCode`/`StockIssue.ScreenCode`
are the stock-movement source discriminator. The only definition is 152 seeded `Screens`
rows. No enum or constants class exists.
**Impact.** A wrong literal silently misattributes stock movements and corrupts stock
position reports. Invisible to the compiler.
**Action.** Generate a `ScreenCodes` static class from the seed and replace all literals
before the API exposes stock operations.

### R-11 — `IApprovalService` depends on a Razor page type
**Confirmed.** `IApprovalService.cs` declares
`using static V.SMART.Shared.Pages.Planning_Module_Pages.Authorization_Pages.Authorization;`.
Plus 13 other business/data/mapping files reference `V.SMART.Shared.Pages`.
**Impact.** The business layer cannot be consumed without the UI assembly; the approval
workflow cannot be exposed over HTTP as-is.
**Action.** Move the shared types into `ViewModels/`; remove all `Pages` references from
non-UI projects; add an architecture test to keep it that way.

### R-12 — Document numbering race condition
**Inferred (high confidence)** — the risk stands, but **the stated cause was wrong**; see the
correction below.

~~Inferred. ~20 repositories derive the next document number with `SELECT TOP 1 * … ORDER BY
… DESC` and no lock, no `UPDLOCK`, no serializable transaction, no DB sequence.~~

> **Corrected 2026-08-12 (Confirmed).** Two errors, one of them dangerous:
>
> **1. The lock hint is already there.** All **36** repository files that derive a next
> number use `FROM <Table> WITH (UPDLOCK, ROWLOCK)`. Example —
> `SalesAndLabourRepository/SalesDCRepository/MfgDcRepository.cs:31-36`:
> ```sql
> SELECT TOP 1 * FROM MfgDc WITH (UPDLOCK, ROWLOCK)
> WHERE suffix = {0} ORDER BY TRY_CAST(DcNo AS INT) DESC
> ```
> Exactly one site lacks a hint:
> `ProductionRepository/ProductionIssueWOAssyRepo/ProductionIssueAssyRepository.cs:73-81`.
>
> **Why this matters:** anyone acting on the old wording would add a hint that is already
> present and conclude the race was fixed. It is not.
>
> **2. The race is real anyway, for different reasons.** `UPDLOCK, ROWLOCK` takes an update
> lock on the **row read** — it is not a **range** lock. Two sessions that read when no row
> qualifies lock nothing at all, and a concurrent insert of a *higher* number is a phantom
> that `ROWLOCK` cannot prevent. Worse, unless the read and the subsequent insert share one
> explicit transaction, the lock is released at statement end, leaving the read→insert gap
> unprotected. Closing it requires range locking (`HOLDLOCK`/`SERIALIZABLE`) across
> read+insert, an application lock, or a sequence — **not** a stronger row hint.
>
> **3. "~20 repositories" undercounts.** There are **38 raw-SQL sites across 36 files**, plus
> ~7 lock-free LINQ sites (e.g. `MfgPoService.cs:1623-1661` `GetNextSaleOrderNoAsync`,
> `:1699-1727` `GetNextOANumberAsync`) and a third mechanism entirely: an allocation-table
> read-modify-write in `CommonService.cs:1845-1963` (`GenerateAutoRunningNoAsync`) with
> inline copies in `MfgDcService`, `MfgInvService`, `ExpInvService` and
> `LabourInvoiceService`. Three mechanisms, not one.
>
> **Two constraints on the remedy, discovered with it:**
> - `MfgDcService.cs:377-381` **decrements** `LastNumber` on delete, to avoid gaps. That
>   rules out a plain `CREATE SEQUENCE`, which cannot be decremented.
> - `MfgDcService.IsDuplicateDcNoAsync:771-790` scopes uniqueness **by `CustId`**, so an
>   unqualified `(DcNo, Suffix)` unique index would reject data the application currently
>   accepts.
>
> **Related defect (Confirmed):** `CommonService.cs:1957-1961` swallows its exception and
> returns `null`, so a failed allocation silently yields a null document number.
>
> **Related divergence (Confirmed):** there are **two** financial-year implementations with
> the **same boundary but different output shapes** — `FinancialYearHelper.cs:13`
> (`Month >= 4` → `"/{yyyy}-{yy}"`) and `CommonService.cs:1849-1851` (`Month > 3` →
> `"{yy}-{yy}"`). Which one reaches the stored `Suffix` is **Unknown** and is M2-B12-01's
> job. **M2-B12-03 is forbidden from unifying them** — the suffix is user-visible on
> statutory documents.
**Impact.** Concurrent creation produces duplicate document numbers. Currently masked by
low concurrency in Blazor Server; an API will increase concurrency.
**Action.** Verify against the live schema for unique constraints (Q-10). Replace with a
DB sequence or a `sp_getapplock`-guarded allocation. Add idempotency keys on create
endpoints.

### R-13 — Unknown index coverage under new load
**Unknown.** Only one index-related migration (`AddInDexingToCustomer`). List screens run
`SearchWithDynamicFilterAsync` over large document tables.
**Impact.** An SPA increases query volume and concurrency; missing indexes will surface as
timeouts (note the 60 s command timeout and 300 s report timeout already configured).
**Action.** Capture an execution-plan baseline from a production-sized tenant before Phase 3.

### R-14 — ~~Build output committed to the repository~~ → **RETRACTED / RESTATED**
**Retracted 2026-08-12 (INV-029).** The original entry claimed — marked *Confirmed* — that
`frontend/vsmart-erp/dist/`, `frontend/vsmart-erp/.angular/cache/`, `.vs/` and
`*.csproj.user` were committed. **That is false.** It conflated *present on disk* with
*tracked by git*.

Verified against `git ls-files` (2,162 tracked paths):

| Pattern | Tracked files |
|---|---|
| `dist/` · `.angular` · `node_modules/` · `.vs/` · `csproj.user` · `/bin/` · `/obj/` | **0 each** |

They are correctly ignored — `frontend/vsmart-erp/.gitignore:4,10,32` and the root
`.gitignore:9,37`. Per [KB-002](../source-of-truth-rules.md), the repository is
authoritative over the register: **the register was wrong.** Acting on it as written would
have meant an unnecessary destructive history rewrite, colliding with M0-05.

**Restated risk (Confirmed).** The real issue is the inverse — large parts of the tree are
**not tracked at all**:

| Path | Tracked files in `HEAD` | Consequence |
|---|---|---|
| **`V.SMART/V.SMART.Api/`** — the entire Web API project | **0** | **The backend the React app is being built on is not in source control.** Untracked, not gitignored (`git check-ignore` exits non-zero) |
| **`docs/`** — the entire knowledge base | **0** | All analysis, ADRs, risk register and the execution plan exist only on one disk |
| `frontend/` (the whole Angular pilot) | **0** | |
| `.github/` (incl. an empty `workflows/`) | **0** | CI cannot run until this is committed |
| `NexGen-ERP---2025-master.sln` (the solution that builds) | **untracked** | a fresh clone gets a different, deleted-on-disk `.sln` |

The only tracked `.sln` is `Bhargavi V.SMART ERP - 2025.sln`, which is *deleted* in the
working tree. So a fresh clone does not reproduce the developer's environment, and CI cannot
run until `.github/` is committed. This bears directly on gate **G0**'s "rebuild from source
control alone" criterion.

**Action.** Handled by **M0-00** (deliberate disposition for all 37 working-tree entries,
including committing `.github/` and resolving the solution-file rename) and **M0-08**
(verify the ignore rules, hoist them somewhere durable — the nested
`frontend/vsmart-erp/.gitignore` disappears when M2-C11 archives the pilot — and add an
automated check so the property is enforced rather than assumed). No history rewrite is
needed for build output.

---

## Medium

### R-15 — Invalid GST rate silently coerced to zero
**Confirmed.** `CommonConstants.GetIGST/GetGST` use `FirstOrDefault(r => r == rate)`,
returning `0` for an unlisted rate rather than raising.
**Action.** Return `decimal?` or throw; validate at the API boundary.

### R-16 — QR token expiry not enforced
**Confirmed.** `GetUserByQrToken` checks `QrToken`, `IsQrEnabled`, `IsActive` but **not**
`QrExpiryDate`, which the schema stores.
**Action.** Add the expiry predicate when building the QR login endpoint.

### R-17 — `SessionTimeoutService` is a shared singleton
**Confirmed.** `AddSingleton` with one `_lastActivity` field → all users share one idle
clock.
**Action.** Do not port. Implement idle timeout client-side plus server token expiry.

### R-18 — `CurrentUserService.GetUserRoleAsync()` always returns empty
**Confirmed.** Reads claim type `"role"`; the providers write `ClaimTypes.Role`. Currently
zero call sites, so latent.
**Action.** Fix or delete; do not replicate in the API.

### R-19 — Login swallows exceptions
**Confirmed.** `UserRepository.cs:44-48` catches all exceptions and returns `null`.
**Impact.** A database outage is reported as "invalid username or password"; incidents are
misdiagnosed.
**Action.** Let infrastructure failures propagate to a 500; keep 401 for genuine
credential failure.

### R-20 — `DetailedErrors = true` unconditionally in Blazor Server
**Confirmed.** `AddServerSideBlazor(o => o.DetailedErrors = true)` in
`V.SMART.Web/Program.cs`, plus `"DetailedError": true` in `appsettings.json`.
**Impact.** Stack traces reach the browser in production.
**Action.** Gate on `IsDevelopment()`.

### R-21 — Incomplete `IQSMART` → `V.SMART` rename
**Confirmed.** `V.SMART.Web/Program.cs` imports five `IQSMART.Shared.*` namespaces
alongside `V.SMART.Shared.*`, and one `V.SMARTV.Shared.…` (a typo namespace).
**Impact.** Confusing navigation; suggests other half-renamed identifiers.
**Action.** Complete the rename in one mechanical pass.

### R-22 — Two overlapping design systems in the UI
**Confirmed.** MudBlazor 8 and Bootstrap 5 CSS both loaded and both used.
**Impact.** Visual inconsistency; part of why the product looks dated.
**Action.** Resolved by construction in the React rewrite — one library only.

### R-23 — Flat-file logging with no aggregation, and an unsanitised path
**Confirmed.** `FileLoggingService` writes flat text files under `App_Data/Logs/`.
**Impact.** No searchability, no alerting, unbounded growth, lost on container restart.
**Action.** Structured logging (Serilog) to a real sink (M2-B11); preserve the *user-action
audit trail* as a first-class feature.

> **Corrections and an added security finding, 2026-08-12 (Confirmed).**
>
> **1. "Per user per day" is true of only one stream.** `LogUserAction` writes
> `Logs/UserLogs/{date}_User_{UserName}.txt` (`:31`). But `WriteDeveloperLog` writes a
> **single shared** `Logs/DeveloperLogs/{date}_ErrorLog.txt` for the whole application
> (`:76`) — so under concurrency it is a contention point *and* an interleaving hazard, not
> merely unsearchable.
>
> **2. `UserName` is interpolated into a file path with no sanitisation** (`:31`).
> `Path.Combine` does not sanitise: a username containing `..` traverses, and a *rooted*
> username causes `Path.Combine` to discard the preceding segments entirely — an arbitrary
> file-write primitive. Severity depends on who controls usernames: today they are
> administrator-created, but **a `/register` route exists** (Q-09), and if self-registration
> is reachable this is exploitable by an anonymous user. **Sanitise on the way in, and settle
> Q-09.** Not previously recorded.
>
> **3. Every logging failure is swallowed.** `LogInternalFailure` (`:89-104`) terminates in
> an empty catch, so the audit trail can fail silently and indefinitely. An audit trail that
> can silently stop is not an audit trail — this matters for the "preserve it as a
> first-class feature" instruction above.
>
> **4. `_basePath` may be null on some targets.** The `#if ANDROID || WINDOWS || MACCATALYST`
> branch (`:11-16`) has its assignment commented out. Source state is **Confirmed**; the
> runtime consequence on the Windows-targeted build is **Unknown** and must be checked before
> M2-B11 relies on the path.

### R-24 — No API error contract
**Confirmed.** `CurrencyController` returns two different 400 shapes; no exception
middleware; no `ProblemDetails`.
**Action.** Standardise before controller proliferation
([`api/api-readiness-assessment.md`](../api/api-readiness-assessment.md)).

### R-25 — Business logic executed in Razor with direct `SaveAsync`
**Confirmed.** 91 `SaveAsync` call sites inside `Pages/`.
**Impact.** Writes outside any service transaction; partial-write risk.
**Action.** Fold into service methods during `@code` extraction.

### R-26 — Duplicated DI composition roots
**Confirmed.** `V.SMART.Web/Program.cs` (34.8 KB, 242 registrations) and
`V.SMART/MauiProgram.cs` (38.6 KB) register the same graph independently.
**Impact.** They will drift; a service added to one host is missing in the other.
**Action.** Extract a shared `AddVSmartDomain(this IServiceCollection)` extension in
`V.SMART.Shared` and call it from all three hosts.

### R-27 — Hardcoded developer-machine values in the MAUI project
**Confirmed.** `PackageCertificateThumbprint`, `AppInstallerUri = D:\` in
`V.SMART.csproj`.
**Action.** Move to build parameters.

---

## Low

| # | Item | Evidence |
|---|---|---|
| R-28 | Folder/namespace typos: `Data/Maintanence`, `Estimaton`, `Fesibility`, `ProuctionCompRepo`, `EstiamateId`, `Advaceadjustment`, `Sub-Contrect GRN`, `/hr-masterr` | throughout |
| R-29 | Empty folders declared in `V.SMART.Shared.csproj` (~30 `<Folder Include=…>` entries) | `.csproj` |
| R-30 | 219 migrations, ~2.5M LOC, ~90% of repo size | `Migrations/` |
| R-31 | Dead role `"ERPAdmin"` in `AuthorizeView` but absent from `UserRole` | `NavMenu.razor` vs `Data/Enum/UserRole.cs` |
| R-32 | Large blocks of commented-out code (e.g. `ReportExecutor.cs:47-80`) | multiple files |
| R-33 | `Underconstruction.razor` shipped in the component set | `Components/` |
| R-34 | Angular pilot will become dead code once React starts | `frontend/vsmart-erp/` |
| R-35 | Two per-tenant path conventions (`CompanyName` vs `Hostname`) | `TenantProvider` vs `ReportService` |
| R-36 | Inconsistent route casing (`/MfgPO/create` vs `/mfgPO/details`) | `Pages/` |
| R-37 | `docs/ARCHITECTURE.md` is an unfinished template with `[TODO: ANALYZE]` markers presented as documentation | `docs/` |

---

## Priority sequence

**Week 1 (do these regardless of the migration):** R-01, R-02, R-04, R-09.
**Before the second API controller:** R-03, R-24, R-11.
**Before any stock-touching module migrates:** R-05 (tests first), R-07, R-10.
**During Phase 2–3:** R-06 (per module), R-12, R-13, R-26.
**Opportunistic:** everything Low.
