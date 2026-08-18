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
(that dependency is now `Completed` and merged — file-level serialisation satisfied, safe to
proceed).

**M0-14 also edits `V.SMART/V.SMART.Web/Program.cs`, which `M0-03-03` would also edit** —
`M0-03-03` inserts startup configuration validation after line 180 and before line 226; the
line this task changes (around line 192, **re-verify against current code, do not trust this
task file's line numbers** — KB-002 authority order) sits between those two points. `M0-03-03`
is currently `Blocked` (its Hard prerequisite `M0-03-02` is `Needs Review`, not `Completed` —
see *Why this task, not another* below), so it is **not in flight**. No live conflict exists
right now, but re-check `git status` / the tracker for `M0-03-03` before editing
`Program.cs`, in case a parallel session has since opened it.

Confirm `M0-03-01` is `Completed` (it is — merged `f55db52`) before touching
`appsettings.json`. If the `ConnectionStrings` section of that file still contains a
credential when you open it, `M0-03-01` did not actually land as claimed — stop and report
this task as `Blocked`, do not proceed.

---

## Why this task, not another

Selected per [`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
at `M0-03-02`'s close-out (2026-08-18). `M0-03-02` validated `PASS` and moved to
`Needs Review` (committed on `migration/M0-03-02-hardcoded-connection-strings-csharp`,
`e6e5295`, unmerged) — **not** `Completed`, so it does not unblock `M0-03-03` (Hard
prerequisite requires `Completed`, not `REVIEW`).

Candidate set from the tracker at this point: **`M0-14` only**.
- `M0-03` is a parent container — never worked directly, skipped.
- `M0-03-02` just closed to `Needs Review` — not re-selectable as `Ready`.
- `M0-03-03` is `Blocked` on `M0-03-02` reaching `Completed`.
- `M0-02` is `Blocked`⁶ on a DBA (no owner scheduled).
- `M0-04` is `Blocked`⁴ on an unidentified human owner.
- `M0-07` is `Blocked`⁷ on `origin` push access / GitHub org admin rights.
- `M0-01-03` is `Needs Review`, not `Ready` — its remaining step is a human-executed rebuild
  drill, not resumable by a session.
- Everything in M2+ is behind Gate `G0`, which is not yet met.
- `M0-14`'s only Hard prerequisite, `M0-03-01`, is genuinely `Completed` (merged `f55db52`);
  it is not a parent container; it is not blocked on an unscheduled human step; per the
  *Same-file conflicts* table it shares `V.SMART/V.SMART.Web/appsettings.json` only with
  `M0-03-01` (already `Completed`, not in-flight) — no live same-file conflict.

Sole candidate — no rank tie-break needed.

## Dependencies

| Dependency | Class | State |
|---|---|---|
| **M0-03-01** — `appsettings.json` → environment / user-secrets | Hard (file-level only) | **Completed**, merged `f55db52`. Serialises edits to `V.SMART/V.SMART.Web/appsettings.json`. |
| **M0-03-03** — fail-fast startup validation | Soft (file-level only) | **Blocked**, not in flight. Both would edit `V.SMART/V.SMART.Web/Program.cs`; since M0-03-03 has not started, no live conflict — but re-check before editing. |
| M0-00 — clean version-control baseline | Hard | **Completed**, transitively via M0-03-01. |
| Deployment environment configuration (`ASPNETCORE_ENVIRONMENT` genuinely not `Development` in production) | Information | **Unknown** — Q-16 (deployment topology) is unanswered. The fix is only effective if this holds; record it as an assumption and flag Q-16 in the final report. |

## Relevant Documentation

Read only these.

| doc_id | Path | Why |
|---|---|---|
| TASK | [`tasks/M0-14.md`](tasks/M0-14.md) | The binding specification — read in full |
| KB-083 | [`prompt-template.md`](prompt-template.md) | Verified-commands table; note whether the `V.SMART.Web.csproj` build result is already filled in from a later measurement (M0-15 recorded 6,698 warnings, 0 errors for it 2026-08-17 — re-verify this is still current) |
| KB-060 R-20 | [`../risks/technical-debt-register.md`](../risks/technical-debt-register.md) | The risk this task closes |
| KB-015 | [`../architecture/frontend-architecture-existing.md`](../architecture/frontend-architecture-existing.md) | The Blazor Server host this option belongs to |
| KB-004 | [`../open-questions.md`](../open-questions.md) | Q-16, deployment topology — still Unknown |
| KB-003 | [`../investigation-registry.md`](../investigation-registry.md) | INV-029 — amend, do not open a new id |
| KB-002 | [`../source-of-truth-rules.md`](../source-of-truth-rules.md) | Code wins; re-verify line numbers before citing them |

## Relevant Existing Code (read before editing)

- `V.SMART/V.SMART.Web/Program.cs` — around lines 187-193 per the task file's 2026-08-12
  reading (`AddRazorComponents().AddInteractiveServerComponents()` then
  `AddServerSideBlazor(options => { options.DetailedErrors = true; })`). **Re-verify these
  line numbers against current code** — `M0-03-01` (merged) and other M0 work may have moved
  them.
- `V.SMART/V.SMART.Web/appsettings.json` — the `"DetailedError": true` key (singular, not
  `DetailedErrors`), reported at line 15 as of 2026-08-12. Re-verify.
- `V.SMART/V.SMART.Api/Program.cs:107` — `if (app.Environment.IsDevelopment())` around
  Swagger. **Read-only** — this is the correct existing pattern to follow; do not modify it.
- `V.SMART/V.SMART.Web/appsettings.Development.json` — contains only a `Logging` section
  (Confirmed 2026-08-12); relevant only if the JSON key at `appsettings.json:15` turns out to
  be live (expected: dead).

**Must not change:** any other line of `Program.cs` (in particular the tenant/DbContext
registrations and the ~242-registration DI graph), `V.SMART/V.SMART.Api/Program.cs`,
`V.SMART/V.SMART.Shared/**`, `V.SMART/V.SMART/appsettings.json` (MAUI host, no
`DetailedError` key), anything under any `bin/` directory, the `ConnectionStrings` section of
`V.SMART/V.SMART.Web/appsettings.json`.

## Business Rules

**None modified.** This alters only the verbosity of error information sent to the browser —
no calculation, validation, permission or persistence path is affected.

## Carried forward from M0-03-02 (closed out 2026-08-18, `Needs Review`)

- `V.SMART/V.SMART.Shared/Data/MigrationData/ApplicationDbContextFactory.cs`,
  `MasterDbContextFactory.cs`, and `V.SMART/V.SMART/MauiProgram.cs` no longer contain
  connection-string literals — not relevant to M0-14's own file set, but confirms the pattern
  ("configuration read, throw on missing value, no silent default") this task should follow
  for its own environment-conditional logic.
- Two `M0-03-02` branches now exist:
  `migration/M0-03-02-hardcoded-connection-strings-csharp` (current, commit `e6e5295`) and a
  superseded `migration/M0-03-02-hardcoded-connection-strings` (no `-csharp`, pre-M0-15-recut)
  — no file overlap with `M0-14`'s scope, so no same-file conflict applies.
- The Api build baseline moved to **6,694 warnings, 0 errors** on `M0-03-02`'s branch (from
  6,695) — if this task's Api build cross-check reports a different count, compare against
  6,694, not the older 6,695 figure, once `M0-03-02` is merged. Until merge, `master`'s
  baseline is still 6,695; state which baseline you are comparing against.

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
- `docs/kb/execution/prompt-template.md` — fill in the measured Web build result if still
  unrecorded.
- `tasks/M0-14.md` — record the outcome; move `status`; bump `last_verified`.
- `task-tracker.md` (KB-081) — update as the last step.

## Completion Conditions

Reaches `COMPLETED` only after human review and merge (KB-088 "Who may set COMPLETED"). The
honest in-session end state is `Needs Review` once implemented, verified and committed.

---

## Sequence

| | Task | Status |
|---|---|---|
| **Previous** | M0-03-02 — Hardcoded connection strings in C# | `Needs Review` 2026-08-18 (validated PASS on attempt 1, unmerged, `migration/M0-03-02-hardcoded-connection-strings-csharp`) |
| **Current** | **M0-14 — Gate `DetailedErrors` on `IsDevelopment()`** | `READY` |
| **Next (candidate)** | None else dependency-ready as of this selection — re-derive at M0-14's close-out. `M0-03-03` becomes a candidate only once `M0-03-02` reaches `Completed` (reviewed and merged). |

The next task is **selected, not assumed** — apply
[`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
against the tracker at completion time, because status may have moved (in particular,
`M0-03-02` or `M0-01-03` may have been merged by then).

---

## Open flags on this task

- **Re-verify every line number this task file and `tasks/M0-14.md` cite** before editing —
  both were last verified 2026-08-12, and `M0-03-01` has since merged into `master`, which may
  have shifted line numbers in `appsettings.json` or `Program.cs`.
- **Do not run concurrently with a session that opens `M0-03-03`.** Check `git status` and
  `task-tracker.md` for `M0-03-03` immediately before touching `Program.cs`.
- State plainly whether the deployment-environment assumption (`ASPNETCORE_ENVIRONMENT` is
  not `Development` in production) holds — it cannot be verified from the repository, and Q-16
  remains open.
