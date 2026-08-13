# Capture status — stored procedures (task M0-01-02)

This file records **who captured what, from where, and when** — filled in by the DBA
performing the capture (see `db/RUNBOOK-capture-stored-procedures.md`), then reconciled
by the AI session that runs `db/tools/verify-capture.sh` afterward.

**Never put a connection string, hostname, IP address, username, or password in this
file.** Identify the tenant by name or ID only — see the runbook, §7.

---

## Capture record

| Field | Value |
|---|---|
| Source tenant (name or ID — **never** a connection string) | TBD |
| SQL Server instance (hostname/instance label only, no credential) | TBD |
| Capture date | TBD |
| Operator (name of the person who ran the capture) | TBD |
| Tool used | TBD — `db/tools/Export-StoredProcedures.ps1` or the manual SSMS fallback (runbook §6) |
| Rehearsed against a non-production copy first? | TBD (yes/no — should be yes) |

---

## Per-procedure outcome

One row per `missing` name in `db/stored-procedures/manifest.csv` (82 as of this
writing — confirm the current count with
`tail -n +2 db/stored-procedures/manifest.csv | grep -c ",missing,"` before treating 82
as fixed). Fill in every row; do not leave a name unmentioned.

Outcome values: `captured` · `not found` · `permission denied` · `encrypted` ·
`multiple objects matched` · `other (explain)`.

| Procedure name | Outcome | Owner (if not simply captured) | Notes |
|---|---|---|---|
| _(TBD — populate from the capture run's summary table)_ | | | |

> Half A of this task (tooling) does not know which procedures will fail to capture —
> that can only be known after the capture is actually attempted. This table is
> intentionally empty except for its structure until Half B/C fills it in.

---

## Findings requiring a human decision

Anything found during capture that is **not** a simple "captured successfully" goes
here, in addition to the per-procedure table above — especially:

- A procedure genuinely absent from the source tenant. This is one of the two most
  valuable outcomes of the whole task: either the name is dead code that should be
  removed from the application, or it is a real latent defect (a screen that will throw
  the moment a user opens it). Both need a human decision — record it here, don't
  resolve it yourself.
- A procedure found under a schema other than `dbo`. The application always executes
  `dbo.{procedureName}`
  (`V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/ReportExecutor.cs:25,27,35-39,105-107`),
  so this is very likely a live defect, not a false alarm.
- A procedure that exists under more than one schema (ambiguous — do not guess which
  one the application means).
- Anything about a procedure's body that looked like a bug, a dead branch, or an
  apparent duplicate of another procedure. The capture step must not fix or merge
  these — only record them.

| Finding | Procedure(s) | Recorded by | Date | Status |
|---|---|---|---|---|
| **No populated tenant database is currently known to be accessible.** A rehearsal capture attempt was made against the only local database the repo owner had running — `NexGenErpDb` on `DESKTOP-FIIBE97\SQLEXPRESS` (Windows Authentication). The connection succeeded, but `db/tools/list-deployed-procedures.sql` returned **zero rows** — this database has **no `Sp_*` procedures deployed at all**, not even the 13 already scripted in `Existing Store Procedures/StoredProcedures/`. No `.bak`, `.dacpac`, or seed/deploy script exists anywhere in the repo that would install procedures onto a fresh database (confirmed by repo search), so this is consistent with a database created from EF schema alone, never seeded with procedures by any process this repo controls. The repo owner does not currently know of any other database — local, remote, demo, or backup — that has been used to actually run this application. **Until a populated source is located, this task cannot proceed past tooling rehearsal**, and per Q-02 (open-questions.md), how a tenant database is normally brought to a working state (migrations + procedures) remains unknown. This also means the 13 already-scripted procedures currently have no known live counterpart to validate against, and if no other environment exists, some or all of the 82 `missing` procedures' original DDL may not exist anywhere the project currently has access to. | All 82 `missing` rows (manifest.csv), and by extension the 13 already-scripted procedures' currency is unverified too | AI session (M0-01-02), per repo owner's direct confirmation | 2026-08-13 | **Escalated — needs a human decision**: locate a populated database (old backups, a colleague/former developer, a hosting/demo account, a different machine) or accept that this logic may need to be reconstructed rather than captured. |

---

## Verification

Run from the repository root once files are delivered:

```bash
bash db/tools/verify-capture.sh
```

Paste its full output below (or attach it to the task ticket) once it has been run
against the delivered capture.

```
(not yet run — no .sql files have been delivered as of this skeleton's creation)
```
