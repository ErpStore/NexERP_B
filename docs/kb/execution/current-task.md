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
last_verified: 2026-08-20
dependencies: [KB-081, KB-082, KB-088, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Active task — `M2-C04-02` — Form controls + validation display

**Task file:** [`tasks/M2-C04-02.md`](tasks/M2-C04-02.md).

**Status:** `Ready`. Not yet started — no branch exists yet, attempt 0 of 3.

### Why this task, now

`M2-A01-03` (per-request rights caching) closed this session as **`Needs Review`, not
`Completed`** — it is implemented, validated `PASS`, and committed on
`migration/M2-A01-03-rights-cache`, but unmerged. Per [KB-088 "Who may set
COMPLETED"](workflow.md#who-may-set-completed) and the [Ready-task selection
rule](dependency-graph.md#ready-task-selection-rule) step 1, a Hard prerequisite must be
genuinely `Completed`, not `Needs Review`, to release its dependents. So `M2-A02`, `M2-A07`
and `M2-A08` — the three tasks that name `M2-A01-03` as their sole `depends_on` — **remain
`Blocked`** until that branch is reviewed and merged. Selection falls back to the P0 `Ready`
pool that was already sitting unclaimed before `M2-A01-03` was picked:

- **Step 1 (candidate set).** P0, `Ready`, not a parent container, no unanswered
  Information dependency, no unscheduled human step: `M0-01-03`, `M2-B04`, `M2-B12-01`,
  `M2-C04-02`, `M2-C04-03`, `M2-C10`. (`M2-B09` is `Ready` too but P1, ranked after all of
  these.) No same-file conflict — working tree at `master` tip aside from this task's own
  bookkeeping commits, no other `M2-*` branch in flight.
- **Step 2 (most downstream unblocking).** Counted by grepping every row's `Depends On`
  column in `task-tracker.md` for tasks that would become genuinely `Ready` (all *other* Hard
  prerequisites already `Completed`) once the candidate merges — not merely tasks that name
  it:
  - `M2-C04-02` → releases `M2-C05-01` (`depends_on: M2-C04-02, M2-B02`; `M2-B02` is
    `Completed`, so `M2-C04-02` is the only thing still missing). `M2-C05` also names
    `M2-C04-02`, but `M2-C05` is a parent container (never worked directly) so it does not
    count.
  - `M2-B12-01` → releases `M2-B12-02` (`depends_on: M2-B12-01` only).
  - `M0-01-03`, `M2-B04`, `M2-C04-03` → release nothing (no row anywhere names them as a Hard
    prerequisite).
  - `M2-C10` → releases nothing outright: its one nominal dependent, `M2-C07`, also needs
    `M2-C05-01`, which is not `Completed`.
  - `M2-C04-02` and `M2-B12-01` tie at **one** real release each.
- **Step 3 (critical path) breaks the tie.** The [project critical
  path](dependency-graph.md#project-critical-path) runs `...M2-A03 → M2-C05-01 →
  M2-C05-03 → M2-D01...`. `M2-C05-01`'s only remaining gate is `M2-C04-02`, so `M2-C04-02` is
  on the critical path by construction even though the path diagram elides the intermediate
  step. `M2-B12-01` / `M2-B12-02` are not drawn on the critical path at all — `M2-B12` is a
  side investigation.
- `M2-C04-02` wins outright. No further tie-break needed.

### What this task does

Builds the shared form layer specified by KB-051 §Forms — `FormLayout`, `FormSection`,
`FormField`, and the input control set — over Mantine 7, React Hook Form and Zod (ADR-003),
plus one validation-display mechanism every control uses. Frontend-only, in
`frontend/nexgen-web/src/shared/components/form/` (path proposed by the task, directory
convention from KB-050). Builds no ERP screen, no `DataGrid` (M2-C05), no editable grid
(M2-C07), no overlays/toasts (M2-C04-03), no business validation rule — those stay in the
server per the standing constraint. Full detail, acceptance criteria, and testing
requirements: [`tasks/M2-C04-02.md`](tasks/M2-C04-02.md).

### Read before starting

- **Pure frontend, no backend dependency.** `depends_on: [M2-C04-01]` only (design tokens,
  already `Completed`). It needs no API, no database, no `V.SMART.Api`/`V.SMART.Shared`
  change at all — do not touch those trees.
- **This task does not implement business validation.** Rule logic stays server-side per
  `CLAUDE.md`'s standing constraints; this task builds the *display* mechanism for whatever
  validation errors the server (or Zod schema mirroring a server contract) produces.
- **`M2-C05` and `M2-C05-01` are the reason this task is prioritized now** — `M2-C05-01`'s
  only remaining gate is this task, and `M2-C05-01` sits on the critical path. Do not let that
  urgency pull `DataGrid`/grid work into this task's scope; `M2-C05-01` is a separate task.
- Two tasks — `M2-C04-02` and `M2-C04-03` — both depend only on `M2-C04-01` and are
  independent of each other (different concerns: forms vs. overlays/toasts). If capacity
  allows a second concurrent stream, `M2-C04-03` is available in parallel; this file names
  only `M2-C04-02` as the one to execute now.

### Do not

Build any ERP screen, `DataGrid`, editable grid, overlay/toast, or the app shell. Do not
touch `V.SMART.Api` or `V.SMART.Shared`. Do not start `M2-C05`, `M2-C05-01`, `M2-C04-03`, or
any other task once this one closes. Do not merge or push the `M2-A01-03` branch left over
from the previous session — it is unrelated to this task and awaits owner review.

---

## Carried forward from `M2-A01-03`'s close-out (still relevant)

- **`migration/M2-A01-03-rights-cache` (tip `0fde6fb`) is `Needs Review`, validated `PASS`,
  unmerged.** It blocks nothing this task needs, but until it merges, `M2-A02`, `M2-A07` and
  `M2-A08` stay `Blocked` — do not treat them as available even though `task-tracker.md` will
  show `M2-A01-03` progressing.
- **Q-29** (60 s post-revocation staleness window) — engineering half settled (TTL,
  absolute expiration, startup guard, zero-TTL bypass all implemented and tested); the
  product half — is a 60 s window acceptable — is still open for the repository owner.
  Unrelated to this task.
- **R-41** (`docs/kb/risks/technical-debt-register.md`) — the API's rights cache has no
  `SizeLimit` cap, deliberately deferred. Unrelated to this task.
- **Q-27 / Q-28** (duplicate `UserRight` rows; API-only login never seeds rights) remain
  **Unknown** / open for `M2-A02`. Unrelated to this task.

## Ready and unclaimed once `M2-C04-02` closes

Selection rule: [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).
Listed for whoever plans the task after this one — not to be started now.

| Task | What | Est. | Note |
|---|---|---|---|
| `M2-C05-01` | Server-paged table core | 4 d | Released by this task (its only other gate, `M2-B02`, is already `Completed`) |
| `M2-C04-03` | Modal, drawer, toast, states | 3 d | `Ready` now, independent of this task, zero tracked dependents |
| `M2-B12-01` | INV-012 document-numbering + financial-year investigation | 2 d | `Ready`, one dependent (`M2-B12-02`) |
| `M0-01-03` | SP deployment script + rebuild runbook | 1 d | `Ready`²¹, zero tracked dependents |
| `M2-B04` | Decouple `IApprovalService` + 13 `Pages` refs | 1 wk | `Ready`, zero tracked dependents |
| `M2-C10` | Decimal handling — no float money arithmetic | 2 d | `Ready`; dependent `M2-C07` also needs `M2-C05-01`, not yet `Ready` |
| `M2-B09` | Reference-data endpoints + caching | 3 d | `Ready`, P1 |
| `M2-A02` | Apply `[RequireScreen]`/`[RequireRight]` to `CurrencyController` + denial tests | 1 d | `Blocked` until `migration/M2-A01-03-rights-cache` is reviewed and merged |
| `M2-A07` | `GET /api/v1/me` | 2 d | `Blocked`, same reason |
| `M2-A08` | Row-level scoping + account gates (Q-05…Q-08) | 3 d | `Blocked`, same reason |
