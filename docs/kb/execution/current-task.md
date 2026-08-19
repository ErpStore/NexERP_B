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

### Run State — `Blocked` on the repository owner, 2026-08-19

**Do not re-dispatch this task expecting a different result.** It is implemented on
`migration/M0-12-02-calculationservice-characterisation` (`050f06b`), attempt 1 of 3, 0
escalations, validator verdict `FAIL`/`failureCategory: environment`. **11 of 12 acceptance
criteria are objectively `MET`** — re-run and re-derived independently, not taken from the
implementer's report: `dotnet test` → 73/73 passing (twice); `dotnet build V.SMART.Api
--no-incremental` → 0 errors, 6,694 warnings (at the 6,695 baseline); `git diff --stat
master...HEAD` → 9 files, zero under `V.SMART/`; all 19 BR-CALC-001 rows and all 3
BR-CALC-002 rows covered by named tests; both tax branches; both `.5` rounding midpoints and
a negative `RoundOff`; the silent early returns with twelve fields asserted unmutated; the
fixed-vs-percentage header-discount asymmetry; the unlisted-GST-rate/R-15 pair; exact
`decimal` comparison throughout (grepped, no `double`/tolerance/parsed string); KB-030,
KB-060, KB-004 (new Q-23, Q-24) and KB-003 all updated in-commit.

**The twelfth — criterion 8's second half, "the suite passes in CI on the branch" — is
unmeetable from inside an execution session.** It requires pushing the branch to `origin` so
`ci.yml` runs on a hosted GitHub Actions runner. `git ls-remote --heads origin` lists eight
branches and this one is not among them. `CLAUDE.md` § Standing constraints forbids pushing
without an explicit in-conversation instruction, and this dispatch's `allow_push` was
`false`. This is the same wall already hit by `M0-07` (signed off `Completed` with the gap
open, `d79e1a4`) and by `M0-12-01` (resolved only once the owner explicitly authorised the
push, **Q-22**). A diagnosis pass reproduced the identical result independently and agrees:
`disposition: blocked`, no code or test file touched, no fix applied.

**What resumes this task:** the repository owner (Vivek) choosing (A) explicitly authorise
pushing `migration/M0-12-02-calculationservice-characterisation` and observe the
`Test - V.SMART.Shared.Tests` CI step green, or (B) waive criterion 8's "in CI" half and
re-home it, per the M0-07 precedent. Neither option requires re-implementing anything —
`050f06b` already stands. Full record:
[`tasks/M0-12-02.md` § Execution Record (2026-08-19)](tasks/M0-12-02.md#execution-record-2026-08-19),
[`failure-log.md` § M0-12-02 · attempt 1](failure-log.md#m0-12-02--attempt-1--2026-08-19) and
its diagnosis entry, [`task-tracker.md`](task-tracker.md) footnote 14,
[`runner-state.md`](runner-state.md) (KB-093).

**`M0-09` (P1, 0.5 d) and `M0-06` (P1, 1 d) remain independently `Ready`** and are not
blocked by this — `M0-12-02` is not a Hard prerequisite for either. A later session may
select one of them under the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
if the owner has not yet resolved `M0-12-02`, but this close-out does not make that call.

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

**`Completed` and merged (`3f6dfa8`, 2026-08-19)** on the owner's in-conversation
instruction. Implemented on `migration/M0-13-stockmanagerservice-characterisation` (commit
`9d8d7be`), validated `PASS` (`scopeOk: true`, `failureCategory: none`, all 12 acceptance
criteria `MET`), zero regressions. `dotnet test` re-run on `master` **after** the merge:
**36 passed, 0 failed.**

> **This released `M0-11`, and it is now blocked on the owner rather than on a task.** The
> Q-01 product decision on silent FIFO under-issue has its dependency clear, but rule 1 of the
> [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) keeps a
> human-decision task `Blocked` with a named owner — **no runner may self-select it.**
> `M0-13`'s tests pin R-07's current behaviour deliberately rather than fixing it, so the
> decision is now made against a fixed baseline.

Full record:
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
