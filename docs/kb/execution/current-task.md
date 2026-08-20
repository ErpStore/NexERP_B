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
last_verified: 2026-08-20
dependencies: [KB-081, KB-082, KB-088, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Active task — `M2-B12-01` — INV-012: document numbering + financial-year investigation

**Task file:** [`tasks/M2-B12-01.md`](tasks/M2-B12-01.md). **Status:** `Blocked` — escalation
budget exhausted, awaiting a decision from the repository owner, **Vivek**. This is *not* an
"an attempt in progress" state to resume automatically ([KB-088 §7](workflow.md)'s resume
test); it is a stop condition
([KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks)) that requires a
human before any further automated work happens on this branch.

**Deliberately left pointing here, not advanced to the next candidate.** A prior session on
this same branch prematurely closed this task out as `Needs Review` (validated `PASS`) and
rewrote this file to point at `M2-A08` next (commit `cba467c`). A *subsequent* validation pass
— on the same branch, after that close-out — caught a real defect the `PASS` had missed. This
close-out corrects that: the true state is `Blocked`, and this file is rewritten back to
`M2-B12-01` so a human or a later run resumes the actual open item rather than restarting
`M2-A08`'s investigation against a stale premise.

### Run State — what actually happened, most recent first

1. **`FAIL`**, tip `fa4a2ad`. Acceptance criterion *"INV-012 is Complete in KB-003 with
   evidence rows in the KB-083 format"* failed: `investigation-registry.md:54` still read
   *"inline in four document services"* under `Confidence: Confirmed`, while
   `grep -rn "LastNumber" V.SMART/V.SMART.Shared/BusinessLayer/` shows **six** —
   `MfgDcService`, `MfgInvService`, `ExpInvService`, `LabourInvoiceService`,
   `LabourDcOutgoingService`, `SubConDcOutService`. The three commits between `58e7bee` and
   `fa4a2ad` had corrected this exact undercount in KB-100, KB-060 and KB-030 but never
   opened KB-003 — the one document the sweep missed.
2. **Escalated** — this branch's second `FAIL`, crossing `escalate_after_failures: 2`.
   `max_escalations: 1` for this task; this is the only escalation it gets.
3. **Diagnosed and fixed**, committed as `8a54f96` — `investigation-registry.md` corrected to
   six named services with widened evidence and `source_files`, plus the same stale count
   fixed a second time in `document-numbering.md:99` (KB-100 §2's taxonomy row).
4. **Stopped, not re-validated.** The escalation budget (`max_escalations: 1`) is now fully
   spent, so the orchestrator stopped rather than spend the task's last attempt slot on an
   unsupervised re-validation of an unreviewed fix. **Attempts used: 2 of 3. Escalations used:
   1 of 1.**

Full detail: [`tasks/M2-B12-01.md` § Execution Record (2026-08-20) — Session Close-out:
STOPPED, escalation budget exhausted](tasks/M2-B12-01.md#execution-record-2026-08-20--session-close-out-stopped-escalation-budget-exhausted),
`task-tracker.md` footnote ²⁸, and `failure-log.md`'s entries for this branch.

### Decision needed from the repository owner (Vivek) — one of

- **A** — review `8a54f96` directly (one commit, four files) and merge if it holds up; does
  not require a fresh automated validation.
- **B** — authorize a reset attempt/escalation budget for this task so the orchestrator can
  re-validate `8a54f96` under normal flow.
- **C** — send it back for another implementation pass if a further defect turns up on
  inspection.

**Until one of these happens, do not self-select `M2-B12-01` again** — it is not `Ready`, it
is `Blocked` pending a human decision. `M2-B12-02`'s Hard prerequisite remains unmet either
way (`Needs Review` and `Blocked` both fail
[KB-082 step 1](dependency-graph.md#ready-task-selection-rule)), so it stays correctly
`Blocked` too.

### Branch

`migration/M2-B12-01-inv-012-numbering`, tip `8a54f96`. **Not merged, not pushed.**
`docs/` only in the diff — no `V.SMART/` path touched by any commit on this branch.

---

## Also awaiting owner merge — unaffected by the above

`M2-C00` (`migration/M2-C00-kb050-angular-rewrite`, `b3c0e6e`) and `M2-A07`
(`migration/M2-A07-me-endpoint`, `61da4bd`) are validated `PASS` and unmerged. **`M2-A02` must
settle `Q-28` before it starts.** An API-only administrator holds **zero** `UserRight` rows,
because `AuthController.Login` never calls `SyncRightsForUserAsync`. Annotate
`CurrencyController` before that is answered and the administrator authenticates successfully
into an empty UI — the R-40 failure mode, moved to the API side.

## Ready and unclaimed, once a human unblocks `M2-B12-01` (or in parallel, on a fresh branch)

| Task | What | Est. | Note |
|---|---|---|---|
| `M2-A08` | Row-level scoping + account gates (Q-05…Q-08) | — | P0, Hard dependency of `M2-D01`; was queued next by the superseded close-out — still genuinely `Ready`, just no longer *this* file's active task |
| `M2-B04` | Decouple `IApprovalService` | 1 wk | P0, zero tracked dependents |
| `M2-B01` | API versioning → `/api/v1` | 1 d | P1 |
| `M2-B05` | Typed `ScreenCodes` constants (R-10) | 2 d | P1 |
| `M2-B06` | File upload / download endpoints | 1 wk | P1 |
| `M2-B09` | Reference-data endpoints + caching | 3 d | P1, released by `M2-B02` |
| `M2-B11` | Health checks + structured logging (R-23) | 3 d | P2 |
| `M0-01-03` | Deployment script + rebuild runbook | 1 d | P0 in KB-081, but owner-only human rebuild drill — not autonomously startable |

**A session may now run more than one of these.** The standing rule is *"pick the next task
that can actually be done"*, with a five-part test in [`CLAUDE.md`](../../../CLAUDE.md) §
Standing constraints. **One task, one branch, cut from `master`** is unchanged, as is *never
merge, never push*.
