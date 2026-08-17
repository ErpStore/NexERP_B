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
| **Task ID** | **M0-03-01** |
| **Task** | `appsettings.json` → environment / user-secrets |
| **Status** | `READY` |
| **Milestone** | M0 — Stabilise (Gate G0) |
| **Type** | Security |
| **Priority** | P0 |
| **Estimate** | 0.5 d |
| **Full specification** | [`tasks/M0-03-01.md`](tasks/M0-03-01.md) |
| **Branch** | `migration/M0-03-01-externalise-appsettings-secrets` |
| **Commit subject** | `M0-03-01: Externalise Web and Api appsettings secrets to user-secrets/environment` |

---

## Objective

Remove every credential and environment-specific value from the `appsettings.json` files that
ship in the repository, and source them at runtime from user-secrets in development and
environment variables elsewhere, so that no secret is readable from a checkout and each host
is configured per environment rather than per commit.

The hosts in scope are `V.SMART.Web` and `V.SMART.Api`. Behaviour must not change: the same
configuration keys resolve to the same values at runtime, only their storage moves. Rotating
the exposed credentials is **not** part of this task — that is `M0-04` — and neither is purging
them from git history, which is `M0-05`. Full specification, including the exact files expected
to change: [`tasks/M0-03-01.md`](tasks/M0-03-01.md).

---

## Run State

| Field | Value |
|---|---|
| **Runner state** | `NOT_STARTED` — no run has opened this task |
| **Canonical status** | `READY` (the row above; KB-081 is authoritative) |
| **Attempt** | 0 of 3 (`max_retries: 2`) |
| **Failure log** | no entries — [`failure-log.md`](failure-log.md) (KB-092) |

**Live run state is in [`runner-state.md`](runner-state.md) (KB-093), not here.**

---

## Why this task, not another

Selected per [`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
at M0-15's close-out (2026-08-17). Candidates in the tracker's `Ready` set were `M0-04`,
`M0-08`, `M0-03`, `M0-03-01`, `M0-02`.

- `M0-03` is a parent container — never worked directly.
- P0 tier: `M0-04` and `M0-03-01` (`M0-08` and `M0-02` are P1, ranked below the P0 tier and
  not reached).
- Rank step 2, most downstream unblocking: `M0-03-01` directly unblocks **two** tasks
  (`M0-03-02`, `M0-14`); `M0-04` unblocks **one** (`M0-05`, shared with `M0-03`). `M0-03-01`
  wins.
- No same-file conflict with any in-flight work — nothing else is currently open.

**A tracker correction worth making when someone next touches it, not made here to keep this
task single-scope:** `M0-02` depends on DBA access nobody has scheduled, so it is arguably
`BLOCKED` on a human rather than genuinely `Ready` — `dependency-graph.md`'s worked example
already flags this. It did not affect this selection since `M0-02` is P1 and never reached
rank step 4.

## Dependencies

| Dependency | Class | State |
|---|---|---|
| **M0-00** — clean version-control baseline | Hard | `COMPLETED` — satisfied |
| **M0-04** — rotate the exposed credentials | Information | Not required to exist yet — placeholder/local values are sufficient for this task; runs in parallel |
| **INV-029** — repository exposure / toolchain findings | Information | **Already has a row** in `docs/kb/investigation-registry.md` (added by M0-00, amended by M0-15) — see *Carried forward* below, the task file's own text describing it as missing is stale |
| **M0-14** — gate `DetailedErrors` | Deployment (reverse) | Edits the **same file** (`V.SMART/V.SMART.Web/appsettings.json`); sequenced *after* this task. Do not touch `"DetailedError"` here |

## Relevant Documentation

Read only these.

| doc_id | Path | Why |
|---|---|---|
| TASK | [`tasks/M0-03-01.md`](tasks/M0-03-01.md) | The binding specification |
| KB-083 | [`prompt-template.md`](prompt-template.md) | Verified-commands table — updated by M0-15, now current |
| KB-080 | [`README.md`](README.md) §7 | M0 scope, sequencing, G0 gate |
| KB-060 | [`../risks/technical-debt-register.md`](../risks/technical-debt-register.md) | R-01, R-02 |
| KB-014 | [`../architecture/multi-tenancy.md`](../architecture/multi-tenancy.md) | Why this task does **not** externalise per-tenant connection strings |
| KB-003 | [`../investigation-registry.md`](../investigation-registry.md) | INV-029 — reuse, do not re-add |

## Relevant Existing Code

- `V.SMART/V.SMART.Web/appsettings.json`, `V.SMART/V.SMART.Api/appsettings.json` — edited
- `V.SMART/V.SMART.Web/appsettings.Development.json`, `V.SMART/V.SMART.Api/appsettings.Development.json` — left as-is (no credentials, confirmed 2026-08-12; re-verify)
- `V.SMART/V.SMART.Web/V.SMART.Web.csproj`, `V.SMART/V.SMART.Api/V.SMART.Api.csproj` — `UserSecretsId` added
- `V.SMART/V.SMART.Web/Program.cs`, `V.SMART/V.SMART.Api/Program.cs` — **read only**
- `.gitignore` — **read only**, check before assuming anything is ignored

## Business Rules

**None.** This task moves where configuration values are read from; it does not change any
calculation, validation, permission or persistence path.

## Carried forward from M0-15 (toolchain/build baseline — Needs Review, 2026-08-17)

- **The installed SDK set has drifted and is now pinned.** A root `global.json` exists,
  pinning `10.0.400` with `rollForward: latestFeature`. `dotnet build` commands in this task
  will use that SDK, not `10.0.300`/`10.0.302` as older prose in this task's own file
  describes — that is expected and not a deviation to report.
- **The Api warning baseline to compare against is 6,695** (confirmed reproducible,
  `docs/kb/execution/M0-15-build-baseline.md`, KB-086) — this task's own acceptance criteria
  already cite that number, it has not changed.
- **`docs/kb/investigation-registry.md`'s INV-029 row already exists** (added by M0-00,
  amended by M0-15 with the solution-build finding and the corrected warning-code breakdown).
  This task's spec describes INV-029 as a row this session must add — that is now stale;
  reuse the existing row, do not re-add it or create a duplicate.
- **`docs/kb/execution/README.md` §6 finding 3 was not updated by M0-15**, despite being on
  that task's own documentation list — a recorded gap in
  [`tasks/M0-15.md` § Execution Record](tasks/M0-15.md#execution-record-2026-08-17). Not this
  task's concern; noted so a future session doesn't assume it was done.
- **`V.SMART/V.SMART.Api/` remains untracked by design** (the known checkout trap in
  `CLAUDE.md`). This task makes its `appsettings.json` sanitised and trackable but must **not**
  `git add` the whole directory — that decision belongs to M0-00/M0-05, and mixing it in here
  would put two task scopes on one branch (the task file already says this explicitly).

## Acceptance Criteria

The full checklist is [`tasks/M0-03-01.md` § Acceptance Criteria`](tasks/M0-03-01.md#acceptance-criteria)
and it is binding. Summary:

- `git grep -n "Password="` and `git grep -n "154.61"` over `V.SMART/V.SMART.Web` and
  `V.SMART/V.SMART.Api` both return **zero** hits.
- `V.SMART/V.SMART.Api/appsettings.json`'s `Jwt:Secret` is `""`, no 32+ character secret
  literal anywhere.
- Neither `appsettings.json` contains a commented-out connection string.
- `V.SMART/V.SMART.Web/appsettings.json` still contains `"DetailedError": true`, untouched.
- Both `.csproj` files contain a `UserSecretsId`.
- `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` → 0 errors, warning count recorded
  and compared against 6,695.
- `V.SMART.Api` starts with user-secrets-supplied configuration; fails with the existing
  explicit message when `Jwt:Secret` is removed.
- A developer note (README or new `docs/CONFIGURATION.md`) lists all five keys in both
  environment-variable and user-secrets form.
- `docs/kb/investigation-registry.md` contains an INV-029 row (already true — verify, don't
  duplicate).
- The diff touches only the files listed under *Files Expected to Change* in the task file.

## Testing Requirements

No automated tests — no test project exists (INV-023). `dotnet test` **must not** be run.
Verification is a successful build, a manual start of each host against user-secrets-supplied
configuration, and a `git grep` sweep proving credentials are gone from the working tree. Full
steps: [`tasks/M0-03-01.md` § Testing`](tasks/M0-03-01.md#testing).

## Documentation Updates

Landing this task requires these documents to be updated in the same commit — the task is not
complete while any of them still describes the old arrangement:

- `docs/CONFIGURATION.md` *(created by this task — does not exist on `master` yet)* — how each
  host is configured, which keys
  are required, and how to populate them via user-secrets locally and environment variables
  elsewhere. This is the document a new developer follows to get a working checkout running.
- [`tasks/M0-03-01.md`](tasks/M0-03-01.md) — record the outcome and move `status` to
  `Needs Review`.
- [`task-tracker.md`](task-tracker.md) (KB-081) — M0-03-01 → `Needs Review`. KB-081 is the
  status authority; update it as the last step.
- [`docs/kb/risks/technical-debt-register.md`](../risks/technical-debt-register.md) — close or
  amend the entry covering credentials committed in `appsettings.json`, noting that history
  still contains them until `M0-05` runs.
- Any secret discovered that was not already known belongs in
  [`open-questions.md`](../open-questions.md), not in a commit message.

## Completion Conditions

This task reaches `COMPLETED` only when every acceptance criterion is objectively met, the
manual start/config checks were actually run and observed, documentation updates landed, and
the diff is committed single-scope on `migration/M0-03-01-externalise-appsettings-secrets`.
Reaching `REVIEW` (committed, unmerged, awaiting a reviewer) is the expected end state of an
execution session.

---

## Sequence

| | Task | Status |
|---|---|---|
| **Previous** | M0-15 — toolchain and build baseline | `Needs Review` 2026-08-17 (validated PASS, unmerged) |
| **Current** | **M0-03-01 — appsettings.json → environment / user-secrets** | `READY` |
| **Next (candidate)** | M0-03-02 — hardcoded connection strings in C# | `BLOCKED` on this task |
| **Next (independent)** | M0-04, M0-08, M0-02 | all `READY`, none conflict with this task's files |

The next task is **selected, not assumed** — apply
[`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
against the tracker at completion time, because status may have moved.

---

## Open flags on this task

- None known. The working tree is clean apart from the documented stash
  (`PRE-M0-15: local tenant DB debugging …`, recoverable via `git stash apply`) and the
  by-design-untracked `V.SMART/V.SMART.Api/`. Re-verify `git status --porcelain` before
  starting, per this project's standing rule — state may have moved since 2026-08-17.
