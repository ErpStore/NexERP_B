# Runbook — Capture missing stored procedures from a live tenant database

**Audience:** a DBA (or DBA-equivalent) who has not read the project's migration plan. This
document is self-contained.

**What you're doing:** copying the exact, currently-deployed T-SQL for 82 stored procedures
out of one nominated tenant database and into this git repository, so the application no
longer depends on undocumented database state that exists nowhere in source control.

**Time estimate:** under an hour if the PowerShell path works; longer if you're doing it by
hand in SSMS.

---

## 0. Before you start

- [ ] You have `VIEW DEFINITION` (or `db_owner`) on **one** tenant database you've been told
      to use as the source. If nobody has told you which tenant, stop and ask — see
      *Choosing the tenant*, below, is **not** your call to make alone; the criteria are
      here, but the choice itself must be recorded by whoever asked for this capture.
- [ ] You have a **non-production copy** of that database (or an equivalent low-risk target)
      to rehearse against first. The capture script is marked **UNVERIFIED** — nobody has
      run it against a real SQL Server yet — so the first run should not be against anything
      that matters.
- [ ] You can reach this git repository's working copy from the machine you'll run the script
      on (to read `db/stored-procedures/manifest.csv` and write the output files there).

## 1. The secrets rule — read this before you connect to anything

**Never paste a connection string, a password, or a row from the application's `Tenants`
table into a ticket, a chat message, a log file, or a commit.** The `Tenants` table
(`V.SMART/V.SMART.Shared/Data/TenantInfo.cs`) stores per-tenant connection strings in
**plaintext** — that is a known, tracked risk (R-01) in this project, currently being fixed
separately. Do not add to it. If you need to hand off a connection detail to someone else, use
whatever secure channel your organisation already has for credentials — not this repository,
not this ticket.

The tooling in this runbook is built around that rule:

- **`Export-StoredProcedures.ps1` never writes a connection string, username, or password to
  any file.** You pass them as command-line parameters or build a credential object
  interactively; nothing is logged or persisted by the script.
- **`db/stored-procedures/CAPTURE-STATUS.md` records the tenant by name/id only** — never by
  connection string. See step 5.

## 2. Choosing the tenant

Multi-tenancy in this application is **database-per-tenant** — every tenant has its own full
copy of the schema, and there is no single database that holds "the" procedures for all of
them. Whoever asked for this capture should already have nominated a tenant; if not, the
criterion is:

> Prefer a tenant in **active production use across the widest set of modules**. A
> lightly-used tenant may simply be missing procedures that another tenant's users exercise
> regularly, and a capture from it would look complete while actually being partial.

Whichever tenant is used, whether that choice was representative of every other tenant is a
**separate, later question** (task M0-02, "Q-14" in the project's open-questions log) — you
are not answering it here, and you don't need to.

## 3. Get your bearings — what already exists

From the repository root:

```bash
cat db/stored-procedures/manifest.csv | head -5
tail -n +2 db/stored-procedures/manifest.csv | grep -c ",missing," || true    # your worklist size — expect 82
```

`db/stored-procedures/manifest.csv` is the authoritative worklist. Every row with
`status = missing` is a procedure you need to capture. **Do not add or remove rows from this
file** — if it looks wrong (a name you know doesn't exist, or one you know is missing from
the list), record that as a finding when you hand back, don't edit the file yourself.

Optionally, run `db/tools/list-deployed-procedures.sql` against the source tenant first (paste
it into SSMS, or run it via `Invoke-Sqlcmd`) to see everything that's actually deployed there
before you start capturing — this can surface a name that's spelled slightly differently than
the manifest expects, which is worth flagging rather than silently skipping.

## 4. Run the capture

### Path A — PowerShell (preferred, if available)

```powershell
# From the repository root, in PowerShell:
Import-Module SqlServer   # if this fails, see "If PowerShell isn't available", below

# Windows Authentication (preferred):
.\db\tools\Export-StoredProcedures.ps1 -ServerInstance "your-tenant-sql-server" -Database "YourTenantDb"

# SQL Authentication (only if Windows Auth isn't available):
$cred = Get-Credential   # prompts interactively — never type the password directly in a script or on the command line
.\db\tools\Export-StoredProcedures.ps1 -ServerInstance "your-tenant-sql-server" -Database "YourTenantDb" -Credential $cred
```

**This script has never been run against a real database by the session that wrote it.**
Rehearse it against your non-production copy first (step 0). Read the output carefully — it
reports, per procedure: `[CAPTURED]`, `[NOT FOUND]`, `[MULTIPLE MATCH]`, `[NON-DBO SCHEMA]`, or
`[ERROR]`. None of those outcomes are silent; if something looks wrong, stop and ask rather
than re-running with guesses.

If `Invoke-Sqlcmd` is missing:

```powershell
Install-Module -Name SqlServer -Scope CurrentUser
```

### Path B — manual, via SSMS

If PowerShell or the `SqlServer` module isn't available, do it by hand:

1. Open `db/tools/list-deployed-procedures.sql` in SSMS, connected to the source tenant
   database.
2. Run it. For each row where `ProcedureName` matches a `missing` name from the manifest:
   - Right-click the procedure in Object Explorer → **Script Stored Procedure as** →
     **CREATE OR ALTER To** → **New Query Editor Window**. (If your SSMS version only offers
     "CREATE To", script it that way and manually change the first line from
     `CREATE PROCEDURE` to `CREATE OR ALTER PROCEDURE` — see step 6, this is the **only**
     edit you're allowed to make.)
   - Save the result as `db/stored-procedures/<ProcedureName>.sql`, using the **exact**
     procedure name (case included) as the file name.
   - Save with UTF-8 encoding, **without** a byte-order mark (in SSMS: **File → Save As** →
     the dropdown next to Save → **Save with Encoding** → "Unicode (UTF-8 without signature)
     - Codepage"), and make sure the file uses LF (Unix) line endings, not CRLF — most text
     editors have a "line ending" setting in the status bar or a Save-As option for this.

Either path produces the same thing: one `.sql` file per captured procedure in
`db/stored-procedures/`.

## 5. The one rule that matters most: do not touch the body

**Do not reformat, re-indent, "clean up", or fix anything you notice in a procedure while
capturing it — even something that looks obviously wrong.** These procedures compute figures
that appear on real invoices, e-way bills, and other statutory documents. There is currently
no automated test that would catch a change to their logic. If you see:

- a `SELECT *`,
- what looks like a bug,
- inconsistent formatting or casing,
- a deprecated SQL construct,

**leave it exactly as it is** and note it in `CAPTURE-STATUS.md` (step 6) instead. Someone
will decide, deliberately, whether it's worth fixing — later, with a diff a reviewer can read,
not silently inside a "capture" step.

**The only transformation you are allowed to make** is changing the very first line from
`CREATE PROCEDURE ...` or `ALTER PROCEDURE ...` to `CREATE OR ALTER PROCEDURE ...`, so the
file can be redeployed safely more than once. The PowerShell script does this automatically;
if you're capturing by hand (Path B), do it by hand, and nothing else.

## 6. Record what happened

Open `db/stored-procedures/CAPTURE-STATUS.md` and fill in:

- the **source tenant**, by name or id **only** — never a connection string or server address;
- the **capture date**;
- your name (**operator**);
- a row per procedure: captured / not found / permission denied / multiple objects matched /
  found under a non-`dbo` schema / found but flagged for review (with why).

**If a procedure could not be captured, that is one of the most useful things you can report
— not a failure on your part.** A procedure the application calls that doesn't exist in the
source tenant means either the application has dead code calling a name that was deleted long
ago, or it's a real defect waiting for a user to trigger it. Either way, someone needs to know
— write it down, don't skip it silently.

## 7. Handback checklist

- [ ] Every `missing` row in `db/stored-procedures/manifest.csv` has either a matching `.sql`
      file, or an explicit reason recorded in `CAPTURE-STATUS.md`.
- [ ] `CAPTURE-STATUS.md` is filled in: tenant identifier, date, operator, per-procedure
      outcome. No connection string, password, or IP address appears anywhere.
- [ ] You have not edited `db/stored-procedures/manifest.csv` or
      `db/stored-procedures/README.md`.
- [ ] You have not touched anything under `V.SMART/` or
      `Existing Store Procedures/StoredProcedures/`.
- [ ] Run the verification harness and paste its full output into the task ticket:
      ```bash
      bash db/tools/verify-capture.sh
      ```
      It must exit with `PASS` (warnings are fine and worth reading; a `FAIL` means something
      above needs fixing before handback — the script tells you exactly which file and why).
- [ ] Commit the new `.sql` files and the filled-in `CAPTURE-STATUS.md` on the branch named in
      the task ticket. Do not push to `master`.

That's the whole task. Thank you.
