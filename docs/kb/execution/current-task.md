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
last_verified: 2026-08-19
dependencies: [KB-081, KB-082, KB-088]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Active task: **M0-12-02** — Characterisation tests for `CalculationService`

Full spec: [`tasks/M0-12-02.md`](tasks/M0-12-02.md). Type Testing, P0, estimate 2.5 d, Gate
G0. Its sole Hard prerequisite, `M0-12-01`, is `Completed` and merged (`bdee81f`,
2026-08-19) — `dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj` works.

**Why this task, not another.** Selection rule:
[`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).
Of the tasks the `M0-12-01` merge released (`M0-12-02`, `M0-13`, `M0-09`, `M0-06`), `M0-13`
was implemented in the immediately preceding session (see below) and the remaining
candidates are `M0-12-02` (P0), `M0-09` (P1) and `M0-06` (P1). P0 ranks first, so
`M0-12-02` is selected — no further tie-break needed.

**What it does.** Pins the exact current output of `CalculationService.UpdateTotalsAsync`
(`V.SMART/V.SMART.Shared/Services/CalculationService.cs:12-114`) across all nine numbered
steps, both tax branches (intra-state CGST+SGST vs inter-state IGST), the rounding
behaviour, and the boundaries it handles silently — **without modifying
`CalculationService.cs` under any circumstances**. Same shape as the just-completed
`M0-13`: a green suite that records current behaviour, including anything surprising, with
test names that make the behaviour legible to a non-engineer.

**Not blocked on a database fixture.** `CalculationService` has no constructor dependencies
(`CalculationService.cs:10-12` — confirmed no declared constructor, no fields) — it is a
pure unit. Even if a future session finds the InMemory EF fixture unusable, this task is
unaffected; it needs only the test project M0-12-01 created.

**Siblings, not conflicts.** `M0-13`, `M0-09` and `M0-06` all branch from the same
`M0-12-01` parent but touch different files
(`InventoryService/StockManagerService.cs`, `SalesService/MfgPoService.cs`, seed data
respectively) — do not edit their files from this task, and nothing in this task's scope
overlaps theirs.

**Read before starting:** [`tasks/M0-12-02.md`](tasks/M0-12-02.md) in full — it names two
narrow corrections this task must write back to KB-030 (a citation range and the
rounding-only-at-step-9 fact) that are not yet recorded, plus R-15 (GST rate coerced to
zero) from KB-060.

## Most recently closed: `M0-13` — Characterisation tests for `StockManagerService`

**`Needs Review`, not `Completed`** — implemented on
`migration/M0-13-stockmanagerservice-characterisation` (commit `9d8d7be`), validated
`PASS` (`scopeOk: true`, `failureCategory: none`, all 12 acceptance criteria `MET`), zero
regressions. Only the repository owner may promote it to `Completed`
([KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed)) — the branch is
unmerged. Full record:
[`tasks/M0-13.md` § Execution Record (2026-08-19)](tasks/M0-13.md#execution-record-2026-08-19).

**What it did.** 25 new tests (suite 11 → 36, all green, run twice) pinning all 16
statements of BR-STK-001 (FIFO allocation) and BR-STK-002/R-07 (the silent under-allocation
defect) — the defect is asserted as current behaviour, **not fixed**. `git diff --stat`
showed zero files under `V.SMART/`. KB-030, KB-060, KB-004 (Q-01) and the investigation
registry (INV-011 annotated; new row **INV-036**, the general recipe for testing an
EF-backed service through `IUnitOfWork`) were all updated in the same commit.

**What a future session needs from this, if relevant to its work:**

- **INV-036** (`docs/kb/investigation-registry.md`) records the harness recipe any future
  service-characterisation task should reuse: mock `IUnitOfWork`, back only the repository
  properties the service touches with **real** repositories over **one** real
  `ApplicationDbContext` (EF Core InMemory, per INV-031), and make the mock's `SaveAsync`
  forward to `context.SaveChangesAsync()` — `Repository<T>` never persists on its own.
  `M0-12-02` will not need this (no database dependency), but any later
  `IStockManagerService`-adjacent or repository-backed test should read it first.
- **Three negative results are recorded, not just implied:** the InMemory provider cannot
  pin FIFO tie-breaking on an identical `AddDate` (it sorts stably; SQL Server does not),
  SQL-Server-vs-InMemory null-equality agreement on `RcSubID`, or `[Precision]` rounding —
  all three need a real SQL Server instance, which no test infrastructure in this
  repository has.
- **`M0-11` (the Q-01 product decision) stays `Blocked`** until `M0-13` is reviewed and
  genuinely `Completed` — the selection rule requires a Hard prerequisite to be
  `Completed`, not merely `Reviewed`. Do not select `M0-11` believing `M0-13`'s green
  validation alone clears it.

## Other open blockers, unaffected by this change

- **`Needs Review`** — implemented, validated, committed on its own branch, awaiting a
  human review-and-merge/sign-off step that no autonomous session may perform on its own
  authority ([KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed)):
  `M0-01-03`, `M0-13`.
- **`Blocked` on an unscheduled human**, not on any task: `M0-04` (unidentified owner —
  tracker footnote 4).
- **`Blocked`, transitively:** `M0-11` (behind `M0-13`'s merge), `M0-10` (behind `M0-09`,
  itself `Ready`), `M0-05` (behind `M0-04`).
- **A parent container**, never worked directly: `M0-01`, `M0-12`.

Full detail on why each is blocked and who the candidate owner is:
[`runner-state.md`](runner-state.md) (KB-093) § *Blocked on* / *Owner to unblock ...* rows,
and [`task-tracker.md`](task-tracker.md) (KB-081) footnotes 1, 4, 13.

> **This does not open M2.** Gate G0 still has zero of seven exit criteria ticked.
> `M0-01-03`'s rebuild drill, `M0-07`'s CI branch-protection criterion and `M0-04`'s
> credential rotation remain human-owned and unchanged by this session.
