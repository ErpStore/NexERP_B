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

## Selected: `M2-D01` — Currency end-to-end in Angular

Full spec: [`tasks/M2-D01.md`](tasks/M2-D01.md). **Attempt 1 — closed `Blocked`, close-out
recorded 2026-08-26. Do not re-dispatch until the Run State below changes.**

## Run State — STOPPED / BLOCKED (2026-08-26)

`M2-D01` was dispatched on branch `migration/M2-D01-currency-end-to-end` (cut from `master` at
`39a9e11`) and stopped before writing any feature code. Its own *Prerequisites* section
(`tasks/M2-D01.md:160-177`) names `M2-C02`, `M2-C03`, `M2-A04` and `M2-A05` as transitively
required and instructs: *"If any of these is incomplete, stop and report Blocked. Do not stub
the missing piece."* All four remain `Blocked` in `task-tracker.md` (`M2-C02` line 157, `M2-C03`
line 162, `M2-A04` line 114, `M2-A05` line 115), and their frontend artefacts are confirmed
absent from disk: `frontend/nexgen-web/src/app/core/auth/`, `core/http/` and `layout/shell/`
each hold only a `.gitkeep` — `PermissionService`, `requireScreen()`, `*appHasRight` and
`error.interceptor.ts` do not exist. `app.routes.ts:5` still carries `M2-C01`'s comment "No
guard: guards arrive with M2-C02".

`M2-D01`'s three `depends_on` entries (`M2-C05-03`, `M2-A02`, `M2-B10`) genuinely are
`Completed` and merged — the selection pass that picked it (footnote ⁷⁷, `task-tracker.md`) was
correct on that half. The five-part selectability test in `CLAUDE.md` only walks `depends_on`,
not a task file's own narrative *Prerequisites* table, which is why this task read `Ready` when
it was not startable. That process gap is recorded as **`Q-97`** in
[`open-questions.md`](../open-questions.md) for a human or a later runner-policy change to
resolve — it is not this task's to fix.

**Unblocking chain, most upstream first:** `M0-04` (credential rotation runbook — `Blocked`,
owner-only, see below) → `M2-A04` (refresh tokens) → `M2-A05` (CORS/tenant) and, once
`M2-A04`/`M2-A07` land, `M2-C02` (auth/guards/permission store) → `M2-C03` (app shell) →
`M2-D01` becomes startable only once `M2-C02`, `M2-C03`, `M2-A04` and `M2-A05` are all
`Completed` and merged to `master`.

**Named owner of the root blocker:** repository owner **Vivek** — `M0-04` requires
credential-rotation rights only he holds and was deferred by his own 2026-08-19 G0 decision.
Nothing downstream moves until he acts on it or explicitly re-prioritises.

**Validation:** none run — final validator verdict `{"verdict":"none","note":"validation did
not complete"}`, since there was no implementation to validate. Attempts used: 1 of 3.
Escalations: 0.

**No file under `V.SMART/` or `frontend/` changed.** Updated this close-out:
`tasks/M2-D01.md` (Execution Record appended, `status: Blocked`), `task-tracker.md` (row +
footnote ⁷⁸), `runner-state.md` (Status), this file.

### What a resuming session should do

Do **not** re-run the five-part `depends_on` test on `M2-D01` and re-select it — that test
already passes and produced this exact stop. Instead check whether `M2-C02`, `M2-C03`,
`M2-A04` and `M2-A05` have all reached `Completed` **and merged to `master`**. If not all four
have, `M2-D01` is still not startable; look at other `Ready` rows in `task-tracker.md` instead
(see *Carried forward* below).

### Carried forward — still true, untouched by this pass

- **`M2-C05-02`** is genuinely `Ready` and dependency-ready, ranked below `M2-D01` on priority
  alone (P1 vs P0) — now that `M2-D01` cannot proceed, `M2-C05-02` is the natural next
  candidate for a future selection pass, not automatically re-selected by this one (this
  session was instructed to close out only, not start another task).
- **`M2-C06`** (`Needs Review`, `migration/M2-C06-record-picker-dialog`, tip `a47d016`) releases
  nothing and no task names it as a Hard prerequisite — reviewable at leisure, not urgent to
  merge.
- **`M0-04`** (credential rotation runbook) closed `Blocked` on a separate, **unmerged** branch
  (`migration/M0-04-credential-rotation-runbook`) — its own designed terminal state, since no
  human with production access participated. Do not re-dispatch it without first checking
  whether that branch should be merged — a merge decision, not a selection one. It is the root
  of `M2-D01`'s block, above.
- **`M0-06`** (fails part 5, unmerged `Blocked` branch) and **`M0-11`** (fails part 2, `Product
  Decision`, owner-only) remain excluded, unchanged from every prior pass.
- **`M2-A03`** (`Needs Review`) still needs a human to make the CI job a *required* status check
  on `master`. Owner: Vivek.
- **`M2-B08`**, **`M2-B12-01`**, **`M2-C10`** stay `Blocked` on environment/escalation-budget
  grounds already recorded — untouched by this pass.
- **Q-71, Q-81, Q-82, Q-83, Q-84, Q-91, Q-92, Q-93, Q-97** and **R-43, R-76, R-77, R-78** are
  untouched by this pass beyond `Q-97` itself, which this task's block raised.
