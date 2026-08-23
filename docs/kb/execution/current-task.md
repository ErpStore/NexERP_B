---
doc_id: KB-089
title: Current Task
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: [BR-SO-003]
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

## ▶ M2-C04-03 — Feedback: modal, drawer, toast, empty/loading/error states

**Task file:** [`tasks/M2-C04-03.md`](tasks/M2-C04-03.md) — the overlay/feedback layer of
[KB-051 §Overlays and §Feedback](../../frontend-new/design-system.md#overlays) as standalone
Angular components over **PrimeNG only**: `Modal`, `Drawer`, `ConfirmDialog` (optional
required reason), `Popover`, `Tooltip`, `ContextMenu`, `Toast`, `InlineAlert`, `BusyOverlay`,
`Skeleton`, `ProgressBar`, `EmptyState`, `ErrorState`, `PermissionDeniedState`. Locations
`frontend/nexgen-web/src/app/shared/components/overlay/` and `.../feedback/` — **to be
created**. `BR-SO-003` (mandatory cancellation reason on a Sales Order/line) supplies the
*capability* (`ConfirmDialog`'s reason field); the rule itself stays server-side — this task
never implements it client-side.

**Why this one.** This session (2026-08-23) re-selected after finding `current-task.md` still
pointing at `M2-C10`, which an intervening session had already dispatched and closed
**`Blocked`** (attempt 1 `FAIL`, category `environment` — its binding criterion needs a
*measured* wire format from a live `[Authorize]`d endpoint, and this workstation's
`ConnectionStrings:MasterDb`/`Jwt:Secret` are both empty; not a code defect, owner **Vivek**,
task-tracker.md footnote 52). `task-tracker.md`'s own "Current state" section (line ~268)
states outright: **"Only one task is genuinely selectable: `M2-C04-03`"** — the other three
`Ready` rows all fail the five-part test (`M0-06` already has a branch; `M0-11` is a `Product
Decision`; `M2-A02` is gated on unanswered **Q-28** and **R-65**). Full reasoning:
[`runner-state.md`](runner-state.md) Current task / selection_note, `task-tracker.md` rows
`M2-C10` and `M2-C04-03`.

### Five-part "can actually be done" check

1. Hard prerequisite `M2-C04-01` — `Completed` and merged to `master`. **Met**
   (task-tracker.md line 158). `M2-C01` (also Hard) — `Completed` and merged. **Met.**
2. Not a `Product Decision`. **Met** — `task_type: Frontend`.
3. Not blocked on an unanswered open question. **Met.** No open question gates this task's own
   scope (M2-A06's correlation-id soft dependency has a documented fallback if unmet; see
   task file § Dependencies).
4. Task file not superseded/stale. **Met** — no live ⛔ banner; re-specified for Angular by
   `M2-C12-01` (merged), `last_verified: 2026-08-22`.
5. No sibling branch open on the same files. **Met** — `git branch --no-merged master`
   (checked 2026-08-23, this session) lists no branch touching
   `frontend/nexgen-web/src/app/shared/components/overlay/`,
   `frontend/nexgen-web/src/app/shared/components/feedback/`, or `M2-C04-03.md`.

### Read before starting

- [`tasks/M2-C04-03.md`](tasks/M2-C04-03.md) in full, including § Prerequisites and
  § Dependencies — `M2-C04-02` (form controls) is a **Soft** dependency only: use
  `app-form-field` + `app-textarea` for `ConfirmDialog`'s reason field if they exist
  (`M2-C04-02` closed `Needs Review`, unmerged — check whether it has since merged before
  assuming the real components are available); otherwise use a bare `[pTextarea]` with a
  `TODO` and replace it later. Do not build a parallel field/error mechanism.
- [KB-051 §Overlays, §Feedback, §State patterns](../../frontend-new/design-system.md) — the
  full 14-primitive contract, the seven-state pattern, and Principle 6 ("errors are specific"
  — server messages shown verbatim).
- [ADR-007](../../decisions/ADR-007-angular-stack.md) — PrimeNG only.
- `BsModal.razor` (Confirmed, KB-015 §Shared components) — existing confirm-dialog/reason-box
  reference; `BR-SO-003` is the rule that requires the mandatory reason on Sales Order
  cancellation.

### Run State — not yet dispatched

Selected this Select-only pass (2026-08-23, this session). No branch cut, no implementer
dispatched. Next session should dispatch per [`workflow.md`](workflow.md) (KB-088) rather than
re-run Select.

`M2-C10` is **not** selectable again until the environment blocker clears (a reachable
`MasterDb` + `Jwt:Secret`, or a relaxed wire-format criterion) — owner **Vivek**. No other
`Ready` row passes the five-part test; see `runner-state.md` § Next ready task.
