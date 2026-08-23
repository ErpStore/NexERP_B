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

## ▶ M2-C04-02 — Form controls + validation display

**Task file:** [`tasks/M2-C04-02.md`](tasks/M2-C04-02.md) — the form layer of
[KB-051 §Forms](../frontend-new/design-system.md#forms): `form-layout`, `form-section`,
`form-field`, and the input set, as standalone Angular components over PrimeNG and typed
Reactive Forms, plus **one** validation-display mechanism used by every control.

**Why this one.** `M2-C04-01` closed `Completed` and merged to `master` on 2026-08-23
(`a4150e0`, merge `4d4b0c3`) after the owner resolved **R-45** (`endOfLine: "auto"`,
`4af2f4f`); a follow-up commit (`5250328`) released `M2-C04-02` and `M2-C04-03` as `Ready` in
`task-tracker.md`. **`current-task.md` itself was not updated by that commit** — it still
named `M2-C04-01` `Blocked` until this Select pass corrected it; that was a stale pointer, not
a live attempt in progress (confirmed against `task-tracker.md` and `runner-state.md`'s own
Status history before treating `M2-C04-01` as finished).

Three `P0` `Ready` candidates were released by `M2-C04-01`'s merge: `M2-C10` (decimal
handling, 2 d), `M2-C04-02` (4 d) and `M2-C04-03` (3 d). `M2-C04-02` wins rank 2 (most
downstream unblocking, [`dependency-graph.md`](dependency-graph.md) § *Ready-task selection
rule*): it is a named Hard prerequisite of **two** tracker rows (`M2-C05`, `M2-C05-01`),
against **one** for `M2-C10` (`M2-C07`, itself further gated on `M2-C05-01` and unanswered
**Q-71**) and **none** for `M2-C04-03` (a *Soft* dependency only, for `InlineAlert`, with a
documented local-placeholder fallback in `M2-C04-02`'s own task file if `-03` has not landed).
`M2-C04-02` also sits directly on the project's stated critical path
(`M2-C04-01 → M2-C04-02 → M2-C05-01 → M2-C05-03 → M2-D01 → …`), which neither sibling does —
the same tie-break already used to rank `M2-C04-01` over `M2-C10` at the previous Select pass.
Full reasoning: [`runner-state.md`](runner-state.md) Current task, `task-tracker.md` row
`M2-C04-02`.

### Five-part "can actually be done" check

1. Hard prerequisite `M2-C04-01` — `Completed` and merged to `master` (`4d4b0c3`,
   2026-08-23). **Met.**
2. Not a `Product Decision`. **Met** — `task_type: Frontend`.
3. Not blocked on an unanswered open question. **Met.** **Q-69** (whether a re-specification
   may swap an `axe` a11y criterion for a static template lint) is answered and explicitly
   recorded as *not* blocking. `M2-C10` — the decimal module — is a Hard dependency only for
   3 of the task's ~10 controls (`number-input`, `currency-input`,
   `amount-or-percent-input`); the task file itself specifies a non-local fallback if
   `M2-C10` has not merged first (Implementation Steps, step 8), so `M2-C10` being merely
   `Ready` rather than `Completed` does not gate this task.
4. Task file not superseded/stale. **Met** — no ⛔ banner, re-specified for Angular by
   `M2-C12-01` (merged), `last_verified: 2026-08-22`.
5. No sibling branch open on the same files. **Met** — `git branch --no-merged master`
   (checked 2026-08-23) lists no branch touching `frontend/nexgen-web/` or `M2-C04-02.md`;
   unrelated worktrees exist (`M0-03-*`, `M0-04`, `M0-06`, `M2-A08`, `M2-B12-01`), none
   touching this task's files.

### Read before starting

- [`tasks/M2-C04-02.md`](tasks/M2-C04-02.md) in full — dense: scope (`form-layout`,
  `form-section`, `form-field`, the input set), what is explicitly out of scope (`DataGrid`
  M2-C05, `LineItemGrid` M2-C07, overlays/toasts M2-C04-03, the shell M2-C03, dialogs
  M2-C06), and the zoneless/`OnPush` change-detection constraint (no `zone.js` in this
  workspace — every control must be `ChangeDetectionStrategy.OnPush`).
- **Money must not be re-solved here.** `number-input` and `currency-input` delegate to
  `M2-C10`'s decimal module rather than parsing numbers locally — do not invent numeric
  parsing to route around `M2-C10` still being `Ready` rather than merged.
- KB-051 §Forms, §State patterns, §Accessibility commitments — the specification.
- `ADR-007-angular-stack.md` — typed Reactive Forms, validator shapes generated from OpenAPI
  (consumed as hand-written validators in tests only for now — no hand-written schema for a
  real ERP entity), PrimeNG only.
- KB-015 §Forms and validation — what the existing Blazor implementation does today
  (`DataAnnotations` + `EditForm` + `DataAnnotationsValidator`), the behaviour this layer
  must preserve the *intent* of, not translate literally.

### Run State — not yet dispatched

Selected this Select pass (2026-08-23). No branch cut, no implementer dispatched. Next
session should dispatch per [`workflow.md`](workflow.md) (KB-088) rather than re-run Select.

`M2-C10` (`Ready`, `P0`, 2 d) and `M2-C04-03` (`Ready`, `P0`, 3 d) remain genuinely
selectable, independent candidates — neither touches `M2-C04-02`'s files — for a parallel
session; see `runner-state.md` § Next ready task.
