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
| **Task ID** | **M0-04** |
| **Task** | Rotate the exposed credentials |
| **Status** | `READY` |
| **Milestone** | M0 — Stabilise (Gate G0) |
| **Type** | Security |
| **Priority** | P0 |
| **Estimate** | 1 d |
| **Full specification** | [`tasks/M0-04.md`](tasks/M0-04.md) |
| **Branch** | `migration/M0-04-<slug>` (not yet cut) |

---

## Objective

Rotate every credential that has been published, and record that it was done. **Most of
this task is outside the repository** — rotating a SQL login on a production host,
re-keying the GST e-Invoice/e-Way gateway account, and any production deployment step are
human actions requiring production access this session does not have. What an AI session
can and must deliver: (1) a rotation runbook (`docs/runbooks/credential-rotation.md` —
does not exist yet), (2) a complete credential-usage inventory including locations outside
the repository, (3) an objective human verification checklist, and (4) confirming Q-19's
answer is current (it already is — see *Carried forward* below). Full spec:
[`tasks/M0-04.md`](tasks/M0-04.md).

If nobody with production access performs the actual rotation during this session, the
honest end status is `Blocked` (or `Needs Review` for the deliverable documents alone), not
`Completed` — and the report must name who it is blocked on.

---

## Run State

| Field | Value |
|---|---|
| **Runner state** | `NOT_STARTED` — no run has opened this task |
| **Canonical status** | `READY` (the row above; KB-081 is authoritative) |
| **Attempt** | 0 of 3 (`max_retries: 2`) |
| **Failure log** | no M0-04 entries — [`failure-log.md`](failure-log.md) (KB-092) |

**Live run state is in [`runner-state.md`](runner-state.md) (KB-093), not here.**

---

## Why this task, not another

Selected per [`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
at M0-03-01's close-out (2026-08-17). M0-03-01 validated `PASS` on attempt 2 and moved to
`Needs Review` (committed, unmerged) — **not** `Completed`, so it does not unblock
`M0-03-02` (Hard prerequisite requires `Completed`, not `REVIEW`).

Candidate set from the tracker's `Ready` tier: `M0-04`, `M0-08`, `M0-03` (parent — skipped),
`M0-02`.

- P0 tier: only `M0-04` (`M0-08` and `M0-02` are P1).
- No same-file conflict with any in-flight work — nothing else is currently open. (`M0-04`'s
  `source_files` do list `V.SMART/V.SMART.Web/appsettings.json` and
  `V.SMART/V.SMART.Api/appsettings.json`, the same files M0-03-01 just edited — but M0-03-01
  is no longer in-flight, it is committed and closed out, so this is not a live conflict.)
- `M0-04` has no Hard repository dependency (`depends_on: []`); its Hard dependencies are
  organisational (a named person with SQL/GitHub/gateway access), which the task itself is
  required to identify and escalate rather than wait on silently.

## Dependencies

| Dependency | Class | State |
|---|---|---|
| **Q-19** — is the public repo visibility intended? | Hard (task's own step 1) | **Already answered** 2026-08-12 — owner deliberately made the repo public; see *Carried forward* below. Re-verify it is still filed in `open-questions.md` before treating step 1 as satisfied; do not re-raise a duplicate Q-19. |
| Person with production SQL access | Hard (for the actual rotation) | Not established in-repository. Escalate, do not guess who. |
| Person with GST e-Invoice/e-Way gateway account access | Hard (for that one credential) | Not established in-repository. Escalate. |
| **M0-03** (externalise configuration secrets, parent) | Soft | M0-03-01 (its first child) is done and `Needs Review`, not yet merged. If M0-03's mechanism has not landed by the time rotation happens, deploy new values via environment variables directly and record that as interim — the task file's own sequencing note. |
| **M0-05** (purge git history) | Deployment (reverse) | Strictly after this task. Must not run before M0-04 completes. |
| **M0-06** (remove seeded default Administrator credential) | Information | A separate credential (R-09) with its own task — list it in the inventory, do not fold it in here. |

## Relevant Documentation

Read only these.

| doc_id | Path | Why |
|---|---|---|
| TASK | [`tasks/M0-04.md`](tasks/M0-04.md) | The binding specification |
| KB-083 | [`prompt-template.md`](prompt-template.md) | Verified-commands table |
| KB-080 | [`README.md`](README.md) §6, §7, §23 | The exposure finding, M0 scope, Q-19's origin |
| KB-060 | [`../risks/technical-debt-register.md`](../risks/technical-debt-register.md) | R-01, R-02, R-09 |
| KB-014 | [`../architecture/multi-tenancy.md`](../architecture/multi-tenancy.md) | **Essential** — per-tenant connection strings live in plaintext in the `Tenants` table; rotating a DB login without updating those rows takes every tenant offline |
| KB-004 | [`../open-questions.md`](../open-questions.md) | Q-19 is already filed and answered — confirm, do not re-file |
| KB-003 | [`../investigation-registry.md`](../investigation-registry.md) | INV-029 for exposure facts — reuse, do not re-derive. See *Carried forward*: the file-count numbers in `tasks/M0-04.md` itself are stale |

## Relevant Existing Code (read-only for the inventory)

- `V.SMART/V.SMART.Web/appsettings.json`, `V.SMART/V.SMART.Api/appsettings.json` — now
  sanitised in the working tree by M0-03-01; still contain the exposed values in `HEAD`
  history until M0-05.
- `V.SMART/V.SMART.Shared/Data/MigrationData/ApplicationDbContextFactory.cs`,
  `.../MasterDbContextFactory.cs`, `V.SMART/V.SMART/MauiProgram.cs` — the three C# files
  still carrying hardcoded credentials (M0-03-02's scope to fix; M0-04 only inventories and
  rotates the value, it does not change how these files read it).
- `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/EInvoiceAPIService/EinvoiceDatabaseService.cs`,
  `.../EWayDatabaseService.cs`
- `V.SMART/V.SMART.Shared/Data/TenantInfo.cs`,
  `V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantDbContextFactory.cs`
- `V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs` (R-09 seeded account — inventory only, M0-06 owns the fix)

Write targets: `docs/runbooks/credential-rotation.md` (**to be created** — `docs/runbooks/`
does not exist yet), `docs/kb/open-questions.md`, `docs/kb/investigation-registry.md`,
`docs/kb/risks/technical-debt-register.md`.

## Business Rules

**None directly**, but the rotation procedure must respect existing tenant-resolution
behaviour (KB-014): `TenantDbContextFactory.CreateDbContext()` builds each tenant's
`DbContext` from `TenantInfo.ConnectionString`, read from the master database's `Tenants`
table, and tenant-resolution failure is **silent** — `GetCurrentTenant()` returns `null` and
logs to `Console`, then the factory throws `NullReferenceException`
(`V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantDbContextFactory.cs:14-26`,
per KB-014). A botched rotation presents as opaque 500s, not a clear auth error — the
runbook must say so and include rollback.

## Carried forward from M0-03-01 (closed out 2026-08-17, `Needs Review`)

- **`V.SMART/V.SMART.Web/appsettings.json` and `V.SMART/V.SMART.Api/appsettings.json` are
  now sanitised in the working tree** (empty-string keys, no live values) — but this changes
  nothing about M0-04's own scope: the values that need rotating are the ones that were
  *ever* committed/exposed, which are still recoverable from `HEAD` history until M0-05
  purges it, and are still in use on developer machines via user-secrets/local config. Do
  not treat the working-tree cleanup as if it were rotation.
- **`docs/CONFIGURATION.md` now exists**, documenting the five keys
  (`ConnectionStrings:MasterDb`, `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience`,
  `Jwt:ExpiresMinutes`) in both environment-variable and user-secrets form. M0-04's rotation
  runbook should reference it rather than re-describing the injection mechanism.
- **`tasks/M0-04.md`'s own "four files" count is stale.** M0-03-01's close-out re-verified
  `git grep -l "<SA password>" HEAD` and found **eight** files as of 2026-08-17 (the original
  four C#/JSON files plus five KB documents that quote the password inside their own example
  `git grep` commands — `tasks/M0-03-01.md`, `tasks/M0-03-02.md`, `tasks/M0-04.md`,
  `tasks/M0-05.md`, `risks/technical-debt-register.md`). **Re-run the grep yourself before
  building the inventory** — do not copy either the "four" or "eight" number without
  re-verifying, since M0-03-01's own commit changed the count once already (nine → eight by
  removing `V.SMART.Web/appsettings.json`'s match). The KB documents' self-matches are a
  known, harmless artifact (quoting the string inside a code fence), but confirm this before
  discounting any hit.
- **A JWT secret second copy exists** at
  `V.SMART/V.SMART.Api/Auth/JwtTokenService.cs:20-21` — the same null-only guard pattern as
  `Program.cs:56-57`, not previously recorded in the C-4 inventory row in `tasks/M0-04.md`.
  Add it when building the inventory; it does not change the *value* to rotate, only the
  count of places that validate it.
- **A stash remains parked**, `PRE-M0-15: local tenant DB debugging …` (`6dbf4b47b8ff`),
  touching `V.SMART/V.SMART.Web/appsettings.json` and
  `V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantDbContextFactory.cs` with a
  *different* local `.\SQLEXPRESS`/`NexGenErpDb_Master` connection string and null/empty
  guards. Applying it would re-introduce a credential into the file M0-03-01 just sanitised
  and would conflict with it. Do not apply it as part of this task; if it needs reconciling,
  that is a decision for whoever owns that local debugging work, not an automatic action
  here.
- **`V.SMART/V.SMART.Api/` remains untracked by design** (the known checkout trap in
  `CLAUDE.md`). Its `appsettings.json` and `.csproj` are already in a sanitised/UserSecretsId
  state on disk (provenance unknown, pre-existing before M0-03-01 ran) but are not committed.
  M0-04 rotating the *value* inside that file does not require it to be tracked.

## Acceptance Criteria

Full checklist: [`tasks/M0-04.md` § Target Result / Implementation Steps`](tasks/M0-04.md).
Summary — four deliverables, all reviewable:

1. `docs/runbooks/credential-rotation.md` — ordered procedure, one section per credential
   (owner, blast radius, window, procedure, rollback, verification).
2. A credential inventory table (start from the C-1…C-6 rows already drafted in
   `tasks/M0-04.md`, extend and re-verify — do not assume the drafted rows are still
   accurate; re-check file:line citations).
3. A human verification checklist, objectively checkable, with date and name fields.
4. Confirmation that Q-19 is correctly recorded in `open-questions.md` (it already is;
   verify, do not duplicate).

And, only if a human with production access actually performs a rotation during this
session: the checklist filled in for that credential. Otherwise the honest status is
`Blocked`, named on the person needed.

## Testing Requirements

No automated tests apply to this task — it produces documentation and (conditionally) a
production credential change outside the repository's build/test surface. `dotnet test`
must not be run regardless (no test project exists, INV-023).

## Documentation Updates

- `docs/runbooks/credential-rotation.md` — created.
- `docs/kb/open-questions.md` — confirm Q-19's entry is current; do not re-add it.
- `docs/kb/investigation-registry.md` — add a new inventory investigation row (next free
  `INV-0xx` id — check the registry at execution time, do not assume a number).
- `docs/kb/risks/technical-debt-register.md` — update R-01/R-02/R-09 with rotation status.
- `tasks/M0-04.md` — record the outcome, move `status`, bump `last_verified`.
- `task-tracker.md` (KB-081) — update as the last step.

## Completion Conditions

This task reaches `COMPLETED` only when the runbook, inventory, and checklist exist, Q-19
is confirmed filed, and — if rotation itself was performed in-session — the checklist is
filled in and verified. If rotation requires a human with production access this session
does not have, `Blocked` (naming the owner) is the honest end state for that part, even
while the documentation deliverables land as `Needs Review`.

---

## Sequence

| | Task | Status |
|---|---|---|
| **Previous** | M0-03-01 — `appsettings.json` → environment / user-secrets | `Needs Review` 2026-08-17 (validated PASS on attempt 2, unmerged) |
| **Current** | **M0-04 — Rotate the exposed credentials** | `READY` |
| **Next (candidate)** | M0-05 — purge secrets from git history | `Blocked` on this task and on `M0-03` (parent) |
| **Next (independent)** | M0-08, M0-02 | both `READY`, neither conflicts with this task's files |

The next task is **selected, not assumed** — apply
[`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
against the tracker at completion time, because status may have moved.

---

## Open flags on this task

- **Organisational access is unresolved.** No named person with production SQL access, or
  with the GST e-Invoice/e-Way gateway account, is recorded anywhere in the repository. This
  task's first real deliverable-blocking action, after confirming Q-19, is surfacing that
  gap to a human — not guessing who it is.
- The working tree should be clean apart from the documented stash
  (`PRE-M0-15: local tenant DB debugging …`) and the by-design-untracked
  `V.SMART/V.SMART.Api/`. Re-verify `git status --porcelain` before starting — state may have
  moved since 2026-08-17.
