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
last_verified: 2026-08-16
dependencies: [KB-081, KB-082, KB-088]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

| Field | Value |
|---|---|
| **Task ID** | **M0-15** |
| **Task** | Toolchain and build baseline |
| **Status** | `READY` |
| **Milestone** | M0 — Stabilise (Gate G0) |
| **Type** | DevOps |
| **Priority** | P0 |
| **Estimate** | 0.5 d |
| **Full specification** | [`tasks/M0-15.md`](tasks/M0-15.md) |
| **Branch** | `migration/M0-15-build-baseline` |
| **Commit subject** | `M0-15: Record the toolchain and build baseline` |

---

## Run State

This task's **classification** — intrinsic to the task, so it lives with the task.

| Field | Value |
|---|---|
| **Runner state** | `NOT_STARTED` — no run has opened this task |
| **Canonical status** | `READY` (the row above; KB-081 is authoritative) |
| **Complexity** | `MEDIUM` — derived: DevOps base, 0.5 d, one hard prerequisite, no business rules ([KB-091 §4](autonomous-runner.md#4-classifying-a-task)) |
| **Risk** | `LOW` — measures only; no `.csproj`, `.cs` or `.razor` file may be modified |
| **Attempt** | 0 of 3 (`max_retries: 2`) |
| **Failure log** | no entries — [`failure-log.md`](failure-log.md) (KB-092) |

**Live run state — which phase, which agent, which model, why a run stopped — is in
[`runner-state.md`](runner-state.md) (KB-093), not here.** One owner per fact: this file
describes the task, that one describes the run.

Two open flags below must be cleared **before** any run opens this task: the branch point and
the dirty working tree. Both are safety stops under
[KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks) if they cannot be
reconciled.

---

## Objective

Establish, in writing and reproducibly, **what builds, with which SDK, in how long,
producing how many warnings** — and decide whether the SDK version is pinned via
`global.json`. Close the one question INV-029 left open: whether
`dotnet build NexGen-ERP---2025-master.sln` succeeds, and under what conditions.

This task **measures**; it does not fix. It produces the numbers M0-07 turns into a CI gate.

## Dependencies

| Dependency | Class | State |
|---|---|---|
| **M0-00** — clean version-control baseline | Hard | `COMPLETED` — satisfied |
| .NET SDK on the execution machine | Deployment | Record what is actually present at execution time |
| MAUI workloads | Information | Present locally on 2026-08-12; **assume absent on CI** |
| **M0-08** — `.gitignore` hygiene | Soft | Must **not** run concurrently ([KB-080 § Parallel Work](README.md#parallel-work)) |

Downstream: **M0-07** consumes this task's baseline document verbatim.

## Relevant Documentation

Read only these.

| doc_id | Path | Why |
|---|---|---|
| TASK | [`tasks/M0-15.md`](tasks/M0-15.md) | The binding specification — implementation steps, full acceptance criteria |
| KB-083 | [`prompt-template.md`](prompt-template.md) | The verified-commands table **this task updates** |
| KB-003 | [`../investigation-registry.md`](../investigation-registry.md) | INV-029 (amend), INV-023 (reuse unchanged) |
| KB-080 | [`README.md`](README.md) | §6 finding 3, §7 M0 — deep-link only |
| KB-010 | [`../architecture/system-overview.md`](../architecture/system-overview.md) | Projects and hosting |
| KB-060 | [`../risks/technical-debt-register.md`](../risks/technical-debt-register.md) | R-05 |

## Relevant Existing Code

Read-only. **No `.csproj`, `.cs` or `.razor` file may be modified by this task.**

- `NexGen-ERP---2025-master.sln` — built, not edited
- `V.SMART/V.SMART.Shared/V.SMART.Shared.csproj` — multi-target
- `V.SMART/V.SMART.Web/V.SMART.Web.csproj`
- `V.SMART/V.SMART.Api/V.SMART.Api.csproj`
- `V.SMART/V.SMART/V.SMART.csproj` — MAUI target frameworks
- Repository root — `global.json` may be **created** here, conditionally

## Business Rules

**None.** This task does not touch business behaviour.

## Acceptance Criteria

The full checklist is [`tasks/M0-15.md` § Acceptance Criteria](tasks/M0-15.md#acceptance-criteria)
and it is binding. Summary of what must be objectively true:

- `docs/kb/execution/M0-15-build-baseline.md` exists with standard KB frontmatter, every
  claim tagged Confirmed / Inferred / Unknown.
- `dotnet --list-sdks`, `dotnet --info` and `dotnet workload list` output recorded verbatim.
- All four projects' target frameworks recorded with `file:line` citations.
- API project builds with **0 errors**; its warning count recorded from **two clean runs**.
- Web project outcome, duration and warning count recorded (previously unmeasured).
- Solution-build outcome recorded — success, or the exact failing project and error id.
- Whether the solution builds **without MAUI workloads** answered Confirmed, or explicitly
  marked Unknown with the reason.
- Top warning codes listed with counts; `MUD0002`'s count and percentage stated numerically.
- A pin-or-don't-pin `global.json` decision recorded with reasoning.
- A single recommended CI build command stated for M0-07, with justification.
- KB-083's verified-commands table has no remaining "not verified" / "not yet measured" cell
  for the three non-MAUI build commands.
- INV-029 amended in KB-003.
- `git status --porcelain` shows only the intended documentation (and conditional
  `global.json`) changes.

## Testing Requirements

**No automated tests.** There is no test project (INV-023, Confirmed) — `dotnet test` finds
nothing and **must not be used** until M0-12-01 creates one.

The build is the test:

1. `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` → 0 errors. A non-zero error count
   is a **blocking finding**, not a measurement — stop and report.
2. Every measurement repeated **twice from a clean `bin`/`obj`**, both runs recorded. A
   warning count that is not reproducible cannot become a CI gate.
3. The solution-build attempt recorded with full first-error output if it fails.

Verification commands: [`tasks/M0-15.md` § Verification Commands](tasks/M0-15.md#verification-commands).

## Documentation Updates On Completion

Per [`tasks/M0-15.md` § Documentation Updates](tasks/M0-15.md#documentation-updates):

- **Create** `docs/kb/execution/M0-15-build-baseline.md` — `grep -rn "KB-086" docs/kb/`
  first and take the next free id if claimed. **KB-088/089/090 are now taken** (workflow,
  this file, task-template) — see [INDEX.md § doc_id allocation](../INDEX.md#doc_id-allocation).
- KB-083 `prompt-template.md` — verified-commands table + toolchain note
- KB-003 `investigation-registry.md` — amend INV-029 (no new id)
- KB-080 `README.md` — §6 finding 3
- KB-081 `task-tracker.md` — M0-15 status
- KB-005 `INDEX.md` — register the new baseline document
- **This file** — hand over to the next task (see below)

## Completion Conditions

This task reaches `COMPLETED` only when every acceptance criterion above is objectively met,
the build measurements were actually run and observed, the documentation updates landed, and
the diff is committed single-scope on `migration/M0-15-build-baseline`.

Reaching `REVIEW` (committed, unmerged, awaiting a reviewer) is the expected end state of an
execution session — [`workflow.md` § Who may set COMPLETED](workflow.md#who-may-set-completed).

---

## Sequence

| | Task | Status |
|---|---|---|
| **Previous** | M0-00 — clean version-control baseline | `COMPLETED` 2026-08-14 |
| **Current** | **M0-15 — toolchain and build baseline** | `READY` |
| **Next (candidate)** | M0-02 — confirm stored-procedure drift across tenants (Q-14) | `READY`, independent, needs DBA access |
| **Next (unblocked by this)** | M0-07 — CI pipeline | `BLOCKED` on M0-15 **and** M0-08 |

The next task is **selected, not assumed** — apply
[`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
against the tracker at completion time, because status may have moved.

---

## Open flags on this task

- **M0-08 is not merged.** The tracker records M0-08 as `Needs Review` on its own branch. On
  disk, commit `e0a7092` (`M0-08: Verify build output is ignored…`) is an ancestor of
  `migration/M0-15-build-baseline` — i.e. this branch was cut from M0-08's, not from
  `master`. Verify the branch point before measuring, and state in the baseline document
  which tree the numbers describe. A baseline measured on an unreviewed mixture is not
  reproducible, which is exactly what this task exists to prevent.
- The working tree currently shows modifications to
  `V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantDbContextFactory.cs` and
  `V.SMART/V.SMART.Web/appsettings.json`, plus untracked `V.SMART.Api/`. Implementation step
  1 requires a clean tree — reconcile against M0-00's documented quarantine list before
  measuring, and stop if it cannot be reconciled.
