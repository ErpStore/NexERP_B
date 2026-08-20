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
status: complete
confidence: mixed
last_verified: 2026-08-20
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

**Still open — second action item.** Auditing **every** other `CanDelete…` method for the
same pattern remains open as **INV-025 / task M0-10**. M0-09 deliberately touched exactly
one method. An API makes these branches far easier to reach than the current UI does.

> **Related gap noticed while fixing, not acted on (Confirmed, 2026-08-19).** The guard is
> **advisory**: `MfgPoService.DeletePOByPOIdAsync` (`MfgPoService.cs:790-801`) never calls
> `CanDeleteSalesOrderAsync`; the only enforcement is the caller,
> `Pages/SalesAndLabour_pages/SalesPo_Pages/MfgPOList.razor:1079-1090` (`HandleDelete`),
> while `ConfirmDelete_Click` (`:1108`) deletes at `:1118` without re-checking (corrected from
> an earlier `:1119` citation by the M0-09 validator, 2026-08-19). So M0-09
> hardens *what the check reports*, not the delete path itself. A future
> `DELETE /api/v1/sales-orders/{id}` must call the guard server-side or repeat this gap.
> Scoped to **M0-10** as a lead; deliberately out of M0-09's two-line scope.

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

> **Full inventory: [KB-100 `modules/document-numbering.md`](../modules/document-numbering.md)**
> — produced 2026-08-20 by `M2-B12-01` (INV-012, now Complete). Read it before acting on this
> entry: it carries the complete call-site inventory, the format catalogue, the financial-year
> rules, the ten `BR-DOC` series-sharing rules and the concurrency analysis.
> **The classification stays `Inferred (high confidence)` and must not be upgraded here** —
> reading code proves a race is *possible*, not that one has *occurred*. Only `M2-B12-02`'s
> live-database duplicate census can upgrade it.

~~Inferred. ~20 repositories derive the next document number with `SELECT TOP 1 * … ORDER BY
… DESC` and no lock, no `UPDLOCK`, no serializable transaction, no DB sequence.~~

> **Corrected 2026-08-12 (Confirmed).** Two errors, one of them dangerous:
>
> **1. The lock hint is already there.** **37 of the 38** raw-SQL sites — spread over all
> **36** repository files that derive a next number — use
> `FROM <Table> WITH (UPDLOCK, ROWLOCK)`. Example —
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
>   **Widened 2026-08-20 (M2-B12-01, Confirmed): there are FOUR such sites, not one** —
>   also `MfgInvService.cs:982-985`, `ExpInvService.cs:256-259` and
>   `LabourInvoiceService.cs:645-648`.
> - `MfgDcService.IsDuplicateDcNoAsync:771-790` scopes uniqueness **by `CustId`**, so an
>   unqualified `(DcNo, Suffix)` unique index would reject data the application currently
>   accepts. **Corrected 2026-08-20:** the census is **81 `IsDuplicate*Async` occurrences
>   across 59 files**, not the 62/41 previously circulated; scoping varies by module and is
>   tabulated in [KB-100](../modules/document-numbering.md) §7.
>
> **Related defect (Confirmed):** `CommonService.cs:1957-1961` swallows its exception and
> returns `null`, so a failed allocation silently yields a null document number.
> **Added 2026-08-20:** the invoice allocator does the same at `CommonService.cs:2195-2199`,
> and `MfgPoService.GetNextSaleOrderNoAsync:1656-1659` **fails open** — its `catch` returns
> the literal `"SO-0001"`, so a transient fault yields a number that very probably already
> exists.
>
> **Related divergence — the open half is now ANSWERED (Confirmed, 2026-08-20, M2-B12-01).**
> There are **two** financial-year implementations with the **same boundary but different
> output shapes** — `FinancialYearHelper.cs:13` (`Month >= 4` → `"/{yyyy}-{yy}"`) and
> `CommonService.cs:1849-1851` (`Month > 3` → `"{yy}-{yy}"`).
> ~~Which one reaches the stored `Suffix` is **Unknown** and is M2-B12-01's job.~~
> **`FinancialYearHelper` produces every stored `Suffix`; the `CommonService` version is dead
> code** — its `financialYear` local is assigned at `:1851` and `:1971` and **never read**
> (a `grep` over that file returns exactly those two lines, both assignments), and the
> allocators scope on the `suffix` *parameter*, which 53 Razor pages populate from
> `FinancialYearHelper`. The divergence is therefore **latent, not active**.
> **M2-B12-03 is still forbidden from unifying them** — the suffix is user-visible on
> statutory documents, and a refactor that routed the `{yy}-{yy}` shape into storage would
> change how a statutory document reads.
>
> **4. A SECOND discriminator, with inverted values (Confirmed, 2026-08-20).** Alongside
> `Company.BookTypeDc`, `GenerateInvoiceAutoRunningNoAsync` (`CommonService.cs:2078-2201`)
> branches on `Company.BookTypeInvoice` — and **its values 1 and 2 mean the opposite of the
> DC allocator's**. Ten branches in total, registered as `BR-DOC-001`…`BR-DOC-010` in
> [KB-030](../business-rules/business-rule-inventory.md). A remedy that covers only
> `BookTypeDc` misses half the rules.
>
> **5. ~7 of the 38 raw-SQL sites are DEAD CODE (Confirmed, 2026-08-20).** `GetLastDcNoAsync`
> (3 repositories), `GetLastInvNoAsync`, `GetLastExpInvNoAsync`, `GetLastLabInvoiceNoAsync`
> and `GetLastReqNoAsync` have **no live caller** — every remaining reference is a
> commented-out line. **The remedy surface is 31 live sites, not 38.**
**Impact.** Concurrent creation produces duplicate document numbers. Currently masked by
low concurrency in Blazor Server; an API will increase concurrency.
**Action.** Verify against the live schema for unique constraints (Q-10) — `M2-B12-02`, whose
input is [KB-100](../modules/document-numbering.md) §9. Then replace the allocation with a
`sp_getapplock`-guarded or `HOLDLOCK`-ranged read+insert, or an atomic
`UPDATE … SET LastNumber = LastNumber + 1 … OUTPUT` — **not a plain DB sequence**, which
constraint 1 above rules out. Add idempotency keys on create endpoints.
*(Corrected 2026-08-20: this line previously proposed "a DB sequence", contradicting the
decrement-on-delete constraint recorded immediately above it.)*

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

## Medium

### R-15 — Invalid GST rate silently coerced to zero
**Confirmed.** `CommonConstants.GetIGST/GetGST` use `FirstOrDefault(r => r == rate)`,
returning `0` for an unlisted rate rather than raising.
**Action.** Return `decimal?` or throw; validate at the API boundary.

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

### R-23 — Flat-file logging with no aggregation, and an unsanitised path
**Confirmed.** `FileLoggingService` writes flat text files under `App_Data/Logs/`.
**Impact.** No searchability, no alerting, unbounded growth, lost on container restart.
**Action.** Structured logging (Serilog) to a real sink (M2-B11); preserve the *user-action
audit trail* as a first-class feature.
**Input now available (M2-A06, 2026-08-20).** Every API request carries a correlation id
(`Activity.Current?.Id ?? HttpContext.TraceIdentifier`), returned in the `X-Correlation-Id`
response header and in every `problem+json` body's `traceId`. M2-B11 should enrich its sink
with that value rather than inventing a second id;
`V.SMART/V.SMART.Api/Middleware/CorrelationId.cs` is the one definition.

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
| R-30 | 219 migrations, ~2.5M LOC, ~90% of repo size | `Migrations/` |
| R-31 | Dead role `"ERPAdmin"` in `AuthorizeView` but absent from `UserRole` | `NavMenu.razor` vs `Data/Enum/UserRole.cs` |
| R-32 | Large blocks of commented-out code (e.g. `ReportExecutor.cs:47-80`) | multiple files |
| R-33 | `Underconstruction.razor` shipped in the component set | `Components/` |
| R-34 | ~~Angular pilot will become dead code once React starts~~ — **REVERSED by ADR-007 (2026-08-20): the pilot becomes the baseline.** The live risk is now the opposite: `frontend/nexgen-web/` (React) is the dead code, removed by the re-scoped `M2-C01` | `frontend/vsmart-erp/`, `frontend/nexgen-web/` |
| R-35 | Two per-tenant path conventions (`CompanyName` vs `Hostname`) | `TenantProvider` vs `ReportService` |
| R-36 | Inconsistent route casing (`/MfgPO/create` vs `/mfgPO/details`) | `Pages/` |
| R-37 | `docs/ARCHITECTURE.md` is an unfinished template with `[TODO: ANALYZE]` markers presented as documentation | `docs/` |

---

## Priority sequence

**Week 1 (do these regardless of the migration):** R-01, R-02, R-04, R-09.
**Before the second API controller:** R-03, ~~R-24~~ (closed 2026-08-20, M2-A06), R-11.
**Before any stock-touching module migrates:** R-05 (tests first), R-07, R-10.
**During Phase 2–3:** R-06 (per module), R-12, R-13, R-26.
**Opportunistic:** everything Low.
