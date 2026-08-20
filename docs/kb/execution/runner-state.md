---
doc_id: KB-093
title: Autonomous Runner State
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-20
dependencies: [KB-089, KB-091, KB-092, KB-081]
---

# Autonomous Runner State

**Machine-owned. Small by design.** This is the runner's own control state — whether a run is
live, what it is on, how many attempts it has spent, and why it stopped. It is written at
every state transition by `.claude/workflows/migration-runner.js`, so a run that is killed
mid-task resumes from this file rather than from a conversation.

It is **not** a status report. Those live elsewhere and this file never duplicates them:

| Question | Read instead |
|---|---|
| What is every task's status? | [`task-tracker.md`](task-tracker.md) (KB-081) — the authority |
| What is the active task, in detail? | [`current-task.md`](current-task.md) (KB-089) |
| Why did an attempt fail, and what was tried? | [`failure-log.md`](failure-log.md) (KB-092) |
| What are the routing and retry rules? | [`autonomous-runner.md`](autonomous-runner.md) (KB-091) |

If this file and KB-081 disagree about a task's status, **KB-081 wins** and this file is
corrected.

---

## State

| Field | Value |
|---|---|
| **Status** | `BLOCKED` — `M2-B12-01`'s escalation budget is exhausted; it needs a human decision before any further automated work on that branch. **This corrects the state previously recorded here** (`RUNNING`, `M2-A08` dispatched at attempt 0): that entry was written on the strength of a `PASS` claimed for `M2-B12-01` at tip `58e7bee`, which `failure-log.md`'s own attempt-4 entry for that exact tip contradicts — it recorded `FAIL` (see **Last validation**), and no genuine `PASS` of `58e7bee` exists anywhere in this repository. The claimed `PASS` is treated as an error by the session that wrote it, not as a result a later `FAIL` superseded. `M2-A08` was never actually branched or dispatched — the "attempt 0" reflected an intent, not a live agent — so nothing about it needs unwinding; it stays `Ready` and unclaimed. |
| **Stop reason** | **`BLOCKED`: escalation budget exhausted on `M2-B12-01`.** Sequence: validated `FAIL` at tip `fa4a2ad` (INV-012's KB-003 evidence block said "four document services", re-derived from source as six) crossed `escalate_after_failures: 2`, so the task escalated (`max_escalations: 1`, now fully spent) and the escalated diagnosis fixed it, committed `8a54f96`. That fix has **not** been re-validated — the escalation budget being spent, the orchestrator stopped rather than spend the task's last attempt slot on an unsupervised re-validation. **Attempts used: 2 of 3. Escalations used: 1 of 1.** Full detail: `tasks/M2-B12-01.md` § Execution Record (2026-08-20) — Session Close-out: STOPPED, escalation budget exhausted. This sits on top of, and is independent of, the owner's earlier `/migration-stop` request (2026-08-20), already honored cleanly at this same task's boundary before this escalation history played out. Nothing was merged or pushed; no task was marked `Completed` as part of any of this. |
| **Run started** | 2026-08-19, spanning through `M2-B02`, `M2-A01-02`, `M2-A01-03`, and `M2-B12-01`, where it now sits `BLOCKED`. |
| **Last transition** | 2026-08-20 — this close-out. Corrected `task-tracker.md`'s `M2-B12-01` row from the premature `Needs Review (validated PASS)` reading to `Blocked`, with the reason and named owner. Corrected `current-task.md` back to `M2-B12-01` (with this Run State) from `M2-A08`, which a prior in-flight session had queued next on the strength of the same premature `PASS`. `M2-A08` itself needed no correction — never branched, never dispatched, still genuinely `Ready` in KB-081 for whoever picks a candidate once `M2-B12-01` is unblocked. |
| **Current task** | `M2-B12-01` — `Blocked`, awaiting the repository owner's (Vivek's) decision. Not resumable by an autonomous session. |
| **Current phase** | `BLOCKED` (KB-088 orthogonal flag), sitting on top of what would otherwise be `TESTING`/`VALIDATING` in the ordinary lifecycle. |
| **Current agent** | n/a — no agent can act until the owner decides. |
| **Current model** | n/a. |
| **Attempt** | `M2-B12-01` at **2 of 3** attempts used, **1 of 1** escalations used. No further attempt is available without an owner-authorized budget reset. |
| **Escalations** | 1 of 1 (this task, fully spent) |
| **Last validation** | `M2-B12-01`, tip `fa4a2ad` — validator verdict **`FAIL`** (`failureCategory: acceptance-criterion`, `scopeOk: true`). Seventeen of eighteen acceptance criteria independently re-checked `MET` — including the 38-row Mechanism A citation sweep (machine-verified, zero mismatches), the six-pattern negative-result re-run, R-12 correctly left `Inferred (high confidence)` (not upgraded), and KB-100's registration in KB-005. The one `NOT MET`: *"INV-012 is Complete in KB-003 with evidence rows in the KB-083 format"* — the row is `Complete` with five KB-083-format blocks, but the first block's `Finding` at `investigation-registry.md:54` read "inline in four document services" under `Confidence: Confirmed`, while `grep -rn "LastNumber" V.SMART/V.SMART.Shared/BusinessLayer/` shows **six**. `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` → 0 errors / 6,695 warnings, baseline; both test suites green (117/117, 84/84); `git diff --stat` → 7 files, all `docs/`. Diagnosed and fixed as `8a54f96` (not yet re-validated — see **Stop reason**). This same tip (`58e7bee`) had previously been recorded here, in error, as `PASS`; `failure-log.md`'s own attempt-4 entry for `58e7bee` shows `FAIL`, matching this entry, so the `PASS` reading is corrected rather than treated as a genuine earlier result. |
| **Tasks processed this run** | `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01`, `M2-B07` — all `Completed` and merged. `M2-C04-01` — implemented, validated `PASS`, `Completed` and merged. `M2-A06` — implemented, validated `PASS`, `Needs Review`, then owner-merged and `Completed`. `M2-B02` — implemented, validated `PASS` after one in-attempt retry, `Completed` and merged (`feec964`), released `M2-B09` only. `M2-A01-02` — implemented, validated `PASS`, `Completed` and merged (`ed559ad`), released `M2-A01-03`. `M2-A01-03` — implemented across 2 attempts, validated `PASS`, closed `Needs Review` (unmerged, `0fde6fb`) — does **not** yet release `M2-A02`/`M2-A07`/`M2-A08` pending merge. `M2-B12-01` — implemented, validated `PASS` at one tip then `FAIL` at a later tip on the same branch, escalated once, fixed but not re-validated, closed **`Blocked`** (unmerged, `8a54f96`) — does **not** release `M2-B12-02`, which remains correctly `Blocked`. Run halts here, on this task, needing the owner. |
| **Classification** | `M2-B12-01` (blocked) — `task_type: Investigation` → base **MEDIUM** ([KB-091 §4.1](autonomous-runner.md#41-base-complexity-from-task_type)); raised to **HIGH** (document-numbering is a named §4.2 trigger). **Risk LOW** — read-only investigation writing only KB prose, confirmed by the diff across every commit on the branch: no code, no `Program.cs`/`appsettings*`, no secret, no live-observable behaviour change. `M2-A08` (identified as next, not dispatched) — `task_type: Security` → base **HIGH**, independently also meeting four §4.2 raise triggers; **Risk HIGH**. Recorded for whoever resumes; not acted on here. |
| **Models this run** | `M2-A01-02`: Implement `opus`, Validate `opus`. `M2-A01-03`: Implement `opus`, Validate `opus`. `M2-B12-01`: Investigate `opus` (HIGH complexity); Diagnose (escalated) `opus`; no ordinary Implement/Validate role — the task produces no code. |
| **Next ready task** | **None selectable by this close-out.** `M2-B12-01` occupies the only Investigation-type P0 slot with a live dependent (`M2-B12-02`) and is `Blocked`, not `Ready` — not re-selected. `M2-A08` (Row-level scoping + account gates, P0, Hard dependency of `M2-D01`) was identified as the next dependency-ready candidate by the pre-block bookkeeping and that identification still holds on its own merits (`M2-A01-03` `Completed`/merged, no gating open question, no sibling branch) — but selecting and dispatching it is left to whoever resumes the run, not done as part of this close-out, which only records `M2-B12-01`'s true state. See `current-task.md`'s "Ready and unclaimed" table for the full candidate list. |
| **Process note — id allocation** | **Four cross-branch id collisions have now occurred, all on 2026-08-19** — six KB/INV/Q ids, `M2-C01`'s footnote ¹⁸, and a `Q-31` double-claim caught during `M2-B07`'s merge bookkeeping (`Q-31` was already held by `M2-B07` itself; the new question became **Q-32**). Every one was caught by hand at merge, which is not a control. `grep`-before-claim cannot see a sibling branch, and it cannot see an id claimed earlier in the same session. `git branch --no-merged master` must be checked before claiming any id. This recurs until the allocation rule itself changes. |

### Status values

| Status | Means |
|---|---|
| `RUNNING` | A run is live and processing a task |
| `STOPPED` | No run is live. A clean, expected end — budget reached, or no ready task |
| `BLOCKED` | A run halted needing a human. **Stop reason and owner are mandatory** |
| `STOP_REQUESTED` | A human asked the current run to finish its task and stop. The runner checks this at the top of each task and exits cleanly |

---

## Requesting a stop

Set **Status** to `STOP_REQUESTED` and record who asked and why. The runner reads this file at
the start of every task, so the request takes effect at the next task boundary — the in-flight
task finishes its validate/record cycle rather than being abandoned half-implemented.

That is the safe stop. Killing the run mid-task is also safe for the repository — every
transition is written here before the next begins — but it leaves a task part-implemented on
its branch, which someone has to reconcile.

---

## Pre-run flags for `M0-15`

Both were `safetyStop` conditions ([KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks))
and a run will halt on them rather than measure something unreproducible. **Both are now
resolved (2026-08-17). Kept as history — re-check them, do not assume they stay clear.**

- ✅ **Branch point — RESOLVED 2026-08-17 by re-cutting from `master`.** The branch had been cut
  from `migration/M0-08-gitignore-build-output`; this stopped the first run. It was reset to
  `master` and the three non-M0-08 commits were cherry-picked back, dropping only `e0a7092`
  (M0-08), which remains safe on its own branch. New history:
  `31cfa95` (master) → `998f7d0` → `7905c83` → `fece832`.
  Verified: `git merge-base HEAD master` → `31cfa95`, **identical to master's tip**;
  `git merge-base --is-ancestor e0a7092 HEAD` → false.
  Pre-re-cut state is preserved at tag `backup/M0-15-pre-recut-2026-08-17` (`ef861c3`).
  **Side effect:** M0-08's `Needs Review` status travelled with `e0a7092`, so KB-081 on this
  branch again lists M0-08 as `Ready`. It self-corrects when M0-08 is reviewed and merged.
- ✅ **Dirty working tree — RESOLVED 2026-08-17.**
  `V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantDbContextFactory.cs` and
  `V.SMART/V.SMART.Web/appsettings.json` were stashed as
  `PRE-M0-15: local tenant DB debugging …` (stash commit `6dbf4b47b8ff`) — local tenant-DB
  debugging work, not part of any task: null/empty-tenant guards in the factory, and a
  `MasterDb` connection string repointed at a local `.\SQLEXPRESS` / `NexGenErpDb_Master`.
  Recoverable with `git stash apply`; the stash holds full file contents, not just a diff.
  `V.SMART.Api/` remains untracked **by design** — see the untracked-directory checkout trap in
  `CLAUDE.md`; never stash or clean it.

No flag is open as of 2026-08-17, so a run may open M0-15. Re-verify both before each run —
they are cheap to check (`git merge-base HEAD master`, `git status --porcelain`) and expensive
to get wrong, since the whole point of M0-15 is a baseline someone else can reproduce.
