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

## ▶ M2-C12-03 — Re-specify for Angular: the list and CRUD shell

**Task file:** [`tasks/M2-C12-03.md`](tasks/M2-C12-03.md) — re-specify `M2-C05.md`,
`M2-C05-01.md`, `M2-C05-02.md`, `M2-C05-03.md` and `M2-C06.md` for Angular/ADR-007, removing
each file's `⛔ STOP — this specification is superseded` banner in the same change that removes
its React content (the atomicity rule — see below).

**Why this one.** `M2-C12-02` (auth, app shell, decimal handling, pilot-adoption) closed
`Needs Review` 2026-08-22 on `migration/M2-C12-02-respec`, independently validated `PASS`
(all 8 acceptance criteria `MET`, `scopeOk: true`). `M2-C12-03` and `M2-C12-04` are the two
remaining `Ready`, `P0`, 1 d siblings — same `depends_on: [M2-C00, M2-C01]` (both `Completed`
and merged), disjoint `source_files` from each other and from `M2-C12-02`'s batch, and from the
three other live worktrees (`wt-M0-10`, `wt-M2-A08`, `wt-M2-B01` — none touch
`docs/kb/execution/tasks/M2-C0{5,6,7,8,9}*.md`, verified via `git worktree list` 2026-08-22).
Tied on every ranking signal; `M2-C12-03` picked as the next sequential batch after `-02`,
consistent with the pattern the prior two closes used. **`M2-C12-04` is flagged as a tied,
equally-selectable candidate, not silently dropped** — a session with spare capacity could take
it too. `M2-C12-05` stays `Blocked` behind all four sub-tasks by design: it owns the whole-tree
`task-tracker.md`/`dependency-graph.md` restatement and must run last.

**Read [`M2-C12`](tasks/M2-C12.md) first** — it holds the translation table, rationale,
out-of-scope list and failure history that `M2-C12-03` narrows rather than repeats. Also read
the **atomicity rule** (`M2-C12-03.md` — *"A file's ⛔ banner may only be removed in the same
change that removes its React instructions... If you cannot finish a file, leave its banner in
place and report it as not done."*) and the completed **`M2-C12-01`**/**`M2-C12-02`** Execution
Records as the worked precedent for exactly which greps, frontmatter diffs and citation checks
each acceptance criterion expects.

### Five-part "can actually be done" check

1. Hard prerequisites `M2-C00`, `M2-C01` — both `Completed` and merged. **Met.**
2. Not a `Product Decision`. **Met** — `task_type: Documentation`.
3. Not blocked on an unanswered open question. **Met** — nothing in `open-questions.md` names
   `M2-C05*`/`M2-C06` as gated.
4. Task file not superseded/stale. **Met** — no ⛔ banner on `M2-C12-03.md` itself, `status:
   Ready`, `last_verified: 2026-08-22`.
5. No sibling branch open on the same files. **Met** — checked above.

### What `M2-C12-02` leaves for this task to reuse, not rediscover

- The **atomicity-rule grep** (`grep -niE 'mantine|tanstack|zustand|react hook form|\bzod\b|
  \.tsx|jsx|axios|\bmsw\b|vite'`) will very likely hit the literal substring `vite` inside
  **Vitest** — the current, verified Angular test runner (KB-083). That is not a live
  wrong-stack instruction; both `M2-C12-01` and `M2-C12-02` recorded the identical false
  positive and explained why in their Execution Records. Quote the output; don't suppress it.
- KB-083's verified Angular command table (`prompt-template.md` § Verified repository commands)
  has **no `npm run coverage` script**. If `M2-C05*`/`M2-C06` specify a numeric coverage
  criterion the way `M2-C10` did, `M2-C12-02` restated it as an enumerated per-export test
  obligation rather than inventing a coverage gate that doesn't exist — the same move is
  available here if the same gap appears.
- `frontend/vsmart-erp/` (the old pilot) may legitimately appear in "files that must not
  change" or as a citation source — it is the subject of the still-open **Q-38**
  (`open-questions.md:70`), unrelated to and unaffected by this batch. Do not resolve Q-38 here;
  it belongs to `M2-C11`, which stays `Blocked` until the owner rules.
- The KB-090 required-section set, the `re-specification note` shape
  (`tasks/M2-C01.md:28`), and the criterion-7 footprint guard
  (`git diff --name-only master...HEAD | grep -v '^docs/kb/'`, expected empty, now the corrected
  wording after Q-70 was answered) all apply unchanged — see `M2-C12-02.md`'s Execution Record
  for a worked example of every one.

### Do not start this task in this session

This hand-off is a **close-out and selection record only** — the prior session's instruction was
close out `M2-C12-02` and select next, **not implement**. A future session picks this up at
Select/Investigate per [`workflow.md`](workflow.md) (KB-088) and
[`autonomous-runner.md`](autonomous-runner.md) (KB-091).
