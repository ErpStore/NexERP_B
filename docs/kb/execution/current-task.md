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
last_verified: 2026-08-21
dependencies: [KB-081, KB-082, KB-088, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## No task is in flight — the runner is stopped, and the blocker is the merge queue

`M2-B04` closed `Needs Review` on 2026-08-21 (attempt 2, validated `PASS`). The Select phase
that followed produced an **empty candidate set**, so the run halted rather than guessing. That
is a [KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks) stop and a
successful outcome of the loop, not a failure. Owner: **Vivek**. Full control state:
[`runner-state.md`](runner-state.md).

### The one thing that would change this

**Six branches carry a claimed `PASS` and are unmerged; a seventh task is `Blocked` on the
owner.** Nothing they depend on is missing; nothing is being re-derived. Until they land on
`master`, [selection rule](dependency-graph.md#ready-task-selection-rule) step 1 keeps every one
of their dependents `Blocked`, because a prerequisite that is `Needs Review` is not `Completed`.

Each row below was read from the branch itself this session, not inherited from the previous
runner state — which on two of them was **wrong** (see *A correction*, below).

| Task | Branch | Tip | State on that branch |
|---|---|---|---|
| `M2-B04` | `migration/M2-B04-decouple-pages-references` | `5ca1c10` | `Needs Review`, validated `PASS` (attempt 2) |
| `M2-A08` | `migration/M2-A08-row-scope-and-account-gates` | `bca92fd` | `Needs Review`, validated `PASS` — its tracker row says so |
| `M2-A08` ⚠ | `migration/M2-A08-row-level-scoping` | `6e6633a` | **A second branch for the same task**, live in `wt-M2-A08`, doing different work (INV-028 → KB-120, answering Q-05…Q-08). Its own tracker row still reads `Ready`. **Needs an owner decision on which branch is the real `M2-A08`.** |
| `M2-A07` | `migration/M2-A07-me-endpoint` | `e3bc96c` | `Needs Review`, validated `PASS` |
| `M2-C00` | `migration/M2-C00-kb050-angular-rewrite` | `b3c0e6e` | `Needs Review`, validated `PASS` — releases the whole `M2-C` tree |
| `M2-B01` | `migration/M2-B01-api-versioning` | `045a7f4` | Close-out commit claims `PASS`, **11 of 12 criteria met, criterion 4 partial**. Its own tracker row was never updated off `Ready`. |
| `M0-10` | `migration/M0-10-candelete-guard-audit` | `fc8e0c0` | Close-out commit claims `Needs Review` after attempt 3, regression repaired. Its own tracker row was never updated off `Ready`. |
| `M2-B12-01` | `migration/M2-B12-01-inv-012-numbering` | `407d0ba` | 🚩 **`Blocked` — escalation budget exhausted, awaiting Vivek.** Not `PASS`. |

**Never merge or push from an execution session** ([`CLAUDE.md`](../../../CLAUDE.md) § Standing
constraints). These are listed so the owner can act, not so a session can.

### A correction, and why it is worth reading

The runner state this session inherited claimed **`M2-B12-01` was validated `PASS` and awaiting
merge**. It was not. That branch's own tip commit is *"Record close-out — BLOCKED, escalation
budget exhausted, **corrects a premature PASS**"*, and its runner-state says plainly that the
earlier `PASS` was claimed for a tip (`58e7bee`) whose own failure-log entry recorded `FAIL` —
*"no genuine `PASS` of `58e7bee` exists anywhere in this repository."* The task is `Blocked` with
2 of 3 attempts used and its single escalation spent; the escalated fix at `8a54f96` has never
been re-validated.

That false `PASS` had already propagated into two files before this session, and this session
propagated it once more before checking. **It was caught only because `git stash list` happened
to show a branch name next to the words "corrects a premature PASS".** The same check found
`M2-B01` and `M0-10`, neither of which the inherited state mentioned at all.

**The lesson is the one footnote ²¹ already paid for once:** a status inherited from a sibling
branch is a claim, not a fact, and `git log --oneline -2 <branch>` costs nothing. Read the branch.

### Why every remaining task was excluded

| Task | Why not |
|---|---|
| `M0-01-03` | **The rank winner (P0), stopped at KB-091 §8 item 5.** See below — the block is now narrower than the task file claims. |
| `M2-B09` | `Ready`, P1, but **dropped at selection step 2**: its `source_files` name `V.SMART/V.SMART.Api/Program.cs` and `Controllers/CurrencyController.cs`, and in-flight `M2-B01` (live in `wt-M2-B01`) names both. Becomes the obvious next pick the moment `M2-B01` lands. |
| `M2-B01`, `M0-10` | Each already has a branch with a close-out commit claiming `PASS`/`Needs Review`, plus a live worktree. Finished-and-unmerged, not available to re-take. |
| `M2-A02` | `Ready` but gated on the unanswered **Q-28**: an API-only administrator holds zero `UserRight` rows because `AuthController.Login` never calls `SyncRightsForUserAsync`. Annotating `CurrencyController` before that is answered authenticates the administrator into an empty UI. |
| `M2-C01` | `Blocked` behind `M2-C00`'s merge. |
| `M2-B04`, `M2-A08`, `M2-C00`, `M2-A07` | Done, `PASS`, unmerged (see table above). |
| `M2-B12-01` | **`Blocked`, not done** — escalation budget exhausted, named owner **Vivek**, and the escalated fix at `8a54f96` has never been re-validated. Re-dispatching it autonomously would spend the task's last attempt slot on exactly the unsupervised re-validation the previous orchestrator declined to spend it on. |

## `M0-01-03` — the decision the owner actually needs to make

**Task file:** [`tasks/M0-01-03.md`](tasks/M0-01-03.md). **Status:** `Ready`, P0, 1 d, 0 attempts
used. Every repository-side artefact is already **on `master`**:
`db/deploy-stored-procedures.ps1`, `db/RUNBOOK-rebuild-tenant-database.md`,
`db/REBUILD-DRILL-LOG.md` (skeleton, every field `TBD`), and 91 `.sql` files under
`db/stored-procedures/`. **The only outstanding work is executing the rebuild drill and
recording the outcome** — which is the last open half of **G0 exit criterion 1**.

**The task file's step 7 is out of date and should not be followed as written.** It says *"You
cannot execute it — there is no SQL Server instance reachable from this session and no
credential to use if there were."* Re-verified this session, that is **false**:

- `MSSQL$SQLEXPRESS` is **Running**;
- `sqlcmd` is present (`…/Client SDK/ODBC/170/Tools/Binn/SQLCMD.EXE`) and so is the `SqlServer`
  PowerShell module;
- it is reachable by **Windows integrated auth**, so no credential need be acquired or reused.

Tracker footnote ²¹ already recorded this on 2026-08-19 and reclassified the task `Needs
Review` → `Ready`. What genuinely remains unavailable is narrower than "an environment", and it
is two things:

1. **A named operator.** The task requires the drill be *"executed end to end at least once by a
   named person"*, and the log records who. That is an accountability requirement, not a
   technical one — a session cannot satisfy it by signing itself.
2. **The UI smoke test** (runbook step 7): start the Blazor host, log in with the seeded
   `Administrator` account, open a list screen, run one report, and **print one document** —
   the print path being the one that proves `Sp_Print_CompanyDetails` deployed
   (`ReportService.cs:74-77`). Note R-09: that credential is a known default, so the drill
   environment must be disposable.

**What a session could do without either** — and what it would be worth — is runbook steps 2–6:
create throwaway master and tenant databases on the local instance, apply `MasterDbContext`'s
schema, insert one `Tenants` row, apply the 219 EF migrations, run
`db/deploy-stored-procedures.ps1`, and verify the deployed procedure count against
`manifest.csv`. That would produce the **first real evidence for Q-02** (how EF migrations reach
a tenant database — still *Unknown*) and the first real test of whether the deployment script's
**ordering assumption holds**, which the task file itself flags as *Inferred, not verified*
(deferred name resolution). It would leave criterion 7 and the operator field open and honest.

**This needs the owner's authorisation before a session does it**, because it executes DDL
against a live instance that also carries the real `NexGenErpDb_Master` and a 197-table
`NexGenErpDb`, and because the task file names a human for exactly this step. Offering it is
the useful action; assuming it is not.

## Also true right now

- **`M2-B04`'s `PASS` leaves two gates open** and the reviewer should not read it as
  "BR-APPR-001 observed intact". Acceptance criterion 9, the manual approval-workflow
  regression, is `NOT CHECKABLE` without a tenant-DB credential (Q-14 / R-01 / Q-32); the MAUI
  head was not built. The mechanical argument that nothing could have changed is strong — the
  diff touches no method body, signature, attribute or mapping — but it is an argument, not an
  observation.
- **The headline dependency M2-B04 was written to sever turned out to be dead text.**
  `Authorization.razor` contains zero occurrences of `static` and declares no type, so the
  `using static` import set was provably empty. The task removed a documentation-level
  architectural violation and installed a CI-enforced guard; it did **not** sever a real
  compile-time coupling. R-11 and BR-APPR-001 now say so without overclaiming.
- **New question `Q-55`** — whether `FundTransFilter.Bank`, now unreferenced by any repository
  or Razor caller, should be deleted. Raised rather than acted on.
- **Three sibling worktrees were live when this run resumed** (`wt-M0-10`, `wt-M2-A08`,
  `wt-M2-B01`), none of them this session's. `git worktree list` is part of selection now, not a
  curiosity — the tracker cannot see them, and `M2-A08` already has two branches from two
  sessions doing the same task.
