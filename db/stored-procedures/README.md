# db/stored-procedures/

Captured DDL for the stored procedures the V.SMART / NexGen ERP application calls via
`ReportExecutor` (analytical reports) and `ReportService` (printed FastReport documents).
See `docs/kb/architecture/stored-procedure-inventory.md` (doc_id `KB-102`) for the full
reconciliation this directory's contents are checked against, and
`docs/kb/execution/tasks/M0-01-02.md` for how the files in here are captured.

## What belongs here

One `.sql` file per procedure, for every `procedure_name` in `manifest.csv` whose `status`
is `missing`. Nothing else. In particular:

- Do **not** add stub or placeholder files. A file that looks like DDL but is not deployable
  (or is deployable but wrong) is worse than the procedure being absent, because
  M0-01-03's deployment script would happily deploy it and the defect would surface only
  when a report or a printed document is opened in production.
- Do **not** move, copy, or duplicate the 13 files that already exist in
  `Existing Store Procedures/StoredProcedures/` into this directory. That relocation is
  M0-01-03's job, not this directory's.
- Do **not** add a procedure that is not in `manifest.csv`, and do not add a `.sql` file
  for a `scripted`, `case_mismatch`, or `unreferenced` row.

## `manifest.csv` — the worklist

Machine-readable, UTF-8, LF line endings, one row per reconciled procedure name.

| Column | Meaning |
|---|---|
| `procedure_name` | The name exactly as it appears in the source reference (or, for `unreferenced` rows, as declared in the existing `.sql` file) |
| `status` | `scripted` · `case_mismatch` · `missing` · `unreferenced` |
| `scripted_file` | Repo-relative path to the existing `.sql` under `Existing Store Procedures/StoredProcedures/`, or empty |
| `reference_count` | Total matches in `.cs`/`.razor` under `V.SMART/`, including commented-out lines |
| `live_reference_count` | Matches excluding lines detected as commented out |
| `first_reference` | `path:line` of the first live reference; if none, the first commented one |
| `notes` | Free text; non-empty for every row whose `status` is not `scripted` |

`manifest.csv` and this README are produced by task **M0-01-01** and are **not** edited by
M0-01-02 or any later task — if the manifest looks wrong, that is a finding for M0-01-01 to
re-run, not an edit to make here.

## Conventions M0-01-02 (the capture task) must follow

These bind every `.sql` file placed in this directory:

1. **One procedure per file.** File name is exactly `<ProcedureName>.sql`, matching the
   `procedure_name` column character for character (same case as captured from the server).
2. **Idempotent by construction.** Every file's first statement is
   `CREATE OR ALTER PROCEDURE [dbo].[<Name>]` — never plain `CREATE PROCEDURE` or
   `ALTER PROCEDURE`. This is true even if the deployed procedure currently uses one of those
   forms; converting the leading keyword to `CREATE OR ALTER` is the **only** permitted
   transformation of a captured body (see below).
3. **Encoding and line endings.** UTF-8 **without** a BOM. **LF** line endings, not CRLF.
   Several of the existing 13 files in `Existing Store Procedures/StoredProcedures/` carry a
   BOM — do not repeat that here.
4. **No `USE <database>` statement.** No `GO` before the `CREATE OR ALTER` — it must be the
   first statement in its batch, so the file can be executed as a single batch.
5. **No secrets, ever.** No credential, connection string, host name, IP literal, or tenant
   name in any `.sql` file. The source tenant and capture date belong in
   `CAPTURE-STATUS.md`, identified by tenant name/id only — never by connection string.
6. **Faithful transcription only.** The captured body must match what is deployed. Do not
   reformat, "clean up," fix a bug you notice, add `SET NOCOUNT ON`, or change
   `ANSI_NULLS`/`QUOTED_IDENTIFIER` settings or parameter defaults while capturing. Any
   difference you notice between the captured text and something else (a naming convention,
   an apparent bug) is a finding to record in `CAPTURE-STATUS.md`, not a change to make in
   the file.

## Verification

`db/tools/verify-capture.sh` (added by M0-01-02) mechanically checks a delivered capture
against this manifest — every `missing` row has a matching file, every file's declared name
matches its file name, every file starts with `CREATE OR ALTER PROCEDURE`, encoding and line
endings are correct, and no secret pattern is present. It requires no database and is safe to
run repeatedly.
