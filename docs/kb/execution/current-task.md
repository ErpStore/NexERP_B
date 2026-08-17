---
doc_id: KB-089
title: Current Task
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: [Tenants]
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
| **Task ID** | **M0-02** |
| **Task** | Confirm stored-procedure drift across tenant databases (Q-14) |
| **Status** | `BLOCKED` — tooling half delivered and committed 2026-08-17 (`migration/M0-02-sp-drift-across-tenants`, `c1ab752`); analysis half awaiting per-tenant fingerprint CSVs |
| **Milestone** | M0 — Stabilise (Gate G0) |
| **Type** | Investigation |
| **Priority** | P1 |
| **Estimate** | 1 d |
| **Full specification** | [`tasks/M0-02.md`](tasks/M0-02.md) |
| **Branch** | `migration/M0-02-sp-drift-across-tenants` (not yet cut) |

---

## Objective

Answer **Q-14**: *do the 94 stored procedures differ between tenant databases?* Deliver a
repeatable, database-free comparison harness plus a DBA-facing runbook; then, only once the
DBA drops per-tenant fingerprint CSVs into `db/drift/`, run the comparison, classify every
procedure name, and either answer Q-14 with evidence or explicitly defer it with a named
owner. Full spec: [`tasks/M0-02.md`](tasks/M0-02.md).

**This task has a human-gated half, exactly like M0-01-02.** Determine which half you are in
*before* doing anything else:
- `db/drift/` empty or absent → build the tooling and runbook only, then **stop** and report
  `Blocked — awaiting per-tenant fingerprints`.
- `db/drift/*.csv` present → run the comparison, classify, write the finding, drive the
  decision (Options A/B/C — see the task file; a session may only *present* Option B, never
  take it).

Do **not** connect to any database, seek any credential, or reuse the compromised ones
already in this repository's history (those are M0-04's/M0-05's concern).

---

## Run State

| Field | Value |
|---|---|
| **Runner state** | `BLOCKED` — attempt 1 delivered the tooling half only, then stopped as designed |
| **Canonical status** | `Blocked`⁶ (the row above; KB-081 is authoritative — see `task-tracker.md` footnote 6) |
| **Attempt** | 1 of 3 (`max_retries: 2`) |
| **Failure log** | no M0-02 entries — this is not a validation failure, it is the task's own documented `Blocked` outcome when `db/drift/` is empty. [`failure-log.md`](failure-log.md) (KB-092) |
| **What ran** | `db/tools/list-deployed-procedures.sql` extended with `hash_raw`/`hash_normalised`; `db/tools/compare-tenant-fingerprints.sh`, `db/RUNBOOK-tenant-drift-check.md`, `db/drift/README.md` created and verified against synthetic fixtures (no database, no fabricated CSV); committed on `migration/M0-02-sp-drift-across-tenants` (`c1ab752`), unmerged. |
| **Why it stopped** | `db/drift/` holds zero tenant fingerprint CSVs (Confirmed) — the analysis half cannot run without them, and this session may not acquire or reuse a database credential. |
| **Blocked on** | A DBA with `VIEW DEFINITION` on ≥2 tenant databases, plus a working tenant list (Q-12 unanswered). Owner: DBA, first candidate operator **PavanKunar**. Full detail: [`tasks/M0-02.md` § Execution record](tasks/M0-02.md#execution-record--2026-08-17-tooling-half). |
| **To resume** | Hand `db/RUNBOOK-tenant-drift-check.md` to the DBA, drop the resulting CSVs into `db/drift/`, then re-open at the task file's Implementation Steps §9. Do not re-derive the tooling. |

**Live run state is in [`runner-state.md`](runner-state.md) (KB-093), not here.**

---

## Why this task, not another

Selected per [`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
at M0-08's close-out (2026-08-17). M0-08 validated `PASS` on attempt 1 and moved to
`Needs Review` (committed on `migration/M0-08-build-output-guard`, unmerged) — **not**
`Completed`, so it does not unblock `M0-07` (Hard prerequisite requires `Completed`, not
`REVIEW`; `M0-07`'s other Hard prerequisite, `M0-15`, is also `Needs Review`).

Candidate set from the tracker: `M0-02` only.
- `M0-03` is a parent container — never worked directly, skipped.
- `M0-04` is `Blocked` on an unidentified human owner (production SQL / GST gateway access)
  — excluded from `Ready` per the selection rule's "not blocked on an unscheduled human step"
  clause.
- `M0-08` is now `Needs Review`, not `Ready`.
- `M0-02`'s only Hard prerequisite, `M0-01-02`, is genuinely `Completed`; it is not a parent
  container; it is not blocked on an unscheduled human step (Q-12, the tenant list, is a
  *soft* information gap this task itself works around by using a documented working list,
  not a hard blocker); it shares no file with any in-flight work.

Sole candidate — no rank tie-break needed.

## Dependencies

| Dependency | Class | State |
|---|---|---|
| **M0-01-02** — DDL capture from a live tenant | Hard | **Completed.** Supplies `db/stored-procedures/`, `db/stored-procedures/CAPTURE-STATUS.md` (source tenant), and `db/tools/list-deployed-procedures.sql` (the fingerprint query this task reuses/extends). |
| **M0-01-01** — 94-name reconciliation | Hard | **Completed.** Supplies `manifest.csv`, the name list scoped to what the app actually calls. |
| Multi-tenant DBA access (≥2 tenant databases) | Hard (external) | **Not obtainable by an AI session.** If only one tenant is reachable, the honest outcome is an explicit deferral of Q-14, not a guess. |
| Q-12 — authoritative tenant list | Information (soft) | **Unanswered**, owned by ops, not due until M6-03. Use a working list and say where it came from — the four per-tenant template folders under `V.SMART/V.SMART.Shared/wwwroot/templates/` are a useful (Inferred, not Confirmed) starting point. |
| M0-01-03 (deployment script + rebuild runbook) | Deployment (downstream) | If behavioural drift is found, its script needs a per-tenant path — **raise a finding against it**, do not implement the fix here. |
| Product owner | Hard (decision, conditional) | Only needed if drift is found and Option B (reconcile) is even to be *presented* — never taken by a session. |

## Relevant Documentation

Read only these.

| doc_id | Path | Why |
|---|---|---|
| TASK | [`tasks/M0-02.md`](tasks/M0-02.md) | The binding specification — read in full; it is long but every section is load-bearing |
| KB-083 | [`prompt-template.md`](prompt-template.md) | Verified-commands table |
| KB-080 §7 | [`README.md`](README.md) | M0 deliverable — "Q-14 answered or explicitly deferred with reason" |
| KB-060 | [`../risks/technical-debt-register.md`](../risks/technical-debt-register.md) | R-04 (what this closes), R-01 (why no fingerprint may carry a credential) |
| ADR-005 | [`../decisions/ADR-005-reporting-and-printing.md`](../decisions/ADR-005-reporting-and-printing.md) | Per-tenant report-template override — the evidenced reason to suspect drift |
| KB-012 | [`../architecture/database-architecture.md`](../architecture/database-architecture.md) | Database-per-tenant; isolation by connection string; no `TenantId` discriminator |
| KB-014 | [`../architecture/multi-tenancy.md`](../architecture/multi-tenancy.md) | Tenant resolution |
| KB-004 | [`../open-questions.md`](../open-questions.md) | Q-14, Q-12, Q-02 |
| KB-003 | [`../investigation-registry.md`](../investigation-registry.md) | INV-027 (closed by M0-01-02, do not reopen), INV-009 (reused, not re-derived); allocate the next free id for this task's new row — **verify it is still INV-030**, do not assume |

## Relevant Existing Code (read-only)

- `db/stored-procedures/manifest.csv`, `db/stored-procedures/CAPTURE-STATUS.md`,
  `db/tools/list-deployed-procedures.sql` — M0-01-02's output; the baseline and the reusable
  fingerprint query.
- `V.SMART/V.SMART.Shared/Data/MasterDbContext.cs`, `.../TenantInfo.cs` — the `Tenants`
  directory (contents Unknown from the repo).
- `V.SMART/V.SMART.Shared/Services/ReportViewer/ReportService.cs:71` — injects the tenant's
  own connection string into the loaded report; the per-tenant-customisation precedent.
- `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/ReportExecutor.cs:27`
  — procedures are invoked as `EXEC dbo.{procedureName}`; comparison scope is the `dbo`
  schema.
- `V.SMART/V.SMART.Shared/wwwroot/templates/` — four per-tenant FastReport folders + `default`
  (Confirmed as folders; **Inferred**, not Confirmed, as evidence of live tenants).

Write targets: `db/tools/compare-tenant-fingerprints.sh` (new), `db/RUNBOOK-tenant-drift-check.md`
(new), `db/drift/README.md` (new), `docs/kb/architecture/stored-procedure-drift.md` (new),
plus `docs/kb/open-questions.md`, `docs/kb/investigation-registry.md`, `docs/kb/INDEX.md`,
`docs/kb/architecture/stored-procedure-inventory.md`, conditionally
`docs/kb/risks/technical-debt-register.md` and `db/tools/list-deployed-procedures.sql` (only
if it lacks schema name or either hash).

**Must not change:** anything under `db/stored-procedures/` (the captured baseline —
measuring it is the point, adjusting it is the single worst mistake here),
`db/tools/verify-capture.sh`, `db/deploy-stored-procedures.ps1`, anything under `V.SMART/`,
any `appsettings*.json`.

## Business Rules

**None modified.** This is a read-only investigation — no procedure, service or application
code changes. The specific temptation to guard against: on finding tenant B's procedure
differs from tenant A's, the instinct is to make them match. **Do not** — a divergent
procedure may encode a paid customisation or an unpropagated fix, and reconciling changes ERP
behaviour (report figures, statutory document content) for every tenant that loses its
variant, silently and untested (no test project exists — INV-023, Confirmed). That
reconciliation is Option B in the task file and belongs to the product owner alone.

## Carried forward from M0-08 (closed out 2026-08-17, `Needs Review`)

- `tools/check-no-build-output.sh` now exists and is committed on
  `migration/M0-08-build-output-guard` — not relevant to M0-02's own file set, but note for
  awareness: once merged, any new file this task creates under `db/` or `docs/` will be swept
  by that guard's pattern only if it matches a build-output/IDE-state name, which none of
  M0-02's deliverables do.
- `M0-07` (CI pipeline) remains `Blocked` until both `M0-15` and `M0-08` reach `Completed`
  (i.e. reviewed and merged) — not this task's concern, but explains why CI is still not
  available to lean on for M0-02's verification; all of M0-02's verification commands are
  designed to run standalone, matching this.
- Two `M0-08` branches now exist (`migration/M0-08-gitignore-build-output`, superseded, and
  `migration/M0-08-build-output-guard`, current) — no file overlap with `db/` or `M0-02`'s
  scope, so no same-file conflict applies.

## Acceptance Criteria

Full checklist: [`tasks/M0-02.md` § Acceptance Criteria`](tasks/M0-02.md#acceptance-criteria)
(two halves — tooling, and analysis). Summary:

**Tooling half** (always deliverable, no database needed):
1. `db/tools/compare-tenant-fingerprints.sh` — reads every `db/drift/*.csv`, joins on
   `procedure_name`, classifies, and **fails loudly** on mismatched CSV headers (test this
   deliberately with a deformed copy).
2. `db/tools/list-deployed-procedures.sql` emits schema name, `create_date`, `modify_date`,
   definition length, `hash_raw`, `hash_normalised` — extend if it doesn't already.
3. `db/RUNBOOK-tenant-drift-check.md` — DBA-executable, names the no-secrets rule explicitly.
4. `db/drift/README.md` — CSV schema and naming convention.
5. No credential/connection-string/host/IP literal anywhere under `db/` — run the secret scan
   before every commit.

**Analysis half** (conditional on `db/drift/*.csv` existing):
6. ≥2 tenant fingerprints present (including the M0-01-02 source tenant) **or** Q-14 recorded
   as explicitly deferred with reason + named owner.
7. Every `manifest.csv` name classified; class counts sum correctly, arithmetic printed.
8. `docs/kb/architecture/stored-procedure-drift.md` records method, tenants (by label only),
   counts, every `divergent` name.
9. `hash_normalised` match classified **Inferred**, never Confirmed, with the stated reason.
10. Q-14 answered with evidence or explicitly deferred — never left silently open.
11. New INV row (expected **INV-030** — verify), Complete or Partial with blocker named.
12. If drift found: Options A/B/C presented with per-procedure evidence, escalated to a named
    product owner + DBA, a finding raised against M0-01-03 — **no variants directory
    implemented here**.
13. No file under `db/stored-procedures/` or `V.SMART/` touched.
14. `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` → 0 errors.

## Testing Requirements

**No test project exists** (INV-023, Confirmed) — `dotnet test` must not be run. Verification
is entirely of the comparison harness itself and is database-free: header-consistency
(deliberately deformed-copy test), coverage, arithmetic closure, reproducibility, secret scan,
and the standard build-regression guard. Full command list:
[`tasks/M0-02.md` § Verification Commands`](tasks/M0-02.md#verification-commands).

## Documentation Updates

- `docs/kb/architecture/stored-procedure-drift.md` — **created**.
- `docs/kb/INDEX.md` — routing entry for "Do stored procedures differ between tenants?".
- `docs/kb/open-questions.md` — Q-14 answered or explicitly deferred.
- `docs/kb/investigation-registry.md` — new INV row (verify id at execution time).
- `docs/kb/architecture/stored-procedure-inventory.md` — cross-reference; per-tenant caveat if
  drift exists.
- `docs/kb/risks/technical-debt-register.md` — only if drift is found (R-04 caveat).
- `db/RUNBOOK-tenant-drift-check.md`, `db/drift/README.md` — created (operational, no KB
  frontmatter).
- `tasks/M0-02.md` — record the outcome; move `status`; bump `last_verified`.
- `task-tracker.md` (KB-081) — update as the last step.

## Completion Conditions

Reaches `COMPLETED` only after human review and merge (KB-088 "Who may set COMPLETED"). The
honest in-session end state is:
- `Blocked — awaiting per-tenant fingerprints` if `db/drift/` is empty when the tooling half
  is done (the expected first-pass outcome — do not treat this as failure).
- `Needs Review` if fingerprints were already present and the full analysis, including a
  Q-14 answer or explicit deferral, was completed and committed.

---

## Sequence

| | Task | Status |
|---|---|---|
| **Previous** | M0-08 — `.gitignore` + remove committed build output | `Needs Review` 2026-08-17 (validated PASS on attempt 1, unmerged, `migration/M0-08-build-output-guard`) |
| **Current** | **M0-02 — Confirm stored-procedure drift across tenant databases (Q-14)** | `READY` |
| **Next (candidate)** | None dependency-ready as of this selection — re-derive at M0-02's close-out. `M0-04` remains `Blocked` on an unidentified human owner; `M0-07` remains `Blocked` on `M0-15`/`M0-08` reaching `Completed`. |

The next task is **selected, not assumed** — apply
[`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
against the tracker at completion time, because status may have moved (in particular, `M0-15`
or `M0-08` may have been merged by then, which would move `M0-07` into the candidate set).

---

## Open flags on this task

- **This is very likely a two-session task**, like M0-01-02: the first session builds the
  tooling and stops `Blocked — awaiting per-tenant fingerprints` unless `db/drift/*.csv`
  already exists on disk (check before assuming this is the tooling half). Report explicitly
  which half was executed.
- **Never connect to a database, and never invent a tenant list.** Q-12 is genuinely
  unanswered; say which working list was used and where it came from.
- The strongest temptation in this task — "just make the divergent procedures match" — is
  explicitly forbidden. Present, never decide, and never implement Option A or B.
