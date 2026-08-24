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
last_verified: 2026-08-24
dependencies: [KB-081, KB-082, KB-088, KB-091, KB-092, KB-093, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## ▶ No task is currently selectable

`M2-C13` (defer the confirm-dialog host, bring the initial bundle back inside Angular's 600 kB
warning budget, R-69) closed **`Needs Review`** this session (2026-08-24) — independently
validated `PASS` on attempt 1 of 3, `scopeOk: true`, `failureCategory: none`, 0 escalations.
Branch `migration/M2-C13-defer-confirm-host` (tip `3e821cc`), left unmerged, unpushed, for owner
review. Measured: initial bundle **711.75 kB → 571.20 kB raw / 158.24 kB → 136.72 kB transfer**,
no more budget warning; R-69 marked resolved. Full record:
[`tasks/M2-C13.md`](tasks/M2-C13.md) § Execution Record (2026-08-24),
[`task-tracker.md`](task-tracker.md) footnote ⁵⁶, [`runner-state.md`](runner-state.md).

**Re-checked against the five-part "can actually be done" test** ([KB-082 § Ready-task
selection rule](dependency-graph.md#ready-task-selection-rule)) and confirmed unchanged since
the prior session: none of the remaining `Ready` rows in `task-tracker.md` clears it.

| Task | Ready? | Why it fails the five-part test |
|---|---|---|
| `M0-06` — remove the seeded default Administrator credential | `Ready` | Fails part 5: a sibling branch already exists (`migration/M0-06-remove-default-admin`, confirmed via `git branch --no-merged master` 2026-08-24). |
| `M0-11` — Product decision: silent FIFO under-issue (Q-01) | `Ready` | Fails part 2: `task_type: Product Decision`, owner-only, never self-selectable. |
| `M2-A02` — apply to `CurrencyController` + denial tests | `Ready` (gated) | Fails part 3: gated on unanswered **Q-28** and **R-65**. |

Everything else in the tracker is `Blocked`, `In Progress`, `Not Started`, or already
`Completed`/`Needs Review`. This is a **person-level** stall, not an execution-capacity one —
see `task-tracker.md` § Current state (2026-08-24) for the outstanding owner decisions, in
order of how much each unblocks:

1. **`M0-04`** — rotate the exposed credentials (deferred to end-of-milestone 2026-08-19).
   Unblocks `M2-A04` → `M2-A05` → `M2-C02` → `M2-C03`, and G0 criteria 2/3.
2. **`M2-C10`'s environment** — a reachable DB + credential, or relax its "MEASURED wire
   format" criterion to static analysis. Unblocks `M2-C10`, then `M2-C07`.
3. **Q-28 + R-65**. Unblocks `M2-A02` → `M2-A03`, `M2-B03` → `M2-B10`.
4. **Q-38** — what `M2-C11` is *for*, now `M2-C01` has built the workspace it existed to
   adopt. Unblocks `M2-C11`.
5. **Owner review and merge of unmerged `PASS`/`Needs Review` branches** — several sit ready
   for review and merging any of them may release further `Blocked` tasks (e.g.
   `M2-C05`/`M2-C05-01` need `M2-C04-02` merged, not just `Needs Review`). This session adds
   `migration/M2-C13-defer-confirm-host` (tip `3e821cc`, `PASS`) to that list — nothing in the
   dependency graph names `M2-C13` as a prerequisite, so merging it releases no other task, but
   it removes the last thin margin on the initial-bundle budget. See `task-tracker.md` §
   "Unmerged branches still carrying work" for the current list.

### What a future session should do here

- **Do not re-run Select** against the same three `Ready` rows without a state change —
  nothing about them has changed since 2026-08-23. Check `git branch --no-merged master` and
  `task-tracker.md` § Current state first; if one of the decisions above has been made,
  re-derive selectability from that, not from this file's stale snapshot.
- **Offer the owner a documentation-only task ahead of an unmet gate** rather than stalling,
  per standing guidance — e.g. re-specifying or investigating something that does not need a
  `Ready` row to proceed, if one exists and is worth doing.
- If the owner merges any unmerged `Needs Review`/`PASS` branch (`M2-C04-02`, `M2-C13`,
  `M2-C12-03`, `M2-C12-04`, or others), re-run the five-part test — a merge is the only event
  that changes this file's answer.

### Carried forward from `M2-C13`, for whoever revisits the overlay layer or the app shell

- **R-69** (`docs/kb/risks/technical-debt-register.md`) is now `RESOLVED` — the
  confirm-dialog host is deferred behind `@defer (when confirmHostRequested())`, and the two
  facts it recorded still bind: import both overlay hosts **from their own files**, never the
  `shared/components` barrel (a barrel import takes the initial chunk to ~1.31 MB, a failing
  build); PrimeNG's `requireConfirmation$` is a plain `Subject`, so any future overlay wired
  the same way needs the same pre-mount-queue treatment `ConfirmDialogService` now has.
- `<p-toast>` was deliberately left eager — the confirm-host deferral alone cleared budget, so
  the toast host's own lost-emission hazard (fire-and-forget, no promise to strand, but still a
  silently dropped message) was not taken on. Whoever next touches the toast path should reread
  that reasoning before assuming it, too, should be deferred.
- **R-70**–**R-72** (unrelated to `M2-C13`, carried from `M2-C04-03` and still open): a
  duplicated jsdom fixture across `form/` and `overlay/`; measured PrimeNG 22.1 ARIA/keyboard
  defects worked around per-component; a stale React remnant in KB-051's prose.
- `M2-C05-03`, `M2-C06`, `M2-C08` all consume the overlay primitives once the relevant branches
  merge; `M2-C03` (app shell) is transitively blocked behind `M0-04` and is not imminent
  regardless of the bundle-budget fix.
