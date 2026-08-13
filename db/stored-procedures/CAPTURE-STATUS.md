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
| _(none yet — populated during Half B/C)_ | | | | |

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
