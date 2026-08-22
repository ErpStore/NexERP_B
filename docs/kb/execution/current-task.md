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

## ▶ None — no task is dependency-ready

**`M2-C12-04` closed `Needs Review` 2026-08-22, independently validated `PASS`.** It
re-specified the documents/reports batch (`M2-C07`, `M2-C08`, `M2-C08-01`, `M2-C08-02`,
`M2-C08-03`, `M2-C09`) for Angular/ADR-007 on `migration/M2-C12-04-respec` (tip `9d0ccdd`, cut
from `master` at `f8b4dad`). All 8 acceptance criteria independently re-checked and `MET`; the
atomicity rule held for all six files; `dotnet build V.SMART.Api` re-run as a regression check —
0 errors / 6695 warnings, exact baseline. **Not merged, not pushed** — only the repository owner
sets `Completed`. Full record:
[`tasks/M2-C12-04.md` § Execution Record (Close-out)](tasks/M2-C12-04.md#execution-record-2026-08-22--close-out),
[`task-tracker.md`](task-tracker.md) footnote ⁴⁵, [`runner-state.md`](runner-state.md).

One advisory, non-blocking finding carried forward: `M2-C07.md:55` cites **Q-71**, which exists
only on the unmerged sibling `migration/M2-C12-03-respec` (`open-questions.md:72` there), not yet
on `master` — it resolves once that branch merges. No acceptance criterion depends on it.

**Context: `M2-C12-01` (the design-system tree) is `Completed` and merged.** The owner answered
[**Q-70**](../open-questions.md) — the criterion-7 contradiction that blocked it — by rewriting
criterion 7 across all five `M2-C12-*` sub-task files (`f8b4dad`) to state the real footprint
(batch files, the task file itself, the tracker row, and the KB bookkeeping files the work may
touch) with the guard `git diff --name-only master...HEAD | grep -v '^docs/kb/'` returning empty.
`M2-C12-01` was then merged `--no-ff` as `Completed`. See `task-tracker.md` footnote ⁴³.

### Why nothing is dependency-ready

Every `Ready` row in [`task-tracker.md`](task-tracker.md) was checked against CLAUDE.md's
five-part "can actually be done" test:

| Task | `Ready`? | Why excluded |
|---|---|---|
| `M0-06` | Yes | Sibling branch `migration/M0-06-remove-default-admin` already exists |
| `M0-11` | Yes | `Product Decision` — owner-only, never self-selectable |
| `M2-A02` | Yes (gated) | Gated on unanswered **Q-28** |
| `M2-C12-02` | Yes | Sibling branch `migration/M2-C12-02-respec` already exists |
| `M2-C12-03` | Yes | Sibling branch `migration/M2-C12-03-respec` already exists |
| `M2-C12-05` | No — `Blocked` | Runs last, behind all four `M2-C12-0{1..4}`; `M2-C12-04` itself is unmerged |

Confirmed via `git branch --no-merged master` (both `migration/M2-C12-02-respec` and
`migration/M2-C12-03-respec` are present) and by reading each row's `depends_on` against
`task-tracker.md`. Nothing else in the tracker carries status `Ready`.

### What would unblock work

- **Merging any of `M2-C12-04`, `M2-C12-02`'s branch, or `M2-C12-03`'s branch** (once reviewed)
  clears its own sibling-branch exclusion for a future attempt at that same task, and moves
  `M2-C12-05` one step closer to dependency-ready (it needs all four `M2-C12-0{1..4}` `Completed`
  *and* merged).
- **An owner answer to Q-28** (does an API-only-authenticated user ever acquire `UserRight` rows)
  releases `M2-A02`.
- **An owner decision on `M0-11`** (the silent FIFO under-issue product question) releases that
  task directly, being a `Product Decision`.
- `M0-06`'s existing branch (`migration/M0-06-remove-default-admin`) needs review/merge or
  disposal before that task is selectable again.

Nothing here needs execution right now — the blockers are review/merge and owner rulings, not
investigation or implementation.
