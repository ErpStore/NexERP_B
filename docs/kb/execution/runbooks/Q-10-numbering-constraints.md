---
doc_id: KB-101
title: "Q-10 — Numbering constraints and duplicate census: DBA runbook and results"
module: execution
source_files:
  - docs/kb/execution/runbooks/Q-10-numbering-constraints.sql
  - docs/kb/modules/document-numbering.md
entities: [MfgQuote, MfgDc, MfgInv, DcRunningNumber, InvoiceAutoRunningNumber]
api_endpoints: []
database_tables: [MfgQuote, MfgDc, MfgInv, PurchPo, PurchaseGRN, DcRunningNumbers, InvoiceAutoRunningNumbers]
business_rules: []
status: awaiting-execution
confidence: n/a
last_verified: 2026-08-25
dependencies: [KB-100, KB-004, KB-060, KB-014]
---

# Q-10 — Numbering constraints and duplicate census

> **Phase 1 of 3 is complete. Phases 2 and 3 need a human.**
>
> | Phase | Who | State |
> |---|---|---|
> | 1 — write the script and this runbook | an AI session | **Done**, 2026-08-25 |
> | 2 — run the script against named tenants | **a named DBA, using their own credentials** | **Not done** |
> | 3 — paste the raw output here and interpret it | an AI session or the owner | **Not started** |
>
> No result in this document is real until phase 2 happens. The *Results* section
> below is deliberately empty. **A fabricated result would be worse than no result**,
> because [`M2-B12-03`](../tasks/M2-B12-03.md) will change how document numbers are
> allocated on the strength of what lands here, and document numbers appear on tax
> invoices, delivery challans and e-way bills.

## 1. What this answers, and why it matters

**[Q-10](../../open-questions.md)** — *do the document-number columns carry unique
constraints in the live tenant databases?* The EF model is not the answer:
[KB-100 §9](../../modules/document-numbering.md) records exactly one `IsUnique()` index
across ~51 document series (`MfgQuote.QuoteNo`, `ApplicationDbContext.cs:579-582`), but a
constraint could have been added directly in a tenant database and never modelled, and the
reverse is equally possible.

The stakes are in **R-12** ([KB-060](../../risks/technical-debt-register.md)), the numbering
race. It is currently `Inferred (high confidence)` — reading the code proves a race is
*possible*, not that one has *happened*. Only a duplicate count from live data can move it
to `Confirmed`, and only this script produces that count.

**A second question rides along.** `IsDuplicateDcNoAsync`
(`SalesService/MfgDcService.cs:771-790`) scopes its duplicate check by `CustId`, so the
application **today tolerates** the same `DcNo` under the same `Suffix` for two different
customers. An unqualified unique index on `(DcNo, Suffix)` would therefore **refuse data the
application currently accepts**. That is why every series is counted under two scopings —
see §4.

## 2. Who runs this, and where

> ⚠️ **This table is unfilled, and it is the reason the task closes `Blocked`.**
> **The repository does not contain a DBA's name or a tenant list**, and an AI session may
> neither acquire production credentials nor invent a name to satisfy a checklist. The
> owner fills this in; the DBA then runs the script.

| Field | Value | Source |
|---|---|---|
| DBA who will run it | *(unfilled — owner to name)* | Not recorded anywhere in `docs/kb/` |
| Tenant databases to run against | *(unfilled — see §2.1, the script can enumerate them)* | **Q-12** — the production tenant list is Unknown |
| SQL Server instance | `154.61.76.112,1533` *(the only instance named in the KB — confirm before use)* | `docs/runbooks/credential-rotation.md` |
| Master database | `NexGenErpDb_Master`, table `Tenants` | `MasterDbContext.cs:5-9`, `TenantInfo.cs:3-9` |
| Minimum permission needed | `SELECT` on user tables **and** `VIEW DEFINITION` on the database | §3 |

**Named-role fallback, if no individual is available:** whoever holds `sysadmin` on
`154.61.76.112,1533` — the same role `M0-04`'s credential-rotation runbook depends on for
C-1/C-2. Naming that role is not the same as naming a person, and this runbook does not
pretend otherwise.

### 2.1 You do not need a tenant list to start

**Q-12** records that the production tenant list is Unknown; five template folders exist
(`acucom`, `sns`, `srinuenggind`, `sharadaelectrou1`, `default`) but they are **templates,
not confirmed production tenants**. Rather than guess, derive the list — run this against
the master database first:

```sql
SELECT * FROM dbo.Tenants ORDER BY 1;
```

That row set **is** the tenant list, and it is the authority. Please send it back with the
results; it also partially answers Q-12. Then run the main script once per tenant database.

## 3. How to run it

The script is [`Q-10-numbering-constraints.sql`](Q-10-numbering-constraints.sql), beside
this file.

**It is read-only.** It runs `SELECT` statements and reads system catalogue views. It writes
nothing, changes no schema object and changes no server setting. Verified mechanically —
the file contains none of the ten write keywords anywhere, comments included (§6). It also
sets `READ UNCOMMITTED`, so it will not block live users.

1. Open the file in **SSMS**.
2. **Query → Results To → Results to Text.** Not "Results to Grid" — grid output loses the
   batch structure and is painful to paste back.
3. Select the target tenant database in the database dropdown. **One tenant at a time.**
4. Execute the whole file.
5. **Save the entire text output to a file named after the tenant**, for example
   `Q-10-output-<tenantname>.txt`.
6. Repeat from step 3 for each tenant database.
7. Send every output file back **unedited** — including any error text (§3.1).

**`sqlcmd` alternative**, if SSMS is not to hand:

```
sqlcmd -S 154.61.76.112,1533 -d <TenantDatabase> -E -s"|" -W ^
       -i Q-10-numbering-constraints.sql -o Q-10-output-<TenantDatabase>.txt
```

Use `-U <login> -P <password>` in place of `-E` if not using Windows authentication. **Do
not paste a password into this document, a commit message, or a chat message.**

**How long it takes.** Under a minute per tenant on a small database. The heaviest work is
the `GROUP BY` scans in blocks 2–4; on the largest tables these are index-less aggregate
scans, so on a very large tenant allow a few minutes. There is no lock risk to live users.

### 3.1 If something errors, that is useful — send it anyway

The file is split into batches by `GO`, and every series is wrapped in
`IF OBJECT_ID('dbo.<Table>', 'U') IS NOT NULL`. So:

- a **table absent** from this tenant reports `SERIES-ABSENT` and moves on;
- a **column named differently** in this tenant fails only its own batch, and every other
  batch still runs.

Both outcomes are **findings, not failures** — they are evidence of schema drift between
tenants, which is exactly what `M0-02`/Q-14 is separately chasing. Please do not tidy error
text out of the output.

## 4. What the script measures

The preamble prints `DB_NAME()`, `@@SERVERNAME`, `SYSUTCDATETIME()` and `@@VERSION`, so each
output file **self-identifies** and needs no annotation from you.

Then, per series — **51 in total**, generated from
[KB-100 §9](../../modules/document-numbering.md):

| Block | Label in output | What it answers |
|---|---|---|
| **1** | `BLOCK1-CONSTRAINTS`, `BLOCK1-NUMBERCOLUMNS` | Every unique index and unique key constraint in the database, with its full key column list, filter and primary-key flag — **this is the direct answer to Q-10**. The companion query lists every number-shaped column, so a series whose column is named differently here still shows up. |
| **2** | `BLOCK2-APPSCOPE`, `BLOCK2-SAMPLE` | Duplicates under the scoping **the application itself uses** — for `MfgDc` that is `(DcNo, Suffix, CustId)`. Plus up to 20 sample groups, largest first. |
| **3** | `BLOCK3-UNQUALIFIED` | Duplicates under **`(number, suffix)` only**. **The gap between block 2 and block 3 is the number of groups a naive unique index would refuse but the application accepts today.** This block is not redundant; see §1. |
| **4** | `BLOCK4-SHAPE` | A **format-shape census**: digit runs collapse to a single `#`, so `MFG0012/2025-26` and `MFG12/2025-26` show as distinct historical shapes. `M2-B12-03` must preserve the user-visible format exactly and cannot do that blind. |

**On `Suffix`:** financial-year suffixes are stored **with a leading slash** (`/2025-26`) —
KB-100 §5. The script never filters on a suffix literal; it groups by the stored value, so
the stored form is what appears in the output.

## 5. Results

<!-- awaiting DBA output -->

*Nothing has been run. When output arrives, it is pasted here **verbatim**, one subsection
per tenant, before any interpretation is written.*

### 5.1 Duplicate census

*One row per series per tenant. **Zeros are written as `0`, never left blank** — a blank
cell is indistinguishable from "not measured", and the difference matters here.*

| Tenant | Series | Unique constraint present? | Block 2 groups (app scoping) | Block 3 groups (unqualified) | Gap (3 − 2) | Distinct shapes |
|---|---|---|---|---|---|---|
| *(awaiting execution)* | | | | | | |

### 5.2 Interpretation

*Per tenant, written only after the raw output is pasted above.*

## 6. Verifying the script is read-only before you run it

You are being asked to run a script against production. Check it yourself rather than
trusting this document:

```
grep -ciE "CREATE|ALTER|DROP|INSERT|UPDATE|DELETE|MERGE|TRUNCATE|EXEC|DBCC" \
    docs/kb/execution/runbooks/Q-10-numbering-constraints.sql
```

**Expected: `0`.** This was run on 2026-08-25 against the committed file and returned `0`
for each of the ten keywords individually, comments included. The script's only verbs are
`SELECT`, `SET NOCOUNT`, `SET TRANSACTION ISOLATION LEVEL` and `GO`.

## 7. What this does not tell us

Stated plainly, because a partial answer read as a complete one is how R-12 gets closed
prematurely.

1. **Unchecked tenants.** The answer is per tenant and says nothing about tenants not run.
   Record it as *checked / known*, and note the **total tenant count is Unknown pending
   Q-12** — so even the denominator is provisional.
2. **A constraint present today may be younger than the damage.** A unique index found in
   block 1 does **not** prove the series was always protected. It could have been added
   after duplicates it would have prevented, and adding it would have required removing them
   first. A clean block 2 alongside a present constraint is therefore consistent with *"there
   were duplicates once, and someone cleaned them up"*.
3. **Q-02 is still open** — how migrations reach tenant databases is Unknown. So a
   constraint present in one tenant carries no implication for any other tenant, and the
   absence of one carries no implication either.
4. **`READ UNCOMMITTED` means the counts are approximate under concurrent writes.** That is
   the right trade for not blocking live users, but a count taken during a busy period is
   indicative, not exact. Re-run a surprising series when the system is quiet.
5. **This measures data, not the race.** Zero duplicates does **not** prove the allocator is
   safe — only that the race has not yet lost. R-12 stays `Inferred (high confidence)` on a
   zero result; it becomes `Confirmed` only on a non-zero one. It never becomes "closed" on
   the strength of this script.
6. **Dead series are still counted.** KB-100 §3.4 records 7 of 38 Mechanism A sites as
   having no live caller, and `StockIssueRequest`'s allocator as dead. Their tables are still
   queried here, so a duplicate found in one of them is a historical artefact, not a live
   defect.

## Related documents

- [KB-100](../../modules/document-numbering.md) — document numbering and financial-year
  suffixes; **§9 is the table this script was generated from**.
- [KB-004](../../open-questions.md) — Q-10 (this question), Q-02, Q-12, Q-14.
- [KB-060](../../risks/technical-debt-register.md) — R-12, the numbering race.
- [KB-014](../../architecture/multi-tenancy.md) — database-per-tenant; why no tenant column
  appears in any scope in the script.
- [`M2-B12-02`](../tasks/M2-B12-02.md) — the task this runbook belongs to.
- [`M2-B12-03`](../tasks/M2-B12-03.md) — the consumer: race-safe allocation and idempotency.
