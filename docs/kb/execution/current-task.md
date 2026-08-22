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
last_verified: 2026-08-22
dependencies: [KB-081, KB-082, KB-088, KB-091, KB-092, KB-093, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## ▶ M2-C12-04 — Re-specify for Angular: documents and reports

**Task file:** [`tasks/M2-C12-04.md`](tasks/M2-C12-04.md) — re-specify `M2-C07.md`, `M2-C08.md`,
`M2-C08-01.md`, `M2-C08-02.md`, `M2-C08-03.md`, `M2-C09.md` for Angular/ADR-007, removing each
file's `⛔ STOP` banner atomically with its React content, the same shape `M2-C12-02` and
`M2-C12-03` already closed clean with.

**Status: selected as the next dependency-ready task. Not started, not dispatched.**

### Why this task, now

- `M2-C12-03` (list/CRUD shell — `M2-C05*`, `M2-C06`) closed this session: **`Needs Review`**,
  independently validated `PASS` on attempt 2 of 4 (attempt 1 `FAIL`, category `regression` — it
  dropped the stack-independent runtime `axe` acceptance criterion, already answered by **Q-69**;
  fixed on the same branch). Branch `migration/M2-C12-03-respec` (tip `1c412ba`), **not merged,
  not pushed** — only the repository owner may set `Completed`
  ([KB-088](workflow.md#who-may-set-completed)).
- Candidates for this pick were `M2-C12-04` and `M2-C12-05`. `M2-C12-05` is `Blocked` behind all
  four `M2-C12-01`..`-04` by design (it owns the whole-tree tracker/`dependency-graph.md`
  restatement and must run last) — not selectable yet, since `M2-C12-04` itself has not started.
  `M2-C12-04` was the only genuine candidate.
- Five-part "can actually be done" test: (1) both Hard prerequisites `M2-C00`, `M2-C01` are
  `Completed` **and merged** (`0da6a35`, `2dd4e53`); (2) not a `Product Decision`; (3) not gated
  on an unanswered open question — **Q-70** (the criterion-7 wording) was answered and applied to
  all five `M2-C12-*` sub-tasks at `f8b4dad`, so it no longer blocks this batch; (4) the task file
  carries no ⛔ banner and is not marked superseded or stale; (5) no sibling branch touches its
  files — `git worktree list` shows `wt-M0-10`, `wt-M2-A08`, `wt-M2-B01`, none on
  `docs/kb/execution/tasks/M2-C0{7,8,9}*.md`; no branch named `M2-C12-04` exists yet
  (`git branch -a` checked).
- Frontmatter: `task_type: Documentation`, `complexity: MEDIUM`, `risk: LOW` (explicit, wins over
  derivation) — the identical shape `M2-C12-01`..`-03` carried.

### What the implementer needs to know before starting

- **Read [`M2-C12`](tasks/M2-C12.md) first** — it holds the translation table, rationale,
  out-of-scope list and failure history this sub-task narrows rather than repeats.
- **The atomicity rule is absolute:** a file's `⛔` banner may only be removed in the same change
  that removes its React content. If a file cannot be finished, leave its banner in place and
  report it as not done — half-done is worse than not started. `M2-C12`'s unsplit attempt 1 failed
  exactly this way across 23 files; `M2-C12-03` attempt 1 did **not** repeat that failure, but it
  did drop an unrelated acceptance criterion (`axe`) — read the note below before assuming "no
  banner left behind" is the only regression class to watch for.
- **Carry forward from `M2-C12-03`'s close-out, do not re-derive:**
  - The `axe` runtime-accessibility acceptance criterion is **stack-independent and must survive**
    re-specification, translated only in *how* (an `a11y.spec.ts`/`@testing-library/angular` under
    the existing `npm run test:ci`, no new command or dependency). **Q-69** is answered — do not
    re-raise it, do not drop the criterion on the true-but-incomplete premise that
    `axe-core` isn't installed today; check the `depends_on` chain to `M2-C04-01`
    (`M2-C04-01.md:388`) first. Worked examples on `master`: `M2-C04-02.md:391-392,408`; on this
    branch: `M2-C05-01.md`, `M2-C05-02.md`, `M2-C05-03.md`, `M2-C06.md`.
  - **Q-71** ([`open-questions.md`](../open-questions.md)) — `ADR-007-angular-stack.md:98` promises
    a resolution ("`LineItemGrid` re-evaluated, see below") that does not exist in the document; a
    full-file grep finds `LineItemGrid` only at `:98` and `:206`. **This batch is where it is
    likely to bite**, since `M2-C07` is the task that names `LineItemGrid`. **Do not infer a table
    technology from a dangling pointer.** The nearest resolving prose is `:144-152`
    (PrimeNG-over-headless reasoning, AG Grid named as `M2-C07`'s fallback) — cite that directly,
    as `M2-C12-03` did for `DataGrid`, rather than following `:98`'s broken "see below". If the
    fallback decision cannot be made without resolving the ADR gap itself, stop and raise it
    against Q-71 rather than guess; accepted ADRs are immutable, so even a pointer fix is the
    owner's call.
  - Every `frontend/` path must be `frontend/nexgen-web/…`; every quoted command must appear
    verbatim in [KB-083's verified Angular table](prompt-template.md#verified-repository-commands)
    — no `npm run test` (only `test:ci` is verified), no coverage command (none exists).
  - `depends_on`, `business_rules`, `priority`, `estimate` and the Gate/Priority/Estimate table
    row must stay byte-unchanged in every file; prove it by diffing, don't assert it.
  - Every `V.SMART/` `file:line` citation must stay byte-unchanged, or its change must be
    justified individually in the Execution Record.
  - **Diff confinement (criterion 7's corrected wording):** the six/seven-path footprint is
    normal and compliant — the batch files, this task file (`M2-C12-04.md`, whose Execution
    Record is where the atomicity-grep output is quoted, an *Always* update per KB-088 §4),
    `task-tracker.md`'s own row for this sub-task, and only if raised, `open-questions.md`,
    `failure-log.md`, `runner-state.md`, `current-task.md`. Nothing under `V.SMART/`, `tests/`,
    `frontend/` or `.github/`.
- **Branch fresh from `master`:** `migration/M2-C12-04-respec`, cut from `master` (currently
  `f8b4dad` — re-verify at start, since `M2-C12-01`/`-02`/`-03` are all unmerged and do not move
  it). One task, one branch.
- **Never merge or push.** Status on completion is `Needs Review`, not `Completed` — only the
  repository owner may set `Completed` ([KB-088](workflow.md#who-may-set-completed)).

### What this leaves selectable, unchanged by this close-out

`M2-C12-05` stays `Blocked` behind all four `M2-C12-01`..`-04` by design. `M2-C11` remains
`Blocked` on **Q-38** (owner ruling on what it is for under ADR-007) — deliberately not
re-specified by `M2-C12-02`. `M2-A02` (Q-28, R-65), `M2-A04` (unrecorded block, needs owner
ruling), `M0-06` (branch already exists), `M0-11` (Product Decision, owner-only), `M2-B05`
(Blocked, awaiting re-specification onto R-66), `M2-B12-01` (Blocked, escalation budget
exhausted), `M0-01-03` (merged, `Needs Review`, awaiting a named operator for runbook §7) are all
unchanged; see [`task-tracker.md`](task-tracker.md) for current status on each.
