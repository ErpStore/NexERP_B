# `db/stored-procedures/`

The capture root for the stored procedures the V.SMART / NexGen ERP application calls. This
directory currently holds only the reconciliation output of
[M0-01-01](../../docs/kb/execution/tasks/M0-01-01.md) — a manifest, this README, and **no
`.sql` files**. The `.sql` DDL itself is scripted from a live tenant database by
[M0-01-02](../../docs/kb/execution/tasks/M0-01-02.md), not by this task.

See [`docs/kb/architecture/stored-procedure-inventory.md`](../../docs/kb/architecture/stored-procedure-inventory.md)
(KB-102) for the full reconciliation, methodology, and findings behind `manifest.csv`.

## `manifest.csv`

The worklist. UTF-8, LF line endings, header row required. Columns:

| Column | Meaning |
|---|---|
| `procedure_name` | The name exactly as it appears in the source reference (or, for `unreferenced` rows, as declared in the `.sql` file) |
| `status` | `scripted` · `missing` · `unreferenced` · `case_mismatch` |
| `scripted_file` | Repo-relative path to the existing `.sql` under `Existing Store Procedures/StoredProcedures/`, or empty |
| `reference_count` | Total matches in `.cs`/`.razor`, including commented-out lines |
| `live_reference_count` | Matches excluding lines a heuristic identifies as commented out |
| `first_reference` | `path:line` of the first live reference; if none, the first commented one (see `notes`) |
| `notes` | Free text; non-empty for every row whose `status` is not `scripted` |

Regenerate the reference side with [`../tools/sp-inventory.sh`](../tools/sp-inventory.sh); the
declared side comes from the `CREATE`/`ALTER PROCEDURE` statement in each existing `.sql`
file, never the file name (see the KB document's *Findings* for why that distinction matters —
`Sp_Print_PurchaseOrder.sql` declares a name nothing calls).

## Conventions M0-01-02 must follow when it adds `.sql` files here

1. **One procedure per file.** File name is exactly `<ProcedureName>.sql`, matching the
   declared name character for character (the name in `manifest.csv`'s `procedure_name`
   column for `missing` rows, or the corrected spelling for the `case_mismatch` row).
2. **Idempotent by construction.** Every file begins with
   `CREATE OR ALTER PROCEDURE [dbo].[<Name>]` so deployment can be re-run safely. The 11
   already-scripted procedures are inconsistent on this point — see KB-102 *Finding 5*
   (mixed casing, mixed `[dbo].[...]` vs. bare bracketing) — do not carry that inconsistency
   forward into newly captured files.
3. **Encoding.** UTF-8 **without** a BOM, LF line endings. 6 of the 11 existing files carry a
   UTF-8 BOM (KB-102 *Finding 5*); new captures should not repeat that.
4. **No `USE <database>` statement.** No `GO` before the `CREATE OR ALTER` statement — it must
   be the first statement in its batch, so the deployment script (M0-01-03) can run each file
   standalone against any tenant database.
5. **No secrets.** No credential, connection string, host name, or tenant name in any `.sql`
   file. The repository is public (Q-19, resolved 2026-08-13) and its already-committed
   credentials (R-01, R-02) must not gain company. The **source tenant and capture date**
   belong in the KB document or the capture task's decision log, not in the `.sql` file
   itself.
6. **No stub files.** Do not create an empty or placeholder `.sql` file for a name you have
   not yet captured — a stub that looks like DDL and is not is worse than an absence, because
   a deployment script would happily "deploy" it as a no-op or, worse, an empty procedure body.

## What is *not* here

- The 13 procedures that already have DDL remain at their existing location,
  `Existing Store Procedures/StoredProcedures/` — this task does not move, rename, or edit
  them (that relocation, if any, is M0-01-03's decision).
- `Sp_Print_PurchaseOrder` is declared there but referenced nowhere in application code
  (`status: unreferenced` in the manifest). Whether to keep, delete, or investigate it further
  (procedure-to-procedure call? `.frx` binding? genuinely dead?) is a human decision this task
  does not make — see KB-102 *What this method cannot see*.
