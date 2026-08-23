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
  - V.SMART/V.SMART.Shared/Data/MigrationData/ApplicationDbContextFactory.cs
  - V.SMART/V.SMART.Shared/Data/MigrationData/MasterDbContextFactory.cs
  - V.SMART/V.SMART/MauiProgram.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/EInvoiceAPIService/EinvoiceDatabaseService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/EInvoiceAPIService/EWayDatabaseService.cs
  - db/deploy-stored-procedures.ps1
  - db/RUNBOOK-rebuild-tenant-database.md
  - V.SMART/V.SMART.Web/Services/WebFileUploadService.cs
status: complete
confidence: mixed
last_verified: 2026-08-23
dependencies: [KB-011, KB-012, KB-013, KB-040, KB-102]
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

> **Status update, 2026-08-17 (M0-03-01, Confirmed by direct inspection):** the
> **working-tree configuration files are now clean**. `V.SMART/V.SMART.Web/appsettings.json`
> declares `ConnectionStrings:MasterDb` as `""` and the commented production connection
> string (the production host and the `bspl` user named above) was **deleted outright**;
> `V.SMART/V.SMART.Api/appsettings.json` likewise carries `""`. Both hosts now take the value
> from user-secrets (development) or `ConnectionStrings__MasterDb` (servers/CI) — see
> [`docs/CONFIGURATION.md`](../../CONFIGURATION.md). `git grep -n "Password=" --
> "V.SMART/V.SMART.Web" "V.SMART/V.SMART.Api"` and the equivalent grep for the production
> host's IP address both return zero hits.
>
> **Still outstanding, and not touched by M0-03-01:**
> - the hardcoded connection strings in the two `MigrationData` factories and
>   `MauiProgram.cs` — **M0-03-02**;
> - rotation of every exposed credential — **M0-04** (the values are compromised regardless
>   of the working tree being clean);
> - the git-history purge — **M0-05**. `git grep -l "<the SA password>" HEAD` still returns
>   the committed files, which is expected at this point and is not an M0-03-01 failure.
> - the plaintext per-tenant connection strings in the `Tenants` table (KB-014) — file
>   configuration was never their source, so this task could not affect them.
>
> **Finding, 2026-08-17 (M0-03-01):** the SA password literal is quoted **in this document**
> at line 36 and in several `docs/kb/execution/tasks/*.md` files (inside example `git grep`
> commands), exactly repeating the R-02 exposure pattern recorded below. M0-05's purge
> surface therefore includes KB documents, not only source files. Out of scope for M0-03-01,
> which is forbidden from rewriting history.

> **Status update, 2026-08-18 (M0-03-02, Confirmed by direct inspection and by
> `git grep --untracked`):** the **working tree now holds no database-credential literal at
> all**, in C# or in configuration. The C# sites listed above, with the line numbers they
> occupied before this task, were:
>
> | File | Lines (pre-M0-03-02) | What was there |
> |---|---|---|
> | `V.SMART/V.SMART.Shared/Data/MigrationData/ApplicationDbContextFactory.cs` | `:13` active, `:14` commented | SA literal; commented production host `154.61.76.112,1533` / `bspl` |
> | `V.SMART/V.SMART.Shared/Data/MigrationData/MasterDbContextFactory.cs` | `:11` commented, `:12` active | commented production host; active SA literal |
> | `V.SMART/V.SMART/MauiProgram.cs` | `:228` commented, `:231` active, `:235` commented | production host; active SA literal; third host `VK-7-HP\SQLEXPRESS` |
> | `V.SMART/V.SMART.Web/appsettings.json`, `V.SMART/V.SMART.Api/appsettings.json` | — | already cleaned by M0-03-01 |
>
> Both design-time factories now resolve their connection string through
> `V.SMART/V.SMART.Shared/Data/MigrationData/DesignTimeConnectionString.cs` (environment
> variable, then this project's user-secrets) and **throw** when neither supplies a value —
> `MasterDbContextFactory` on `ConnectionStrings:MasterDb`, `ApplicationDbContextFactory` on
> the new `ConnectionStrings:DesignTimeTenantDb` (it builds a *tenant* context, so it must
> not overload the master key). `MauiProgram.cs` reads `ConnectionStrings__MasterDb` /
> `ConnectionStrings:MasterDb`; the two commented registrations are deleted. There is no
> default value anywhere — see [`docs/CONFIGURATION.md`](../../CONFIGURATION.md).
>
> The **gateway** credential (item 4 above) is a *distinct* credential requiring its own
> rotation, with its own owner and its own procedure — it is not covered by any
> connection-string remediation. Its comments at
> `EinvoiceDatabaseService.cs:1413-1414` and `EWayDatabaseService.cs:900-901` are now
> deleted; the values remain compromised until **M0-04** rotates them at the GST gateway.
>
> **Still outstanding:** rotation of every exposed value (**M0-04**), the git-history purge
> (**M0-05** — `git grep -l "<the SA password>" HEAD` still returns the committed files and
> several KB documents, which is expected), and the plaintext per-tenant connection strings
> in the `Tenants` table (KB-014).
>
> **Correction, 2026-08-18 (M0-03-02):** item 2's `bspl` list is stale in one respect.
> `V.SMART/V.SMART.Web/Components/App.razor` and
> `V.SMART/V.SMART.Shared/Pages/Master_Module_pages/Identity_Pages/Login.razor` contain
> **no** occurrence of `bspl` in any casing today (`grep -ic` returns `0` for both). The
> surviving non-credential occurrences are `Pages/Home.razor:198,316` (public contact email
> `contactbspl@nexgenerp.com`), `V.SMART.Shared.csproj:19-20` (image file names),
> `Components/ProcessingOverlay.razor:31` (image name), three `upi://pay` sample strings in
> `Payments.razor:1412`, `Receipts.razor:1414`, `AdvanceAdjustment.razor:1359`, and
> `V.SMART/V.SMART.Api/wwwroot/config/tenant.json:2,5`. **None is a credential.**

> **Status update, 2026-08-18 (M0-03-03, Confirmed by running both hosts):** both web hosts
> now **refuse to start** when `ConnectionStrings:MasterDb` is missing, empty, whitespace, or
> equal to a known pre-rotation default. The check is
> `V.SMART/V.SMART.Shared/Services/StartupConfigurationValidator.cs`, called from
> `V.SMART/V.SMART.Api/Program.cs` and `V.SMART/V.SMART.Web/Program.cs` immediately after
> `WebApplication.CreateBuilder(args)`. The known-default deny-list is held as **SHA-256 hex
> digests only** — no plaintext re-enters the tree — and covers the six distinct historical
> connection strings recoverable from git history (`c12c5b2` Web `appsettings.json`, both
> `MigrationData` factories, `MauiProgram.cs`, and the later local variant on `6dbf4b4`),
> plus one further local variant recorded by an earlier session from the then-untracked
> `V.SMART.Api/appsettings.json`. It is a **tripwire, not a security control**: it catches
> only exact matches of values already known to be leaked.
> **This closes only the "fail startup" clause of the action above. Rotation (M0-04), the
> history purge (M0-05) and the plaintext per-tenant connection strings in the `Tenants`
> table (KB-014) all remain open.** Implemented and validated `PASS` on
> `migration/M0-03-03-startup-config-validation` (merged to `master` 2026-08-18) — see
> [`tasks/M0-03-03.md` § Execution Record](../execution/tasks/M0-03-03.md#execution-record-2026-08-18).

### R-02 — JWT signing secret committed
**Confirmed.** `V.SMART.Api/appsettings.json` `Jwt:Secret` holds a hardcoded default value
containing the literal words "Change In Production" — i.e. it was never rotated.
**Impact.** Anyone with the repo can forge tokens for any user and any `TenantId` —
complete cross-tenant compromise once the API is live.
**Action.** Move to secret storage; rotate; fail startup if the secret is missing or is
the known default (M0-03-01, M0-03-03).

> **Status update, 2026-08-17 (M0-03-01, Confirmed by direct inspection and by running the
> host):** `V.SMART/V.SMART.Api/appsettings.json` now has `Jwt:Secret` as `""`; the value is
> supplied by user-secrets locally and by `Jwt__Secret` on servers/CI. Rotation (M0-04) and
> the history purge (M0-05) remain outstanding — the previously committed value stays
> compromised.
>
> **Finding for M0-03-03 (Confirmed, reproduced 2026-08-17, not inferred):** the existing
> guard `builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret
> is missing from configuration.")` at `V.SMART/V.SMART.Api/Program.cs:56-57` is a **null**
> check only. Now that `appsettings.json` declares the key with an empty value — which is what
> M0-03-01 mandates, so the configuration shape is discoverable — the guard's behaviour now
> depends on *how* the secret is missing. Both cases were run, not inferred:
>
> | How `Jwt:Secret` is missing | Observed startup failure |
> |---|---|
> | Key **removed** from every configuration source (removed from `appsettings.json` *and* no user-secret / `Jwt__Secret`) | `System.InvalidOperationException: Jwt:Secret is missing from configuration.` at `V.SMART/V.SMART.Api/Program.cs:56` — the application's own message, as designed |
> | Key **present but blank** (`""` in `appsettings.json`, no user-secret / `Jwt__Secret`) | `System.ArgumentException: IDX10703: Cannot create a 'Microsoft.IdentityModel.Tokens.SymmetricSecurityKey', key length is zero.` at `V.SMART/V.SMART.Api/Program.cs:58` — a framework message, not the application's own |
>
> **The fail-fast safety property holds in both cases — the host does not start.** The gap is
> diagnostic quality in the second case, which is the one the committed configuration shape
> produces on a machine with no secret configured. That is the case M0-03-03 must close.
> A second, independent copy of the same null-only guard exists at
> `V.SMART/V.SMART.Api/Auth/JwtTokenService.cs:20-21` and is not mentioned in M0-03-01's or
> M0-03-03's task file. M0-03-03 must harden **both**, and must check for empty/whitespace,
> the known default value, and minimum length — not merely `null`.

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

> **Status update, 2026-08-18 (M0-03-03, Confirmed by running the host — every case below was
> executed, not inferred):** the "**fail startup if the secret is missing or is the known
> default**" clause of the action above is **DELIVERED**. `V.SMART.Api` now throws
> `InvalidOperationException` before any other registration when `Jwt:Secret` is null, empty,
> whitespace, shorter than 32 UTF-8 bytes, or equal to the known default (matched by SHA-256
> digest — the plaintext is not in the tree), and when `Jwt:Issuer` or `Jwt:Audience` is
> missing or empty. Observed:
>
> | Case | Observed |
> |---|---|
> | `Jwt:Secret` unset | `InvalidOperationException: Startup configuration is invalid: Jwt:Secret is missing, empty, or whitespace…` |
> | `Jwt:Secret` empty | same message |
> | `Jwt:Secret` 10 characters | `…Jwt:Secret is shorter than the required 32 bytes in UTF-8…` |
> | `Jwt:Secret` = the known default | `…Jwt:Secret matches a known published default value (see …R-02)…` |
> | `Jwt:Issuer` / `Jwt:Audience` whitespace | `…Jwt:Issuer is missing, empty, or whitespace…` / same for `Jwt:Audience` |
> | all keys set to valid non-default values | host starts normally |
>
> No message repeats the offending value (R-23: the flat-file logger writes plaintext per user
> per day). The `IDX10703` framework message recorded above is no longer reachable — the
> application's own message now fires first.
>
> The **second guard** flagged above at `V.SMART/V.SMART.Api/Auth/JwtTokenService.cs:20-21`
> was also hardened: it now delegates to
> `StartupConfigurationValidator.ValidateJwtSecret(IConfiguration)` instead of carrying its own
> null-only `?? throw`, so there is exactly one code path deciding whether `Jwt:Secret` is
> acceptable and the two cannot drift.
>
> **Still open: rotation (M0-04) and the history purge (M0-05).** The previously committed
> secret stays compromised; this task only guarantees it cannot be *used*. Implemented and
> validated `PASS` on `migration/M0-03-03-startup-config-validation` (merged to `master` 2026-08-18) — see
> [`tasks/M0-03-03.md` § Execution Record](../execution/tasks/M0-03-03.md#execution-record-2026-08-18).

### R-03 — Authorization enforced only in the UI layer
**Confirmed.** `BaseUserRightsComponent` + `RightsHelper` are the only permission checks;
no service or repository checks rights. `CurrencyController` carries a bare `[Authorize]`
with no screen-right check.
**Impact.** Every REST endpoint is accessible to any authenticated user regardless of their
`UserRight` rows. Blocks any production API rollout.
**Action.** [ADR-004](../decisions/ADR-004-server-side-authorization.md). Must land before
the second controller is written.
**Update 2026-08-20 (M2-A01-02) — the mechanism landed; the risk stays OPEN.**
`V.SMART/V.SMART.Api/Authorization/` now contains `[RequireScreen]`, `[RequireRight]`,
`[NoScreenRight]`, the `IUserRightsProvider` seam and `ScreenRightAuthorizationFilter`,
registered globally in `Program.cs`, plus `ScreenRightStartupValidator` for the KB-105 D-4/D-6
misannotation checks. **No controller declares the attributes**, so nothing is enforced yet
and every endpoint remains as exposed as this entry describes. Closing tasks: **M2-A02**
(annotate `CurrencyController`) and **M2-A03** (permission-matrix suite).
One sub-condition of KB-105 D-4 is deliberately not yet enabled and is M2-A02's to switch on:
an authenticated action on a controller carrying *no* `[RequireScreen]` at all is presently
allowed through rather than refused, because refusing it — in the startup form, refusing to
start the host — would have broken the API's six existing unannotated endpoints in the same
change that introduced the mechanism. Until M2-A02 flips it, a controller added without
annotations is silently unprotected, which is the R-03 failure mode itself.
**Close-out addendum, 2026-08-20 (independent validation, not the implementer's own finding):**
the globally registered `ScreenRightAuthorizationFilter` constructs `IUserRightsProvider` (and
therefore `IUnitOfWork` → `UnitOfWork.cs:488` → `TenantDbContextFactory.GetCurrentTenant()`) via
DI on **every** request that reaches MVC's filter pipeline, including unannotated actions where
the filter's own short-circuit means it never calls `GetAsync`. Not a regression today —
`UseAuthorization()` middleware rejects a tokenless caller before MVC builds the filter
pipeline (verified live: `401`, not a DI-construction `503`), and `AuthController.Login`
already constructs `IUnitOfWork` in its own constructor. Becomes relevant once `M2-A02`
annotates a real controller and an authenticated request's tenant is unresolvable: the tenant
`DbContext` then resolves one filter-pipeline step earlier than before this task. A lazy
provider injection (e.g. `Func<IUserRightsProvider>`) removes it if `M2-A02`'s validation
surfaces it as an actual problem — recorded here so it is not rediscovered as a mystery.

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

**Impact.** Every SPA login bypasses three live gates. Trial enforcement, device binding
and QR expiry silently cease to exist the moment the SPA becomes the front door — and
because two of the three are client-asserted or duplicated in the UI, they cannot simply be
lifted as-is.

**Action.** Task **M2-A08**. Each gate needs a product decision before it is ported: turning
on an enforcement that was previously bypassable *can lock existing users out*. Device
binding in particular cannot be ported unchanged — a server-side API cannot call
`IJSRuntime`, and client-asserted device identity is not a security control.

**Status after M2-A08 (2026-08-20) — partly closed, and the remainder is deliberate.**
The **trial gate** is now enforced on `POST /api/auth/login`, with all three carve-outs ported
verbatim and the message byte-for-byte from `Login.razor:273`; it returns `403` with a
distinguishable `ProblemDetails.type`, never a `401`. The **QR expiry** half is fixed in the
query (see R-16), though the API still has no QR endpoint. The **device gate** is ported as a
tested evaluator (`V.SMART/V.SMART.Api/Auth/AccountGates.cs`, `DeviceGate`) but is **not wired
in**: decision P4 is deferred and unanswered — **Q-40**. So one of the three bypasses is closed,
one is fixed but unreachable, and one is open **on the record** rather than by omission.
[KB-108](../architecture/row-scope-and-account-gates.md) §4.

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
> **Independently re-verified 2026-08-13 (M0-01-01, Confirmed).** All of the above was
> re-derived from scratch in a separate session, not copied from this register: 94
> referenced / 13 declared / 11 exact `scripted` / 1 `case_mismatch`
> (`Sp_Print_MFGDC` declared vs. `Sp_Print_MfgDC` called) / 1 `unreferenced`
> (`Sp_Print_PurchaseOrder`) / **82 `missing`**. Same figures, independently reproduced. The
> full reconciliation, methodology and per-name evidence now live in
> [KB-102](../architecture/stored-procedure-inventory.md), with a machine-readable worklist
> at `db/stored-procedures/manifest.csv` and a re-runnable reference-index generator at
> `db/tools/sp-inventory.sh`. `db/stored-procedures/` did not exist before this task.
**Impact.** A tenant database cannot be rebuilt from the repository. Reports and the entire
`ReportExecutor` path break in any fresh environment. No review, no versioning, no rollback
for procedure changes.
**Action.** Script all procedures from a live tenant database into
`db/stored-procedures/`, one file each, and add a deployment step. **Do this before any
other work** — it is cheap and it is currently a single-point-of-failure for the product.

> ✅ **The "add a deployment step" half is now DONE and, as of 2026-08-21, EXECUTED.**
> `db/deploy-stored-procedures.ps1` (M0-01-03) was written blind, with no database access, and
> carried an `UNVERIFIED` banner for eight days. The M0-01-03 rebuild drill ran it for the
> first time against a database freshly rebuilt from this repository's EF migrations:
> **91 applied / 0 skipped / 0 failed in 2.16 s**, completeness check passing with 0
> undocumented gaps, and a second consecutive run reporting the same 91 with the target's
> procedure count unchanged — so the `CREATE OR ALTER` idempotency claim holds in practice.
> The script's **ordering** assumption (deferred name resolution ⇒ order does not matter),
> previously *Inferred*, is now **Confirmed** for this procedure set. Evidence:
> `db/REBUILD-DRILL-LOG.md` §6.
>
> **The 4 genuinely-absent procedures below are unchanged by this** — they have no DDL to
> deploy, and the drill confirmed they are correctly absent from the rebuilt database (count
> = 0). R-04 stays open on that gap alone; the deployment-step half of it is closed.

> **Updated 2026-08-13 (M0-01-02 half B/C, Confirmed) — the gap is now 4, not 82, but read
> the caveats before downgrading this to non-Critical.**
>
> **78 of the 82 `missing` procedures now have captured DDL** in `db/stored-procedures/`,
> verified by `db/tools/verify-capture.sh` (0 hard failures) — faithful transcriptions
> (`CREATE OR ALTER`, UTF-8 no BOM, LF, no body edits), each independently cross-checked
> against the source text before capture. Full per-procedure record, source, date and
> operator: `db/stored-procedures/CAPTURE-STATUS.md`. `INV-027` is now `Complete`
> (`docs/kb/investigation-registry.md`).
>
> **4 remain genuinely absent** (not a tool defect — cross-checked against the source text
> independently of the live query): `Sp_BomAnalysis`, `Sp_Print_Estimation`,
> `Sp_Print_Receipts`, `Sp_Print_SingleProcessInspection`. Escalated in
> `CAPTURE-STATUS.md` for a human decision, per procedure: dead code (delete the call
> site) or latent defect (the calling screen throws on first use in any rebuilt
> environment).
>
> **Why this stays Critical, not downgraded to Medium/Low, despite closing 78/82:**
> 1. **Provenance is not a nominated production tenant.** The captured DDL's actual origin
>    is `IQSMARTDEMO_DB_2025-26`, a demo database, manually relayed through a local
>    `NexGenErpDb` copy — not a direct capture from a live customer tenant. Whether a demo
>    tenant's procedure set is representative of production is exactly Q-14
>    (`docs/kb/open-questions.md`), owned by M0-02 and still open. A "no" answer there
>    would mean this capture is a starting point, not the final word, and the effective
>    gap could reopen wider than 4.
> 2. **No deployment path exists yet.** Capturing DDL into source control is necessary
>    but not sufficient for "a fresh SQL Server can be brought to a working tenant
>    database from source control alone" (G0 exit criterion 1) — that wiring is
>    M0-01-03's job, not done here.
> 3. The 4 still-open names are unresolved, not closed — one or more could be a live
>    defect waiting to surface.

> **Updated 2026-08-13 (M0-01-03) — deployment step + single-source-of-truth relocation
> both done; G0 criterion 1 still NOT met, drill outstanding.**
>
> This task's own action item — "add a deployment step" — is satisfied by
> `db/deploy-stored-procedures.ps1`: idempotent (`CREATE OR ALTER` everywhere), takes
> connection details as parameters/environment variables only (no hardcoded credential, no
> reuse of R-01/R-02's committed values), refuses to run against an incomplete manifest
> unless explicitly overridden, fails fast and names the offending file, and deploys every
> `.sql` file under `db/stored-procedures/` (recursively — see below).
>
> The competing-locations problem this register flagged is also closed: the 13 files that
> used to live in `Existing Store Procedures/StoredProcedures/` are relocated (via `git mv`,
> bodies unchanged except the mandated BOM strip on 6 of them) into
> `db/stored-procedures/relocated-legacy/`. That folder is retired to a pointer `README.md`.
> `db/stored-procedures/` (flat directory + `relocated-legacy/` subdirectory) is now the
> single authoritative location for every procedure's DDL. The subdirectory split exists
> only because `db/tools/verify-capture.sh` — M0-01-02's harness, not editable by this task
> — enumerates the flat directory with a non-recursive glob and hard-fails any file whose
> manifest status is not `missing`; putting the 13 relocated files there directly would have
> broken that check (verified empirically: 25 hard failures before the subdirectory was
> chosen). `db/deploy-stored-procedures.ps1` deploys both locations together; only
> `verify-capture.sh` and a human reading the directory need to know about the split — see
> `db/stored-procedures/README.md` for the full reasoning.
>
> **Still not resolved, deliberately, per this task's own constraints — escalated to a
> human, not decided here:**
> - `Sp_Print_MFGDC` vs. `Sp_Print_MfgDC` (case-only mismatch) — kept as declared, not
>   renamed either side.
> - `Sp_Print_PurchaseOrder` (unreferenced) — retained and deployed, not deleted.
> - The 4 genuinely-absent procedure names above are exactly as absent as before; nothing in
>   M0-01-03 could change that (no database access — R-01/R-02 constraints held throughout).
>
> **Why this is still Critical, not Resolved:** `db/deploy-stored-procedures.ps1` has **never
> been executed against a real database** — no SQL Server instance or credential was
> available to the session that wrote it, by design (see M0-01-03's own constraints). G0
> exit criterion 1 ("a fresh, empty SQL Server can be brought to a working tenant database
> from source control alone … and the app runs against it") is **not met** until a named
> person runs `db/RUNBOOK-rebuild-tenant-database.md` end to end and records the outcome in
> `db/REBUILD-DRILL-LOG.md` — which is currently a skeleton, every field `TBD`. Downgrade
> this entry only after that drill succeeds (or after its failures are fixed and it
> succeeds on a later attempt) — not on the strength of the tooling existing and looking
> correct on inspection.

### R-05 — No automated tests, no CI
**Confirmed, and still open.** A test project and a CI pipeline now exist; *coverage* does not.

> **Coverage added 2026-08-19 (M0-12-02) — the second of the two services R-05 names.**
>
> `tests/V.SMART.Shared.Tests/Services/CalculationServiceCharacterisationTests.cs` (30 tests)
> and `tests/V.SMART.Shared.Tests/Services/CommonConstantsGstRateTests.cs` (7 tests) add **37**
> characterisation tests over `CalculationService.UpdateTotalsAsync` and the
> `CommonConstants` GST rate lists, taking the suite from 36 to **73 tests, 73 passing**
> (`dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj`, run twice, identical
> both times). They pin all nine algorithm steps, both tax branches, the three silent early
> returns, the divide-by-zero guard, the negative-basic-amount boundary, the tax-inclusive TCS
> base, `MidpointRounding.AwayFromZero` on two distinct midpoints, the signed `RoundOff`, and
> the absence of any intermediate rounding. `CalculationService` needs no fixture — it is a
> pure unit (`CalculationService.cs:10-12`).
>
> **Both** services R-05 names — `ICalculationService` and `IStockManagerService` — are now
> covered. That is G0 exit criterion 6 met **locally**. The remaining ~283 business services
> are uncovered, and **CI has still never run green on a hosted runner**, so **R-05 stays
> open.**

> **Coverage added 2026-08-19 (M0-13) — the first real business-behaviour coverage.**
>
> `tests/V.SMART.Shared.Tests/Services/StockManagerServiceCharacterisationTests.cs` adds **25**
> characterisation tests over `StockManagerService`, taking the suite from 11 to **36 tests,
> 36 passing** (`dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj`, run
> twice, stable). Unlike the M0-12-01 smoke tests, these assert behaviour: the FIFO allocation
> order, `RcSubID` and `StoreId` discrimination, track reversal on re-issue,
> `AddOrUpdateStockAsync`'s consumed-quantity arithmetic, both delete guards, all five
> user-facing exception message strings, and the R-07 drift.
>
> This closes the *first* of the two services R-05 names. `IStockManagerService` is now covered;
> the rest of the ~285 business services are not, and CI has still never run green on a hosted
> runner. **R-05 stays open.**

> **Status 2026-08-19 (M0-12-01) — the harness exists. The safety net does not.**
>
> - `tests/V.SMART.Shared.Tests/` is the repository's first test project, registered in the
>   `.sln`, running **11 tests, 11 passing** via
>   `dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj`, and wired into
>   `.github/workflows/ci.yml` after the analyzer gates.
> - **Those 11 tests assert almost nothing about business behaviour.** They are smoke and
>   harness tests: discovery, assembly loadability, one `CalculationService.UpdateTotalsAsync`
>   call asserting only that `GrandTotal` moved off its default, two test-double construction
>   tests, and six EF-fixture tests pinning the INV-031 findings. Read "11 passing" as "the
>   loop works", never as "the logic is covered".
> - **The CI step has still never run on a hosted runner** — an execution session cannot push
>   — so the M0-07 caveats below are unchanged: no green run, no required status check.
>
> This risk closes when M0-12-02/M0-13 land real characterisation tests **and** CI is green on
> `master` as a required check. M0-12-01 moved the blocker, it did not close the risk.

> **Status 2026-08-17 (M0-07) — the CI half is addressed, the tests half is untouched, and
> neither is fully closed.**
>
> - **CI: built, not yet live.** `.github/workflows/ci.yml` now runs hygiene guard → restore →
>   build → analyzer warning gate on every push and every PR to `master`, gated against a
>   committed, ratcheting baseline in `ci/warning-baseline.json` (Api 6,693 / Web 6,695
>   warnings, 0 errors) with no `-warnaserror`. The gate was proven to fail on a deliberately
>   introduced warning and to tolerate a decrease. **But it has never executed on a hosted
>   runner** — an execution session cannot push — so there is no green run, `master` does not
>   yet carry the workflow, the baseline is still marked `provisional`, and no required status
>   check exists. Full and honest detail: [KB-087](../execution/ci-pipeline.md) §7 (what was
>   verified) and §8 (what was not).
> - **Tests: unchanged.** There is still no test project (INV-023). M0-07 deliberately added
>   **no** `dotnet test` step, only a commented placeholder naming **M0-12-01**. The "every
>   refactor is unverifiable" impact below therefore still stands in full.
>
> This risk closes when M0-12-01/M0-13 land characterisation tests **and** CI is green on
> `master` as a required check — not before.
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
> - ~~`ApplicationDbContext.OnModelCreating` calls the **relational-only `ToView(null)` 65
>   times**, so the EF Core **InMemory** provider probably cannot build the model at all;
>   **SQLite in-memory** probably can.~~ **Both halves of that inference were falsified by
>   M0-12-01 on 2026-08-19 (Confirmed, executed).** InMemory builds the model and applies the
>   `HasData` seeds; **SQLite fails** with
>   `Microsoft.Data.Sqlite.SqliteException: SQLite Error 1: 'near "MAX": syntax error'`,
>   caused by nine `[Column(TypeName = "nvarchar(max)")]` attributes on `Attendance` and five
>   Inspection entities — see INV-031 in [KB-003](../investigation-registry.md). The fixture
>   ships on InMemory; **M0-13, M0-09 and M0-06 are not blocked.** New debt this creates:
>   InMemory cannot catch a LINQ-translation regression and does not enforce foreign keys, so
>   nothing in this repository yet tests SQL semantics.

---

## High

### R-06 — ~184,000 LOC of logic inside Razor `@code` blocks
**Confirmed.** 57% of 321,661 Razor LOC. Traced in `MfgPOUpsert.razor`: validation,
quantity balancing, cancellation, short-close, and cascade rules all live in the page.
**Impact.** Deleting the Blazor UI without extraction destroys real ERP behaviour. Drives
every "Very High" complexity rating in the feature map.
**Action.** Per-module triage into three buckets — presentation (discard), data loading
(becomes API calls), business logic (extract to service). Extraction happens **before** the
corresponding Angular screen is built, and is validated against the still-running Blazor app.

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

> **Status 2026-08-19 (M0-13) — PINNED, NOT FIXED. This risk stays OPEN.**
>
> Nothing in `StockManagerService.cs` changed. The absence of a post-loop `remainingQty > 0`
> check between the loop's close at `:231` and `SaveAsync()` at `:233` was re-verified against
> the working tree on 2026-08-19 and is still there.
>
> What changed is that the behaviour is now **asserted green** by named characterisation tests
> in `tests/V.SMART.Shared.Tests/Services/StockManagerServiceCharacterisationTests.cs`, so any
> future change to it turns a test red instead of passing unnoticed:
>
> - `S13_R07_IssueOrUpdateStock_WhenNoBatchHasBalance_ThrowsNoAvailableStockToIssue`
> - `S14_R07_IssueOrUpdateStock_WhenBatchesExistButTotalBalanceIsShort_SilentlyUnderAllocatesAndDoesNotThrow`
>   — asserts the drift numerically: `IssueQty 100 − Σ UsedQty 30 == 70m`
> - `S15_R07_IssueOrUpdateStock_WhenReIssueIncreasesQuantityBeyondAvailableStock_SilentlyUnderAllocatesOnTheUpdatePathToo`
> - `S16_R07_IssuingOneHundred_ThrowsAgainstZeroStock_ButSilentlyDriftsByNinetyNineAgainstOneUnit`
>
> The decision to keep or tighten the behaviour remains **M0-11 / Q-01**, and it is now taken
> against a measured baseline rather than against unpinned code. **Do not close R-07** until
> that decision is taken and applied as its own reviewable change.
>
> **Additional behaviour surfaced while pinning (Confirmed).** On the create path the
> `StockIssue` row is committed at `:154-155` *before* tracking runs, so even the *refusal*
> case leaves an orphan `StockIssue` for the full quantity with zero `StockIssueTrack` rows.
> M0-11's brief should cover this as well as the drift.

### R-08 — Copy-paste defects in delete guards — **RESOLVED (first action item only)**
**Confirmed (the defect, until 2026-08-19).** `MfgPoService.cs:504` tested `hasInvoice`
where it computed `hasExpInvoice`; `:525` tested `hasRc` where it computed `hasCR`. Two
guards were unreachable, so a Sales Order with only an export invoice, or only a contract
review, could be deleted.
**Impact.** Referential-integrity violation → orphaned downstream documents.

**Resolved 2026-08-19** by task **M0-09**, branch `migration/M0-09-delete-guard-fix`,
commit *"M0-09: Fix two unreachable delete guards in CanDeleteSalesOrderAsync (R-08)"* —
two identifier changes and nothing else. Pinned by
`tests/V.SMART.Shared.Tests/Services/MfgPoServiceDeleteGuardTests.cs`
(`CanDeleteSalesOrder_WithOnlyExportInvoice_IsRefused`,
`CanDeleteSalesOrder_WithOnlyContractReview_IsRefused`, plus the regression pair for the
Tax Invoice and Route Card guards). Both new tests were observed to fail before the fix,
returning `(True, "Sales Order can be safely deleted.")`, which is the proof the guards
were unreachable.

**Behaviour change — operations must be told.** A Sales Order whose only downstream
document is an **export invoice**, or only a **contract review**, used to be reported
deletable and is now refused with the existing message
("Cannot delete this Sales Order as a Export - Invoice transaction exists." /
"Cannot delete this Sales Order as a Contract Review transaction exists."). Nobody's data
changes: `CanDeleteSalesOrderAsync` is a read-only eligibility check, so this only stops a
deletion that would have orphaned documents.

**Second action item — CLOSED 2026-08-21 by task M0-10 (INV-025). Output:
[KB-061](delete-guard-audit.md).** The audit read every guard in the family and found
**exactly one surviving instance of this defect class — the one already recorded below**
(`MfgPoService.cs:613-615`, in `CanSalesOrderItemCancelCheckAsync`). **R-08 as a *class* is
eradicated**; that single known instance is now carried separately as **R-60**.

The audit's substantive output is four *different* defect classes, none of which R-08
anticipated — carried as **R-61** (14 guards nobody calls, Medium), **R-62** (3 guards that
can never refuse and a fourth inert after one check; plus 3 that throw on a missing row),
**R-63** (29 service files with an unguarded delete, plus an upstream-only integrity
model) and **R-64** (77 of 79 guards run outside the delete transaction). Each matters more
to the API than R-08 did. [KB-061](delete-guard-audit.md) carries the full 79-row inventory
**including every guard judged `Correct`** — do not re-read the methods.

**Scope correction, superseding the two notes below.** The verified population is **79**
implementations of `public async Task<(bool CanDelete, string Message)>` across **61** files
(2026-08-21, Confirmed) — not "~40", not the "63" recorded below, and not the "64" in the
M0-10 task file. Those figures count only guards *named* `CanDelete*`; **15 more return the
identical tuple under a different name** (`CanRemove…`, `ToCheckStockQtyIssued`,
`NeedTocheckRejection`, `ValidateBeforeDeleteBySlNoAsync`, …). **Scope guard work by return
shape, not by name** — see [KB-061](delete-guard-audit.md) §1.2.

> **Related gap noticed while fixing, not acted on (Confirmed, 2026-08-19).** The guard is
> **advisory**: `MfgPoService.DeletePOByPOIdAsync` (`MfgPoService.cs:790-801`) never calls
> `CanDeleteSalesOrderAsync`; the only enforcement is the caller,
> `Pages/SalesAndLabour_pages/SalesPo_Pages/MfgPOList.razor:1079-1090` (`HandleDelete`),
> while `ConfirmDelete_Click` (`:1108`) deletes at `:1118` without re-checking (corrected from
> an earlier `:1119` citation by the M0-09 validator, 2026-08-19). So M0-09
> hardens *what the check reports*, not the delete path itself. A future
> `DELETE /api/v1/sales-orders/{id}` must call the guard server-side or repeat this gap.
> Scoped to **M0-10** as a lead; deliberately out of M0-09's two-line scope.
>
> **Followed up 2026-08-21 (M0-10): this is not a `MfgPOList` quirk — it is universal.**
> **77 of 79** guards run outside the delete transaction; only two do not
> (`EmployeeService.cs:174`, `ProductionLogService.cs:288`). The check-then-act gap with a
> user round-trip inside it is the house pattern. Promoted to **R-64**; evidence in
> [KB-061](delete-guard-audit.md) §5.2.

> **Scope correction, 2026-08-12 (Confirmed) — SUPERSEDED 2026-08-21 by M0-10. The verified
> count is 79 across 61 files, by return shape; see the scope correction above and
> [KB-061](delete-guard-audit.md) §1.1. The "63" below is retained only because the three
> methods it names are still the right worked example.** "~40 methods" understated the audit
> by more than half. A scoped grep over `V.SMART/V.SMART.Shared/BusinessLayer/` returns **63**
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
>
> **M0-10's answer, 2026-08-21: the `CanDelete` prefix is not enough either.** Searching by
> prefix still misses **15** guards returning the identical tuple under names like
> `CanRemoveQuoteAsync` and `ToCheckStockQtyIssued`. Only the **return shape** finds all 79.

> **Second unreported same-pattern instance, found by the M0-09 validator, 2026-08-19
> (Confirmed).** `MfgPoService.cs:613-615`, inside `CanSalesOrderItemCancelCheckAsync`
> (a line-level cancel guard, **not** a `CanDelete…` method): `hasCR` is computed at `:613`
> from `ContractReviews.GetQueryable().AnyAsync(...)`, but the guard at `:614-615` tests
> `hasRc` — the Route Card boolean computed earlier at `:608` — so the Contract Review branch
> is unreachable, identically to BR-SO-002 before this task's fix. Not touched by M0-09
> (outside its authorised two-line surface); not fixed. **This means M0-10's brief, scoped as
> "audit `CanDelete…Async`", would miss this by name** — the method is
> `CanSalesOrderItemCancelCheckAsync`. M0-10 should widen its search to any guard method that
> computes one boolean and tests another, not just the `CanDelete…`/`CanDelete` family. See
> also `INV-025`'s scope note in `docs/kb/investigation-registry.md`.
>
> **Re-verified present, unchanged, 2026-08-21 by M0-10** — `hasCR` at `:613`, `if (hasRc)` at
> `:614`, return at `:615`. It is now carried as **R-60** and proposed for repair as task
> `M0-10a`.

### R-60 — The one surviving R-08 instance: `CanSalesOrderItemCancelCheckAsync`
**Confirmed — by reading *and* by an executed test.** `MfgPoService.cs:613` computes
`hasCR` from `ContractReviews`; `:614` tests `hasRc`, the Route Card boolean from `:608`. By
`:614` `hasRc` is necessarily `false`, so the Contract Review refusal at `:615` is
unreachable.
**Impact.** A Sales Order **line** whose only downstream document is a Contract Review can be
cancelled.
**Why it has its own row.** R-08 is closed as a *class* — this is the **only** surviving
instance across the whole 93-method guard family ([KB-061](delete-guard-audit.md) §1.3, §3.1),
and it needs a row that does not read as "an audit is still outstanding".
**Empirically proven 2026-08-21 (M0-10).**
`tests/V.SMART.Shared.Tests/Services/MfgPoServiceDeleteGuardTests.cs` →
`CanSalesOrderItemCancel_WithOnlyContractReview_IsRefused`, run against **unmodified**
`MfgPoService.cs`, observed `Actual: Tuple (True, "Item can be safely Cancell.")` where the
guard's own `Message` promises a refusal. Full output:
[KB-061](delete-guard-audit.md) §3.1 and §7. The test is committed **`Skip`-ped** — M0-10 is
an audit and may not repair the defect — and is the acceptance test for `M0-10a`.
**Action.** One identifier — `hasRc` → `hasCR` — plus removal of that test's `Skip`, which
must then go green. Proposed as `M0-10a`, 0.5 d.

> **R-61 is filed under `## Medium`, not here.** It was briefly filed in this `High` section
> while [KB-061](delete-guard-audit.md) §3.2 rated it *Medium*; the contradiction was
> corrected 2026-08-21 (M0-10, attempt 2) in favour of **Medium**, because no delete is
> wrongly permitted today — the risk is entirely prospective. R-61 opens the Medium section,
> immediately before R-15.

### R-62 — Three guards can never refuse, a fourth is inert after one check; three are wired to live delete buttons
**Confirmed.** `StoreInterTransService.cs:211` (body is a single `return (true, …)`),
`GroupingService.cs:136` and `EstimateService.cs:735` contain **no** `(false, …)` return
outside their `catch`. `AppointmentLetterService.cs:43` is the fourth member of the group but
**does** refuse, on a `Staff`-name match at `:58-61`; everything after that is inert. Detected
as *"no refusal path outside `catch`"* (which yields the first three) plus the liveness
detector (which caught the fourth as a dead computation); evidence
[KB-061](delete-guard-audit.md) §3.3.
**Impact.** The Grouping and Estimation list pages show the user an eligibility check that
**always says yes**; the Appointment Letter page shows one that says yes to everything except
a duplicate `Staff` name. `GroupingList.razor:549`, `EstimationList.razor:411`,
`AppointmentLetterList.razor:485`.
**Second defect class, same row: dereference before null check — three members, not one
(Confirmed 2026-08-21, M0-10 attempt 2).** Each loads a header row and dereferences it with no
null test anywhere in the body, so a row that does not exist throws
`NullReferenceException`, is caught, and is rethrown — surfacing as HTTP **500** from a future
delete endpoint rather than as a decision:

| Guard | Loads | Dereferences unchecked at |
|---|---|---|
| `AppointmentLetterService.cs:43 CanDeleteAppointmentletterAsync` | `po` at `:47-50` | `:52` (`po.CandidateID`) — which also makes the null check at `:62-63` unreachable |
| `OutSourcingService/Purchase_Invoice_Service/PurchaseInvoiceService.cs:1283 CanDeletePurchaseInvAsync` | `invoice` at `:1287-1290` | `:1292`, `:1295` |
| `PlanningService/RcReleaseService.cs:803 CanDeleteRcReleaseAsync` | `rcRelease` at `:807` | `:818` |

The other two header-loading guards in the same "no `== null` test" bucket —
`LabourSCNService.cs:229` and `PurchaseSCNService.cs:1690` — are **null-safe**; they use
positive-form `!= null` tests, which is why the scanner missed them. Census and scope limit:
[KB-061](delete-guard-audit.md) §5.3a.

**Also product-visible text for the wrong document.** `AppointmentLetterService.cs:63` and
`:71` return *"Sales Order can be safely deleted."* for an Appointment Letter (BR-SO-001
migration note: `Message` strings are product UX).
**Whether the permissiveness is correct cannot be determined from the code** — see **Q-64**.
**Action.** Rule discovery before code. Folded into `M0-10d`; the null-safety half into
`M0-10c`.

### R-63 — 29 service files expose a delete with no guard of any shape, and the integrity model only guards upstream
**Confirmed.** 163 public `Delete*` methods across 89 files; 61 files carry a tuple guard;
**29 files have a delete and no guard of any shape.** Five verified as live, reachable UI
delete paths — `MfgDcService.cs:322`, `MfgInvService.cs:936`, `PaymentsService.cs:1310`,
`ReceiptsService.cs:1341`, `ProductionIssueAssyService.cs:1049`. Full list:
[KB-061](delete-guard-audit.md) §5.1.
**The asymmetry is the sharper half.** `CanDeleteSalesOrderAsync` refuses when a Sales DC, Tax
Invoice or Export Invoice exists (BR-SO-001) — but **the DC, the Tax Invoice and the Export
Invoice can each be deleted with no check at all.** Integrity is enforced upstream only.
Deliberate unwind order, or omission? Undeterminable from code — **Q-62**.
**Also here: three live delete buttons whose guard is commented out** —
`PaymentList.razor:451`, `ReceiptList.razor:452`, `AdvanceAdjustmentList.razor:433`. The
commented call is `EnquiryPurchaseService.CanDeleteEnquiryAsync`, a copy-paste from the
Enquiry page that never applied to Payments, so **restoring it verbatim would guard the wrong
document** — **Q-63**, then `M0-10e`.
**Impact.** Missing guards are a larger referential-integrity hole than broken ones, and an
API surfaces every one of them.
**Action.** `M0-10d` (specify, 3–5 d, P1) and `M0-10e` (0.5 d, P2).

### R-64 — Guards are advisory: 77 of 79 run outside the delete transaction
**Confirmed.** Only `EmployeeService.cs:162 DeleteEmployeeAsync` (transaction `:164`, guard
`:174`) and `ProductionLogService.cs:275 DeleteProductionLogByLogId` (`:277`, guard `:288`)
call a guard inside the delete transaction. The other 77 are called from Razor `@code` — 67
pages, 68 call sites — plus one API controller, `CurrencyController.cs:101`.
**The house pattern is check-then-act with a user round-trip in the gap.** `HandleDelete(id)`
runs the guard, then shows a JavaScript confirmation modal; `ConfirmDelete_Click` deletes
**without re-checking** — `CurrencyList.razor:597/630`, `ItemList.razor:855/919`,
`MfgPOList.razor:1083/1118`. This supersedes R-08's note, which recorded it for `MfgPOList`
alone.
**Impact.** The window is not scheduler jitter; it is however long the user takes to read a
modal. An API makes it concurrently reachable by any HTTP client.
**Action.** A single binding decision for every future delete endpoint — **Q-60**. Also note
**36 of 64** guards *rethrow* on internal error, so they surface as `500`, not as a refusal
([KB-061](delete-guard-audit.md) §4); a delete endpoint cannot treat "guard threw" as "guard
refused".

### R-09 — Default administrator account with a committed password hash
**Confirmed.** `ApplicationDbContext.cs:1136` seeds `UserName = "Administrator"` with a
fixed PBKDF2 hash, `Role = Administrator`, `IsActive = true`, in **every** tenant database.
**Impact.** A known default credential across all tenants. The plaintext is recoverable
offline from the committed hash.
**Action.** Force a password change on first login; or seed with a random per-deployment
password; or disable the account after real users exist.

### R-10 — ~~`ScreenCode` magic numbers with no typed definition~~ → **misidentified; the real magic number is `storeId`**
> ⛔ **CORRECTED 2026-08-21 (INV-044). The central claim below — *"which callers pass as
> literals"* — is FALSE, and was marked `Confirmed` without being checked against a call
> site.** No `screenCode` literal exists anywhere in the codebase. The code resolves the
> screen code **at runtime, from the database, by screen name**:
>
> ```csharp
> // e.g. SCNAddUpsert.razor:791
> screenCode = await _scnGenService.GetScreenCodeByScreenNameAsync(ScreenName);
> ```
>
> **Evidence (all re-derived this session, negative results included):**
>
> | Check | Result |
> |---|---|
> | `GetScreenCodeByScreenNameAsync` call sites | **166** (61 Razor pages) |
> | Assignments matching `screenCode = <integer>` | **1**, and it is commented out (`SalaryDetails.razor:252`) |
> | Stock-call expressions captured and inspected | **244** |
> | …passing an integer literal in the `screenCode` position | **0** |
> | `GetQtyBalQtyByStockAddAsync` calls passing a literal `screenCode` | **0** — every one passes the variable |
>
> **The `152` figure in the paragraph below is also wrong** — 152 rows are *seeded*, but later
> migrations delete two, and every real database holds **150**. See **R-65**.
>
> **What this does to the task built on it.** [M2-B05](../execution/tasks/M2-B05.md) exists to
> *"replace the magic integer literals currently passed as `screenCode`"*. **There are none to
> replace.** Its literal-replacement deliverable, and the "prove no value changed" verification
> that is called *"the single most important verification step in the task"*, both have no
> subject. Generating the 152-constant class alone would produce a file with **no call site to
> use it**. The task is `Blocked` pending re-specification by the owner — see tracker
> footnote ³¹.
>
> **What is still true and still worth doing:** the *secondary* value the task names. ADR-004's
> `[RequireScreen("…")]` takes a hand-typed screen-name string, and
> `V.SMART.Api/Authorization/ScreenCatalogue.cs` already hard-codes that vocabulary — wrongly,
> with two names that exist in no database (**R-65**). A generated, database-derived constants
> class would serve that need *and* fix R-65. That is a different task from the one written,
> and it belongs with **M2-A02**, not here.

**Original text, retained because the *class* of risk is real — it is the parameter that was
wrong, not the concern:** `StockManagerService.AddOrUpdateStockAsync/IssueOrUpdateStockAsync`
take `int screenCode`. `StockAdd.ScreenCode`/`StockIssue.ScreenCode` are the stock-movement
source discriminator. The only definition is the seeded `Screens` rows. No enum or constants
class exists. **Impact:** a wrong literal silently misattributes stock movements and corrupts
stock position reports, invisible to the compiler. **That impact is real — see R-66, where the
literals actually are.**

---

### R-66 — Hardcoded `storeId` literals `6` and `7` in stock movements
**Confirmed by measurement, 2026-08-21 (INV-044).** *(Id `R-66` follows **R-65**; **R-64** is
held by the unmerged `migration/M0-10-candelete-guard-audit` branch — checked with
`git branch --no-merged master`.)*

This is the risk **R-10 was reaching for and misfiled**. `AddOrUpdateStockAsync`'s second
parameter is `storeId`, and **55 call sites pass a bare `6` or `7`**:

```csharp
// ToolCribReturnService.cs:158 — a rejection movement
await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId.Value, 6, sub.RejQty,
    1, null, screenCode, sub.TCReturnSubId, entity.TCReturnNo, entity.TCReturnDate, sub.RejRemark);

// LabourSCNService.cs:733 — a rework movement
await _stockManagerService.AddOrUpdateStockAsync(subItem.ItemId ?? 0, 7, (subItem.ReworkQty.Value), …);
```

**What they mean, confirmed against two databases:**

| `StoreId` | `StoreName` | Rebuilt from source | Live dev DB |
|---|---|---|---|
| 6 | `REJECTION STORE` | present | present |
| 7 | `REWORK STORE` | present | present |

All 9 `Stores` rows are migration-seeded and **byte-identical between the rebuilt and the live
database**, so today these literals are *correct*. The risk is not that they are wrong now.

**Impact.** `storeId` sits at position **2** in a twelve-parameter list of mostly `int`s,
adjacent to `itemId` — transposing them compiles cleanly and silently books stock to the wrong
store. Unlike `screenCode`, which the code looks up by name and therefore cannot get wrong,
`6` and `7` are unnamed and unchecked. They also encode a **business assumption** — that
rejection and rework are distinct stores with those ids — in 55 places rather than one.

**Also worth noting:** a `storeId` is *tenant data*, not a compile-time catalogue. It is
seeded identically today, but nothing enforces that a tenant cannot renumber or add stores, and
no constraint ties literal `6` to `REJECTION STORE`. That is what makes this worse than R-10
as originally written, not better.

**Action.** Name them — a `StoreIds` constants class, or better, resolve by name the way
`screenCode` already is, which would make the two paths consistent. **Do this before the API
exposes stock operations**, for R-10's original reason: an API multiplies callers and removes
the UI's implicit context. Owner **Vivek**; needs a task, and is the obvious candidate for
M2-B05's re-specification.

### R-11 — `IApprovalService` depends on a Razor page type — **CLOSED 2026-08-21 (M2-B04)**
**Was Confirmed.** `IApprovalService.cs` declared
`using static V.SMART.Shared.Pages.Planning_Module_Pages.Authorization_Pages.Authorization;`.

**Count corrected (Confirmed, M2-B04).** This entry said "13 other" files; the true figure was
**16 `using` directives across 15 distinct non-UI files** — `Data/AccountsModule/FundTrans.cs`
carried two (`:11` and `:12`). KB-041's "14 reference `Pages`" was wrong for the same reason.
The full pre-fix inventory, with line numbers, is the table in
[`execution/tasks/M2-B04.md` § *Existing Behavior to Preserve*](../execution/tasks/M2-B04.md).

**The proposed action was wrong, and the real fix was much smaller.** This entry said "move
the shared types into `ViewModels/`". No type had to move. **15 of the 16 directives imported
nothing and were simply deleted**; the compiler confirmed it (`dotnet build
V.SMART/V.SMART.Api/V.SMART.Api.csproj --no-incremental` → 0 errors). Nine of the ten
referenced page namespaces declare no public type at all, and the tenth (`GridMode` in
`Report_Module_Pages.TrackReports_Pages`) is declared inside `@code`, so nothing referenced it
from outside either.

**The `using static` on `Authorization` imported nothing.**
`V.SMART/V.SMART.Shared/Pages/Planning_Module_Pages/Authorization_Pages/Authorization.razor`
contains **zero** occurrences of the keyword `static` (Confirmed — `grep -c "static"` returns
`0`), so a `using static` on it imports an empty set. The `UserAuthority` parameter in
`IApprovalService.ApproveAsync` comes from `V.SMART.Shared.Data.Master.Admin_Module`, imported
at `IApprovalService.cs:1`.

**Exactly one directive was load-bearing** —
`V.SMART/V.SMART.Shared/ViewModels/AccountsViewModel/FundTransFilterVM.cs`. Its `Bank`
property was typed against the **Razor component** `…Bank_Pages.Bank`, not the EF entity
`Banks` (`V.SMART/V.SMART.Shared/Data/Master/Accounts_Module/Banks.cs:6`). Deleting the
directive produced `CS0246` at that line. Fixed by retyping the property to `Banks?`, not by
moving any type. The property is read-but-never-assigned outside the ViewModel, so the retype
is behaviour-neutral — but whether it was ever *meant* to carry the entity is **Q-55**.

**Guard installed.** `tests/V.SMART.Shared.Tests/Architecture/NoPagesReferenceFromDomainTests.cs`
holds two complementary tests: a reflection scan of the compiled assembly (catches a real
type-level dependency; cannot see an unused `using`) and a source-text scan of
`V.SMART/V.SMART.Shared/**/*.cs` excluding `Pages/` (catches the unused-`using` case). Both
were **demonstrated to fail** on a seeded violation on 2026-08-21 and pass after reverting it.
The Shared suite runs in CI (`.github/workflows/ci.yml`), so reintroduction now fails the
build. No `scripts/check-no-pages-references.sh` was created — the test project already
existed, so the task's shell-script fallback did not apply.

**Still open, deliberately:** `V.SMART.Shared` remains **one assembly** containing both the
domain and 333 Razor pages. M2-B04 removed the compile-time *references*, not the physical
coupling; splitting `Pages/` into its own project is a separate, much larger change and is not
scheduled. Nothing prevents a future `using` other than the guard above.

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

**M0-08 closure (2026-08-17) — build-output half of R-14: severity re-rated High → Low
(preventive, enforced).** Re-audited after M0-00 and the intervening M0-01/M0-03/M0-15 work,
which is the event that could have made the original claim true (`git add frontend/` on a
tree containing `node_modules/`, `dist/` and `.angular/`):

| Check | Observed 2026-08-17 |
|---|---|
| `git ls-files` | **2,451** tracked paths (was 2,162 at the 2026-08-12 audit) |
| `git ls-files \| grep -E -i "(^\|/)(bin\|obj\|dist\|node_modules\|\.angular\|\.vs\|out-tsc\|bazel-out\|packages)/\|\.(user\|suo\|userosscache\|rsuser)$\|\.db-lock$\|(^\|/)TestResults/\|\.vsidx$"` | **no output** (exit 1). Still zero — committing `frontend/` did not drag build output in |
| All 2,451 tracked paths piped through `git check-ignore -v --stdin` | **no output** — no new rule shadows an already-tracked file |

The durable-protection gap is now closed. With `frontend/vsmart-erp/.gitignore` temporarily
moved aside — the state M2-C11 creates permanently — every previously nested-only path still
resolves against the **root** `.gitignore`: `frontend/vsmart-erp/dist` → `.gitignore:381`
(`**/dist/`), `frontend/vsmart-erp/.angular/cache` → `.gitignore:382` (`**/.angular/`),
`frontend/vsmart-erp/node_modules` → `.gitignore:286` (`node_modules/`), `.vs` →
`.gitignore:37`, `*.csproj.user` → `.gitignore:9`, `bin`/`obj` → `.gitignore:30,31`.
The nested file was restored byte-identical and is **not** modified by M0-08.

**Enforcement (the residual action, now discharged).** `tools/check-no-build-output.sh` runs
the identical audit pattern, needs only `git` and a POSIX shell, takes no arguments, resolves
the repository root itself, and exits `1` listing every offending path. Proven in both
directions this session: exit `0` on the current tree, and exit `1` naming the path after a
throwaway `V.SMART/dist/m008-guard-proof.txt` was force-added (then fully reverted; exit `0`
again). **The exact CI step M0-07 must add:**

```yaml
- name: No build output tracked
  run: bash tools/check-no-build-output.sh
```

A `.ps1` sibling was **not** created: it is conditional on a Windows-hosted CI runner, and
[KB-086](../execution/M0-15-build-baseline.md) does not settle the runner OS. If M0-07 picks
`windows-latest`, note that `bash` is available on GitHub's Windows images, so the `.sh` guard
still runs; a `.ps1` is only needed for a runner without any shell. Recorded as a decision for
M0-07, not invented here.

No React ignore rules were added — no React app exists yet, so its output paths are unknown.
**M2-C01 must add its own build-output rules to the root `.gitignore`** when it scaffolds the
React app; `**/dist/` already covers the common case.

The **restated** half of R-14 (large parts of the tree untracked) is *not* closed by M0-08 and
keeps its High rating for the parts still outstanding; `V.SMART/V.SMART.Api/` and `docs/` are
now tracked (commits `2c224b6`, M0-00), the `.sln` disposition remains M0-00's record.

---

### R-67 — `SaveCorresFileAsync` writes a zero-byte file and reports success
**Confirmed** (M2-B06, 2026-08-21). `V.SMART/V.SMART.Web/Services/WebFileUploadService.cs:100-104`:

```csharp
100   await using var fileStream = File.Create(fullPath);
101   await using var inputStream = file.OpenReadStream(20 * 1024 * 1024);
102  // await inputStream.CopyToAsync(fileStream);        <-- COMMENTED OUT
104   return "/" + relativePath.Replace("\\", "/");
```

The destination file is created, **the copy that would fill it is commented out**, and the method
returns the path as though the upload succeeded. Every correspondence and drawing uploaded through
the Blazor UI therefore lands on disk as **0 bytes**, with no error shown to the user.

**Impact.** Silent data loss on the Blazor upload path, for correspondence and drawing documents.
This is not a migration artefact — it is live in the running application today.

**Why it is survivable in practice, and why that is not a reason to leave it.** The bytes are
stored a second time, in the `Correspondence.Image` column (`Correspondence.cs:14`, written by
`CorrespondenceUpload.razor:306-309`), and the two live download screens disagree about which copy
to serve: `CorrespondenceListByReference.razor:357-363` abandoned the path and serves the column,
while `CorrespondanceList.razor:319-321` still opens the empty file. The defect is therefore
invisible on one screen and total on the other. Anything reading the on-disk tree directly — a
backup, a report, a future service — sees only empty files.

**Not fixed here, deliberately.** M2-B06 constraints forbid editing `WebFileUploadService.cs`, for
two stated reasons: a user may have built a workaround around the current behaviour, and mixing a
live-app bug fix into a migration task makes the diff impossible to review or revert independently.

**The API path does not reproduce it.**
`V.SMART/V.SMART.Api/Services/ApiFileUploadService.cs:155` performs the copy, and
`FileEndpointSecurityTests.RoundTrip_stored_bytes_are_identical_and_the_layout_matches_blazor`
asserts byte identity — the assertion `WebFileUploadService` would fail.

**Action.** Owner decision: whether to uncomment line 102 on the Blazor path. Doing so changes the
observable behaviour of the live application — files stop being empty — so it needs its own task
and its own regression check, not a quiet edit. Until then, treat the on-disk correspondence tree
as unreliable and the `Image` column as the source of truth.

---

## Medium

### R-51 — PrimeNG 22 ships client-side licence enforcement, and no key is configured
**Confirmed, 2026-08-21 (M2-C01).** `primeng@22.1.0` contains
`showInvalidLicenseBanner()`, documented in its own type declaration as injecting *"a
fixed-positioned banner into the bottom-right of the page when the PrimeNG license cannot be
verified"*, rendered in a **closed-mode shadow root** with `all:initial` on the host and *"no
semantically obvious id, slowing down trivial hide-by-selector attempts"*
(`frontend/nexgen-web/node_modules/primeng/types/primeng-license.d.ts:1-21`). `providePrimeNG()`
accepts a `license?: string` option (`.../types/primeng-config.d.ts:258`). With no key set, the
scaffolded app logs `[PrimeUI] PrimeUI license is not configured` on every load — observed in the
Playwright run of `e2e/smoke.spec.ts`.

**Why it matters.** [ADR-007](../decisions/ADR-007-angular-stack.md) selects PrimeNG as **the**
single component library for the whole SPA and its version table records no licence cost. The ADR
was accepted on 2026-08-20; this behaviour was found on 2026-08-21, one day later, by the task
that first installed the package. Every `M2-C` and `M3` screen builds on PrimeNG, so the cost of
discovering a mandatory licence later rises with each one.

**What is *not* claimed.** Whether the banner actually renders for this project's usage, what the
key costs, and whether the components themselves are gated were **not** determined — that needs
PrimeNG's commercial terms, which is a procurement question, not a code question. No key was
invented and none was configured.

**Action.** Owner decision, tracked as **Q-66** in [`open-questions.md`](../open-questions.md).
Either buy and configure a key through `providePrimeNG({ license })` — from configuration, never
committed — or re-open ADR-007's UI-library choice while only one placeholder screen exists.

---

### R-61 — Fourteen delete guards have no caller at all
**Confirmed.** Nine guard names have **zero** call sites anywhere in `V.SMART/`, `tests/`,
`frontend/` or `docs/`; three further implementations are unreachable because the call site
injects a different interface. Full list and evidence:
[KB-061](delete-guard-audit.md) §3.2.
**Impact.** No delete is wrongly permitted today — the guards are simply never consulted. The
risk is **future**: an endpoint author greps for a guard, finds one of these, and promotes
never-exercised logic straight to production enforcement.
**Severity note (2026-08-21, M0-10 attempt 2).** Filed **Medium**, matching
[KB-061](delete-guard-audit.md) §3.2. Attempt 1 filed the row physically inside the `High`
section while its own text and KB-061 both said Medium; the placement was the error, not the
rating — nothing is presently mis-permitted, so *"schedule"* is the right disposition.
**Note.** Two of the fourteen are dead clones of a reachable guard, differing only in message
wording (`"Prouction SCN"` vs `"Prouction SCN Assembly"`, spelling as in source) — duplication
rot, not divergent logic.
**Action.** Wire or delete, per guard. Proposed as `M0-10b`, 1 d, P2.

### R-15 — Invalid GST rate silently coerced to zero — **PARTIALLY RESOLVED (M2-B09, 2026-08-21)**
**Confirmed.** `CommonConstants.GetIGST/GetGST` use `FirstOrDefault(r => r == rate)`,
returning `0` for an unlisted rate rather than raising.
**Action.** Return `decimal?` or throw; validate at the API boundary.

> ✅ **The API-boundary half is done. The in-process half is not, and this row stays open.**
>
> **What M2-B09 delivered.** `[GstRate]`
> (`V.SMART/V.SMART.Api/Contracts/GstRateAttribute.cs`) is a `DataAnnotations` attribute that
> any request DTO carrying a GST rate applies. An off-ladder rate is rejected with a **400**
> `application/problem+json` naming the field and listing every permitted value (ADR-002 §4);
> `0.000` is still accepted, because an over-eager fix that rejected zero would break every
> genuinely zero-rated line. It supports both ladders separately — `28.000` is a valid IGST
> rate and *not* a valid CGST/SGST rate, and validating against the wrong list would pass
> silently in production, so the tests pin that too.
>
> **It deliberately does not call `GetIGST`/`GetGST`.** It cannot: their return value *is* the
> ambiguity. Membership is tested against `IGSTRates`/`GSTRates` directly, where "absent" and
> "zero" are distinct answers. A test asserts this explicitly by pinning
> `GetIGST(19m) == GetIGST(0m) == 0m` and then showing the attribute tells the two apart.
>
> **Why the helpers were not changed.** `CommonConstants.cs` is untouched — it is on M2-B09's
> *Files That Must Not Change* list. `GetIGST`/`GetGST` have **105 call sites** across the
> Blazor app (`grep`, 2026-08-21); changing their return type or making them throw alters
> behaviour for every one of them. That is a separate decision with a separate blast radius,
> and it is what remains of this row.
>
> **The *disagreement* below is untouched and is still the harder half.** `CalculationService`
> applies any rate it is given while a sanitising caller turns an unlisted rate into zero tax —
> 170 on one path, 0 on the other. **M2-B09 does not resolve that**; it only ensures a bad rate
> cannot enter through the API. Fixing R-15 fully still means deciding which path is
> authoritative.
>
> Full design and rationale: [KB-124](../api/reference-data-and-caching.md) §3.

> **Pinned by executable tests 2026-08-19 (M0-12-02). Not closed.**
>
> | Test (`tests/V.SMART.Shared.Tests/Services/…`) | What it pins |
> |---|---|
> | `CommonConstantsGstRateTests.GetIGST_WithUnlistedRate_SilentlyReturnsZero_R15` | `GetIGST(17m)`, `GetIGST(-5m)` and `GetIGST(28.0001m)` all return `0m` — `CommonConstants.cs:25` |
> | `CommonConstantsGstRateTests.GetGST_WithUnlistedRate_SilentlyReturnsZero_R15` | the same for `GetGST` (`:26`), including that an *IGST* rate of 18 is unlisted for CGST/SGST and so returns `0m` |
> | `CommonConstantsGstRateTests.GetIGST_CannotDistinguishNotFoundFromTheListedZeroRate_R15` | why the defect is undetectable at the call site: `GetIGST(17m) == GetIGST(0m)`, because `0.000m` is a legitimately listed rate (`:13`, `:20`) |
> | `CalculationServiceCharacterisationTests.S21_R15_AnUnlistedGstRate_IsAppliedWithoutValidationOrCoercionToZero` | the other half of the hazard — `CalculationService` does **not** route rates through these helpers and charges the unlisted 17% in full (170 on a taxable 1000) |
>
> **Sharpened statement (Confirmed, M0-12-02).** R-15 is not a single defect but a
> *disagreement*: `CalculationService.cs:12-114` contains no reference to `CommonConstants`
> and applies any rate it is given, while any caller that sanitises a rate through
> `GetIGST`/`GetGST` first turns an unlisted rate into zero tax. The same mistyped rate
> therefore produces 170 on one path and 0 on the other. Fixing R-15 must decide which path
> is authoritative, not merely change the helper's return type.
>
> Whether any tenant database actually stores an off-list rate is **Unknown** — it would need
> a query against a real tenant database, which no test infrastructure here has. That is what
> separates "latent" from "live".
>
> These tests assert the defective behaviour **deliberately**. Do not "fix" them; if R-15 is
> repaired, update them in the same commit as the production change.

### R-16 — QR token expiry not enforced in the query — **RESOLVED (query half) by M2-A08, 2026-08-20**
**Restated first, because the original wording was wrong.** The risk was never "expiry is
never checked". `QrExpiryDate` **was** checked — post-query, in two duplicated Razor copies
(`QrLogin.razor:50-56` "QR Code Expired", `Login.razor:422-429` "QR Expired"). The real risk
was: **the query returned expired users, and only the two Blazor callers happened to reject
them.** Any third caller — an API controller, a background job, a mobile client — got a valid
`User` for an expired token. **Confirmed.**

**Resolved.** M2-A08 added the predicate to `UserRepository.GetUserByQrToken`, which is the one
place that makes correctness independent of the caller. A null `QrExpiryDate` still returns the
user (it is not an expired one), matching both callers' `HasValue` guard. Both Blazor checks
are left in place — a redundant check is not a bug. Pinned by
`tests/V.SMART.Shared.Tests/Repositories/UserRepositoryQrTokenTests.cs`.
**Still open:** the API has no QR endpoint at all, so nothing exercises the fixed query yet.

### R-38 addendum (M2-A08, 2026-08-20) — the newly recorded defects
Each was found while porting the gates and is **recorded, not fixed**: the Blazor UI runs on
all of them and M2-A08 was not permitted to edit `Pages/**` or `BusinessLayer/**`.

| # | Defect | Evidence |
|---|---|---|
| a | **`GetUserTrialAsync` is dead code** — zero call sites | `IUserRepository.cs:14`; `UserRepository.cs:63-72` (`:85` after M2-A08's comment) |
| b | **The lead row filter runs in memory** — every lead materialised, then discarded | `LeadService.cs:133`, `:141-144` |
| c | **The current user is resolved by username string equality**, over a full `GetAllAsync()` of the user table | `LeadService.cs:132-134` |
| d | **`user != null` is tested after `user.UserId` is dereferenced** — dead, harmless only because `:263-268` already returned | `Login.razor:271` |
| e | **`UpdateUserDeviceAsync` records and never refuses** — no comparison, never writes `IsMobile`/`IsDesktop` | `UserService.cs:713-757`, esp. `:730-733`, `:741-744` |
| f | **…and it calls `IJSRuntime` from inside a business service**, so it **cannot run in an API request** | `UserService.cs:722-725` |
| g | **The Leads paging total is computed from the *unscoped* query** for every user, so a scoped user's page count reflects rows they cannot see | `LeadsList.razor:396-401` |
| h | **Two divergent implementations of one CSV predicate** — in-memory string tokens vs four SQL `EF.Functions.Like` patterns | `LeadService.cs:136-144` vs `:35-47` |

Full context: [KB-108](../architecture/row-scope-and-account-gates.md).

### R-17 — `SessionTimeoutService` is a shared singleton
**Confirmed.** `AddSingleton` with one `_lastActivity` field → all users share one idle
clock.
**Action.** Do not port. Implement idle timeout client-side plus server token expiry.

### R-18 — `CurrentUserService.GetUserRoleAsync()` always returns empty
**Confirmed.** Reads claim type `"role"`; the providers write `ClaimTypes.Role`. Currently
zero call sites, so latent.
**Action.** Fix or delete; do not replicate in the API.
**Encountered and avoided by M2-A07 (2026-08-20).** `GET /api/v1/me` needs the caller's role
and deliberately does **not** call this method: `MeController.ReadRole()` reads
`ClaimTypes.Role` (falling back to the short `"role"` name only if the JWT handler's inbound
map is switched off, which is the opposite of this bug's read-`"role"`-only). A test asserts
`MeController` takes no `CurrentUserService` dependency at all, so the bug cannot creep back in
through this endpoint. **The risk itself remains open and still has zero call sites** — nothing
in M2-A07 fixed `V.SMART.Shared`, which it was forbidden to touch.

### R-19 — Login swallows exceptions
**Confirmed.** `UserRepository.cs:44-48` catches all exceptions and returns `null`.
**Impact.** A database outage is reported as "invalid username or password"; incidents are
misdiagnosed.
**Action.** Let infrastructure failures propagate to a 500; keep 401 for genuine
credential failure.
**Still open after M2-A06 (2026-08-20).** M2-A06 guarantees the *second* half only: a failure
that does escape `V.SMART.Shared` now becomes a `500` with a `traceId`, never a misleading
`4xx`. The swallowing itself is untouched — `UserRepository.cs` is in the shared library and
serves the live Blazor app, and M2-A06 was explicitly forbidden from editing it.

### R-20 — `DetailedErrors = true` unconditionally in Blazor Server — RESOLVED by M0-14
**Confirmed.** Was `AddServerSideBlazor(o => o.DetailedErrors = true)` at
`V.SMART/V.SMART.Web/Program.cs:198` (inside the block starting at line 196), plus
`"DetailedError": true` at `V.SMART/V.SMART.Web/appsettings.json:14`.
**Impact.** Stack traces reach the browser in production.
**Action taken (2026-08-18, M0-14).** `Program.cs:198` now reads
`options.DetailedErrors = builder.Environment.IsDevelopment();`. The JSON key was proven dead
— `git grep -in "DetailedError" -- "V.SMART/"` returned exactly the two hits above and no
binding code anywhere in the solution — so it was deleted from `appsettings.json` rather than
bound. No `appsettings.Development.json` override was needed: `IsDevelopment()` yields `true`
there already, matching current Development behaviour.
**Residual risk (Q-16, open):** this fix is only effective if `ASPNETCORE_ENVIRONMENT` is
genuinely not `Development` in production. That is a deployment fact this repository cannot
verify; see [KB-004](../open-questions.md) Q-16.

### R-21 — Incomplete `IQSMART` → `V.SMART` rename
**Confirmed.** `V.SMART.Web/Program.cs` imports five `IQSMART.Shared.*` namespaces
alongside `V.SMART.Shared.*`, and one `V.SMARTV.Shared.…` (a typo namespace).
**Impact.** Confusing navigation; suggests other half-renamed identifiers.
**Action.** Complete the rename in one mechanical pass.

### R-22 — Two overlapping design systems in the UI
**Confirmed.** MudBlazor 8 and Bootstrap 5 CSS both loaded and both used.
**Impact.** Visual inconsistency; part of why the product looks dated.
**Action.** Resolved by construction in the SPA rewrite — one library only.

### R-23 — Flat-file logging with no aggregation, and an unsanitised path — **RESOLVED FOR `V.SMART.Api` 2026-08-21 (M2-B11); STILL OPEN FOR THE BLAZOR AND MAUI HOSTS**
**Was Confirmed.** `FileLoggingService` writes flat text files under `App_Data/Logs/`.
**Impact.** No searchability, no alerting, unbounded growth, lost on container restart.
**Action.** Structured logging (Serilog) to a real sink (M2-B11); preserve the *user-action
audit trail* as a first-class feature.

**Closed by M2-B11 for `V.SMART.Api`.** Serilog writes compact JSON to two rolling files split
on an `EventType` discriminator: `audit-{date}.json` (`EventType = "UserAction"`, retention
**3650**) and `diagnostics-{date}.json` (retention **14**) — both are Serilog
`retainedFileCountLimit` values, i.e. a **retained file count** that equals days only while a
day fits in one file; see KB-113 § *Separability and retention*. A 64 MB size cap applies, and
`ReadFrom.Configuration` lets a deployment add a second sink without a rebuild. The six audit
fields are **named properties** — `UserName`, `Machine`, `IpAddress`, `Screen`, `Action`,
`AdditionalInfo` — plus `EventType` and the M2-A06 correlation id, so the audit trail is
queryable by user, by screen and by date range. `LogDeveloperError` now logs the `Exception`
object. `ILoggingService`'s three signatures are unchanged and `FileLoggingService` is kept.
Contract: **[KB-113](../architecture/observability.md)**.

> **⚠ It is resolved for one host of three, and that is deliberate — do not "finish the job"
> by moving the registration.** `AddVSmartDomain()` still registers
> `ILoggingService → FileLoggingService`; `V.SMART.Api/Program.cs` overrides it afterwards.
> Neither the Blazor host nor the MAUI head has a Serilog sink configured, so changing the
> shared registration would route a **live** audit trail — 494 call sites across 202 files
> (INV-046) — into a console and a debug window, i.e. **delete** it. Giving `V.SMART.Web` a
> durable structured sink is a separate task. See [KB-113 §6](../architecture/observability.md).

**Two of the four impacts are only partly closed, stated plainly:**
- *Unbounded growth* — closed for the API (rotation, retention, size cap). Open for the other
  two hosts, which still call `File.AppendAllTextAsync` with no cap.
- *Lost on container restart* — **not closed by code, and cannot be.** The new files default to
  `{ContentRoot}/App_Data/Logs`, reproducing the old location. `Observability:Logging:Directory`
  **must** point at a mounted volume in any containerised deployment or the ten-year audit
  retention is fiction.

**Credential leak, addressed as part of the fix (it would otherwise have been made worse).**
Structured logging serialises objects, and `TenantInfo.ConnectionString` is a live credential.
`TenantInfoDestructuringPolicy` reduces any `TenantInfo` reaching any sink to
`{ TenantId, Hostname }`, and `SensitiveDataRedactor` scrubs credential- and locator-shaped
`keyword=value` pairs out of free-text fields. Tested; and a live run whose master *and* tenant
connections both failed produced a diagnostics file with **0 hits** for the password,
`Password`, `TenantInfo`, `SQLEXPRESS` and `NexGenErpDb`.

**Input now available (M2-A06, 2026-08-20).** Every API request carries a correlation id
(`Activity.Current?.Id ?? HttpContext.TraceIdentifier`), returned in the `X-Correlation-Id`
response header and in every `problem+json` body's `traceId`. M2-B11 enriches its sink with
that value rather than inventing a second id;
`V.SMART/V.SMART.Api/Middleware/CorrelationId.cs` is the one definition. **Accepted drift
risk:** `StructuredLoggingService` restates that expression rather than calling it, because
`V.SMART.Shared` cannot reference `V.SMART.Api` — KB-113 §6 records it.

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
> **4. `_basePath` may be null on some targets — ANSWERED 2026-08-21 (M2-B11, INV-046): NO.**
> The `#if ANDROID || WINDOWS || MACCATALYST` branch (`:11-16`) has its assignment commented
> out, which remains **Confirmed** — but the branch is **never compiled**.
> `dotnet msbuild V.SMART.Shared.csproj -getProperty:DefineConstants` returns `TRACE;DEBUG` on
> **both** target frameworks, including `net9.0-windows10.0.19041.0`: those symbols come from
> `Microsoft.NET.Sdk.Maui`, which needs `<UseMaui>true</UseMaui>`, and `V.SMART.Shared.csproj:1`
> uses `Microsoft.NET.Sdk.Razor` and never sets it. A `ProjectReference` is evaluated with its
> own properties, not the consuming MAUI project's. So the `#else` branch always runs,
> `_basePath` is always `AppContext.BaseDirectory/App_Data`, and **the MAUI host does log** —
> to the application directory, like the others. **Confirmed**, no longer Unknown.
>
> **5. The three impacts above are unchanged by M2-B11 for the Blazor and MAUI hosts.** The
> unsanitised username path (`:31`), the swallowed failures (`:89-104`) and the shared
> developer-log file (`:76`) all still exist in `FileLoggingService`, which those two hosts
> still resolve. `V.SMART.Api` no longer touches any of them. Q-09 is still the deciding
> question for the path-traversal severity.

### R-24 — No API error contract — **CLOSED 2026-08-20 (M2-A06)**
**Was Confirmed.** `CurrencyController` returned two different 400 shapes; there was no
exception middleware and no `ProblemDetails`.
**Closed by M2-A06.** `V.SMART/V.SMART.Api/Middleware/` now holds a single
`ProblemDetails` factory (`ApiProblems.cs`), a correlation-id middleware and a global
exception handler, registered by `UseErrorContract()` before `UseCors`. Every error response
across all six endpoints is `application/problem+json`; a business-rule refusal is `409` with
the service's message verbatim in `title`; a `500` carries `traceId` only. The contract is in
[`api/api-overview.md` § *Error contract*](../api/api-overview.md#error-contract-m2-a06) and
is covered by `tests/V.SMART.Api.Tests/` (21 tests, all passing 2026-08-20).
**Still open, deliberately:** *request logging*. Correlation ids are emitted, but they go to
`ILogger` with the default console provider only — a real sink is **R-23** / M2-B11.
**New finding, observed 2026-08-20 during close-out review (not reported by the implementer):**
`ExceptionHandlingMiddleware.cs` calls `context.Response.Clear()` before writing the
`problem+json` body, which discards any CORS headers already added by the inner `UseCors`
middleware. A cross-origin browser client hitting an unhandled exception or an unresolved
tenant (`500`/`503`) therefore sees a CORS failure in the browser console rather than the
`problem+json` body — the response is correct on the wire, but unusable from a browser without
the CORS headers. This is an inherent consequence of registering the error handler *before*
`UseCors`, which is what this task's own spec required, so it is not a defect against M2-A06's
scope — but **M2-A05** (real CORS) must account for it, e.g. by re-adding the CORS headers
inside the exception handler itself, or by special-casing preflight/origin echoing before
`Response.Clear()`.

### R-25 — Business logic executed in Razor with direct `SaveAsync`
**Confirmed.** 91 `SaveAsync` call sites inside `Pages/`.
**Impact.** Writes outside any service transaction; partial-write risk.
**Action.** Fold into service methods during `@code` extraction.

### R-26 — Duplicated DI composition roots
**Confirmed.** `V.SMART.Web/Program.cs` (34.8 KB, 242 registrations) and
`V.SMART/MauiProgram.cs` (38.6 KB) register the same graph independently.
**Impact.** They will drift; a service added to one host is missing in the other.

**Already manifested — the exact, measured divergence (Confirmed, M2-B07 attempt 2,
2026-08-19).** Normalising every registration call on `master` and de-duplicating gives
239 distinct real registrations in `V.SMART.Web/Program.cs` (242 matched lines; 240 distinct,
one of which — `:253` — is commented out) against 239 in `V.SMART/MauiProgram.cs` (243
matched lines). Their union is 249. Thirteen registrations diverged:

| Divergence | Evidence (paths on `master`) |
|---|---|
| **6 registered only in MAUI** — `IRouteCardRepository`, `IRouteCardSubRepository`, `IProductionReturnAssyRepository`, `IProductionReturnAssySubRepository`, `IProductionSCNAssyRepository`, `IProductionSCNAssySubRepository` | `V.SMART/V.SMART/MauiProgram.cs:389,395` and siblings |
| **7 registered only in Web** — `IAssemblyDefLabourService`, `IEstimateService`, `IJobOrderRepository`, `IJobOrderSubRepository`, `ILabourTrackRepository`, `IPrPoRatingService`, `IToolCribServices` | `V.SMART/V.SMART.Web/Program.cs` |
| `IFileOpener` lifetime — `Scoped` in Web, `Singleton` in MAUI | `V.SMART/V.SMART.Web/Program.cs:267`, `V.SMART/V.SMART/MauiProgram.cs:274` |
| `ReportService` registered twice in MAUI | `V.SMART/V.SMART/MauiProgram.cs:200,219` |
| `AddHttpClient()` in Web only; MAUI registers a bare scoped `HttpClient` instead; the API had neither | `V.SMART/V.SMART.Web/Program.cs:243`, `V.SMART/V.SMART/MauiProgram.cs:256` |

**Correction to the 2026-08-19 first pass (INV-039), which this supersedes.** That pass listed
**8** MAUI-only services, including `IContractReviewService` and `IRouteCardService`. Both are
in fact registered in the Blazor host too — `V.SMART/V.SMART.Web/Program.cs:467` and `:518` on
`master` — so the MAUI-only set is 6, not 8, and the claim that `/contractReviewMasterList`
and `/routeCardList` threw a DI resolution error in Blazor does not follow from those two
registrations. See Q-31 in [`open-questions.md`](../open-questions.md).

**Action.** Extract a shared `AddVSmartDomain(this IServiceCollection, IConfiguration)`
extension in `V.SMART.Shared` and call it from all three hosts.

**Status (2026-08-19): RESOLVED by `M2-B07`** — pending review of branch
`migration/M2-B07-add-vsmart-domain`; not yet merged to `master`.
**Evidence.** `V.SMART/V.SMART.Shared/DependencyInjection/ServiceCollectionExtensions.cs`
holds the single composition root; `V.SMART/V.SMART.Api/Program.cs`,
`V.SMART/V.SMART.Web/Program.cs` and `V.SMART/V.SMART/MauiProgram.cs` each call it exactly
once and retain 2 / 7 / 8 host-only registrations respectively, down from 1 / 242 / 243. The
union is preserved exactly — 249 distinct before, 249 distinct after, nothing dropped and
nothing invented. `tests/V.SMART.Shared.Tests/DependencyInjection/AddVSmartDomainTests.cs`
validates the whole graph with `BuildServiceProvider(validateScopes: true, validateOnBuild: true)`
(5 tests, green; 84/84 in the suite). Builds observed: API 0 errors / 6,694 warnings; Web 0
errors / 6,697 warnings; MAUI head 0 errors / 6,671 warnings.
**Re-measured 2026-08-19 (M2-B07 attempt 3, on `31a10ba`).** `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj`
→ `6695 Warning(s) / 0 Error(s)` from cold, exactly the KB-083 baseline — the `6,694` above is
one low. `dotnet build V.SMART/V.SMART.Web/V.SMART.Web.csproj` → `5 Warning(s) / 0 Error(s)`
incremental; the same project built from cold in a `master` worktree gives `6698 Warning(s) / 0
Error(s)`. `dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj` → `Failed: 0,
Passed: 84`. Both hosts start in `Development` and answer requests identically to `master` —
see the *Verification (2026-08-19) — attempt 3* section of
[`docs/kb/execution/tasks/M2-B07.md`](../execution/tasks/M2-B07.md).
**What this does *not* close.** The `IFileOpener` lifetime divergence is a **host**
registration and survives unchanged, by instruction — Web `Scoped`, MAUI `Singleton`. It is
recorded here rather than silently normalised, and needs its own decision.

### R-40 — `V.SMART.Api` opts out of build-time DI validation to be able to start
**Confirmed (M2-B07, 2026-08-19).** `V.SMART/V.SMART.Api/Program.cs` calls
`builder.Host.UseDefaultServiceProvider(… options.ValidateOnBuild = false …)` immediately
before `AddVSmartDomain(…)`. It has to: `WebApplicationBuilder` turns `ValidateOnBuild` on by
itself in the `Development` environment, which both launch profiles set
(`V.SMART/V.SMART.Api/Properties/launchSettings.json:9,18`), and seven registrations in the
shared composition root depend on host seams this API host does not yet have — `ReportService`,
`IUserService`, `IGSTITCService`, `IUserThemePreferenceService`, `ICompanyService`,
`IItemService` and, transitively via `ReportService`, `IEnquirySalesService`. Without the
opt-out the host aborts with `AggregateException("Some services are not able to be
constructed")`, exit code 255 — observed by the M2-B07 validator on `6f452cf`.
**Impact.** For as long as this line stands, a *genuinely* broken registration in the API's
graph is not caught at startup; it surfaces at controller activation instead. The equivalent
guarantee is carried only by
`tests/V.SMART.Shared.Tests/DependencyInjection/AddVSmartDomainTests.cs`, which validates the
same graph with test doubles for the seams — so it cannot catch a seam that the API host, and
only the API host, is missing.
**Action.** Delete the `UseDefaultServiceProvider` block once M2-B06 / M2-B08 supply
`IPathProvider`, `IFileUploadService`, `IFileOpener` and an `IJSRuntime` substitute for this
host; the comment in `Program.cs` says so at the site. `ValidateScopes` was deliberately left
at the framework default, so captive-dependency detection is unaffected.

### R-65 — `ScreenCatalogue` names two screens that exist in no database, and the startup validator accepts them
**Confirmed by direct measurement against two databases, 2026-08-21 (M0-01-03 rebuild drill).**
*(Id `R-65` is the next free one after **R-64**, held by the unmerged
`migration/M0-10-candelete-guard-audit` branch — checked with `git branch --no-merged master`
per KB-093's id-allocation note.)*

`V.SMART/V.SMART.Api/Authorization/ScreenCatalogue.cs` holds a compile-time set of **152**
`ScreenName` values. Real databases hold **150**:

| Source | `Screens` rows |
|---|---|
| Database rebuilt from source control (the drill) | **150** |
| Live development database `NexGenErpDb` (read-only query) | **150** |
| `ScreenCatalogue.cs` | **152** |

`ScreenCode` runs 1…152 with exactly **two gaps, 114 and 115** — the same two in both
databases. At least ten migrations call `DeleteData` against `Screens`; the compile-time
catalogue was copied from the `HasData` seed list **without the subsequent deletes**. Diffing
the catalogue against the rebuilt database gives the two phantoms and nothing else in either
direction:

> **`Bill Paid List`** and **`Bill Pending List`**

**The exposure.** `ScreenRightStartupValidator` refuses to start when a controller declares
`[RequireScreen("…")]` for a name *"which is not one of the 152 seeded"* names. Both phantoms
**are** in that set, so `[RequireScreen("Bill Paid List")]` **passes startup validation** — and
then denies every request forever, because `IUserRightsProvider` can never return a right for a
screen with no row in any tenant database. Startup is silent; every user is locked out of that
endpoint, in every tenant.

This is precisely the failure
[KB-105](../architecture/server-side-authorization-spec.md) warns about in its own words at
`:130` — *"either a silent bypass (R-03 reopened) or a silent lockout across 152 screens."* The
guard works; its input data is wrong by two entries. Note also that KB-105 `:171-173` records
*"Exactly 152 `Screens` rows are seeded"*, *"All 152 `ScreenName` values are unique"* and
*"`Id == ScreenCode` for all 152 rows"* as **Confirmed**. The first is false. The second and
third hold for the 150 real rows (zero name collisions, zero `Id`/`ScreenCode` mismatches) —
the *count* in each is what is wrong.

**Why the original claim looked Confirmed.** It was derived from the `HasData` block in
`ApplicationDbContext.cs` — a correct reading of the seed, and the wrong question. The seeded
state is not the migrated state whenever a later migration deletes seed rows, which is exactly
what happened here. **Reading a seed block is not the same as reading a database**, and this is
the second time in this project that a source-derived claim entered the KB as `Confirmed`
without a check against reality (see `task-tracker.md` footnote ²¹ for the first).

**Action — owner Vivek, lands on `M2-A02`.** That task annotates the first controller and must
not start against a catalogue with two unusable names. Fix: drop the two names, correct the
count to **150**, and re-derive the constant from the **post-delete** state rather than the
`HasData` block — ideally by querying a rebuilt database, which is now a ~1-minute operation
(`db/RUNBOOK-rebuild-tenant-database.md`). **Not fixed by M0-01-03**, which is forbidden to
touch anything under `V.SMART/`. Also correct KB-105 `:171-173`, `:320`, `:593`, `:966`,
[KB-012](../architecture/database-architecture.md)`:113` and
[KB-013](../architecture/auth-and-permissions.md)`:36`, `:118`, all of which state 152.

---

### R-41 — The API's screen-rights cache has no entry cap
**Confirmed (M2-A01-03, 2026-08-20)** for the code; **Inferred** for the exposure.
`V.SMART/V.SMART.Api/Program.cs` registers the shared `IMemoryCache` via `AddMemoryCache()`
with **no** `MemoryCacheOptions.SizeLimit`, so the screen-rights entries
(`screenrights:v1:{tenantId}:{userId}`, one `ScreenRightSet` each) are bounded only by the
60-second TTL and the number of distinct *(tenant, user)* pairs making authorized requests
within it. [KB-105](../architecture/server-side-authorization-spec.md) §8.2 asked for a
configurable cap.
**Why it was left off.** `SizeLimit` is cache-wide: once set, *every* consumer of this
singleton must populate `MemoryCacheEntryOptions.Size` or `Set` throws
`InvalidOperationException` at runtime. Imposing that on all future API code to bound one
consumer was judged the worse trade. `UserRightsProvider` sets `Size = 1` on its entries anyway,
so nothing has to change there when a cap is added.
**Action.** Either set `SizeLimit` once every consumer in the host sets `Size`, or give
`UserRightsProvider` its own `MemoryCache` instance with a configured limit. Neither is urgent:
a value is ≤152 small records and lives at most 60 seconds.


### R-43 — The API test project cannot make a single HTTP-level assertion
**Confirmed (M2-A07, 2026-08-20).** `tests/V.SMART.Api.Tests/V.SMART.Api.Tests.csproj`
references only `Microsoft.NET.Test.Sdk`, `xunit` and `Moq`. There is **no**
`Microsoft.AspNetCore.Mvc.Testing`, no `WebApplicationFactory` and no host — the project's own
infrastructure says so (`Infrastructure/ErrorContractTestContext.cs`: *"No host and no
database"*). Every one of its 148 tests exercises a controller, a filter or a middleware object
directly.
**Impact.** Anything decided *above* MVC is untestable here and is asserted by declaration
instead. Concretely, as of M2-A07:
- **`401` for a request with no token** is produced by the JwtBearer challenge, so the tests
  assert that `[Authorize]` is present and `[AllowAnonymous]` absent — the cause, not the effect.
- **The JWT inbound claim map** (does `ClaimTypes.Role` survive the round trip through
  `JwtTokenService` → wire → `HttpContext.User`?) is **Inferred from framework defaults, never
  observed**. `MeController` reads `ClaimTypes.Role` with a short-`"role"` fallback so it is
  correct either way, but the mapping itself is still unverified.
- **Tenant isolation and cache-key correctness over the wire** are proven at the seam
  (`IUserRightsProvider` receives the token's tenant) and not end to end.
- **CORS, the pipeline order and middleware interaction** are entirely uncovered.
**Action.** Add `Microsoft.AspNetCore.Mvc.Testing` and a `WebApplicationFactory` fixture, most
naturally as part of **M2-A03**'s permission-matrix harness, which needs real requests anyway.
Until then, no session may claim an over-the-wire result from this project.
**Number note.** `R-42` is deliberately skipped: it is claimed by the unmerged
`migration/M2-C00-kb050-angular-rewrite` branch, and reusing the number would produce two
different R-42s on merge.

### R-44 — Unresolvable `TenantId` claim falls back to host-based tenant resolution, contradicting the documented cross-tenant guarantee
**Confirmed (M2-A07 close-out validation, 2026-08-20).** Probed by starting the real API host
and calling `GET /api/v1/me` with a JWT whose `TenantId` claim names a tenant absent from the
`Tenants` table. The response was **`200`**, carrying a *different* tenant's 150-row rights
map, not an error. Root cause is two pre-existing behaviours composing badly, neither touched
by `M2-A07` (which was forbidden to modify either file): `TenantProvider.cs:46-58` falls back
to **host-based** resolution when the id lookup misses, while `UserRightsProvider` keys its
cache on the **claimed** tenant id (`UserRightsProvider.cs:50-53`). This directly contradicts
the unqualified sentence at `UserRightsProvider.cs:17-22` — *"a cross-tenant read is
structurally impossible through this repository"* — and the matching sentence in KB-040's
Tenancy paragraph, both of which need a caveat or a fix.
**Practically bounded today:** `AuthController` only ever mints a `TenantId` claim for a real
tenant, so the fallback path is not reachable through the shipped login flow. It becomes live
the moment any other JWT issuer, a hand-crafted token, or a stale/deleted tenant row enters the
picture.
**Action.** Decide, in front of `M2-A02` and `M2-A08`: either `TenantProvider` should fail
closed (no host fallback) when a `TenantId` claim is present but unresolvable, or every reader
downstream of it (`UserRightsProvider` included) needs to independently re-verify tenant
existence. Tracked as **Q-37** ([`open-questions.md`](../open-questions.md)).

### R-27 — Hardcoded developer-machine values in the MAUI project
**Confirmed.** `PackageCertificateThumbprint`, `AppInstallerUri = D:\` in
`V.SMART.csproj`.
**Action.** Move to build parameters.

### R-39 — Four fire-and-forget `UpdateTotalsAsync` calls are correct only while the engine is synchronous
**Confirmed (M0-12-02, 2026-08-19).** `UpdateTotalsAsync` is declared `async Task` but does
no asynchronous work — its last statement is `await Task.CompletedTask`
(`V.SMART/V.SMART.Shared/Services/CalculationService.cs:113`), so the returned `Task` is
already completed when it is handed back.

Four call sites rely on that without knowing it. They invoke the method from **`void`**
handlers and never await the result:

| Call site | Handler |
|---|---|
| `V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/DebitNote_pages/DebitNoteUpsert.razor:2629` | `private void OnDiscountPercentChanged()` |
| `…/DebitNoteUpsert.razor:2635` | `private void OnPandFPercentChanged()` |
| `…/DebitNoteUpsert.razor:2641` | `private void OnInsurancePercentChanged()` |
| `…/DebitNoteUpsert.razor:2647` | `private void OnTCSPercentChanged()` |

Each calls `_calculationService.UpdateTotalsAsync(DebitNoteVMs);` and then `StateHasChanged()`
on the next line. Today the totals are already computed by the time `StateHasChanged` runs.

**Impact.** BR-CALC-001's migration note requires the same engine to be reachable as
`POST /api/documents/calculate`. The moment any implementation on this path becomes genuinely
asynchronous — an HTTP call, an EF query, a cache lookup — these four handlers render **stale
totals** and swallow any exception into an unobserved task. Nothing warns: there is no
compiler diagnostic for a discarded `Task` returned by a method call statement in a `void`
Blazor handler, and no test would fail except
`S19_UpdateTotalsAsync_CompletesSynchronously_DespiteTheAsyncSignature`, which exists
precisely as that tripwire (`tests/V.SMART.Shared.Tests/Services/CalculationServiceCharacterisationTests.cs`).

**Not affected:** the many `@bind:after='() => _calculationService.UpdateTotalsAsync(…)'`
bindings in the same file (e.g. `:640`, `:694`, `:960`) — those lambdas return the `Task` to
Blazor, which awaits it.

**Action.** Out of scope for M0-12-02 (a Blazor code change, and this task may not modify
`V.SMART/`). Convert the four handlers to `async Task` **before** anything makes the
calculation path asynchronous. Do not treat the synchronous completion as a licence to leave
them.

---

## Low

| # | Item | Evidence |
|---|---|---|
| R-28 | Folder/namespace typos: `Data/Maintanence`, `Estimaton`, `Fesibility`, `ProuctionCompRepo`, `EstiamateId`, `Advaceadjustment`, `Sub-Contrect GRN`, `/hr-masterr` | throughout |
| R-29 | Empty folders declared in `V.SMART.Shared.csproj` (~30 `<Folder Include=…>` entries) | `.csproj` |
| R-30 | **109** migrations (~2.5M LOC, ~90% of repo size) — **corrected 2026-08-21**: "219" counted *files*, and each migration is a `.cs` plus a `.Designer.cs`. Only **108** are applicable; `20260324053747_AddnewTemperveryTable` has no `.Designer.cs` and has never been applied to any database (**Q-65**). The LOC and repo-share figures are unaffected. | `Migrations/`, `db/REBUILD-DRILL-LOG.md` F3 |
| R-31 | Dead role `"ERPAdmin"` in `AuthorizeView` but absent from `UserRole`. **Encountered by M2-A07 (2026-08-20) and deliberately not propagated:** `GET /api/v1/me` returns the role as an opaque string taken from the JWT `ClaimTypes.Role` claim, so the API model neither defines nor can invent this name; a test is the tripwire. Still open in Blazor — three sites, all `<AuthorizeView Roles="Administrator,ERPAdmin,User">`, and because that list also names both real enum members the gate is effectively "any authenticated user with a role" | `NavMenu.razor:36,148`, `Pages/Home.razor:240` vs `Data/Enum/UserRole.cs:3-7` |
| R-32 | Large blocks of commented-out code (e.g. `ReportExecutor.cs:47-80`) | multiple files |
| R-33 | `Underconstruction.razor` shipped in the component set | `Components/` |
| R-34 | ~~Angular pilot will become dead code once React starts~~ — **REVERSED by ADR-007 (2026-08-20): the pilot becomes the baseline.** The live risk is now the opposite: `frontend/nexgen-web/` (React) is the dead code, removed by the re-scoped `M2-C01` | `frontend/vsmart-erp/`, `frontend/nexgen-web/` |
| R-35 | Two per-tenant path conventions (`CompanyName` vs `Hostname`) | `TenantProvider` vs `ReportService` |
| R-36 | Inconsistent route casing (`/MfgPO/create` vs `/mfgPO/details`) | `Pages/` |
| R-37 | `docs/ARCHITECTURE.md` is an unfinished template with `[TODO: ANALYZE]` markers presented as documentation | `docs/` |
| R-42 | **`file:line` citations into KB/ADR *documents* rot silently when the cited document is edited.** Editing a source file usually breaks a citation visibly (the quoted code is gone); inserting 28 lines into an ADR shifts every later citation into plausible-looking but wrong text, and nothing fails. Observed twice: the M0-09 validator correcting a `:1119` citation (see R-14 note), and **M2-C00, where `be818b9` grew `ADR-007-angular-stack.md` from 197 to 225 lines and silently invalidated 7 of 8 ADR-007 citations in KB-050 and 4 of 6 in `M2-C01.md`** — all re-anchored 2026-08-20. Citations into code are load-bearing and stay; citations into prose documents should prefer a `§heading` anchor, and any task editing an ADR should grep for `<adr-filename>:[0-9]` first | `docs/kb/**` |
| ~~R-45~~ | ✅ **RESOLVED 2026-08-23 (`4af2f4f`) — `"endOfLine": "auto"` added to `frontend/nexgen-web/.prettierrc`; `format:check` reports "All matched files use Prettier code style!". Proven line-endings-only: prettier's output for `src/main.ts` was byte-identical to source once CR was stripped.** Original entry: **`npm run format:check` fails on a fresh Windows checkout, on line endings alone, in files nobody touched.** The repository sets `core.autocrlf=true` (with `* text=auto` in `.gitattributes`), so Git writes CRLF to disk; Prettier defaults `endOfLine` to `lf` and `frontend/nexgen-web/.prettierrc` does not override it. Observed 2026-08-23 during `M2-C04-01`: **28 untouched files** reported as unformatted, and stripping carriage returns from `src/main.ts` makes its content byte-identical to Prettier output — i.e. the only difference is the line ending. KB-083 records this command as passing, which it presumably did on a checkout that produced LF. Consequence: the format gate is unusable as a pass/fail signal on Windows, and a real formatting defect would hide in the noise. **Fix is one line** (`"endOfLine": "auto"` in `.prettierrc`) but belongs to whichever task owns the frontend tooling gate, not to a task that happens to notice it — `M2-C04-01` formatted only the files it changed and left the rest alone. | `frontend/nexgen-web/.prettierrc`, `.gitattributes`, `git config core.autocrlf` |

---

## Priority sequence

**Week 1 (do these regardless of the migration):** R-01, R-02, R-04, R-09.
**Before the second API controller:** R-03, ~~R-24~~ (closed 2026-08-20, M2-A06),
~~R-11~~ (closed 2026-08-21, M2-B04).
**Before any `DELETE` endpoint is written (added 2026-08-21, M0-10):** **R-64** first — it is
a decision (Q-60), not a fix, and it binds every delete endpoint in M3/M4. Then **R-60**
(one identifier), then **R-63** (specification work, the long pole). R-61 and R-62 can follow
the module that owns them.
**Before the app shell (M2-C03) is built on PrimeNG (added 2026-08-21, M2-C01):** **R-51** —
it is a procurement decision (Q-66), not a fix, and it is cheapest to answer while exactly one
placeholder screen exists.
**Before any stock-touching module migrates:** R-05 (tests first), R-07, R-10.
**During Phase 2–3:** R-06 (per module), R-12, R-13, R-26.
**Opportunistic:** everything Low.
