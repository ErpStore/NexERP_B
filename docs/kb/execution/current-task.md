---
doc_id: KB-089
title: Current Task
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-23
dependencies: [KB-081, KB-082, KB-088, KB-091, KB-092, KB-093, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## ▶ M2-C10 — Decimal handling: no float money arithmetic

**Task file:** [`tasks/M2-C10.md`](tasks/M2-C10.md) — the `decimal.js`-backed money/quantity
module under `frontend/nexgen-web/src/app/shared/utils/decimal/`: `parseUserInput`, `format`,
comparison helpers and the branded `Money`/`Qty` types, plus the injectable precision policy
traceable to `Companydetails.DecimalPlaces` (`Companydetails.cs:208`). No screen, no UI
component — this is the parsing/formatting primitive.

**Why this one.** `M2-C04-02` (form controls + validation display) closed `Needs Review`
2026-08-23, independently validated `PASS` on `migration/M2-C04-02-form-controls` (tip
`2eb7d8e`) — unmerged, awaiting owner review. Two `P0` `Ready` candidates remain from
`M2-C04-01`'s earlier merge, both independent of `M2-C04-02`'s files: `M2-C10` (decimal
handling, 2 d) and `M2-C04-03` (modal/drawer/toast/states, 3 d). `M2-C10` wins rank 2 (most
downstream unblocking, [`dependency-graph.md`](dependency-graph.md) § *Ready-task selection
rule*): it is a named Hard prerequisite of **one** tracker row (`M2-C07`, itself further
gated on `M2-C05-01` and unanswered **Q-71**), against **none** for `M2-C04-03` (a *Soft*
dependency only, for `InlineAlert`, with a documented local-placeholder fallback — the exact
pattern `M2-C04-02` itself used). Neither sits on the stated critical path
(`M2-C04-01 → M2-C04-02 → M2-C05-01 → M2-C05-03 → M2-D01 → …`). Full reasoning:
[`runner-state.md`](runner-state.md) Current task, `task-tracker.md` row `M2-C10`.

**Carried forward from `M2-C04-02` — read before starting, do not re-derive:**

- `M2-C04-02`'s three numeric controls (`app-number-input`, `app-currency-input`,
  `app-amount-or-percent-input`) already hold their values as **opaque branded `Money`/`Qty`
  types** (`frontend/nexgen-web/src/app/shared/components/form/types.ts`) and parse/format
  through an injected `DECIMAL_PORT` token that **nothing in application code currently
  provides** — a documented `TODO(M2-C10)`. This task's job is to provide that port's real
  implementation, not to redesign the control contract. A fixture
  (`form/fake-decimal-port.ts`) shows the shape tests expect; it is not exported from
  `form/index.ts` and must stay a test-only fixture.
- **Q-74** (open, non-blocking): how the control's `{ value, isAmount }` pair should project
  onto the server's separate `Amount`/`Percent` columns (`DiscountAmount` vs
  `DiscountPercent`, etc. — `CalculationService.cs:29-31,38-42`, polarity `true` = fixed
  amount). Not this task's problem to solve, but relevant context if the decimal module's API
  shape is questioned.
- **R-68** (open, non-blocking): a client-side-only party-completeness gate in
  `CustomerSelection.razor` has no SPA owner. Unrelated to this task; recorded here only so a
  session skimming `M2-C04-02`'s history does not re-investigate it.

### Five-part "can actually be done" check

1. Hard prerequisite `M2-C01` — `Completed` and merged to `master`. **Met.**
2. Not a `Product Decision`. **Met** — `task_type: Frontend`.
3. Not blocked on an unanswered open question. **Met.** No open question gates this task's own
   scope.
4. Task file not superseded/stale. **Met** — no ⛔ banner; re-specified for Angular by
   `M2-C12-02` (merged), `last_verified: 2026-08-22`. `decimal.js` is carried over from
   ADR-003 **unchanged** by [ADR-007](../decisions/ADR-007-angular-stack.md) and is already
   installed (`frontend/nexgen-web/package.json`: `"decimal.js": "^10.6.0"`).
5. No sibling branch open on the same files. **Met** — `git branch --no-merged master`
   (checked 2026-08-23) lists no branch touching `frontend/nexgen-web/src/app/shared/utils/`
   or `M2-C10.md`.

### Read before starting

- [`tasks/M2-C10.md`](tasks/M2-C10.md) in full. Note the re-specification banner: one
  acceptance criterion (100% statement/branch coverage) had its *mechanism* changed to an
  enumerated test list plus review, because this workspace has no verified coverage command
  — do not invent one and do not quote the superseded criterion literally.
- [ADR-007](../decisions/ADR-007-angular-stack.md) — `decimal.js`, carried over from ADR-003
  unchanged.
- `Companydetails.cs:208` — `DecimalPlaces` default `2`, the source of the precision policy
  `M2-C04-02`'s numeric controls already inject.
- `frontend/nexgen-web/src/app/shared/components/form/numeric-base.ts` and `types.ts` — the
  consumer contract this module must satisfy (the `DECIMAL_PORT` token shape, `Money`/`Qty`
  branding). Do not change these files; they belong to `M2-C04-02`.

### Run State — `Blocked` on the repository owner (environment)

Dispatched, implemented and independently validated on branch
`migration/M2-C10-decimal-handling` (tip `2ae6e63`). Verdict: `FAIL`, category
`acceptance-criterion`, `scopeOk: true`. Fourteen of fifteen acceptance criteria were
independently re-derived `MET` — see `tasks/M2-C10.md` § Execution Record (2026-08-23) for the
full list. A same-session diagnosis then resolved one of the two reported failures and could
not resolve the other:

- **Fixed:** the open-questions row claimed a `git branch --no-merged master` result that was
  false (asserted no unmerged branch held `Q-72`, when `migration/M2-C04-02-form-controls`
  already held `Q-72`–`Q-75`). Renumbered to **Q-76**, with an explicit withdrawal of the false
  claim, per [`open-questions.md`](../open-questions.md).
- **Not fixable here — `environment`:** the binding criterion "INV-032 recorded with the
  **measured** wire format" cannot be satisfied on this workstation. The only decimal-bearing
  endpoint, `GET /api/v1/reference/gst-rates`, is `[Authorize]`d, and
  `V.SMART/V.SMART.Api/appsettings.json:33-38` has both `ConnectionStrings:MasterDb` and
  `Jwt:Secret` empty, so no live response can be captured. Raised as **Q-77**
  ([`open-questions.md`](../open-questions.md)); INV-032 sub-finding 1 and **R-70** retagged
  from an invented confidence tag to KB-002's `Inferred`, pointing at Q-77.

**Disposition:** `Blocked`, category `environment` (KB-091 §8 item 5). Attempts used: 1 of 4.
**A human or a later session resumes this, it does not restart it** — re-dispatching an
implementer against the task as currently specified reproduces the same result. Owner
**Vivek** must choose one of: (a) accept the Inferred + Q-77 disposition as satisfying the
criterion, (b) supply a tenant database and a populated `Jwt:Secret` so a session can capture
the live response and upgrade INV-032 to Confirmed, or (c) authorise a separate backend task
giving `tests/V.SMART.Api.Tests` an `Mvc.Testing` host (closing **R-43** too) and let `M2-C10`
close on the other fourteen criteria. Full record: `task-tracker.md` footnote ⁵²,
`tasks/M2-C10.md` § Execution Record (2026-08-23), `failure-log.md` §§ `M2-C10 · attempt 1 ·
independent validation · 2026-08-23` and `M2-C10 · attempt 1 · diagnosis · 2026-08-23`.

No next task was selected this close-out, per explicit instruction not to start another task.
`M2-C04-03` (`Ready`, `P0`, 3 d) remains a genuinely selectable, independent candidate for a
future session — see `runner-state.md` § Next ready task. `M2-C05`/`M2-C05-01` do **not**
become selectable from `M2-C04-02`'s `PASS`: they need it `Completed` and merged, and it is
`Needs Review`, unmerged, on `migration/M2-C04-02-form-controls`, awaiting owner review.
