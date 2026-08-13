# This folder is retired

The 13 stored-procedure `.sql` files that used to live in
`Existing Store Procedures/StoredProcedures/` were relocated on **2026-08-13** by task
**M0-01-03** to:

```
db/stored-procedures/relocated-legacy/
```

They were moved with `git mv` (history preserved) and normalised only in the ways
`db/stored-procedures/README.md` mandates — UTF-8 without a BOM, LF line endings. No
procedure **body** was changed; see the M0-01-03 commit that performed the move for a
byte-level diff (only a leading UTF-8 BOM was stripped from 6 of the 13 files).

`db/stored-procedures/manifest.csv`'s `scripted_file` column was repointed to the new
paths in the same task.

**Do not add `.sql` files here again.** `db/stored-procedures/` (flat directory plus its
`relocated-legacy/` subdirectory) is now the single authoritative location for every
stored procedure's DDL. See `db/stored-procedures/README.md` for why the relocated files
live in a subdirectory rather than the flat top level, and `db/deploy-stored-procedures.ps1`
for how both are deployed together.

This `StoredProcedures/` subfolder itself is gone — git does not track empty directories,
so once its last file was moved out, the directory ceased to exist on disk. This
`README.md` is the only file left under `Existing Store Procedures/`, deliberately, as the
pointer.
