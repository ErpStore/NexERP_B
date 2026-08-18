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
last_verified: 2026-08-18
dependencies: [KB-081, KB-082, KB-088]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Active task: `M0-12-01` — `Blocked` on a human (runner fault, not content)

`M0-12-01` — *Create the test project and wire it into CI* — was correctly selected `Ready`
(its sole Hard prerequisite `M0-07` reached `Completed`) and dispatched to the implementer
(attempt 1 of 4, `opus`). **The implementer returned no result** — no diff, no text, no tool
output — so validation could not run
(`{"verdict": "none", "note": "validation did not complete"}`). Verified at close-out: no
`migration/M0-12-01-*` branch exists, no `tests/` directory exists at the repository root,
`git status --porcelain` is clean, and `master`'s tip is unchanged. **Nothing was implemented;
there is nothing to resume mid-way through — the next session either re-dispatches the
implementer or investigates why the dispatch produced nothing.**

Full record: [`tasks/M0-12-01.md` § Execution Record (2026-08-18)](tasks/M0-12-01.md#execution-record-2026-08-18).
Attempt logged: [`failure-log.md` § M0-12-01 · attempt 1](failure-log.md#m0-12-01--attempt-1--2026-08-18).
Status authority: [`task-tracker.md`](task-tracker.md) (KB-081) footnote 12. Runner state:
[`runner-state.md`](runner-state.md) (KB-093).

**Owner to unblock:** **Vivek** (repository owner / migration lead) — check the runner's
dispatch/agent-invocation layer for this cycle before retrying, in case the fault recurs.
Attempts used: 1 of 4; three retries remain. The task specification itself is unchanged and
valid — do not re-plan it, just re-dispatch once the dispatch fault is understood (or found
not to reproduce).

**Do not start a different task from this file.** `M0-12-01` is the narrowest bottleneck in
M0 (four tasks — `M0-12-02`, `M0-13`, `M0-09`, `M0-06` — declare it as their dependency), and
re-selecting past it without resolving the blocker would just reach the same stop again once
those are opened.

## Other open blockers, unaffected by this stop

- **`Needs Review`** — implemented, validated `PASS`, committed on its own branch, awaiting a
  human review-and-merge/sign-off step that no autonomous session may perform on its own
  authority ([KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed)):
  `M0-01-03`.
- **`Blocked` on an unscheduled human**, not on any task: `M0-04` (unidentified owner — tracker
  footnote 4).
- **Transitively `Blocked`** behind `M0-12-01`: `M0-12`, `M0-12-02`, `M0-13`, `M0-09`, `M0-10`,
  `M0-06`, `M0-11`.
- **A parent container**, never worked directly: `M0-01`, `M0-12`.

Full detail on why each is blocked and who the candidate owner is:
[`runner-state.md`](runner-state.md) (KB-093) § *Blocked on* / *Owner to unblock ...* rows,
and [`task-tracker.md`](task-tracker.md) (KB-081) footnotes 1, 4, 12.

## Most recently closed: `M0-14` — Gate `DetailedErrors` on `IsDevelopment()`

Validated `PASS`, `Completed` (Vivek sign-off, merge `275c6e2`). Full record:
[`tasks/M0-14.md` § Execution Record (2026-08-18)](tasks/M0-14.md#execution-record-2026-08-18).
Discoveries from this task that a future session should reuse rather than re-derive:

- **Line numbers in `V.SMART/V.SMART.Web/Program.cs` have shifted again.** The
  `DetailedErrors` assignment is now at line 198 (was 192 before `M0-03-03` landed); the
  `AddRazorComponents().AddInteractiveServerComponents()` registration and the
  tenant/DbContext registrations shifted by the same 6 lines. Always re-read the file before
  citing a line number in it — it is a shared composition root that several M0 tasks touch.
- **`V.SMART/V.SMART.Web/appsettings.json` no longer has a `DetailedError` key** (deleted,
  proven dead — INV-029 amendment, 2026-08-18).
- **Q-16** (deployment topology / `ASPNETCORE_ENVIRONMENT` in production) remains **Unknown**
  — still open, still worth resolving before relying on any `IsDevelopment()` gate in
  production.
