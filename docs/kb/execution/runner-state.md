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
last_verified: 2026-08-20 (re-confirmed by selection session)
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
| **Status** | `STOPPED` — `M2-C00` validated `PASS` and closed out `Needs Review`. Clean end: the runner does not merge, and no further attempt is authorised or needed. Next session should apply the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) fresh rather than resuming this pointer. 
| **Stop reason** | Task closed out at `Needs Review` pending owner merge — not a blocker, the expected end of a validated-but-unmerged task. See KB-088 § Who may set COMPLETED. 
| **Run started** | 2026-08-19 (spans the 2026-08-19→2026-08-20 autonomous run through `M2-B02`, `M2-A01-02`, `M2-A01-03`, `M2-C00`). Run ended at this close-out. |
| **Last transition** | 2026-08-20 — `M2-C00` attempt 3's committed correction pass was independently validated `PASS` (all nine acceptance criteria `MET`, `scopeOk: true`, diff confirmed docs-only). Task file and tracker updated to `Needs Review`; frontmatter, footnote ²⁸. **`M2-C01`'s tracker row was corrected from `Ready` to `Blocked`** — its Hard prerequisite (`M2-C00`) is `Needs Review`, not `Completed`, so [KB-082 step 1](dependency-graph.md#ready-task-selection-rule) still excludes it as a candidate; the `Ready` label had anticipated a merge that has not happened. Also corrected here: this file's stale claim that `M2-A01-03` was unmerged — `git log` shows it merged to `master` at `edcf126`, and KB-081 footnote ²⁷ already read `Completed`; KB-081 wins per this file's own precedence rule, so `M2-A02`/`M2-A07`/`M2-A08` **are** released (Q-28 still gates `M2-A02` specifically). |
| **Current task** | **None selected.** `M2-C00` is closed (validated, `Needs Review`, awaiting owner merge). `current-task.md` has been rewritten to `M2-A07` per the selection rule below. |
| **Current phase** | n/a — no task in flight. |
| **Current agent** | n/a — this session performed close-out/bookkeeping only, per instruction; no implementation was started or authorised. |
| **Current model** | Implement `sonnet`, Validate `sonnet` (MEDIUM/LOW routing — see Classification row). |
| **Attempt** | `M2-C00`: **closed, `PASS`**, retry budget not exhausted — 2 implementation attempts used (attempt 1 validated `FAIL` on criterion 3, since relaxed; attempt 3's correction pass, committed as `6d0aebb`/`421646a` plus a further re-alignment commit, validated `PASS` on its first validation pass, so no attempt 4 was needed). Last closed task: `M2-A01-03`, **2 of 3** used, `PASS`, `Completed` and merged. |
| **Escalations** | 0 |
| **Last validation** | `M2-C00`, branch tip (merge-base `8b1a261` = `master`) — validator verdict **`PASS`**, `failureCategory: none`, `scopeOk: true`. All nine acceptance criteria independently re-checked `MET`: no React/Vite/Mantine/TanStack/Zustand/Zod instruction remains; `doc_id`/title/filename unchanged; stack table's column diff against `ADR-007-angular-stack.md:86-103` empty across all rows; error-handling section matched line-for-line against the shipped `ApiProblems.cs`/`ProblemTypes.cs`; token storage stated as `M2-C02`'s open decision, pilot `localStorage` explicitly not endorsed; both hardcoded-host files flagged as a defect; STOP banner removed; `M2-C01` re-specified for Angular in the same change; diff is 9 Markdown files under `docs/kb/`, nothing under `V.SMART/`/`frontend/`/`db/`/`.github/`. `dotnet build V.SMART.Api` re-run: 6695 Warning(s)/0 Error(s) — KB-083 baseline, unchanged. One disclosed non-regression accepted, not fixed: 3 stale cross-document anchors into a renamed heading (`M2-C08-01.md` ×2, `M2-C08-02.md`), ruled a judgement call outside any criterion. Full evidence: `tasks/M2-C00.md` § Validation close-out (2026-08-20). Task closed `Needs Review` — not `Completed`; only the repository owner merges it. |
| **Tasks processed this run** | `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01`, `M2-B07` — all `Completed` and merged. `M2-C04-01` — implemented, validated `PASS`, `Completed` and merged. `M2-A06` — implemented, validated `PASS`, `Needs Review`, then owner-merged and `Completed`. `M2-B12-01` — selected once, superseded before being started (twice now); not started, still `Ready` and unclaimed. `M2-B02` — implemented, validated `PASS` after one in-attempt retry, `Completed` and merged (`feec964`), released `M2-B09` only. `M2-A01-02` — implemented, validated `PASS`, `Completed` and merged (`ed559ad`), released `M2-A01-03`. `M2-A01-03` — implemented across 2 attempts, validated `PASS`, **`Completed` and merged** (`edcf126`) — releases `M2-A02` (gated on `Q-28`), `M2-A07`, `M2-A08`. `M2-C00` — implemented across 3 attempts, validated `PASS`, closed `Needs Review` (unmerged) — does **not** yet release `M2-C01`/the `M2-C` tree pending merge. |
| **Classification** | `M2-A01-02` (closed) — complexity **HIGH**, risk **HIGH** (task_type Security, `business_rules` populated, `Program.cs` in `source_files`). `M2-A01-03` (closed) — complexity **HIGH**, risk **HIGH**, same grounds plus `business_rules: [BR-AUTH-002, BR-TEN-001, BR-TEN-002]` and two-project `source_files`. `M2-C00` (closed, `PASS`) — `task_type: Documentation` → base **LOW** ([KB-091 §4.1](autonomous-runner.md#41-base-complexity-from-task_type)); one raise applies under §4.2 (the task specifies auth flow/permission rendering and its `source_files` names `auth.service.ts`, `auth.guard.ts`, `auth.interceptor.ts` — "touches authentication, authorisation" applies even though no code is written), giving complexity **MEDIUM**. `estimate: 2 d` (< 3 d, no raise), `depends_on: [G0]` (1 dependency, no raise), `business_rules: []` (no raise), `source_files` are all frontend paths, not spanning the four .NET projects (no raise). Risk: **LOW** — Documentation-type, writes nothing but KB prose (KB-050), no schema/secrets/`Program.cs`/`appsettings*`, `business_rules` empty, no live-observable behaviour change, matching the explicit LOW row in [KB-091 §4.3](autonomous-runner.md#43-risk). |
| **Models this run** | `M2-A01-02`: Implement `opus`, Validate `opus`. `M2-A01-03`: Implement `opus`, Validate `opus` (same HIGH/HIGH routing). `M2-C00`: Implement `sonnet`, Validate `sonnet` (MEDIUM complexity row of [KB-091 §5.1](autonomous-runner.md#51-the-routing-table); risk LOW sets no floor). |
| **Next ready task** | `M2-C00` is closed (`Needs Review`, unmerged), so `M2-C01` stays genuinely `Blocked` — its Hard prerequisite is not yet `Completed` ([KB-082 step 1](dependency-graph.md#ready-task-selection-rule)) — despite the tracker's earlier re-scope note anticipating it as `Ready`; the tracker row was corrected. `M2-A01-03`'s merge (`edcf126`) released `M2-A07`, `M2-A08` and (gated on `Q-28`) `M2-A02`. Candidate set for this selection: `M2-A07`, `M2-A08`, `M2-B12-01`, `M2-B09`, `M2-B04`, `M0-01-03` (`M2-A02` excluded — `Q-28` unanswered). By downstream-unblocking count, `M2-A07` (releases `M2-C02`) and `M2-B12-01` (releases `M2-B12-02`) tie at 1 dependent each, both P0, both 2 d, neither on the stated critical path — genuinely tied and independent per [selection rule step 4](dependency-graph.md#ready-task-selection-rule). **Selected: `M2-A07`** (`GET /api/v1/me`) — continues the M2-A auth/rights thread `M2-A01-03` just closed, backend-only, no same-file conflict with any in-flight work. `M2-B12-01` remains an equally viable parallel pick; either could be run without conflicting with the other. 
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
