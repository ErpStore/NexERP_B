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
last_verified: 2026-08-18
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
| **Task ID** | **M0-14** |
| **Task** | Gate `DetailedErrors` on `IsDevelopment()` |
| **Status** | `READY` — not yet started |
| **Milestone** | M0 — Stabilise (Gate G0 — this task is P2 hygiene, not on G0's own checklist) |
| **Type** | Security |
| **Priority** | P2 |
| **Estimate** | 0.5 d |
| **Full specification** | [`tasks/M0-14.md`](tasks/M0-14.md) |
| **Branch** | `migration/M0-14-gate-detailed-errors` (not yet cut) |

---

## Objective

Stop the Blazor Server host from sending .NET stack traces to the browser in production.
`DetailedErrors` is currently enabled unconditionally in two places — a hardcoded `true` in
`V.SMART/V.SMART.Web/Program.cs` and a `"DetailedError": true` key in
`V.SMART/V.SMART.Web/appsettings.json`. Both must become conditional on
`IHostEnvironment.IsDevelopment()`. Full spec: [`tasks/M0-14.md`](tasks/M0-14.md).

Serves **R-20** in [KB-060](../risks/technical-debt-register.md).

---

## ⚠ Conflict risk — read before starting

**M0-14 edits `V.SMART/V.SMART.Web/appsettings.json`**, the same file `M0-03-01` edited
(that dependency is `Completed` and merged — file-level serialisation satisfied, safe to
proceed).

**M0-14 also edits `V.SMART/V.SMART.Web/Program.cs`, which `M0-03-03` has already edited on
its own unmerged branch.** `M0-03-03` (`Needs Review` as of 2026-08-18, validated `PASS`,
committed on `migration/M0-03-03-startup-config-validation`, commit `34be11a`, whose parent is
`d4ba526` — `master`'s tip when this task opened, **not** `0a20d62` as an earlier record
misstated — **not merged**) inserted 6 lines of startup-configuration-validation calls
immediately after `var builder = WebApplication.CreateBuilder(args);` (line 180) and before
the `MasterDb` connection-string read (originally line 226). That displaced
`options.DetailedErrors = true;` — this task's own target line — from **line 192 to line
198** *on that branch only*. `master` itself is unaffected until `M0-03-03` is reviewed and
merged, so if this task branches from `master` (the normal case), the target line is still
**192** at the point this task opens.

**Rule for the executor:** re-verify the line number against whatever `master` (or your
actual branch point) currently shows — do not trust either this note or `tasks/M0-14.md`'s
2026-08-12 figures. If `M0-03-03` merges before this task's diff is reviewed, expect a small,
already-flagged mechanical merge in `Program.cs` (both sides insert near the top of
`Program.cs`, well clear of each other once resolved) — `M0-03-03`'s own task file records
this expectation. Say in your final report which line number you found and whether a merge
was needed.

Confirm `M0-03-01` is `Completed` (it is — merged `f55db52`) before touching
`appsettings.json`. If the `ConnectionStrings` section of that file still contains a
credential when you open it, `M0-03-01` did not actually land as claimed — stop and report
this task as `Blocked`, do not proceed.

---

## Why this task, not another

Selected per [`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
at `M0-03-03`'s close-out (2026-08-18). `M0-03-03` validated `PASS` and moved to
`Needs Review` (committed on `migration/M0-03-03-startup-config-validation`, `34be11a`,
unmerged) — **not** `Completed`, so it does not itself become a candidate again, and nothing
downstream of it (none exists in the tracker) is unblocked yet.

Candidate set from the tracker at this point: **`M0-14` only**.
- `M0-03` is a parent container — never worked directly, skipped.
- `M0-03-02` and `M0-03-03` are both `Completed`/`Needs Review` respectively and neither is
  re-selectable as `Ready` (`M0-03-02` genuinely `Completed`; `M0-03-03` sits at `Needs
  Review`, awaiting merge).
- `M0-02` is `Blocked`⁶ on a DBA (no owner scheduled).
- `M0-04` is `Blocked`⁴ on an unidentified human owner.
- `M0-07` is `Blocked`⁷ on `origin` push access / GitHub org admin rights.
- `M0-01-03` is `Needs Review`, not `Ready` — its remaining step is a human-executed rebuild
  drill, not resumable by a session.
- Everything in M2+ is behind Gate `G0`, which is not yet met.
- `M0-14`'s only Hard prerequisite, `M0-03-01`, is genuinely `Completed` (merged `f55db52`);
  it is not a parent container; it is not blocked on an unscheduled human step; per the
  *Same-file conflicts* table it shares `V.SMART/V.SMART.Web/appsettings.json` only with
  `M0-03-01` (already `Completed`, not in-flight) — no live same-file conflict. Its Soft
  co-editor of `Program.cs`, `M0-03-03`, is committed on its own branch and no session is
  currently running against it, so it is not "in-flight" for the purpose of the same-file
  conflict rule — but see the conflict-risk note above for what to expect on merge.

Sole candidate — no rank tie-break needed.

## Dependencies

| Dependency | Class | State |
|---|---|---|
| **M0-03-01** — `appsettings.json` → environment / user-secrets | Hard (file-level only) | **Completed**, merged `f55db52`. Serialises edits to `V.SMART/V.SMART.Web/appsettings.json`. |
| **M0-03-03** — fail-fast startup validation | Soft (file-level only) | **Needs Review**, committed and validated `PASS` but unmerged (`34be11a`). Both edit `V.SMART/V.SMART.Web/Program.cs`; expect a small merge if `M0-03-03` lands before this task's review — see conflict-risk note above. |
| M0-00 — clean version-control baseline | Hard | **Completed**, transitively via M0-03-01. |
| Deployment environment configuration (`ASPNETCORE_ENVIRONMENT` genuinely not `Development` in production) | Information | **Unknown** — Q-16 (deployment topology) is unanswered. The fix is only effective if this holds; record it as an assumption and flag Q-16 in the final report. |

## Relevant Documentation

Read only these.

| doc_id | Path | Why |
|---|---|---|
| TASK | [`tasks/M0-14.md`](tasks/M0-14.md) | The binding specification — read in full |
| KB-083 | [`prompt-template.md`](prompt-template.md) | Verified-commands table; `V.SMART.Web.csproj` build result is now filled in — `M0-03-03`'s branch measured 0 errors / 5 warnings (warm) and 6,697 warnings (`--no-incremental`); re-verify this is still current on your own branch point |
| KB-060 R-20 | [`../risks/technical-debt-register.md`](../risks/technical-debt-register.md) | The risk this task closes |
| KB-015 | [`../architecture/frontend-architecture-existing.md`](../architecture/frontend-architecture-existing.md) | The Blazor Server host this option belongs to |
| KB-004 | [`../open-questions.md`](../open-questions.md) | Q-16, deployment topology — still Unknown |
| KB-003 | [`../investigation-registry.md`](../investigation-registry.md) | INV-029 — amend, do not open a new id |
| KB-002 | [`../source-of-truth-rules.md`](../source-of-truth-rules.md) | Code wins; re-verify line numbers before citing them |

## Relevant Existing Code (read before editing)

- `V.SMART/V.SMART.Web/Program.cs` — around lines 187-193 per the task file's 2026-08-12
  reading (`AddRazorComponents().AddInteractiveServerComponents()` then
  `AddServerSideBlazor(options => { options.DetailedErrors = true; })`). **Re-verify these
  line numbers against your actual branch point** — `M0-03-01` (merged) shifted them once
  already, and `M0-03-03` (unmerged, on its own branch) shifts them again by 6 lines if and
  when it merges.
- `V.SMART/V.SMART.Web/appsettings.json` — the `"DetailedError": true` key (singular, not
  `DetailedErrors`), reported at line 15 as of 2026-08-12. Re-verify.
- `V.SMART/V.SMART.Api/Program.cs:107` — `if (app.Environment.IsDevelopment())` around
  Swagger. **Read-only** — this is the correct existing pattern to follow; do not modify it.
- `V.SMART/V.SMART.Web/appsettings.Development.json` — contains only a `Logging` section
  (Confirmed 2026-08-12); relevant only if the JSON key at `appsettings.json:15` turns out to
  be live (expected: dead).

**Must not change:** any other line of `Program.cs` (in particular the tenant/DbContext
registrations, the ~242-registration DI graph, and — if `M0-03-03` has merged by the time you
open this task — `StartupConfigurationValidator` calls it added), `V.SMART/V.SMART.Api/Program.cs`,
`V.SMART/V.SMART.Shared/**`, `V.SMART/V.SMART/appsettings.json` (MAUI host, no
`DetailedError` key), anything under any `bin/` directory, the `ConnectionStrings` section of
`V.SMART/V.SMART.Web/appsettings.json`.

## Business Rules

**None modified.** This alters only the verbosity of error information sent to the browser —
no calculation, validation, permission or persistence path is affected.

## Carried forward from M0-03-03 (closed out 2026-08-18, `Needs Review`)

- `V.SMART/V.SMART.Shared/Services/StartupConfigurationValidator.cs` (new) is now the single
  place both hosts validate `ConnectionStrings:MasterDb` (and, for the API,
  `Jwt:Secret`/`Jwt:Issuer`/`Jwt:Audience`) before startup — not part of M0-14's own file set,
  but confirms the pattern ("fail fast with an actionable message, never echo the offending
  value") this task's own `IsDevelopment()` gating should sit comfortably alongside.
- `V.SMART/V.SMART.Web/Program.cs` on `M0-03-03`'s branch (`34be11a`) inserted its 6-line
  validator call after line 180, displacing `options.DetailedErrors = true;` from line 192 to
  line 198. If that branch merges before this task's own branch is cut or reviewed, re-verify
  the line number — do not assume 192.
- The Web build baseline measured on `M0-03-03`'s branch: 0 errors, 5 warnings warm /
  6,697 warnings `--no-incremental`. The Api build baseline: 0 errors, 6,695 warnings
  (unchanged from `master`). Compare your own build against whichever branch point you cut
  from.
- One open item for **M0-04**, not relevant to M0-14's own scope: one of `M0-03-03`'s seven
  deny-list digests could not be reproduced from git history (provenance stated honestly in
  the code comment) — see [`tasks/M0-03-03.md` § Execution Record (2026-08-18) — Close-out
  reconciled to master](tasks/M0-03-03.md#execution-record-2026-08-18--close-out-reconciled-to-master).

## Acceptance Criteria

Full checklist: [`tasks/M0-14.md` § Acceptance Criteria`](tasks/M0-14.md#acceptance-criteria).
Summary:
1. `Program.cs` sets `DetailedErrors` from `builder.Environment.IsDevelopment()` — no literal
   `true` remaining.
2. `git grep -in "DetailedErrors = true" -- "V.SMART/"` returns zero hits outside `bin/`.
3. The dead `"DetailedError"` key in `appsettings.json` is deleted — or, if proven live, set
   `false` there / `true` in `appsettings.Development.json` with the binding site named.
4. `AddRazorComponents().AddInteractiveServerComponents()` registration unchanged.
5. `ConnectionStrings` section of `appsettings.json` unchanged and contains no credential.
6. `dotnet build V.SMART/V.SMART.Web/V.SMART.Web.csproj` result recorded.
7. `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` → 0 errors.
8. Two-environment manual check reported, or its absence explicitly stated.
9. KB-060 R-20 updated with exact `file:line`, marked resolved.
10. Report states how this was sequenced against M0-03-01/M0-03-03, and any merge needed.
11. Report records the `ASPNETCORE_ENVIRONMENT` assumption and flags Q-16 as still Unknown.

## Testing Requirements

**No test project exists** (INV-023, Confirmed) — do not run `dotnet test`. Verification is
two builds, a grep, and a two-environment manual check (report honestly if no SQL Server is
available to run it — inspection-only is an acceptable, explicitly-stated outcome).

## Documentation Updates

- `docs/kb/risks/technical-debt-register.md` — R-20: exact `file:line`, mark resolved, bump
  `last_verified`.
- `docs/kb/investigation-registry.md` — amend INV-029 with the negative finding (grep for a
  reader of `DetailedError`, found none — or found one, name it). Bump `last_verified`.
- `docs/kb/execution/prompt-template.md` — the measured Web build result is already recorded
  as of `M0-03-03`; only touch this if your own measurement differs.
- `tasks/M0-14.md` — record the outcome; move `status`; bump `last_verified`.
- `task-tracker.md` (KB-081) — update as the last step.

## Completion Conditions

Reaches `COMPLETED` only after human review and merge (KB-088 "Who may set COMPLETED"). The
honest in-session end state is `Needs Review` once implemented, verified and committed.

---

## Sequence

| | Task | Status |
|---|---|---|
| **Previous** | M0-03-03 — Fail-fast startup validation | `Needs Review` 2026-08-18 (validated PASS on attempt 1 of 4, unmerged, `migration/M0-03-03-startup-config-validation`, `34be11a`) |
| **Current** | **M0-14 — Gate `DetailedErrors` on `IsDevelopment()`** | `READY` |
| **Next (candidate)** | None else dependency-ready as of this selection — re-derive at M0-14's close-out. |

The next task is **selected, not assumed** — apply
[`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
against the tracker at completion time, because status may have moved (in particular,
`M0-03-03`, `M0-01-03`, `M0-07` or `M0-04` may have been reviewed/merged/unblocked by then).

---

## Open flags on this task

- **Re-verify every line number this task file and `tasks/M0-14.md` cite** before editing —
  both were last verified 2026-08-12, and `M0-03-01` (merged) and `M0-03-03` (unmerged, on
  its own branch) have each shifted `Program.cs` line numbers since.
- **Check whether `M0-03-03` has merged before finishing this task's diff.** If it has,
  expect the small mechanical `Program.cs` merge its own task file already flags.
- State plainly whether the deployment-environment assumption (`ASPNETCORE_ENVIRONMENT` is
  not `Development` in production) holds — it cannot be verified from the repository, and Q-16
  remains open.
