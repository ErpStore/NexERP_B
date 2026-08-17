# `db/drift/` — per-tenant stored-procedure fingerprints

This directory holds **one CSV per tenant database**, produced by a DBA and consumed by
`db/tools/compare-tenant-fingerprints.sh`. It exists to answer **Q-14** — *do the stored
procedures differ between tenant databases?* (`docs/kb/open-questions.md`) — created by task
**M0-02**.

The full instructions are in **`db/RUNBOOK-tenant-drift-check.md`**. This file is the short
reference for the file format alone.

---

## 1. The standing rule — read this first

**No credential, connection string, host name, instance name, IP address, login name or
customer-identifying value may appear in any file in this directory** — not in the data, not
in the file name, not in a comment.

Tenant connection strings are stored in plaintext in the `Tenants` table (risk **R-01**,
`docs/kb/risks/technical-debt-register.md`). This repository is public
(**Q-19**, answered 2026-08-12), so anything committed here is world-readable immediately.
If a connection string does reach a commit, reverting is **not** sufficient — escalate to
M0-04 (rotation) and M0-05 (history purge).

Identify each tenant by a **short opaque label** you choose, e.g. `tenant-a`, `tenant-b`,
`baseline`. Keep the label→database mapping outside the repository, with the DBA.

## 2. File naming

```
db/drift/<tenant-label>.csv
```

- lower-case, `[a-z0-9-]` only, no spaces, no dots other than `.csv`;
- the label is what appears in every line of the comparison report, so make it readable;
- one file per tenant, no other `.csv` file in this directory — the comparison script treats
  **every** `*.csv` here as a tenant fingerprint.

## 3. CSV schema — exactly this header, exactly this order

```
schema_name,procedure_name,create_date,modify_date,definition_length,hash_raw,hash_normalised
```

| Column | Content | Why it is here |
|---|---|---|
| `schema_name` | e.g. `dbo` | The application always calls `EXEC dbo.{procedureName}` (`V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/ReportExecutor.cs:27`), so a procedure under any other schema is a finding in itself |
| `procedure_name` | e.g. `Sp_StockLedger` | The join key across tenants |
| `create_date` | ISO-8601, e.g. `2024-01-31T09:15:22` | Cheap history signal |
| `modify_date` | ISO-8601 | High signal: a differing `modify_date` across tenants indicates drift even when the hashes match |
| `definition_length` | integer | Coarse comparison; also catches truncation introduced by the export itself |
| `hash_raw` | 64-char hex, SHA2_256 over the **verbatim** definition | Detects any difference at all |
| `hash_normalised` | 64-char hex, SHA2_256 over the definition with line endings normalised, whitespace runs collapsed and case folded | Separates *cosmetic* from *potentially behavioural* drift |

Produced **only** by *Query B* (`FINGERPRINT_QUERY_VERSION 2`) in
`db/tools/list-deployed-procedures.sql`, run **unchanged** against every tenant. A query that
differs per tenant produces a comparison of the query, not of the procedures — which is why
`compare-tenant-fingerprints.sh` **aborts** when two files' headers differ.

Formatting requirements:

- comma separator, no quoting, **no value may contain a comma** (this is why the human-readable
  `Note` column of *Query A* is deliberately not exported);
- UTF-8, no byte-order mark; `LF` or `CRLF` both accepted;
- one header row, then one row per procedure;
- **no empty and no `NULL` hash.** A `NULL` hash means the collector lacked `VIEW DEFINITION`,
  the object is `WITH ENCRYPTION`, or `HASHBYTES` refused an over-8000-byte input on a
  pre-2016 instance. The script rejects the file — that is a collection failure, and it must
  never be read as "no drift".

## 4. Using them

From the repository root:

```bash
bash db/tools/compare-tenant-fingerprints.sh
```

Exit codes: `0` no drift or cosmetic only · `2` structural failure (bad header, malformed row,
`NULL` hash — output not trustworthy) · `3` fewer than two tenants, so the question is
**undecided**, never "no drift" · `4` drift found that needs escalation.

**Two tenants is the minimum** for the question to be answerable at all, and more is strictly
better. The source tenant recorded in `db/stored-procedures/CAPTURE-STATUS.md` must be one of
them, or the comparison does not include the baseline it is meant to test.

## 5. Status

As of 2026-08-17 this directory contains **no fingerprints**. Q-14 is therefore **open and
undecided**, blocked on a DBA with `VIEW DEFINITION` on at least two tenant databases and on a
working tenant list (**Q-12**).
