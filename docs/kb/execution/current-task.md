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

## No task is `Ready`

As of 2026-08-18 (close-out of `M0-14`), the *Ready-task selection rule*
([KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)) finds
**no** candidate in [`task-tracker.md`](task-tracker.md) (KB-081). Do not start work from
this file until one of the items below is resolved and re-selection finds a candidate.

Every M0 task is now one of (see [`task-tracker.md`](task-tracker.md) for the authoritative
table — this groups it only to explain why nothing is selectable):

- **`Completed`**: `M0-00`, `M0-15`, `M0-08`, `M0-03-01`, `M0-01-01`, `M0-01-02`.
- **`Needs Review`** — implemented, validated `PASS`, committed on its own branch, awaiting a
  human review-and-merge/sign-off step that no autonomous session may perform on its own
  authority ([KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed)):
  `M0-01-03`, `M0-03-02`, `M0-03-03`, `M0-14`.
- **`Blocked` on an unscheduled human**, not on any task: `M0-02` (DBA — see tracker footnote
  6), `M0-04` (unidentified owner — footnote 4), `M0-07` (repository owner / DevOps GitHub
  access — footnote 7).
- **Transitively `Blocked`** behind `M0-07` or `M0-12-01`: `M0-12`, `M0-12-02`, `M0-13`,
  `M0-09`, `M0-10`, `M0-06`, `M0-11`.
- **A parent container**, never worked directly: `M0-03`, `M0-01`, `M0-12`.

## What actually unblocks the next task

This is a genuine stop for a human, not a defect to work around:

1. **Review and merge/sign off** `M0-01-03`, `M0-03-02`, `M0-03-03` and/or `M0-14` — each is
   pure review, no new implementation work, and each Completed review both closes that task
   and may make a currently-`Blocked` downstream task `Ready`. See each task file's
   Execution Record for what to check.
2. **Identify and engage the human owner** for `M0-02` (DBA with `VIEW DEFINITION` on ≥2
   tenant databases), `M0-04` (owner of production SQL / GST gateway credentials), or `M0-07`
   (repository owner / DevOps with `origin` push and GitHub org admin rights).

Full detail on why each is blocked and who the candidate owner is:
[`runner-state.md`](runner-state.md) (KB-093) § *Blocked on* / *Owner to unblock ...* rows,
and [`task-tracker.md`](task-tracker.md) (KB-081) footnotes 1, 4, 6, 7, 8, 9, 10.

## Most recently closed: `M0-14` — Gate `DetailedErrors` on `IsDevelopment()`

Validated `PASS`, closed out as `Needs Review` (awaiting merge, per above). Full record:
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
