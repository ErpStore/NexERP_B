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
last_verified: 2026-08-19
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
| **Status** | `BLOCKED` |
| **Stop reason** | `M2-C04-01` — the retry `migration-debugger` was dispatched against (attempt 1's own `retry` disposition, coverage regression) returned **no result to the orchestrator**. Its process nonetheless left real, uncommitted edits on disk (`ThemeToggle.tsx`, `theme.test.tsx` — a partial fix to two of the four files attempt 1's coverage report named), matching the `M0-12-01`/`M2-B07` precedent that an empty return does not mean an empty disk. Nothing was validated, so there is nothing safe to retry against. Per KB-091 §8 item 1, an empty agent return with nothing validated to act on is a safety stop, not a silent re-dispatch. Full record: `failure-log.md` "M2-C04-01 · attempt 2 · dispatch". This is a tooling/session failure, not a code, business-rule or architecture problem — attempt 1's diagnosis still stands unconsumed, and the uncommitted diff is left as-is on the branch for review. **Owner: Vivek** — resuming needs a person to restart or re-dispatch the run; no product or architecture decision is outstanding. |
| **Run started** | 2026-08-19 |
| **Last transition** | 2026-08-19 — `M2-C04-01` attempt 1 implemented (`cdb147a`) and validated `FAIL`/`regression` (coverage gate, not an acceptance criterion; disposition `retry`). Attempt 2 dispatch to `migration-debugger` returned no result. Session close-out recorded the BLOCKED outcome; branch `migration/M2-C04-01-design-tokens` left unmerged for a human-resumed retry. |
| **Current task** | `M2-C04-01` — Design tokens, theme, light/dark. `Blocked`, not selected for further autonomous work until a human resumes the retry (`task-tracker.md` footnote ²²). |
| **Current phase** | `FAILED` → `DIAGNOSING` interrupted — the debugger dispatch produced nothing. Canonical lifecycle: `TESTING` / `BLOCKED` flag set. |
| **Current agent** | n/a — no agent is live |
| **Current model** | n/a |
| **Attempt** | 1 of 3 used (attempt 2's lost dispatch does not consume budget, per the `M0-12-01` precedent — KB-081 footnote ¹²). Two attempts remain. |
| **Escalations** | 0 |
| **Last validation** | `M2-C04-01` attempt 1 — validator verdict `FAIL`, `failureCategory: regression`. All sixteen acceptance criteria independently re-checked and `MET` (contrast recomputed from scratch: 0 failing pairs, both themes; `typecheck`/`lint`/`test`/`build` all green). What failed is `npm run coverage`, a verified KB-083 command (`prompt-template.md:366`) that this commit's ~700 new lines under `shared/theme/**` broke against `vitest.config.ts:38`'s `branches: 100` floor. Full evidence: `failure-log.md` "M2-C04-01 · attempt 1 · validation". |
| **Tasks processed this run** | `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01`, `M2-B07` — all `Completed` and merged. `M2-C04-01` implemented and validated (attempt 1: FAIL/regression, disposition retry); attempt 2 lost to a tooling failure. Now `Blocked`, awaiting a human-resumed retry. |
| **Classification** | `M2-C04-01` — complexity **HIGH** (base MEDIUM for `task_type: Frontend`, raised once for `estimate: 3 d`, raised again because 3 tasks name it as a Hard prerequisite — `M2-C04-02`, `M2-C04-03`, `M2-C03`); risk **MEDIUM** (default — no schema, no secrets, no `Program.cs`/`appsettings*.json`, `business_rules: []`, does not touch a live Blazor-observable path). `requiresHuman`: false — KB-051 (design-system proposal) fully specifies the token tables this task implements. `safetyStop`: false — tree clean, `HEAD` on `master` at `aaae3a0`. |
| **Models this run** | Implement: `opus`. Validate: `opus`. (HIGH-complexity routing, [KB-091 §5.1](autonomous-runner.md#51-the-routing-table)) |
| **Tied candidate** | `M2-A06` (Exception middleware → `ProblemDetails`) — equally ranked, genuinely independent (different files, backend vs. frontend). Recorded per the selection rule's step 4; not started. |
| **Blocked on** | Nothing, for selection purposes — nine M2 tasks are `Ready`, plus `M0-01-03` and `M0-10`. Standing owner-owned items: `M0-04` `Blocked` (no identified owner, **and now gated on Q-32** — rotating `sa` would break every `Tenants` row that embeds it); `M0-06` `Blocked` on Q-25/Q-26; `M0-11` `Ready` but a `Product Decision`, owner-only and never runner-selectable ([KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks)); `M2-A01-02` nominally `Ready` but its spec contradicts reality — see `current-task.md`. |
| **Next ready task** | Not pre-selected — the runner selects per [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule). Newly released by `M2-B07`: **`M2-B04`**, **`M2-B01`**, **`M2-B05`**, **`M2-B12-01`**. Already `Ready`: `M2-A06`, `M2-C04-01`, `M2-C10`, `M2-C11`, `M2-A01-02` (blocked in practice). Carried M0 debt: **`M0-01-03`** (`Ready` as of 2026-08-19 — the rebuild drill is no longer hardware-blocked, see footnote ²¹) and `M0-10`. `M2-B12` and `M2-C04` are **parent containers** and are never worked directly. |
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
