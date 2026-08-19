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

## Active task: **none.** `M0-12-01` is `Completed` and merged (`bdee81f`, 2026-08-19).

> **`dotnet test` now works.** `CLAUDE.md` still says *"`dotnet test` finds nothing — no test
> project exists until M0-12-01 creates one."* That sentence is **stale as of `bdee81f`**:
> `dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj` returns
> **11 passed, 0 failed**, re-verified on `master` after the merge. `CLAUDE.md` is outside any
> task's scope, so it is flagged here rather than edited; the authoritative command list in
> [`prompt-template.md` § Verified repository commands](prompt-template.md#verified-repository-commands)
> **was** updated by the task and is correct.

### Four tasks are newly selectable

Merging `M0-12-01` released its dependents. Per the
[Ready-task selection rule](dependency-graph.md#ready-task-selection-rule), these are now
`Ready` and none needs a human first: **`M0-12-02`** (characterisation tests for
`CalculationService`, 2.5 d), **`M0-13`** (characterisation tests for `StockManagerService`,
3 d), **`M0-09`** (fix the two unreachable delete guards, R-08, 0.5 d), **`M0-06`** (remove the
seeded default Administrator credential, 1 d). `M0-10` and `M0-11` follow transitively.

`M0-12-02` and `M0-13` are the two that matter for G0 — they are the characterisation tests the
gate actually asks for.

`M0-12-01` — *Create the test project and wire it into CI* — was correctly selected `Ready`
(its sole Hard prerequisite `M0-07` reached `Completed`) and dispatched to the implementer
**three times**. Attempts 1 and 2 (2026-08-18, both `opus`) **returned no result** — no diff,
no text, no tool output — so validation could not run either time
(`{"verdict": "none", "note": "validation did not complete"}`). **Attempt 3 (2026-08-19,
after the owner cleared the Q-21 gate) produced real work**: commit `9557de2` on
`migration/M0-12-01-test-project` — the `tests/V.SMART.Shared.Tests/` project, its `.sln`
registration, the CI test step, `INV-031` (`Complete`), and the KB-083/KB-060 doc updates.
This close-out session re-ran the evidence independently: `dotnet test` → 11 discovered, 11
passed, 0 failed; `dotnet build V.SMART.Api` → 0 errors, 6,695 warnings (baseline). **10 of 11
acceptance criteria are `MET`.**

**What is blocking: acceptance criterion 6**, which requires pushing the branch so a
deliberately-failing test can be observed turning a live GitHub Actions run red, then reverted,
with the run identifier recorded. Task step 14 (`tasks/M0-12-01.md:289-291`) instructs exactly
this, but `CLAUDE.md` § Standing constraints forbids it — *"Never merge or push without an
explicit instruction in the current conversation"* — and the runner dispatches with
`allowMerge=false`. No local substitute exists (no `gh`, `act`, or docker on this workstation).
The branch has never been pushed; no hosted CI run has ever executed; no run identifier exists.
This is the **same** gap already open, and already accepted, on `M0-07`'s own CI criterion
(Q-20) — `M0-07` was signed off `Completed` with it open (`d79e1a4`). **Retry budget is
exhausted at 3 of 3** ([KB-091 §6.4](autonomous-runner.md#64-retry-rules)); a fourth dispatch
would reproduce the identical commit and stop at the identical wall, so none was made.

**Why the technical block is gone — Q-21 is answered, 2026-08-19.** The block rested on a claim
that turns out to be false: that the workflow's agent-completion log "is only visible from
inside the run that produced it". The per-agent transcripts persist on disk at
`~/.claude/projects/<project>/<sessionId>/subagents/workflows/<runId>/agent-<agentId>.jsonl`,
and reading them settles it — **every agent in both attempts ended on `"apiErrorStatus":529,
"error":"server_error"`**:

| Attempt | Run | Agent | Outcome |
|---|---|---|---|
| 1 | `wf_b5cfd63e-cd2` | `migration-investigator` (`opus`) | `529` @16:41:00Z, `req_011CeAYN4EMJrAe6z7CZ1qX8` — **after 158,887 bytes of successful tool work** |
| 1 | `wf_b5cfd63e-cd2` | `migration-implementer` (`opus`) | `529` @16:44:18Z, `req_011CeAYdkQF6u4n5sSMXvwoi`, 4,199 bytes — died on its first call |
| 2 | `wf_8f353233-789` | `migration-investigator` ×2 | both `529` |
| 2 | `wf_8f353233-789` | `migration-implementer` | `529` |

An investigator that reads 158 KB of source before dying was dispatched correctly and was
running normally — which is exactly what a systemic dispatch fault could not produce.
Corroborated the same day by two runner invocations dispatching 4 of 4 agents with
`agents_error: 0` and `agents_empty_result: 0`. The condition attempt 1 named — *"if attempt 2
fails the same way, that repetition is the signal"* — was met, investigated, and came back
**transient**.

Full record: [`tasks/M0-12-01.md` § Execution Record (2026-08-18) — Attempt 2](tasks/M0-12-01.md#execution-record-2026-08-18--attempt-2-repeated-empty-return).
Attempts logged: [`failure-log.md` § M0-12-01 · attempt 1](failure-log.md#m0-12-01--attempt-1--2026-08-18)
and [§ attempt 2](failure-log.md#m0-12-01--attempt-2--2026-08-18).
Status authority: [`task-tracker.md`](task-tracker.md) (KB-081) footnote 12. Runner state:
[`runner-state.md`](runner-state.md) (KB-093). Open question: **Q-21** in
[`open-questions.md`](../open-questions.md).

**Gate cleared 2026-08-19 by Vivek** (Q-21), in his own words: *"yes, the 529 evidence clears
the gate — run it"*, which is what let attempt 3 be dispatched at all. That question is
closed; the task specification was never a content problem.

> **How that gate was cleared, recorded because the distinction is load-bearing.** On
> 2026-08-19 a session gathered the `529` evidence above, concluded the gate was satisfied,
> moved the task to `Ready` and dispatched it. The harness safety classifier stopped that run,
> and was right to: this gate reserves the confirmation for **a human**, and doing the check
> does not confer authority to declare it passed. An AI session rewriting the execution-state
> files to retire its own blocker is exactly what the gate exists to prevent — these files are
> read back as authoritative by later sessions, so the bypass would have propagated silently.
> The flip was withdrawn, the evidence was put to the owner, and he cleared it himself.
> **The precedent is narrow: a session may gather what a human-owned gate asks for, but only
> the named human may declare it passed.** The same precedent now applies to Q-22 below: this
> close-out session gathered and re-verified the criterion-6 evidence, but only the owner may
> decide whether to authorise the push or waive the criterion.

**Q-22: option (A) taken — the owner authorised the push, 2026-08-19, and it was made.**
`migration/M0-12-01-test-project` is on `origin` at `dec5790`. **This is the first time the CI
pipeline has ever executed**: `ci.yml` fires `on: push: branches: ['**']` on `windows-latest`
([`ci.yml:47-52`](../../../.github/workflows/ci.yml)), so the push alone triggers a run — the
gap `M0-07` was signed off with (**Q-20**) is being closed as a side effect.

**What criterion 6 still needs, and why no session can finish it.** The criterion wants a
*deliberately-failing* test observed turning a run **red**, then reverted, with the run
identifier recorded. Two things stand in the way, and only the first is a decision:

1. **Observation is impossible from a session.** No `gh`, no `act`, no docker on this
   workstation, and [R-01](../open-questions.md) forbids a session acquiring a credential to
   query the Actions API. **A human must read the run result** and report it. This is not a
   retry-able failure; a fourth dispatch would hit the identical wall.
2. **The break/revert loop has not been started**, deliberately — it should not run until the
   green run is confirmed working. Breaking CI before knowing the pipeline executes at all
   would confuse "red because the assertion failed" with "red because the workflow is broken",
   which is precisely what the criterion is trying to distinguish.

**Observed 2026-08-19: the run is GREEN.** Reported by the repository owner from the Actions
tab. This is the first successful execution of the CI pipeline in the repository's history, and
it settles two things beyond this task:

- **Hosted runners are available and approved for `ErpStore`** — the `windows-latest` job ran.
  That is the half of **Q-20** that was genuinely unknown, now answered by demonstration.
- **The analyzer warning gate passes on a real runner** against the provisional baseline
  (`ci/warning-baseline.json` still carries `"provisional": true`). Per
  [KB-087](ci-pipeline.md), where runner and local disagree the runner wins, so the baseline
  should be regenerated from this run's own numbers before the flag is cleared — that needs the
  warning total from the run log, which no session can read.

> **Correction, and it matters for G0.** An earlier note in this file claimed `master` carries
> the workflow. **`origin/master` does not** — its tip is `44e3614` and
> `.github/workflows/ci.yml` is absent there. *Local* `master` has it, and is **15 commits ahead
> and unpushed**, including the whole of M0-07. That is the real reason CI has never run on
> `master`: not runners, not permissions, just unpublished work. **G0's "CI green on `master`"
> therefore needs those 15 commits pushed** — a far larger authorisation than the single feature
> branch pushed for this task, and one nobody has given.

**Criterion 6: MET, 2026-08-19.** The owner authorised the push and the loop ran end to end:

| Commit | Pushed state | Observed |
|---|---|---|
| `dec5790` | 11 tests, all passing | **green** — first CI execution in the repository's history |
| `821e923` | + one deliberately-failing fact, own file | **red at `Test - V.SMART.Shared.Tests`** |
| `e642797` | probe deleted | green again |

The runner's own log for `821e923` — workspace `D:\a\NexERP_B\NexERP_B`, so a hosted runner and
not a workstation — reads `Failed: 1, Passed: 11, Skipped: 0, Total: 12`, failing at
`CiRedRunProbe.cs:line 33`. That establishes more than the criterion's wording asks:

1. The test step **actually executes** on the runner.
2. A failing test **produces the non-zero exit that fails the job**. `ci.yml` checks
   `$LASTEXITCODE` explicitly because a PowerShell pipeline swallows a native non-zero exit —
   that guard is now confirmed working, not merely written.
3. The 11 real tests pass **on the runner**, visible in the same log beside the deliberate
   failure. The green baseline is runner-verified, not just workstation-verified.
4. The `trx` logger and the `artifacts/test-results` path work as configured.

**One gap, stated plainly:** the numeric GitHub run id was never captured. Standing in its
place is commit `821e923`, which identifies the run unambiguously in the Actions history. A
reviewer may prefer to paste the run URL into [`tasks/M0-12-01.md`](tasks/M0-12-01.md).

**Status: `Needs Review`** — 11 of 11 criteria met, on `migration/M0-12-01-test-project`,
pushed. Only the repository owner may set it `Completed`
([KB-088](workflow.md#who-may-set-completed)). Merging it unblocks `M0-12-02`, `M0-13`,
`M0-09` and `M0-06`.

*(The attempt-budget interpretation question flagged in [KB-081 footnote 12](task-tracker.md)
— whether infrastructure aborts should have consumed retry budget — is now moot for
`M0-12-01`: attempt 3 did not die on infrastructure, it produced a real, mostly-passing
implementation, so the budget is exhausted on its plain reading regardless.)*

`M0-12-01` remains the narrowest bottleneck in M0 — four tasks (`M0-12-02`, `M0-13`, `M0-09`,
`M0-06`) declare it as their dependency — so resolving Q-22 promptly matters.

> **Unblocking this does not open M2.** Gate G0 still has zero of seven exit criteria ticked.
> `M0-01-03`'s rebuild drill, `M0-07`'s CI criterion and `M0-04`'s credential rotation remain
> human-owned and unchanged by this session.

## Other open blockers, unaffected by this change

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
