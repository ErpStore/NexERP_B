# Runbook — per-tenant stored-procedure drift check

**Audience: the DBA. You do not need to have read anything else in this repository.**
Follow this file top to bottom; it takes about 15 minutes per tenant database.

**Produced by task M0-02.** It exists to answer one question, recorded as **Q-14** in
`docs/kb/open-questions.md`:

> Do the stored procedures differ between tenant databases?

The application is multi-tenant with **one database per tenant**. A previous task
(M0-01-02) captured the `Sp_*` stored-procedure DDL from **one** database into
`db/stored-procedures/`. Whether that single set describes every tenant, or only the one it
came from, is unknown. If tenants have diverged, then a future deployment script would
overwrite one customer's procedures with another customer's — changing report figures and
statutory document content, with no automated test anywhere in this solution to catch it.

You will not export procedure bodies. You will export a small **fingerprint** per procedure
(name, dates, length, two hashes), which is enough to detect drift and small enough to review.

---

## 0. What you must never put in these files

**No credential, connection string, host name, instance name, IP address, login name or
customer name may appear in any file you hand back** — not in the data, not in the file name,
not in a comment.

This repository is public. Tenant connection strings are already stored in plaintext inside
the application's `Tenants` table, which is a known open risk (R-01); do not add to it. If one
does reach a commit, reverting the commit is **not** enough — tell the migration lead
immediately so the credential-rotation task (M0-04) and the history-purge task (M0-05) can be
triggered.

Refer to each tenant by a **short opaque label you invent**: `tenant-a`, `tenant-b`,
`baseline`. Keep the label→database mapping in your own notes, outside this repository.

## 1. Prerequisites

| | |
|---|---|
| Access | `VIEW DEFINITION` (or `db_owner`) on **at least two** tenant databases |
| Tool | SSMS, Azure Data Studio, or `sqlcmd` — whatever you already use |
| Engine | SQL Server 2016 (13.x) or later. On an older engine `HASHBYTES` rejects inputs over 8000 bytes and both hash columns come back `NULL` for long procedures — that is a failed collection, not a result. |
| Files | `db/tools/list-deployed-procedures.sql` from this repository |

**Read-only.** Nothing in this runbook writes to any database. There is no `USE` statement in
the query — you connect to the target database yourself.

## 2. Choosing which tenants to fingerprint

**Two is the minimum for the question to be answerable at all. More is strictly better** —
every extra tenant is another chance to catch a divergence, at ~15 minutes' cost.

Two selection rules:

1. **The baseline must be in the set.** `db/stored-procedures/CAPTURE-STATUS.md` records the
   source of the existing capture. Without it in the comparison, you learn that tenants differ
   from each other but nothing about whether the captured artefacts describe any of them.

   > **Flag for a human, not for you to resolve.** That capture's provenance is not a clean
   > single-tenant capture: `CAPTURE-STATUS.md` ("Provenance caveat") records that the DDL
   > actually originated in a demo database, `IQSMARTDEMO_DB_2025-26`, and was relayed through
   > a local database called `NexGenErpDb` before being exported. So "the source tenant" is
   > ambiguous. **Ask the migration lead which of the two to fingerprint** — ideally both,
   > labelled `baseline-demo-origin` and `baseline-relay`. Whatever you choose, write down in
   > the handback which database each label refers to (in your own notes, not in the CSV).

2. **Prefer tenants most likely to have been customised** — the oldest, the largest, and any
   where support has ever hand-applied a fix. If you have no basis to choose, the application
   carries per-tenant report **templates** for four tenant sub-domains under
   `V.SMART/V.SMART.Shared/wwwroot/templates/` (`acucom…`, `sharadaelectrou1…`, `sns…`,
   `srinuenggind…`). Those folder names are a *starting list*, not a tenant inventory — which
   tenants are actually in production is itself an open question (Q-12).

## 3. Run the fingerprint query

Open `db/tools/list-deployed-procedures.sql`. It contains **two** queries:

- **Query A** — a human-readable listing. Read it on screen if you like. **Do not export it**:
  its `Note` column contains commas and would break the CSV.
- **Query B**, headed `PER-TENANT DRIFT FINGERPRINT (M0-02, Q-14)` /
  `FINGERPRINT_QUERY_VERSION 2` — **this is the one to run and export.**

For **each** tenant database:

1. Connect to that tenant database (Windows Authentication if you have it; if a login is
   required, enter its secret in the tool's own prompt — never in a file, never on a command
   line that gets saved).
2. Confirm the connection really is the intended database — `SELECT DB_NAME();`.
3. Run **Query B only**, unchanged. Changing it for one tenant and not another produces a
   comparison of the query, not of the procedures.
4. Check the result: every row must have a 64-character `hash_raw` and `hash_normalised`. Any
   `NULL` means you lack `VIEW DEFINITION` on that object, the object is `WITH ENCRYPTION`, or
   the engine is pre-2016. Fix the cause and re-run; do not hand back `NULL` hashes.

## 4. Export to CSV

Target file: `db/drift/<tenant-label>.csv` — lower-case label, `[a-z0-9-]` only.

Required header, exactly, as the first line:

```
schema_name,procedure_name,create_date,modify_date,definition_length,hash_raw,hash_normalised
```

Query B's column aliases already produce this header, so any "include column headers" export
gives the right thing.

**SSMS** — Tools → Options → Query Results → SQL Server → Results to Grid → tick *Include
column headers when copying or saving the results*; run Query B; right-click the grid →
*Save Results As…* → CSV.

**Azure Data Studio** — run Query B; in the results grid toolbar choose *Save as CSV*; ensure
headers are included.

**sqlcmd** (from the machine that can reach the tenant; `-E` = trusted connection):

```
sqlcmd -S <your-instance> -d <tenant-database> -E -i db/tools/list-deployed-procedures.sql -s "," -W -h -1
```

If you use `sqlcmd` this way, note that it runs **both** queries in the file — keep only the
second result set, and add the header line above by hand. The GUI route is less error-prone.

Then check the file, before handing it back:

- exactly one header line, then one line per procedure;
- **no value contains a comma** (`awk -F, 'NF!=7' db/drift/<label>.csv` must print nothing);
- no trailing "(NN rows affected)" line, no blank padding lines, no separator line of dashes;
- UTF-8, no byte-order mark;
- nothing in the file identifies the customer, the host or the credential.

## 5. Run the comparison — this is the deliverable

Once **two or more** CSVs are in `db/drift/`, from the repository root:

```bash
bash db/tools/compare-tenant-fingerprints.sh
```

It needs no database and no arguments. It classifies every procedure name:

| Class | Meaning | What happens next |
|---|---|---|
| `identical` | Same verbatim definition in every tenant | Nothing |
| `cosmetic` | Differs only by whitespace/casing (`hash_raw` differs, `hash_normalised` matches) | Recorded; the single artefact set stands |
| `divergent` | Differs beyond formatting | **Escalated to the product owner** — never "fixed" by anyone reading this runbook |
| `missing_in_tenant` | Present in some tenants, absent in others | **Escalated** — a report that works for one customer and throws for another |
| `extra_in_tenant` | Present in a tenant but unknown to the application's worklist | Recorded. **Do not delete it** — it may be a customisation someone paid for |

Exit codes: `0` no drift or cosmetic only · `2` structural failure, output not trustworthy ·
`3` fewer than two tenants, so the question is **undecided** (never "no drift") · `4` drift
found needing escalation.

**Do not reconcile, harmonise or "fix" any difference the script reports.** A divergent
procedure may encode a customisation a customer paid for, or a fix applied to one tenant and
never propagated. Deciding what to do about it is a product-owner decision, taken per
procedure, with the diff attached.

### Self-test (optional, 2 minutes)

The comparison is only trustworthy if it refuses bad input. To prove it does, copy any
fingerprint CSV to a scratch directory outside the repository, change one word in its header
line, and run the script against that directory:

```bash
bash db/tools/compare-tenant-fingerprints.sh /path/to/scratch-dir
```

It must print `FAIL: … header does not match FINGERPRINT_QUERY_VERSION 2` and abort with exit
code 2. If it instead produces a comparison, stop and report it — the harness is broken and
every conclusion drawn from it is unsafe.

## 6. Handback checklist

- [ ] At least two tenant databases fingerprinted, one of them the baseline from
      `CAPTURE-STATUS.md` (see the flag in §2 about which database that is).
- [ ] Every CSV produced by **Query B**, unchanged, `FINGERPRINT_QUERY_VERSION 2`. If any
      tenant was fingerprinted with an older version of the query, **re-collect it** — mixing
      query versions silently invalidates the whole comparison.
- [ ] Every CSV has the exact required header and 7 fields per row.
- [ ] No `NULL` or empty hash in any file.
- [ ] No credential, connection string, host name, instance name, IP address, login name or
      customer name anywhere in the files or their names.
- [ ] Files placed in `db/drift/` as `<tenant-label>.csv`.
- [ ] The label→database mapping recorded in **your own** notes, outside this repository, and
      shared with the migration lead directly.
- [ ] Run `bash db/tools/compare-tenant-fingerprints.sh` and paste its complete output into
      the task ticket for **M0-02**.

## 7. If you can only reach one tenant database

Say so, and stop. Hand back the single CSV and state plainly that only one database was
reachable and why.

Drift is then **undecided** — one fingerprint compared against nothing classifies everything
as `identical`, which is a statement about the arithmetic, not about the tenants. The
consequence is recorded rather than guessed: the captured artefact set describes one database,
and any later per-tenant surprise traces back to this gap. Do not let anyone read a single-file
run as "no drift found".
