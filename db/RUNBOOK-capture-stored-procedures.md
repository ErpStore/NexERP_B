# Runbook — capturing the missing stored procedures (task M0-01-02)

**Audience:** a DBA (or anyone with `VIEW DEFINITION` on a tenant database) who has
not read the migration plan and does not need to. This document is self-contained.

**What you're doing, in one sentence:** copying the text of 82 stored procedures out of
one live tenant database and into 82 plain `.sql` files in this repository, unchanged,
so the ERP's reporting logic finally has a backup that isn't "only inside one server."

**Time:** should be well under an hour of your time once you know which tenant to use —
the tooling does the mechanical work. Finding out *which* tenant may take longer if
nobody has already decided.

---

## 0. Why this matters (skip if you just want the steps)

Right now, 82 of the 94 stored procedures this application calls exist **only inside
tenant databases** — nobody has a copy anywhere else. If the server holding a tenant's
database were lost, those procedures would be gone, along with the ability to print
invoices, e-way bills, and every analytical report in the system for that tenant. This
task fixes that for one tenant, by capturing exact copies into source control.

**You are not changing anything.** Nothing you do here modifies the database, the
application, or any procedure's behavior. You are only *reading* and *copying*.

---

## 1. Preconditions — check these before you start

- [ ] You have `VIEW DEFINITION` (or `db_owner`) on the tenant database you'll capture
      from. If you're not sure, ask whoever manages SQL Server logins.
- [ ] **You have a non-production copy of a tenant database to rehearse against
      first.** Do not run this against production on the first attempt. If you don't
      have a non-production copy available, get one before continuing — a restored
      backup onto a scratch instance is enough.
- [ ] You know which SQL Server instance and which specific tenant database you're
      pointed at. See §3 for how to pick one if nobody has decided yet.
- [ ] PowerShell is available (Windows PowerShell 5.1+ or PowerShell 7+).
- [ ] Check whether the `SqlServer` PowerShell module is installed:
      ```powershell
      Get-Module -ListAvailable -Name SqlServer
      ```
      If nothing is listed and you can install modules (needs internet access):
      ```powershell
      Install-Module -Name SqlServer -Scope CurrentUser
      ```
      **If you cannot install it** (no internet access, no permission), skip to
      **§6 — Manual fallback (no PowerShell module)** below. That path uses only
      SSMS / Azure Data Studio.
- [ ] You have a working copy of this repository, with this branch checked out:
      `migration/M0-01-02-script-missing-procedures`.

**The script you're about to run, `db/tools/Export-StoredProcedures.ps1`, has never
been executed against a real database by anyone as of this writing.** It was written
by an AI session with no database access at all, precisely so it could not touch a
real database while writing it. That is why the rehearsal step above is not optional —
you are the first person to actually run it, and you should verify it does what it
claims before trusting it against production.

---

## 2. The worklist — where it comes from, and what you must not touch

The list of what to capture is `db/stored-procedures/manifest.csv`, produced by an
earlier task (M0-01-01). It has one row per stored procedure the application actually
calls, with a `status` column. **You only need the rows where `status` is
`missing`** — 82 of them as of this writing. The script reads this file itself; you
don't need to copy names out by hand.

**Do not edit `manifest.csv` or `db/stored-procedures/README.md`.** They are inputs
you're being checked against, not files this task changes. If you think a name in the
manifest is wrong, say so in the task ticket — don't fix it yourself.

---

## 3. Choosing the tenant — criteria, not a name

Nobody can tell you *which* tenant to use in the abstract — that choice needs to be
made by someone who knows the tenants, and it needs to be **recorded**, because a
later task (M0-02) will check whether the choice was representative of all tenants.

When choosing:

- **Prefer a tenant in active production use across the widest set of modules.** A
  tenant that only uses, say, Sales and never touches Production or HR may simply be
  missing procedures that another tenant needs — picking it would make this capture
  look more complete than it is.
- **Record the tenant by name or ID, never by connection string**, in
  `db/stored-procedures/CAPTURE-STATUS.md` (§5 below tells you exactly where).
- Understand that this is **one tenant's copy, not a guarantee that every tenant's
  procedures are identical.** Whether they drift is a separate, already-tracked
  question (Q-14 in the knowledge base) — you are not answering it here, just
  supplying its input.

If you don't know how to find the list of tenants, ask whoever manages the
`Tenants` table (see §7, "A note on the Tenants table" below, for what that table is
and why you should never paste from it).

---

## 4. Running the capture

From a PowerShell prompt, with this repository as your working directory (or adjust
the paths below to point at the repo root):

**Rehearsal (non-production tenant copy) — do this first:**

```powershell
.\db\tools\Export-StoredProcedures.ps1 `
    -ServerInstance "YOUR-NONPROD-SQLSERVER\INSTANCE" `
    -Database "YourNonProdTenantDbName" `
    -DryRun
```

`-DryRun` queries the database and reports what it would do, but writes no files.
Review the summary table it prints. If everything you expected shows `captured`, drop
`-DryRun` and run it again for real against the same non-production database, then
open a few of the resulting `.sql` files and compare them by hand against what SSMS
shows for the same procedure ("Script Procedure as → CREATE To → New Query Editor
Window"). They should be identical except for the leading keyword (see §7).

**Real capture, once the rehearsal looks right:**

```powershell
.\db\tools\Export-StoredProcedures.ps1 `
    -ServerInstance "YOUR-PROD-SQLSERVER\INSTANCE" `
    -Database "TheNominatedTenantDbName"
```

This uses **Windows Authentication by default** — no password needed if your Windows
account has access. If you must use SQL authentication instead, see the two supported
options below; **do not** type a password directly into a command that PowerShell will
keep in its history.

**SQL authentication, option A — credential prompt (nothing echoed, nothing saved):**

```powershell
$cred = Get-Credential   # prompts interactively; not logged anywhere
.\db\tools\Export-StoredProcedures.ps1 -ServerInstance "..." -Database "..." -Credential $cred
```

**SQL authentication, option B — password from an environment variable you set
yourself, outside of PowerShell history:**

```powershell
$env:SP_CAPTURE_PASSWORD = "..."   # set this in your own terminal session, not committed anywhere
.\db\tools\Export-StoredProcedures.ps1 -ServerInstance "..." -Database "..." -SqlUserName "your_login" -SqlPasswordEnvVar "SP_CAPTURE_PASSWORD"
```

The script never writes a connection string, username, or password to any file it
produces, and never prints one to the console. Close your terminal / clear
`$env:SP_CAPTURE_PASSWORD` when you're done either way.

It is **safe to re-run** the script if it's interrupted, or if you need to add a
procedure later — it overwrites cleanly per-procedure and does not require starting
over.

---

## 5. Reading the results and finishing up

The script prints a per-procedure status line as it runs, and a summary table at the
end. Possible statuses:

| Status | What it means | What you do |
|---|---|---|
| `captured` | Written to `db/stored-procedures/<Name>.sql`. | Nothing — this is the success case. |
| `not found` | No procedure by that name exists in this database. | Record it in `CAPTURE-STATUS.md` (see below). This is a genuinely useful finding — don't just shrug it off. |
| `permission denied (or encrypted)` | Either your login lacks `VIEW DEFINITION` on that specific object, or the procedure was created `WITH ENCRYPTION`. | Ask for the right permission and re-run, or, if it's genuinely encrypted, escalate — nobody else can read it either without decrypting it first, which is out of scope here. Record it. |
| `multiple objects matched (...)` | The name exists under more than one schema. | Do **not** guess which one is right. Record it and flag it in the task ticket — a human needs to decide. |
| `FAILED - truncation suspected (...)` | The tool detected the definition it received doesn't match the length the server reports — almost certainly a transfer/config issue, not a data issue. | Re-run just that one; if it keeps happening, capture that single procedure manually via §6's SSMS method instead. **Do not** hand in a file for this procedure that came from this failed attempt. |
| `FAILED - unrecognized leading statement` | The captured text didn't start with a recognizable `CREATE`/`ALTER PROCEDURE`. | Inspect it by hand; something unusual is going on with that object. Escalate rather than guess. |
| `query error (...)` | A connection or SQL error while looking it up. | Check connectivity/permissions and retry. |

**Every procedure that is not `captured` needs a line in
`db/stored-procedures/CAPTURE-STATUS.md`** — the file already has a table for this. Fill
in the tenant, date, your name, and the per-procedure outcome. Don't leave a gap
silently unexplained; an unrecorded gap is exactly what this whole task exists to
prevent.

---

## 6. Manual fallback (no PowerShell module)

If `Invoke-Sqlcmd` / the `SqlServer` module genuinely isn't available to you:

1. Open SSMS (or Azure Data Studio) and connect to the nominated tenant database.
2. Open `db/tools/list-deployed-procedures.sql` and run it. It lists every `Sp_*`
   procedure with its schema, timestamps, and a definition fingerprint. Cross-reference
   this against the `missing` rows in `db/stored-procedures/manifest.csv` to see what
   you need.
3. For each `missing` procedure that appears in that list: right-click it in Object
   Explorer → **Script Stored Procedure as → CREATE To → New Query Editor Window**.
4. In the new query window, change the very first line from `CREATE PROCEDURE` (or
   `ALTER PROCEDURE`) to `CREATE OR ALTER PROCEDURE`. **Change nothing else** — see the
   bolded rule in §7.
5. Save the file as `db/stored-procedures/<ExactProcedureName>.sql`. In your editor,
   make sure to save as **UTF-8 without a byte-order mark**, with **LF line endings**
   (in VS Code: bottom status bar → click the encoding, choose "Save with Encoding" →
   "UTF-8"; click the line-ending indicator, choose "LF"). SSMS's default save
   encoding is usually UTF-8 **with** a BOM — check this, it's the single most common
   way this step goes wrong.
6. Repeat for every `missing` procedure you can find. For any name that never appears
   in the SSMS object list at all, that's a `not found` — record it in
   `CAPTURE-STATUS.md` exactly as you would from the script path.

---

## 7. Rules that bind everything you do in this task

> **Do not modify the procedure body in any way, other than the one change described
> below.** Not reformatting, not re-indenting, not fixing a bug you notice, not adding
> `SET NOCOUNT ON`, not touching `ANSI_NULLS`/`QUOTED_IDENTIFIER`, not changing a
> parameter's default value. These procedures compute real report figures and populate
> real statutory documents (invoices, e-way bills, delivery challans). If you change
> what one of them does while "just capturing" it, that change goes live invisibly, the
> next time the deployment step (a later task) runs it — and there is no automated test
> anywhere in this codebase that would catch it.
>
> **The only permitted change:** the leading `CREATE PROCEDURE` or `ALTER PROCEDURE`
> becomes `CREATE OR ALTER PROCEDURE`, so the file can be redeployed safely later
> without needing to know whether the procedure already exists. That's it.
>
> If you notice something odd while capturing — the body looks buggy, two procedures
> look suspiciously similar, the object lives under a schema other than `dbo` — **write
> it down in `CAPTURE-STATUS.md` or the task ticket. Do not fix it. Do not merge it with
> another procedure that looks the same.** Someone with the full picture needs to decide
> what to do with that observation; capturing is not the moment to act on it.

**Secrets — this is not optional either:**

- Never put a connection string, server name, hostname, IP address, username, or
  password into any `.sql` file in `db/stored-procedures/`, or into
  `CAPTURE-STATUS.md`, or into a commit message, or into the task ticket.
- **A note on the `Tenants` table:** the application's tenant directory
  (`V.SMART.Shared/Data/MasterDbContext.cs`) stores one row per tenant with columns
  `{ Id, Name, Hostname, ConnectionString }`. That `ConnectionString` column is stored
  **in plaintext** — this is a known, already-tracked risk (`R-01` in the technical
  debt register), not something you're expected to fix here. But it means: **never
  paste a row from the `Tenants` table into a ticket, a chat message, a log file, or a
  commit** — even to "show which tenant you picked." Record the tenant by its `Name`
  or `Id` only, as `CAPTURE-STATUS.md` asks for.
- Before you consider yourself done, run the verification harness (next section) — it
  includes an automated scan for exactly these patterns.

**Do not touch anything outside `db/stored-procedures/*.sql` and
`db/stored-procedures/CAPTURE-STATUS.md`.** In particular, do not move or edit anything
in `Existing Store Procedures/StoredProcedures/` (a different, later task relocates
those), and do not modify anything under `V.SMART/`.

---

## 8. Handback checklist

Before you consider this done and hand it back:

- [ ] Every `missing` row in `db/stored-procedures/manifest.csv` either has a matching
      `.sql` file, or a recorded, named reason in `CAPTURE-STATUS.md` for why not.
- [ ] `CAPTURE-STATUS.md` has the tenant name/ID, the server, the capture date, and
      your name filled in — no `TBD` left where a real value belongs.
- [ ] You did not paste a connection string or a `Tenants` table row anywhere.
- [ ] You did not modify any procedure's logic — only the leading keyword.
- [ ] Run this from the repository root and **paste its full output into the task
      ticket**:
      ```bash
      bash db/tools/verify-capture.sh
      ```
      It needs no database connection and takes a minute or two. If it reports any
      `FAIL` lines, read them — most are self-explanatory (wrong encoding, wrong line
      endings, a name mismatch) and fixable by re-saving the file correctly. If you're
      not sure how to fix one, hand it back with the failing output attached rather
      than guessing.
