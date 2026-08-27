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
status: partial  # phase 1-3 done for the one known tenant (NexGenErpDb); other tenants Unknown pending Q-12
confidence: mixed  # constraint census Confirmed; duplicate census Confirmed-but-thin (near-empty tenant)
last_verified: 2026-08-26
dependencies: [KB-100, KB-004, KB-060, KB-014]
---

# Q-10 — Numbering constraints and duplicate census

> **All three phases are done for the one tenant this project can currently reach. Other
> tenants remain unknown pending Q-12.**
>
> | Phase | Who | State |
> |---|---|---|
> | 1 — write the script and this runbook | an AI session | **Done**, 2026-08-25 |
> | 2 — run the script against named tenants | the repository owner, in session, `NexGenErpDb` only | **Done**, 2026-08-26 — see §5 |
> | 3 — paste the raw output here and interpret it | an AI session, verified against the raw file | **Done**, 2026-08-26 — see §5 |
>
> **The original plan assumed a named production DBA who does not exist in this repository**
> (§2, as originally written). That assumption changed: `154.61.76.112` — the host this
> document and `M0-04`'s runbook both once called "production" — is confirmed **not this
> project's infrastructure** (see §2's correction note). The only database this project
> actually operates is `NexGenErpDb` on `DESKTOP-FIIBE97\SQLEXPRESS`, and the owner holds
> direct access to it — demonstrated in the same session that rotated its credentials
> (`task-tracker.md` footnote ⁸⁵). Running phase 2 there, now, answers a real question about
> a real database this project owns, rather than waiting indefinitely on a DBA for a host
> that was never this project's to query.

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

> **Corrected 2026-08-26 — this table originally named `154.61.76.112,1533` as "the SQL
> Server instance," inherited from `M0-04`'s runbook before that host's ownership was
> corrected. The owner confirmed in session that `154.61.76.112` is a third party's host, not
> this project's — see `risks/technical-debt-register.md` R-01's correction note and
> `open-questions.md` Q-103. It is not run against here. This project's only reachable
> instance is the one named below.**

| Field | Value | Source |
|---|---|---|
| DBA who will run it | The repository owner, in the same session that rotated C-1/C-3 (`task-tracker.md` footnote ⁸⁵) | This session, 2026-08-26 |
| Tenant databases to run against | `NexGenErpDb` — the sole tenant, `Tenants.Id=1`, `Name='localhost'` | Confirmed by a full `sp_MSforeachdb` sweep of every database's `sys.tables`, this session |
| SQL Server instance | `DESKTOP-FIIBE97\SQLEXPRESS` | `MasterDbContext.cs` consumers; confirmed reachable this session |
| Master database | `NexGenErpDb_Master`, table `Tenants` | `MasterDbContext.cs:5-9`, `TenantInfo.cs:3-9` |
| Minimum permission needed | `SELECT` on user tables **and** `VIEW DEFINITION` on the database | §3 |

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
sqlcmd -S "DESKTOP-FIIBE97\SQLEXPRESS" -d <TenantDatabase> -E -s"|" -W ^
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

**Executed 2026-08-26** against tenant `NexGenErpDb` (`Tenants.Id=1`, `Name='localhost'`),
the sole tenant on this project's only reachable SQL Server instance
(`DESKTOP-FIIBE97\SQLEXPRESS`) — see §2's correction. Run via `sqlcmd` under Windows
Integrated Authentication (`DESKTOP-FIIBE97\Admin`, confirmed `sysadmin` before running,
so no permission gap could hide a result). Full raw output, verbatim, is committed
alongside this file: [`Q-10-output-NexGenErpDb.txt`](Q-10-output-NexGenErpDb.txt) (1,100
lines). Re-verified independently against that file before writing anything below —
every count here was re-derived with `grep`/`awk`, not read once and trusted.

**One tooling correction needed to get a complete run:** the script's `BLOCK1-CONSTRAINTS`
query failed on the first pass with `Msg 1934 ... 'QUOTED_IDENTIFIER'` — `sqlcmd` defaults
that session setting `OFF`, and the query's `STUFF()`/subquery construction needs it `ON`.
Re-run with `-I` added; every other flag matches §3 exactly. Recorded so the next tenant run
does not lose the same block.

### 5.1 Constraint inventory (Block 1) — Q-10's direct answer

**Confirms KB-100 §9 exactly, not merely "consistent with it."** Of every unique index and
unique-key constraint in the live database (202 rows total, `Q-10-output-NexGenErpDb.txt`
lines tagged `BLOCK1-CONSTRAINTS`), **exactly one** sits on a document-number-shaped column
pair: `MfgQuote` — `IX_MfgQuote_QuoteNo_Suffix`, `NONCLUSTERED`, `IsUnique=1`,
`IsPrimaryKey=0`, key `(QuoteNo, Suffix)`. Every other row in the 202 is a surrogate-key
`PK_*` (a numeric `Id`/`SlNo`, never the document number itself) or an unrelated index
(`Users.EmailId`, `Users.UserName`, the two `AssmblyDef*` composite indexes). **Neither
allocation table carries a unique index beyond its own surrogate key**: `DcRunningNumber`
→ `PK_DcRunningNumber(Id)` only; `InvoiceAutoRunningNumber` → `PK_InvoiceAutoRunningNumber(Id)`
only — confirming R-12's premise that the allocation tables themselves have no constraint on
their logical key (`DcType+Suffix` / `InvoiceType+Suffix`).

**Q-10 is answered, for this tenant:** the EF model is not merely silent about live
constraints elsewhere — it is *complete*. What the model shows is what the database has.

### 5.2 Duplicate census (Blocks 2–3)

**46 of 51 series ran successfully; every one reports 0 duplicate groups**, under both
application scoping (Block 2) and the unqualified `(number, suffix)` scoping (Block 3) — the
gap the task file flags as worth watching (§1) is `0` everywhere it could be measured.

**This is not the strong result it looks like, and §7 item 5 already says why: this measures
data, not the race.** The reason every count is zero is visible in Block 4 (below) — this
tenant has almost no transactional data. Only **4 of 51 series hold any row at all**
(`MfgQuote`, `PerformaInv`, `MfgPo`, `EnquirySales` — one row each); the other 42 checked
series are empty tables. A duplicate cannot occur in an empty table, so a zero result here is
expected regardless of whether the underlying race exists. **R-12 stays `Inferred (high
confidence)`, exactly as before** — this result does not move it, per §7 item 5's own rule
("it becomes `Confirmed` only on a non-zero one"). What this genuinely rules out: this
specific tenant's historical data contains no duplicate, which is a fact worth having, just
not the fact Q-10/R-12 were ultimately chasing. **Confirming or refuting R-12 needs a tenant
with real transaction volume**, which — per Q-12 — this project does not yet know the
identity of.

### 5.3 Format-shape census (Block 4)

Only 4 series produced any row (matching §5.2's finding that only those 4 tables hold data):

| Series | Shape | Rows | Example |
|---|---|---|---|
| `MfgQuote.QuoteNo` | `#` | 1 | `1` |
| `PerformaInv.InvNo` | `#` | 1 | `1` |
| `MfgPo.PONo` | `#` | 1 | `1020` |
| `EnquirySales.EnquiryNo` | `#` | 1 | `1021` |

Single shape, single row each — no historical format drift to report from this tenant. Not
informative beyond confirming the pattern is purely numeric here; a tenant with real history
is needed to actually exercise this block's purpose (catching a series that changed format
mid-life).

### 5.4 Script defects found — corrected for the next run, not silently worked around

**Three of 51 series failed with `Msg 207, Invalid column name`** — a defect in the
generated script's column names, not schema drift in the live database (the live schema is
internally consistent; the script guessed wrong):

| Series | Script assumed | Live database actually has |
|---|---|---|
| `PurchPo.PONo /scope:...+RevesionNo` | `RevesionNo` (misspelled) | `RevisionNo` |
| `StockIssueRequest.IssueNo` | `IssueNo` | `RequestNo` |
| `Receipts.PaymentNo` | `PaymentNo` | `ReceiptNo` (also has `ChequeNo`) |

**These three series were not counted in §5.2's 46 — they are neither confirmed zero nor
confirmed non-zero.** `PurchPo` and `Receipts` are live series (not flagged dead in KB-100
§3.4); `StockIssueRequest` is one of the series KB-100 §3.4 already records as dead, so its
gap matters least of the three, per §7 item 6.

**Separately, the two allocation-table series (50/51) reported `SERIES-ABSENT`** — the
script's `IF OBJECT_ID('dbo.DcRunningNumbers', 'U')` (and the invoice equivalent) checked the
**plural** table name; the live tables are singular: `DcRunningNumber`,
`InvoiceAutoRunningNumber` (confirmed in §5.1's Block 1 output, which scans `sys.tables`
generically and is unaffected by this). No information was actually lost — Block 1 already
answers the allocation tables' constraint question directly — but the per-series duplicate
check for these two never ran under either name and should be fixed before the next tenant.

**Recommended before this script is trusted for another tenant:** fix these four table/column
names in `Q-10-numbering-constraints.sql` (series 22 for `PurchPo`, 43 for
`StockIssueRequest`, 49 for `Receipts`, 50–51 for the allocation tables), and re-run against
this tenant to close the 3-series gap.

### 5.5 Interpretation

**Q-10's constraint question is answered, confidently, for this tenant**: the live database
matches the EF model exactly — one protected series (`MfgQuote`), everything else
unprotected, including both allocation tables. **Q-10's duplicate-history question is
answered too, but the answer is "none found in a dataset too small to have produced one"**,
not "none found despite real exposure" — per §7 item 5, this cannot and does not close R-12.
**The practical blocker on actually confirming or refuting R-12 is now Q-12** (the production
tenant list is Unknown) combined with the fact that this project's only currently reachable
database is a near-empty dev environment. `M2-B12-03` (race-safe allocation) should proceed
on R-12's existing `Inferred (high confidence)` rating — this census neither raises nor
lowers that confidence, it only rules out one specific, thin dataset.

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
