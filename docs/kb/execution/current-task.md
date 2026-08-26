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
last_verified: 2026-08-26
dependencies: [KB-081, KB-082, KB-088, KB-091, KB-092, KB-093, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## No dependency-ready task — the run stops here

**Closed out this pass:** `M2-C05-03` (empty/loading/error states + server-side export).
Implemented and independently validated **PASS** on branch
`migration/M2-C05-03-grid-states-and-export` (commit `a3a8ff5`), left unmerged for review.
Recorded `Needs Review` in `task-tracker.md` row 166, footnote ⁷⁵ — not `Completed`, since only
the repository owner may set that. Full record: [`tasks/M2-C05-03.md`](tasks/M2-C05-03.md)
§ Execution Record (2026-08-26).

### Why nothing was selected next

Every row in `task-tracker.md` still reading `Ready` fails the five-part "can actually be done"
test ([KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)):

- **`M0-06`** (Security, P1, `Ready`) — fails part 5. `migration/M0-06-remove-default-admin`
  already carries a full, separately closed-out `Blocked` outcome (commit `5c9b34c`, "record
  Blocked status on Q-25/Q-26"), unmerged. Re-dispatching would duplicate finished work.
- **`M0-11`** (Product Decision, P0, `Ready`) — fails part 2 outright. Owner-only, never
  self-selectable.
- **`M2-C05-02`** (Frontend, P1, `Ready`) — fails part 5 on a genuine same-file conflict, not a
  duplicate. Its own *Expected changed files* row (`tasks/M2-C05-02.md:588`) names
  `data-grid.component.ts`, `data-grid.component.html` and `data-grid.model.ts` — exactly the
  files `M2-C05-03`'s branch changed and left unmerged. Opening it now would edit files a still-
  open sibling branch already edited — *Same-file conflicts — never parallelise*
  (`dependency-graph.md`). **Becomes genuinely selectable once `M2-C05-03` merges or is
  abandoned.**
- **`M2-C06`** (Frontend, P0, `Ready`) — fails part 5. `migration/M2-C06-record-picker-dialog`
  already carries a full `RecordPickerDialog` implementation and its own independently
  validated-`PASS` session close-out (commit `a47d016`), unmerged — dispatched by a concurrent
  session (this runner has no concurrency control; see memory). Re-dispatching would duplicate a
  finished `PASS`.

No other row in `task-tracker.md` reads `Ready`. `nextTaskId` is empty.

### What the next session should do

1. **Check whether `Vivek` has merged any of the unmerged branches above** —
   `migration/M2-C05-03-grid-states-and-export` (this pass), `migration/M2-C06-record-picker-dialog`,
   or `migration/M0-06-remove-default-admin`. A merge to `master` is what actually changes what
   is selectable next: merging `M2-C05-03` releases `M2-C05-02`'s file conflict; merging
   `M2-A02`/`M2-B10` alongside it moves `M2-D01` a step closer (it also needs `M2-C05-03`
   merged, not just `Needs Review`).
2. **If nothing has merged**, re-run the five-part test — it will still fail the same four rows
   for the same reasons, and no new row will have become `Ready` on its own.
3. **Do not** re-dispatch `M2-C06` or the `M0-06` branch's `Blocked` outcome without a merge
   decision first — neither is this session's to make.

### Carried forward — still true, untouched by this pass

- **`M0-04`** (credential rotation runbook) closed `Blocked` on a separate, **unmerged** branch
  (`migration/M0-04-credential-rotation-runbook`) — its own designed terminal state, since no
  human with production access participated. Do not re-dispatch it without first checking
  whether that branch should be merged — a merge decision, not a selection one.
- **`M2-A03`** (`Needs Review`) still needs a human to make the CI job a *required* status check
  on `master`. Owner: Vivek.
- **`M2-B08`**, **`M2-B12-01`**, **`M2-C10`** stay `Blocked` on environment/escalation-budget
  grounds already recorded — untouched by this pass.
- **`M2-D01`** stays `Blocked`: it needs `M2-C05-03`, `M2-A02` and `M2-B10` all merged to
  `master`, and none of the three is (M2-C05-03 is `Needs Review`, unmerged, as of this pass).
- **Q-71, Q-81, Q-82, Q-83, Q-84** (carried from before) plus this pass's new **Q-94** (who
  normalises `ProblemDetails` once M2-C02 lands), **Q-95** (Excel-only vs. Excel/CSV export
  menu — owner scope call), **Q-96** (expose `Content-Disposition`/`X-Correlation-Id` via CORS —
  change request against M2-B06) — none blocks anything today; see `open-questions.md` for
  full text and owners.
- **R-43, R-76 (resolved on `master`), R-77, R-79 (new this pass — CORS header exposure, Low
  impact, cosmetic filename fallback only)** — see `technical-debt-register.md`.

### Environment note carried from the last session that measured it

`node` here was **v22.22.2** as of 2026-08-25; Angular CLI 22.1.5 requires
`^22.22.3 || ^24.15.0 || >=26.0.0`. `nvm install 24` (Node v24.19.0 / npm 11.17.0) was used to
run `lint`/`test:ci`/`build` in the `M2-C05-03` session too. Re-verify at the start of the next
session rather than assuming — this file records what was true then, not a repository change.
