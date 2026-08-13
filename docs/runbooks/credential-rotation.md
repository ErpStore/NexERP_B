# Credential Rotation Runbook

**Task:** [M0-04](../kb/execution/tasks/M0-04.md). **Produced:** 2026-08-13.

This is an operations document, not a knowledge-base document — it carries no `doc_id` and is
not registered in `docs/kb/INDEX.md`.

## Read this first

This runbook was written by an AI session with **no production access, no database
credentials, and no ability to reset any external account.** It cannot rotate anything itself.
What follows is the complete, executable procedure for a human who does have that access. If
you are that human, work through the sections in order — **C-1/C-2 and C-3 must happen
together, in the same maintenance window** (see below); the others are independent.

**Status: `Blocked`.** No credential has been rotated as of this writing. Every checkbox in
the *Verification checklist* is unchecked. Do not read the existence of this document as
"done" — writing the runbook is the repository-side half of M0-04; rotating is the other half,
and only a human can do it.

## Top-priority context: this is a live-exposure rotation, not a precautionary one

`Q-19` (repository public visibility) is **answered**: the repository owner deliberately made
`ErpStore/NexERP_B` public on 2026-08-12, after a corrected investigation (INV-034) confirmed
it had briefly been private. As of that decision, **every credential in this document must be
treated as already harvested** — see
[`docs/kb/open-questions.md` Q-19](../kb/open-questions.md) and
[KB-060 R-01](../kb/risks/technical-debt-register.md#r-01--live-database-credentials-committed-to-source-control)
for the full evidence chain. Rotation is not "before it leaks" — it is incident response for a
leak that has already happened, with an unknown-length window during which the repository was
also briefly *private but still cloned* by anyone who accessed it before that.

**The C-1 SA password and the C-2 production password are not confined to the four source
files listed below.** They were also found in plaintext inside `docs/kb/risks/technical-debt-register.md`
(now redacted — see R-01's note there) and inside four *other* task specification files
(`docs/kb/execution/tasks/M0-03-01.md`, `M0-03-02.md`, `M0-05.md`, `M0-04.md`), each as a
`git grep -l "<value>" HEAD` verification-command argument, not yet redacted (see R-01's note
for why, and the follow-up handed to M0-05). **Rotating the database logins does not make
these old values harmless to leave lying around** — treat "redact every KB document" as part
of this rotation's completeness, not a separate task, even though this runbook does not do
that rewrite itself.

## Credential inventory

Every credential below is classified **Confirmed** / **Inferred** / **Unknown**. Do not treat
an `Unknown` row as "probably fine" — it is a genuine gap this runbook could not close from
the repository alone, and a human must close it before declaring rotation complete.

| # | Credential | Consumed from | Confidence |
|---|---|---|---|
| **C-1** | SQL login `sa` on the local/dev `DESKTOP-R60MNGC\SQLEXPRESS` instance | `V.SMART/V.SMART.Web/appsettings.json:11`; `V.SMART/V.SMART.Shared/Data/MigrationData/ApplicationDbContextFactory.cs:13`; `V.SMART/V.SMART.Shared/Data/MigrationData/MasterDbContextFactory.cs:12`; `V.SMART/V.SMART/MauiProgram.cs:231`. Also expected at `V.SMART/V.SMART.Api/appsettings.json` — **Unknown in this session**: that file does not exist in this checkout (M0-00 deferred committing `V.SMART.Api/` entirely to M0-03-01; verify its content when it is added) | Confirmed (re-verified 2026-08-13, this session, for the four cited files) |
| **C-2** | SQL login on production host `154.61.76.112,1533` (databases `IQSmartDb_Master`, `IQSMARTDEMO_DB_2025-26`) | commented-out at `V.SMART/V.SMART.Web/appsettings.json:10`; `.../ApplicationDbContextFactory.cs:14`; `.../MasterDbContextFactory.cs:11`; `V.SMART/V.SMART/MauiProgram.cs:228`. **A third host** is also exposed, commented, same login shape: `VK-7-HP\SQLEXPRESS` at `MauiProgram.cs:235` — rotate/remove that reference too if it points at a real reachable instance | Confirmed (re-verified 2026-08-13) |
| **C-3** | **Per-tenant connection strings in the `Tenants` table** — plaintext, in the master database, **not in any file** | `V.SMART/V.SMART.Shared/Data/TenantInfo.cs:3-8` (`TenantInfo { Id, Name, Hostname, ConnectionString }`); consumed at `V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantDbContextFactory.cs:14-26` (60-second command timeout at `:22`) | Confirmed (mechanism, re-verified 2026-08-13). **Unknown**: how many rows, which logins they embed — requires database access this session does not have |
| **C-4** | `Jwt:Secret` — the API's token signing key | Per the prior investigation (INV-029, 2026-08-12; **not independently re-verified this session** — `V.SMART/V.SMART.Api/` does not exist in this checkout, deferred by M0-00 to M0-03-01): `V.SMART/V.SMART.Api/appsettings.json:12` (`ExpiresMinutes: 480` at `:15`), read at `Program.cs:56` and by `JwtTokenService` at `:103`, validated at `:60-74` | Confirmed by INV-029, carried forward without re-verification (see gap above) |
| **C-5** | GST e-Invoice / e-Way **gateway** account (username + password) | commented literals at `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/EInvoiceAPIService/EinvoiceDatabaseService.cs:1413-1414` and `EWayDatabaseService.cs:900-901`. **Runtime source traced this session:** the actual credential is decrypted at request time from `Companies.APIEinvoiceLicenseKey` (per tenant) — see **C-7** below, which is the more urgent half of this rotation | Confirmed (both the commented literals and the runtime path, traced 2026-08-13) |
| **C-6** | Seeded default `Administrator` account (fixed PBKDF2 hash, every tenant database) | `V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs:1136` (per [KB-060 R-09](../kb/risks/technical-debt-register.md)) | Confirmed per KB-060. **Owned by [M0-06](../kb/execution/tasks/M0-06.md), not this task.** Listed so nobody assumes M0-04 covered it — do not act on it here |
| **C-7** | **New finding, 2026-08-13.** Hardcoded AES key + IV that decrypts every tenant's e-Invoice/e-Way gateway license key | `V.SMART/V.SMART.Shared/E_Invoice/LicenseProductKey.cs:28-29` (the key/IV themselves — values withheld from this document, same rule as C-1..C-4); reads the encrypted blob from `Companies.APIEinvoiceLicenseKey` (`V.SMART/V.SMART.Shared/Data/Master/Company_Module/Companydetails.cs:204`) via `EinvoiceDatabaseService.cs:147-164` / `EWayDatabaseService.cs:85`; decrypts `username`/`password` at `LicenseProductKey.cs:113-138` | Confirmed (traced end-to-end this session). Recorded in full as [KB-060 R-39](../kb/risks/technical-debt-register.md#r-39--hardcoded-aes-key-decrypts-the-e-invoicee-way-gateway-license-and-rotating-the-gateway-password-alone-does-not-close-it) — **read that entry before rotating C-5**, because resetting the gateway password alone does not close this exposure |

**Not-yet-answered locations, per the task's own requirement to record what could not be
checked** (Confirmed absent from this repository check; **Unknown** whether they exist
outside it):
- CI/CD pipeline variables — no CI exists yet (INV-023, Confirmed: `.github/workflows/` is
  empty). Nothing to check here until M0-07 creates a workflow.
- Deployment scripts, IIS/app-service configuration, `.pubxml`/publish profiles — none found
  under `V.SMART/` (a repository grep found no `.pubxml` files and no IIS `web.config`
  connection strings). **Unknown** whether these exist on the actual deployment host(s),
  which this session cannot reach.
- Developer machines' local `dotnet user-secrets` stores — **Unknown**, by definition outside
  the repository.
- SQL Agent jobs — **Unknown** (this is [Q-15](../kb/open-questions.md), already open and
  unanswered; not re-derived here).
- Monitoring agents, backup/restore scripts — **Unknown**, not found in this repository, not
  reachable from this session.
- SMTP / mail, biometric, and other integration credentials — grepped for hardcoded
  `Password=`/`ApiKey=`/`Secret=`-shaped literals across `.cs`/`.json` outside the six known
  files; **negative result**, none found. The one other external HTTP integration checked
  (`BankService.cs:501`, IFSC lookups via `https://ifsc.razorpay.com`) is a public, keyless
  API — not a credential.

## Rotation procedure

### C-1 and C-2 — Database logins (local `sa`, production `bspl`-equivalent, and the third host at `MauiProgram.cs:235`)

- **Owner:** a person with `sysadmin` (or equivalent) on the affected SQL Server instance(s).
- **Blast radius:** every tenant, for the duration between disabling the old login and every
  consumer having the new one — **if step 3 (below) is skipped or done out of order, every
  tenant goes offline**, because `TenantDbContextFactory.CreateDbContext()` builds every
  tenant's connection from a string stored in the `Tenants` table (C-3), not from these files.
  These files' credentials are the **master/local** connection; `Tenants` rows may embed the
  *same* login, in which case they break too.
- **Window:** required. Rotating a login used by any live connection string forces a brief
  interruption while the new value propagates.
- **Procedure:**
  1. Create a **new, least-privilege** SQL login **alongside** the old one. Do not reuse `sa`
     — grant only what the application's EF Core access pattern needs (read/write on
     application tables; no `sysadmin`, no cross-database access beyond what's actually used).
  2. Deploy the new value to every consumer. If [M0-03](../kb/execution/tasks/M0-03.md) has
     landed, use its environment-variable / user-secrets mechanism. If it has not yet landed,
     deploy directly and **record that as an interim measure** in the checklist below — do not
     silently treat it as done-forever.
  3. **Update every row in the master database's `Tenants` table whose `ConnectionString`
     embeds the old login (C-3).** This is the step most likely to be forgotten, and it is the
     one that takes every tenant down if missed. Query first, change nothing, to see the scope:
     `SELECT Id, Name, Hostname FROM Tenants WHERE ConnectionString LIKE '%<old-login-name>%'`
     (do not paste the query's *output* — the connection strings — into a ticket, chat, or log;
     see *Secrets rule* below).
  4. **Verify the application works on the new login before touching the old one.** Start the
     Blazor host (or hit a health endpoint once one exists) against at least one tenant whose
     row was updated in step 3.
  5. Only then **disable** the old login (do not drop it yet — see rollback).
  6. Separately, once confident, drop the disabled login.
- **Rollback:** re-enable the old login. Because step 5 disabled rather than dropped it, the
  old connection strings — including any `Tenants` row you have not yet updated — immediately
  work again.
- **Verification:** the old login can no longer authenticate
  (`sqlcmd -S <host> -U <old-login> -P <old-password>` fails); the new login is confirmed
  **not** `sa` and has only the grants listed in step 1; every row in `Tenants` was checked and,
  where it embedded the old login, updated; the application serves every tenant that was
  affected.

### C-3 — Per-tenant connection strings in the `Tenants` table

Not a separate rotation — this is **step 3 of the C-1/C-2 procedure above**, listed here only
so it has its own inventory row and cannot be missed. There is no independent "C-3 rotation";
there is only "did you update `Tenants` when you rotated C-1/C-2."

**Longer-term action, outside this runbook's scope but worth recording:** the
`ConnectionString` column is plaintext. [KB-060 R-01](../kb/risks/technical-debt-register.md)'s
action item recommends encrypting it. That is a schema/application change, not a rotation, and
is not this task.

### C-4 — `Jwt:Secret`

- **Owner:** whoever has write access to the API's configuration/secrets store.
- **Blast radius:** every currently-issued JWT is invalidated **the moment** the new secret is
  deployed (tokens live up to 480 minutes / 8 hours per `ExpiresMinutes`, but validation fails
  instantly on the new secret, not gradually). With only 2 controllers / 6 endpoints live
  today (per the prior API-surface investigation), the blast radius is small **now** — that is
  the argument for rotating before M2 puts real traffic on the API, not a reason to delay.
- **Window:** not strictly required (no maintenance window needed — just accept that active
  API sessions re-authenticate), but coordinate with anyone actively testing against the API.
- **Procedure:**
  1. Generate a new value of **at least 32 bytes** from a cryptographic random source (e.g.
     `openssl rand -base64 32`, or the equivalent in your platform's secret-generation tool).
     Never generate it by editing this file or any committed file.
  2. Deploy it via the same mechanism M0-03 establishes for other secrets (environment
     variable / user-secrets / Key Vault). If M0-03 has not landed, deploy directly and record
     that as interim.
  3. Restart the API host so the new value takes effect.
- **Rollback:** redeploy the previous secret value. (Keep a copy in your organisation's actual
  secret manager, never in this repository, if you may need to roll back.)
- **Verification:** a token issued with the *old* secret no longer validates (`401` on any
  protected endpoint); a token issued with the *new* secret works; the new secret is
  confirmed at least 32 bytes and was not typed by hand.
- **Forward note:** [M0-03-03](../kb/execution/tasks/M0-03-03.md) will make the application
  **fail startup** if the JWT secret is missing or is the known default value — so a future
  redeploy with a weak or default secret will be caught by the application itself, not just by
  this runbook.

### C-5 and C-7 — GST e-Invoice / e-Way gateway credential and the AES key that protects it

**Read [KB-060 R-39](../kb/risks/technical-debt-register.md#r-39--hardcoded-aes-key-decrypts-the-e-invoicee-way-gateway-license-and-rotating-the-gateway-password-alone-does-not-close-it)
in full before doing anything here.** These two are listed together because rotating C-5
alone (the gateway password) is **not sufficient** — the AES key protecting it at rest (C-7)
is also compromised, and a new password re-encrypted with the same compromised key is exposed
again the instant it is saved.

- **Owner:** whoever holds the GST e-Invoice/e-Way gateway provider account (**a different
  system and, per the original task's own framing, probably a different owner** from the
  database credentials above) for C-5; whoever owns the `LicenseProductKey.cs` code path — a
  developer with deploy access — for C-7.
- **Blast radius:** a failed C-5 rotation blocks **statutory e-Invoice / e-Way Bill
  generation** — a compliance impact, not just an outage. C-7's fix (re-encrypting every
  tenant's stored license key) risks locking out e-Invoice generation for any tenant whose row
  is mishandled during re-encryption; script and test it, do not hand-edit database rows.
- **Window:** required for C-7 (a per-tenant data migration); C-5's own reset follows the
  gateway provider's process and timing.
- **Procedure:**
  1. **C-5:** Follow the gateway provider's own credential-reset process (outside this
     repository's knowledge — the provider, not this codebase, defines that process). Obtain
     the new username/password.
  2. **C-7, before deploying the new C-5 credential:** generate a new AES key and IV from a
     cryptographic random source. Store them via the application's secrets mechanism (M0-03),
     never committed.
  3. Update `LicenseProductKey.Decrypt` (and the corresponding encryption path — check for
     wherever `Companies.APIEinvoiceLicenseKey` is *written*, which was not located by this
     session's repository sweep and should be confirmed by whoever implements this) to use the
     new key/IV from configuration instead of the hardcoded constants.
  4. Re-encrypt **every tenant's** `Companies.APIEinvoiceLicenseKey` value with the new key,
     embedding the new C-5 credential from step 1. Do this via a script against a
     non-production copy first; verify decryption round-trips correctly before touching
     production tenant data.
  5. Deploy the code change (new key sourced from config) and the re-encrypted data together —
     old ciphertext will not decrypt under the new key, so these cannot be split across a
     window.
  6. Test one e-Invoice generation end-to-end against a real (or sandbox) gateway account
     before considering this done.
- **Rollback:** keep the old AES key and the pre-migration `Companies.APIEinvoiceLicenseKey`
  values backed up before step 4. If the new key/re-encryption has a defect, restore the
  column values and redeploy the previous code (old hardcoded key) as a stopgap — accepting
  that this reintroduces the R-39 exposure — while the new implementation is fixed offline.
- **Verification:** a test e-Invoice generation succeeds using the new gateway credential
  (C-5); the old gateway credential (if the provider supports checking this) no longer
  authenticates; `LicenseProductKey.cs` no longer contains a hardcoded key/IV
  (`git grep` for the old committed values, post-fix, returns nothing in the *new* commits —
  they remain in history until a separate history-purge decision is made, which is outside
  this runbook); every tenant's `APIEinvoiceLicenseKey` was re-encrypted and round-trips.

### C-6 — Seeded default Administrator account

**Not this task.** Owned by [M0-06](../kb/execution/tasks/M0-06.md). Listed in the inventory
above only so nobody reading this runbook assumes it was covered here.

## Secrets rule for everyone executing this runbook

**Never paste a connection string, a password, a `Tenants` table row, or an AES key value
into a ticket, a chat message, a log file, a commit, or this document.** This repository is
public; anything committed here is exposed the same way the original credentials were. Refer
to every secret by its file:line location and by name — e.g. "the SQL login at
`V.SMART/V.SMART.Web/appsettings.json:11`" — never by value.

## Verification checklist

Objectively checkable. Fill in a name and a date for each row as it is completed. An unchecked
row means that part of the rotation has not happened, regardless of what any other document
says.

| # | Item | Done by (name) | Date |
|---|---|---|---|
| 1 | The old SQL login (C-1) can no longer authenticate to the local/dev instance. | | |
| 2 | The old SQL login (C-2) can no longer authenticate to the production host `154.61.76.112,1533`. | | |
| 3 | The third exposed host reference (`VK-7-HP\SQLEXPRESS`, `MauiProgram.cs:235`) was checked — confirmed either decommissioned or also rotated. | | |
| 4 | The new SQL login(s) are **not** `sa` and have least privilege. | | |
| 5 | Every row in `Tenants` that embedded an old login now uses the new one; the application serves every affected tenant. | | |
| 6 | The old `Jwt:Secret` (C-4) no longer validates a token. | | |
| 7 | The new `Jwt:Secret` is at least 32 bytes and was cryptographically generated. | | |
| 8 | The GST gateway credential (C-5) was reset through the provider's process, and one test e-Invoice succeeded. | | |
| 9 | The hardcoded AES key/IV (C-7) has been replaced with a config-sourced value, and every tenant's `Companies.APIEinvoiceLicenseKey` was re-encrypted under it. | | |
| 10 | Q-19 (repository visibility) — **already answered**: public, by deliberate owner decision, 2026-08-12. No further action needed here; listed for completeness. | Kumar | 2026-08-12 |
| 11 | Rotation date(s) and the name(s) of every person who performed a step are recorded above, per-row. | | |

**This task's status remains `Blocked` until every row above (except #10, already satisfied)
is checked.** Do not report or record M0-04 as `Completed` on the strength of this document
existing.
