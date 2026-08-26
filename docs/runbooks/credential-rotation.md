# Runbook — rotate the exposed credentials (task M0-04)

**Audience:** the people who hold production access to each system below. This document is
self-contained; it does not assume you have read the migration plan.

**This document never contains a credential value.** Every secret is named and located by
`file:line`; none is quoted, and no replacement value is generated here.

**Status of this document: the procedure, not the proof.** Writing this runbook rotates
nothing. §8 is the only section that turns a plan into a fact, and it is unsigned as of the
date below. Until it is signed, R-01 and R-02 in
[`docs/kb/risks/technical-debt-register.md`](../kb/risks/technical-debt-register.md) remain
**open**.

| | |
|---|---|
| Written | 2026-08-25 (task M0-04) |
| Evidence re-verified against | `master`, 353 commits, working tree at the date above |
| Rotation performed | **Not yet — nobody with production access has executed this** |

---

## 0. Why this exists, and why now

`https://github.com/ErpStore/NexERP_B.git` is **public**, by the owner's own deliberate
decision (Q-19, answered — see §1). Re-verified for this document on 2026-08-25 by the only
valid test (INV-034): an *unauthenticated* REST call,
`curl -s -o /dev/null -w "%{http_code}" https://api.github.com/repos/ErpStore/NexERP_B`,
returned **`200`**. A plain `git ls-remote` is **not** a valid visibility test on Windows —
the Git Credential Manager authenticates silently and makes a private repository look
public. Do not use it.

A deliberately public repository is not a reason to rotate less urgently. It is the reason
rotation cannot wait. Public repositories are scraped continuously and automatically; the
exposure window is of unknown length, so **assume every credential below is already
harvested** rather than investigating whether it was.

Two things that are *not* substitutes for rotation:

- **Externalising configuration (M0-03, landed)** stops the *next* leak. It removed every
  credential literal from `V.SMART/` — verified for this document: a `git grep -l --untracked`
  for the SA password scoped to `V.SMART/` returns **nothing**. The already-published values
  remain published.
- **Purging git history (M0-05, not yet run)** is damage limitation applied *after*
  rotation. It cannot un-copy what has been taken, and it must not run before this runbook's
  checklist is signed.

### 0a. The exposure has moved from source into the knowledge base

Confirmed 2026-08-25, and this is new since the task was written: the credential literals are
gone from `V.SMART/` but are **still in `HEAD`, inside `docs/kb/`**.

| Secret | Files at `HEAD` that still contain the literal |
|---|---|
| The SA password | `docs/kb/execution/tasks/M0-03-01.md`, `M0-03-02.md`, `M0-04.md`, `M0-05.md`, `docs/kb/risks/technical-debt-register.md` (5 files) |
| The production `bspl` password | `docs/kb/risks/technical-debt-register.md:44` |
| The default `Jwt:Secret` | `docs/kb/execution/M0-00-baseline-decisions.md`, `docs/kb/execution/tasks/M0-00.md`, `docs/kb/execution/tasks/M0-03-01.md`, `docs/kb/execution/tasks/M0-04.md` |

Consequence for M0-05: its purge surface is **not one commit**. History is 353 commits, and
`git log --oneline -S"<the SA password>" -- V.SMART/` shows three (`c12c5b2` introduced it,
`a43e18d` and `e6e5295` removed it) — plus the KB files above, which are live at `HEAD` and
are therefore not a history problem at all but a *current content* problem. Whether those KB
files are redacted, and by which task, is an **owner decision** recorded as **Q-84** in
[`docs/kb/open-questions.md`](../kb/open-questions.md). Rotation makes the question cheap:
once a value is dead, quoting it is embarrassing rather than dangerous. Rotate first.

---

## 1. Q-19 — already answered; do not re-file it

The M0-04 specification instructs "raise Q-19" as the first action. That instruction was
written on 2026-08-12, when Q-19 did not exist. **It exists now and it is answered** —
`docs/kb/open-questions.md`, first row of *Product / business decisions*, struck through and
marked *ANSWERED 2026-08-12 — owner decision*: the owner (Kumar) deliberately set the
repository to public, after INV-029's original "public" finding was corrected to "private"
by INV-034 and then superseded by that decision. Re-filing it would duplicate an answered
question.

What the answer means for this runbook: **the credentials are live-exposed, not
hypothetically exposed.** §8 item 8 asks the operator to re-confirm visibility at rotation
time, because it is a setting that can change again.

---

## 2. Credential inventory

Every credential this task could find, every place it is consumed from, and how confident
that location list is. `Confirmed` rows carry a `file:line` re-verified on 2026-08-25.
`Unknown` rows state exactly what would resolve them — never a guess.

> **The task file's own inventory (`docs/kb/execution/tasks/M0-04.md:237-242`) is stale.**
> Nearly every `file:line` in it predates M0-03-01/-02/-03. The table below supersedes it.

| # | Credential | Consumed from | Confidence |
|---|---|---|---|
| **C-1** | SQL login `sa` on the local/dev SQL Express instance | **No longer in any file.** Removed by M0-03-01/-02; supplied at run time from user-secrets or `ConnectionStrings__MasterDb` (`docs/CONFIGURATION.md:36-42`, `:68-72`). Consuming code: `V.SMART/V.SMART.Web/appsettings.json:10` and `V.SMART/V.SMART.Api/appsettings.json:34` (both now `""`); the design-time factories `V.SMART/V.SMART.Shared/Data/MigrationData/MasterDbContextFactory.cs` and `ApplicationDbContextFactory.cs` via `DesignTimeConnectionString.Resolve`; `V.SMART/V.SMART/MauiProgram.cs` (no literal remains). **Published in git history** — introduced by `c12c5b2`, removed by `a43e18d` / `e6e5295`. | Confirmed |
| **C-2** | ~~SQL login on "production host" `154.61.76.112,1533`~~ **VOID — corrected 2026-08-26 (owner statement): this host does not belong to this project.** No login here for this project to rotate. See the correction note below the table. | Same consumers as C-1 — it was the commented alternative in the same files. **No longer in any `V.SMART/` file**; still in git history, and its password is still in `HEAD` at `docs/kb/risks/technical-debt-register.md:44`. | Confirmed present in source; ownership corrected — **not this project's credential to act on** |
| **C-3** | **Per-tenant connection strings in the `Tenants` table** — plaintext, **in the master database, not in any file in this repository** | Schema: `V.SMART/V.SMART.Shared/Data/TenantInfo.cs:8` (`ConnectionString`). Consumed at `V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantDbContextFactory.cs:19`, which passes it straight to `UseSqlServer` with a 60-second command timeout. There is **no fallback**: if the stored string is wrong, that tenant is down. | Confirmed (mechanism). **Unknown: how many rows, and which login each embeds.** Resolved by a human with read access running `SELECT Id, Name, Hostname FROM Tenants` on the master database — **not by any AI session**. |
| **C-4** | `Jwt:Secret` — the API's token-signing key | `V.SMART/V.SMART.Api/appsettings.json:37` (now `""`); read at `V.SMART/V.SMART.Api/Program.cs:176` and `V.SMART/V.SMART.Api/Auth/JwtTokenService.cs:25`. `Jwt:ExpiresMinutes` is `480` (`appsettings.json:40`). | Confirmed. **Correction to the task file:** its recorded negative result — *"the JWT signing secret is not present in committed history"* — is **false on current `master`**. The default value is in `HEAD` in four KB files (§0a), carried there by M0-00. Rotate regardless. |
| **C-5** | GST e-Invoice / e-Way **gateway** account (username + password) | **Not in any file.** Stored per tenant, AES-encrypted, in `Companies.APIEinvoiceLicenseKey`: read at `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/EInvoiceAPIService/EinvoiceDatabaseService.cs:152` and `EWayDatabaseService.cs:85`; wrapped by `new LicenseProductKey(...)` at `EinvoiceDatabaseService.cs:118` and `EWayDatabaseService.cs:58`; the username and password are pulled out of the decrypted JSON by `LicenseProductKey.GetUserName()` (`V.SMART/V.SMART.Shared/E_Invoice/LicenseProductKey.cs:113`) and `GetUserNameEway()` (`:127`). The previously committed literals in the two services are gone (M0-03-02). | Confirmed (mechanism and storage location — this **answers** the task file's "Unknown where the runtime values come from"). **Unknown: how many tenant rows hold a licence key**, i.e. the blast radius. Resolved per tenant by a human with database access. |
| **C-7** | **The AES key and IV that decrypt every tenant's `APIEinvoiceLicenseKey`** — i.e. the credential protecting C-5 | Hardcoded string literals at `V.SMART/V.SMART.Shared/E_Invoice/LicenseProductKey.cs:28` (`encryptKey`) and `:29` (`encryptiv`), inside `public static string Decrypt(...)` (`:26`). **Tracked, at `HEAD`, in a public repository.** | Confirmed. **New — not in the task file's C-1…C-6.** Consequence: the encryption on `Companies.APIEinvoiceLicenseKey` gives no confidentiality against anyone who has read this repository, so C-5 must be treated as harvested exactly like C-1/C-2 — and **rotating C-5 alone does not fix it**, because the next licence key is protected by the same published key. See §6a. |
| **C-6** | Seeded default `Administrator` account (fixed PBKDF2 hash, in every tenant database) | `V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs` — `HasData` block at `:1135-1147`, `UserName = "Administrator"` at `:1140`, the fixed hash at `:1141`. (The task file's `:1136` is stale by five lines.) | Confirmed, per R-09. **Owned by task M0-06, not by this one.** Listed so nobody assumes M0-04 covered it. Do not act on it from this runbook. |

> **Correction, 2026-08-26 (owner statement, in session) — C-2's host is not ours.** Every
> mention of `154.61.76.112` in this document as "the production host" was an unverified
> inference (it sat beside the real `sa` credential, was commented rather than deleted, used a
> production-shaped database name) — nobody had confirmed which organisation actually operates
> it. The owner has confirmed directly: this IP does not belong to this project, and the
> quoted `bspl` password is **correct** — a real, live, third-party credential, not a stale or
> fabricated one. **Consequence for this runbook:** there is no login on that host for §3 to
> disable, no maintenance window to schedule on its account, and **C-2 is struck from every
> procedure step below** — §3's title, owner and window description, and checklist item 1 all
> originally covered C-1 *and* C-2 together; read them as **C-1 only** now. The exposure is not
> resolved by this correction, only reclassified — see **Q-103** in `open-questions.md` for
> the redaction/notification decision this raises, which this runbook cannot make.

### 2a. Locations outside this repository

Honest `Unknown` beats a guess. Each row states who resolves it.

| Location class | Status |
|---|---|
| CI/CD variables | **Confirmed: none.** `.github/workflows/ci.yml` consumes no GitHub Actions secret; the only textual match for `secrets.` is the comment at `ci.yml:12` ("This repository is public"). Nothing to rotate here. |
| Publish profiles / `web.config` / Docker / `.env` | **Confirmed: none exist.** `git ls-files` matches nothing for `.pubxml`, `PublishProfiles`, `web.config`, `docker` or `.env`. |
| Secret manager (Key Vault or similar) | **Confirmed: none.** No `KeyVault`, `SecretClient`, `AddAzureKeyVault` or `DefaultAzureCredential` anywhere under `V.SMART/`. Secrets are user-secrets and environment variables only (`docs/CONFIGURATION.md`). |
| Developer machines' `dotnet user-secrets` stores | **Unknown, and expected to hold values.** Per-developer, per-machine, invisible to any repository scan. Resolved by asking each developer who has run this solution to re-run the `dotnet user-secrets set` commands in `docs/CONFIGURATION.md:36-42` with the new values, and to confirm the old ones are gone. |
| IIS / app-service configuration on the deployment hosts | **Unknown.** Not visible from a checkout. Resolved by whoever administers the hosting environment. |
| SQL Agent jobs, backup/restore scripts, monitoring agents | **Unknown** — this is **Q-15** in `docs/kb/open-questions.md:72`, still open. INV-022 confirmed the *application* has no background processing; that says nothing about jobs configured on the SQL Server instance itself. **Moot for `154.61.76.112`** now that host is confirmed not this project's (§2 correction note) — this row applies only to instances this project actually operates, resolved by the DBA enumerating logins, jobs and sessions there. |
| Other clones, forks, or GitHub search caches | **Unknown, and not determinable from this repository.** Rotate regardless. Note that a history purge is moot while the literals remain at `HEAD` in `docs/kb/` (§0a). |
| `V.SMART/V.SMART/appsettings.json` (MAUI host) | Checked: contains only `UpdateSettings:SetupPath`. **Not a credential.** |
| `V.SMART/V.SMART.Api/wwwroot/config/tenant.json:2-5` | Checked: a tenant name and a UNC path. Infrastructure disclosure, **not a credential**. |
| Both `appsettings.Development.json` files | Checked: a `Logging` section only. **No secret.** |

### 2b. Confirmed non-issues — do not re-investigate

The literal `bspl` also appears in non-credential contexts (a public contact email address in
`Pages/Home.razor`, and project/markup files). Recorded in INV-029's M0-03-02 amendment in
`docs/kb/investigation-registry.md`. These are not secrets; rotate nothing on their account.

### 2c. The replacement values already have a machine guard

Both web hosts validate configuration at startup before anything else runs —
`V.SMART/V.SMART.Api/Program.cs:28` and `V.SMART/V.SMART.Web/Program.cs:187`, plus a second
call from `V.SMART/V.SMART.Api/Auth/JwtTokenService.cs:24`. `StartupConfigurationValidator`
holds SHA-256 digests of the known pre-rotation connection strings
(`V.SMART/V.SMART.Shared/Services/StartupConfigurationValidator.cs:40-64`) and of the known
default JWT secret (`:66-71`), and enforces a 32-byte minimum on the JWT secret (`:73`,
`ValidateJwtSecret` at `:132`).

**Operationally this means: re-deploying any old value fails startup, loudly.** That is a
safety net, not a substitute for the steps below.

---

## 3. C-1 — the local/dev SQL login

*(Originally "C-1 and C-2" — struck to C-1 only, 2026-08-26; see the correction note under §2.
`154.61.76.112` is not this project's host, so there is no C-2 login here to rotate.)*

**Owner.** Whoever holds `sysadmin` (or equivalent) on the local/dev SQL Express instance.
Name that person before starting; the checklist requires their name.

**Blast radius.** Every tenant whose `Tenants.ConnectionString` embeds the rotated login goes
down the moment the old login is disabled, and stays down until every consumer *and* every
`Tenants` row carries the new value. **The failure is opaque**: tenant resolution returns
`null` and `TenantDbContextFactory.cs:19` dereferences `.ConnectionString` with no guard, so
users see HTTP 500s, not an authentication error. Do not expect a helpful message.

**Window.** Required only if a `Tenants` row actually points at the local/dev instance being
rotated — check first; do not assume it does not.

**Procedure.** In this order:

1. **Create a new, least-privilege SQL login alongside the old one.** Do **not** reuse `sa` —
   KB-060 R-01's action item is explicit: *"Use least-privilege SQL logins, not `sa`."* Grant
   only what the application needs (read/write on the master and tenant databases it actually
   uses; no `sysadmin`, no server-level roles). Leave the old login enabled for now.
2. **Deploy the new value to every consumer** through the mechanism M0-03 established — it has
   landed, so there is no interim path to record:
   - developer machines: `dotnet user-secrets set "ConnectionStrings:MasterDb" …` for
     `V.SMART.Web`, `V.SMART.Api` and (design-time) `V.SMART.Shared`
     (`docs/CONFIGURATION.md:36-42`);
   - servers and CI: the environment variable `ConnectionStrings__MasterDb`
     (`docs/CONFIGURATION.md:68-72`);
   - the MAUI head reads environment variables only because they were explicitly added — see
     the caveat at `docs/CONFIGURATION.md:88`.
3. **Update every row in `Tenants` whose `ConnectionString` embeds the old login.** This is
   the step most often forgotten and the one that takes every tenant down. See §4.
4. **Verify the application serves at least one tenant on the new login** — before touching
   the old one. Both hosts refuse to start on a known-old connection string (§2c), so a clean
   start is itself part of the evidence.
5. **Only then disable the old login** (`ALTER LOGIN … DISABLE`). Do **not** drop it yet.
6. **Drop the old login later**, as a separate change, after a full cycle of confirmed-good
   operation on the new one.

**Rollback.** Re-enable the old login (`ALTER LOGIN … ENABLE`). Because step 5 disabled rather
than dropped it, any `Tenants` row not yet updated resumes working immediately. That is the
entire reason disable precedes drop.

**Verification.** §8 items 1–3.

---

## 4. C-3 — the per-tenant connection strings in the `Tenants` table

**Owner.** Whoever has write access to the master database. Usually the same person as §3, but
confirm it: this is a data change, not a server-configuration change.

**Blast radius.** Per row. A row left on the old login is one tenant down, silently, as
described in §3. A malformed row is the same failure.

**Window.** The same window as §3 — these updates happen inside it, not after it.

**Procedure.**

1. Before changing anything, **take a copy of the `Tenants` table** (`SELECT * INTO
   Tenants_backup_<yyyymmdd> FROM Tenants`, or a full master-database backup). That copy is
   the rollback.
2. Enumerate the rows: `SELECT Id, Name, Hostname FROM Tenants`. Identify which
   `ConnectionString` values embed the old login. Record the count — it is currently
   **Unknown** to this project and belongs in the checklist's evidence column.
3. Update those rows to the new login **one at a time**, verifying the affected tenant loads
   after each update, rather than updating them all and then testing.
4. Re-check that no row still references the old login **before** §3 step 5 disables it.

**Rollback.** Restore the affected rows from the copy taken in step 1, and re-enable the old
login (§3 rollback). Both halves are needed; either alone leaves tenants down.

**Verification.** §8 item 3.

> **Standing debt, not fixed by this rotation:** these strings are stored in **plaintext**.
> Encrypting the column is a separate action item under R-01 and is outside M0-04's scope.

---

## 5. C-4 — `Jwt:Secret`

**Owner.** Whoever manages `V.SMART.Api`'s deployment configuration.

**Blast radius.** **Every currently issued token stops validating the moment the secret
changes.** Tokens live up to 8 hours (`Jwt:ExpiresMinutes` = `480`,
`V.SMART/V.SMART.Api/appsettings.json:40`), so without rotation an old token stays usable for
that long; with rotation, every logged-in user must sign in again immediately. The API surface
is still small, which is an argument for rotating **now**, before M2 puts real traffic on it.

**Window.** None required. Schedule it for a quiet period purely to reduce the number of
people forced to re-authenticate.

**Procedure.**

1. **Generate at least 32 bytes from a cryptographically secure source** — for example
   `openssl rand -base64 48`, or `RandomNumberGenerator.Fill` in .NET. Never a memorable
   phrase, never derived from the old value, and never written into this repository.
2. Deploy it as `Jwt:Secret`: user-secrets on developer machines, `Jwt__Secret` on servers and
   CI (`docs/CONFIGURATION.md:39`, `:68-72`).
3. Start the API. The value is validated on the way up — empty, whitespace, shorter than 32
   UTF-8 bytes, or matching a known default is **refused at startup**
   (`V.SMART/V.SMART.Shared/Services/StartupConfigurationValidator.cs:73`, `:132`, called from
   `V.SMART/V.SMART.Api/Program.cs:28`). A clean start is therefore positive evidence that the
   new value is well-formed and is not the old one.
4. Accept the forced re-authentication. Task **M2-A04** (refresh tokens and revocation) is
   what makes future rotations non-disruptive; it is not in place yet.

**Rollback.** Redeploy the previous value — but note that the previous value is a *published*
one and the validator will refuse it, which is the intended behaviour. In practice the
rollback is "generate another new secret", not "go back".

**Verification.** §8 items 4–5.

---

## 6. C-5 — the GST e-Invoice / e-Way gateway account

**Owner.** **A different system with a different owner.** Whoever holds the relationship with
the gateway licensing vendor, Bhargavi Soft-Tech Pvt Ltd (named in
`V.SMART/V.SMART.Shared/E_Invoice/LicenseProductKey.cs:101,107`), and the GST portal account
itself. Identify this person explicitly; do not assume it is the DBA.

**Blast radius.** A failed rotation blocks **statutory** e-Invoice and e-Way Bill generation —
a compliance impact, not merely an outage.

> **And an availability impact the task file did not anticipate: a bad licence key kills the
> host process.** `LicenseProductKey.cs:123` and `:138` call `Environment.Exit(1)` when the
> decrypted payload lacks `username`/`password`, and `EinvoiceDatabaseService.cs:182` does the
> same when the key fails validation. This is not an exception a request handler can catch — it
> terminates the whole host, for every user, not just e-Invoicing.
> **Therefore: never test a new licence key first on production.**

**Window.** Coordinate with the vendor's own process; it is external to this application.
Because of the `Environment.Exit` behaviour above, treat every tenant update as a change
requiring a window, however small.

**Procedure.**

1. Identify the gateway account owner and obtain the vendor's credential-reset process. Do
   **not** attempt to recover the current values by decrypting a licence key.
2. Reset the gateway credential through the **provider's own process**.
3. Have the vendor issue a **new `APIEinvoiceLicenseKey`** containing the new username and
   password.
4. For each tenant database, **archive the existing `APIEinvoiceLicenseKey` value** before
   overwriting it. That archived value is the rollback.
5. Apply the new key to **one non-production tenant first** and exercise e-Invoicing. If the
   key is malformed the process exits — which is exactly why this step is not done on
   production.
6. Roll out to the remaining tenants one at a time, confirming the host stays up after each.
7. Generate **one test e-Invoice** through the gateway to prove the new credential
   authenticates.

**Rollback.** Restore the archived `APIEinvoiceLicenseKey` per tenant (step 4), and restart the
host if it exited.

**Verification.** §8 items 6–7.

### 6a. C-7 — the AES key and IV protecting C-5

**This is not a configuration value and cannot be rotated by this project.** It is two string
literals compiled into `V.SMART.Shared`
(`V.SMART/V.SMART.Shared/E_Invoice/LicenseProductKey.cs:28-29`), published in a public
repository. Anyone who has read this repository can decrypt any `APIEinvoiceLicenseKey` they
obtain — including the *new* one issued in §6.

**Owner.** The vendor, Bhargavi Soft-Tech Pvt Ltd — the key is very probably shared across
every deployment of their licensing mechanism, not scoped to this customer.

**Blast radius of rotating it.** Every already-issued product key, for every tenant and
plausibly every other customer of the vendor, stops decrypting the moment a new key ships,
unless every key is re-issued in lock-step.

**Procedure.** Escalate to the vendor. Record the outcome — *re-key now* / *re-key on a
schedule* / *accept the risk, with a named owner and a date* — in §8 item 7. Changing the
literals here would be a source change in `V.SMART.Shared`, which M0-04 forbids, and would
break every existing tenant besides.

**Do not treat §6 as closing this.** Rotating C-5 under a published key restores compliance
capability; it does not restore confidentiality.

---

## 7. C-6 — the seeded default `Administrator` account

**Not this runbook's work.** It is owned by task **M0-06**
(`docs/kb/execution/tasks/M0-06.md`), which removes the seeded credential. It appears in the
inventory (§2) only so that nobody later assumes M0-04 covered it. Take no action on it from
here.

---

## 8. Human verification checklist

Sign each row **when it is genuinely true**, not in advance. Every item is objectively
checkable: *"the old login cannot connect"* is checkable; *"credentials rotated"* is not.

| # | Item | Signed off by (name) | Date | Evidence |
|---|---|---|---|---|
| 1 | The old SQL login (**C-1** — C-2 is void, see §2's correction note) can **no longer authenticate** to its instance — demonstrated by a failed connection attempt, not by inspection. | | | |
| 2 | The new SQL login is **not `sa`** and holds least privilege only (no `sysadmin`, no server-level roles). | | | |
| 3 | **Every row in `Tenants`** whose `ConnectionString` embedded the old login now uses the new one, and the application serves **every** affected tenant. Record the row count. | | | |
| 4 | The old `Jwt:Secret` **no longer validates** a previously issued token (tried, and rejected). | | | |
| 5 | The new `Jwt:Secret` is **at least 32 bytes and cryptographically generated**, is deployed, and the API starts cleanly with it. | | | |
| 6 | The C-5 gateway credential was reset **through the provider's own process**, a new `APIEinvoiceLicenseKey` was applied to every affected tenant, and **one test e-Invoice succeeded**. | | | |
| 7 | A decision on **C-7** (the published AES key and IV) has been made and recorded by the person who owns the licensing mechanism: re-key now / re-key on a schedule / accept the risk. State which. | | | |
| 8 | **Q-19 re-confirmed at rotation time** — repository visibility is still the owner's deliberate, current choice. Check with the unauthenticated REST call in §0, not with `git ls-remote`. | | | |
| 9 | **Rotation date and the name of the person who performed** each database-side and gateway-side change are recorded in this table. | | | |

**Until every applicable row above is signed, R-01 and R-02 remain open.** Writing this
runbook does not close them, and neither does executing part of it.

---

## 9. What this runbook deliberately does not cover

- **M0-06** — the seeded default `Administrator` credential (C-6). Separate task.
- **M0-03** — externalising configuration. Already landed; this runbook depends on its
  mechanism (`docs/CONFIGURATION.md`) rather than duplicating it.
- **M0-05** — purging git history. It **must not run before this checklist is signed**.
  Purging first removes the evidence of what was exposed without removing the exposure. Note
  also §0a: the literals are at `HEAD` in `docs/kb/`, so a history purge alone would not
  remove them.
- **Redacting the knowledge base's own prose** (§0a, **Q-84**) — an owner decision, not this
  task's.
- **Encrypting the `Tenants.ConnectionString` column** — an R-01 action item in its own right.
- **Deciding repository visibility** — already decided (Q-19). This runbook only asks that the
  decision be re-confirmed as still current at rotation time.
