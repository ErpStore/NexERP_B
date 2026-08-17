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
last_verified: 2026-08-17
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
| **Task ID** | **M0-07** |
| **Task** | CI pipeline: restore → build → analyzers |
| **Status** | `BLOCKED` (environment) — pipeline, gate and docs built and verified locally 2026-08-17 (`migration/M0-07-ci-pipeline`, `5106929`); six acceptance criteria unmet because the branch was never pushed, no GitHub-hosted Actions run has occurred, `master` carries no workflow, and branch-protection admin rights are unavailable |
| **Milestone** | M0 — Stabilise (Gate G0) |
| **Type** | DevOps |
| **Priority** | P0 |
| **Estimate** | 2 d |
| **Full specification** | [`tasks/M0-07.md`](tasks/M0-07.md) |
| **Branch** | `migration/M0-07-ci-pipeline` (committed, unmerged, **not pushed to `origin`**) |

---

## Objective

Give the repository a real CI pipeline: restore → build (`V.SMART.Api` + `V.SMART.Web`
`.csproj`s, per KB-086, not the untracked `.sln`) → analyzer warning gate, with a committed
warning baseline that fails on any *new* warning code and passes-with-instruction on an
improvement (ratchet). No `dotnet test` yet — that is M0-12-01's concern; this task only
leaves a commented placeholder naming it. Full spec: [`tasks/M0-07.md`](tasks/M0-07.md).

---

## Run State

| Field | Value |
|---|---|
| **Runner state** | `BLOCKED` — attempt 1 built and locally verified the full pipeline, then stopped: the remaining acceptance criteria require a hosted Actions run this session cannot obtain |
| **Canonical status** | `Blocked`⁷ (the row above; KB-081 is authoritative — see `task-tracker.md` footnote 7) |
| **Attempt** | 1 of 3 (`max_retries: 2`) |
| **Failure log** | [`failure-log.md`](failure-log.md) (KB-092) — M0-07 attempt 1, category `environment`, disposition `blocked`. Full validator verdict, diagnosis and evidence transcript recorded there; not reproduced here. |
| **What ran** | `.github/workflows/ci.yml` (hygiene guard → restore → build Api/Web → analyzer gate → `M0-12-01` test-step placeholder), `tools/compare-warnings.{ps1,sh}`, `ci/warning-baseline.json` (Api 6,693 / Web 6,695 warnings), `docs/kb/execution/ci-pipeline.md` (KB-087) — all created, committed (`f2672be`, `777e46c`, `5106929`) on `migration/M0-07-ci-pipeline`, and verified locally: build totals reproduce exactly across two runs, the gate correctly fails on a synthetic new warning code and correctly ratchets on an improvement, `tools/check-no-build-output.sh` exits 0. |
| **Why it stopped** | Five acceptance criteria are unmet and one is not checkable, all for the same reason: the pipeline has never executed on a GitHub-hosted runner. `ci/warning-baseline.json` self-declares `"measured_on": "developer-workstation"` / `"provisional": true`; the branch is absent from `origin` (`git ls-remote --heads origin`); `master` carries no `ci.yml` (`git ls-tree -r --name-only master -- .github`); the required-status-check needs GitHub org admin rights; `gh` CLI is not installed here. |
| **Blocked on** | (1) Answering **Q-20** — does `ErpStore` have GitHub-hosted Actions runner minutes at all? (2) A human with push permission on this branch and GitHub organisation admin rights on `master`'s branch protection. Owner: migration lead / repository admin — not yet named. Full detail: [`tasks/M0-07.md` § Execution Record](tasks/M0-07.md#execution-record-2026-08-17). |
| **To resume** | Answer Q-20, then have the owner push `migration/M0-07-ci-pipeline`, observe one green Actions run, regenerate `ci/warning-baseline.json` from the runner's own numbers (runner wins on any disagreement with the local 6,693/6,695), merge to `master`, and add the check to branch protection. Do not re-derive the pipeline or the gate script — they are done and locally verified. |

**Live run state is in [`runner-state.md`](runner-state.md) (KB-093), not here.**

---

## Why this task, not another

Selected per [`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
once `M0-15`, `M0-08` and `M0-03-01` all reached genuine `Completed` (reviewed and merged by
the repository owner, 2026-08-17) — `M0-07`'s two Hard prerequisites (`M0-15`, `M0-08`) were
then both satisfied, making it `Ready`. `M0-02` (the previous session's task) remained
`Blocked` on a human (DBA fingerprints) and was not re-selected.

## Business Rules

**None modified.** M0-07 adds build automation, a comparison script and documentation; it
does not touch a `.csproj`, `.cs` or `.razor` file (`git diff --stat HEAD -- '*.csproj' '*.cs'
'*.razor'` empty). No legacy Blazor validation, permission/screen-right check, document-
numbering, or tenant-scoping rule was at risk.

## Completion Conditions

Reaches `COMPLETED` only after human review, a hosted CI run, and a merge (KB-088 "Who may
set COMPLETED"). The honest in-session end state, and the one recorded, is `BLOCKED` —
environment — pending the human action listed above under **Blocked on**.

---

## Sequence

| | Task | Status |
|---|---|---|
| **Previous** | M0-02 — Confirm stored-procedure drift across tenant databases (Q-14) | `Blocked` on a human (DBA fingerprints), tooling half delivered, unmerged (`migration/M0-02-sp-drift-across-tenants`) |
| **Current** | **M0-07 — CI pipeline: restore → build → analyzers** | `BLOCKED` (environment) |
| **Next (candidate)** | Not selected this session — re-derive at M0-07's close-out against `task-tracker.md`. `M0-03-02` (Ready, P0, off the critical path, no file overlap with `M0-07`) is the likely next candidate per the Ready-task selection rule, unless a human unblocks `M0-07` or `M0-02` first. `M0-12`/`M0-12-01`/`M0-12-02`/`M0-13`/`M0-09`/`M0-06` all remain `Blocked` pending `M0-07`. |

The next task is **selected, not assumed** — apply
[`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
against the tracker at the next session's start, because status may have moved (in
particular, a human may have unblocked `M0-07` or `M0-02` by then).

---

## Open flags on this task

- **This is an environment stop, not a code defect.** A same-spec retry (attempt 2) would
  only re-obtain the same result — do not spend it without a human first lifting the
  constraint (pushing the branch, or granting explicit push/merge permission in-conversation).
- **Never push or merge without an explicit instruction in the current conversation.**
  Approval for pushing this branch, once given, does not carry to future tasks.
- `ci/warning-baseline.json`'s `"provisional": true` / `"measured_on": "developer-workstation"`
  fields must **not** be edited to read as runner-produced just to satisfy the acceptance
  criterion — that would be the "silently adjusted check" the workflow forbids. Only a real
  Actions run may regenerate them honestly.
