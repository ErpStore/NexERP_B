---
doc_id: KB-089
title: Current Task
module: execution
source_files:
  - V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs
  - V.SMART/V.SMART.Shared/Migrations/ApplicationDbContextModelSnapshot.cs
  - V.SMART/V.SMART.Shared/Services/MultiCompanyService/ITenantProvider.cs
  - V.SMART/V.SMART.Shared/Services/MultiCompanyService/ITenantDbContextFactory.cs
  - V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantDbContextFactory.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgDcService.cs
entities: [MfgQuote, MfgDc, MfgInv, DcRunningNumber, InvoiceAutoRunningNumber]
api_endpoints: []
database_tables: [MfgQuote, MfgDc, MfgInv, PurchPo, PurchaseGRN, DcRunningNumbers, InvoiceAutoRunningNumbers]
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-25
dependencies: [KB-081, KB-082, KB-088, KB-091, KB-092, KB-093, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Selected task: M2-B12-02 — Verify unique constraints in a live tenant database (Q-10)

Full spec: [`tasks/M2-B12-02.md`](tasks/M2-B12-02.md). Selected 2026-08-25, select-only pass,
tip `92e8b28` on `master`, tree clean. **Not yet dispatched — attempt 0.**

### Why this task, and why now

`M2-B12-01` merged to `master` at `f10b5fc` (`Completed`), which is `M2-B12-02`'s sole Hard
prerequisite. `M2-B12-01` established what the **EF model** says: exactly one document-number
unique index exists in it (`MfgQuote(QuoteNo, Suffix)`), and produced the per-series query
inputs `M2-B12-02` needs (KB-100 §9). `M2-B12-02` answers what the **live tenant databases**
actually enforce — the model is not proof of the deployed schema, since no migration-rollout
procedure is documented anywhere (Q-02). [KB-081](task-tracker.md) released the row `Blocked`
→ `Ready` at `92e8b28` the moment that merge landed; this pass independently re-ran the
five-part "can actually be done" test rather than trust the commit message (recorded in full
in [`runner-state.md`](runner-state.md) `Status`) — all five clear, and no other `Ready` row
(`M0-11`, a `Product Decision`) does.

### The shape of this task — same pattern as M0-04

Deliberately split in two, per the task file's own *"The credential problem, stated
honestly"* section:

1. **Phase 1 (AI-deliverable, this session's actual scope):** a **read-only** SQL script that
   checks `sys.indexes` for a unique constraint on every document-number series, **and**
   scans for existing duplicate numbers under the correct scoping — plus a runbook telling a
   named DBA exactly how to run it and what to send back.
2. **Phase 2 (human, not this session):** a **named DBA**, using their own production
   credentials, actually executes the script against at least one named tenant database and
   returns the raw output.

**If phase 2 does not happen during this session, the honest close is `Blocked`** (owner:
Vivek, or whoever is named as the DBA) — **not** `Completed`, and not a failure. This is the
task's own designed *Target Result*, identical in shape to `M0-04`'s.

### What phase 1 must produce

1. The SQL script — one `sys.indexes` check plus one duplicate-count query per document
   series named in KB-100 §9, self-identifying its output (tenant name, run timestamp) so a
   DBA doesn't have to annotate it, with the "awaiting DBA output" marker left in place until
   phase 2 lands. Must account for two findings already on record: `DcRunningNumbers` /
   `InvoiceAutoRunningNumbers` have no unique index on their own logical key in the model
   either, and `Suffix` is stored **with a leading slash** (`/2025-26`) — a naive
   `WHERE Suffix = '2025-26'` silently returns nothing.
2. The runbook — names **the** DBA and **the** tenant database(s) requested, not generic
   placeholders (reuse M0-02's tenant list/DBA contact if that task has already run).
3. A results document (KB-101, to be created) with an empty results section and the
   `<!-- awaiting DBA output -->` marker, ready for phase 2's verbatim paste.
4. Two commits, per the task file's own plan — phase 1 (script + runbook) now, phase 3
   (results + Q-10 answer + R-12 update) only if/when a DBA returns output.

### Classification (KB-091 §4)

`task_type: Database` → base **MEDIUM** (§4.1). One raise applies: the task touches document
numbering directly (§4.2) → **complexity HIGH**. Risk **MEDIUM** (§4.3 default — not
`Security`/`Product Decision`, no schema change since the script is read-only, no
secrets/credentials/`Program.cs`/`appsettings*` touched, `business_rules` empty in
frontmatter, and phase 1 changes no code path a live Blazor user observes). Per §5.1
HIGH-complexity routing: Investigate `opus`, Implement `opus`, Validate `opus`.

`requiresHuman`: **the DBA-execution half (phase 2) needs production database access this
session cannot provide** — flagged, not a start-blocker. Phase 1 is fully AI-executable.

### Carried forward — still true, unaffected by this selection

- **`M0-11`** (`Ready`, P0) still fails part 2 of the five-part test: `task_type: Product
  Decision`, owner-only, never self-selectable (KB-091 §8).
- **`M0-04`** is closed `Blocked` (by design) and merged to `master` at `ad75915`. Do not
  re-open or re-run it — its document deliverables are complete; what remains is human
  rotation, named to Vivek and three other unassigned roles (see `task-tracker.md` footnote
  ⁷¹ for the full escalation list: C-1…C-7, Q-84).
- **`M2-C10`** (`Blocked`⁸⁵) is gated on **Q-85** (money-as-string vs. `double` over the
  wire) — an API-contract decision, owner-only.
- **`M0-06`** (`Blocked`) is gated on **Q-25/Q-26** (whether `UserId = 1` is any tenant's sole
  administrator, and how provisioning must avoid the seeded credential) — owner-only.
- Several `Needs Review` branches remain unmerged and awaiting owner integration
  (`M2-A02`, `M2-A09`, `M2-A10`, `M2-B10`, and now the `M2-B12-01` lineage's own successor
  chain) — see `task-tracker.md` § Current state for the current list.
