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
last_verified: 2026-08-22
dependencies: [KB-081, KB-082, KB-088, KB-091, KB-092, KB-093, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## ▶ M2-C12-01 — Blocked, not resumable without an owner ruling

**Task file:** [`tasks/M2-C12-01.md`](tasks/M2-C12-01.md) — re-specify the design-system tree
(`M2-C04.md`, `M2-C04-01.md`, `M2-C04-02.md`, `M2-C04-03.md`) for Angular/ADR-007.

**Status: `Blocked`, owner Vivek, question Q-70.** This is a close-out, not an in-progress task —
do not resume it without the owner's ruling below. It is left as the pointer so a human or a
later run finds the full record instead of restarting from Select.

### Run State

- **Branch:** `migration/M2-C12-01-respec`, tip `09001a3`. Not merged, not pushed.
- **Attempts:** 3 of 4 used. **Escalations: 1 of 1 used** (KB-091 §6.3).
- **Last validator verdict:** `FAIL`, category `architecture`, `scopeOk: false`. 7 of 8
  acceptance criteria `MET`; criterion 7 `NOT MET`.
- **What failed:** Acceptance criterion 7 (`tasks/M2-C12-01.md:83-84`) requires
  `git diff --name-only master...HEAD` to list **only** the four `M2-C04*` files. It never can:
  criterion 2 (`:70-74`) requires the greps be quoted in this task's own Execution Record, the
  Documentation Updates table (`:99-101`) requires this task's own `task-tracker.md` row and
  authorises `open-questions.md`, and [KB-088 §4](workflow.md#4-which-documents-to-update) makes
  `tasks/<TASK-ID>.md` an unconditional update — so a compliant diff is six or seven paths, never
  four. Observed on `09001a3`: 7 paths (`failure-log.md`, the four batch files, `M2-C12-01.md`
  itself, `open-questions.md`).
- **Why this stopped rather than retried a 4th time:** the contradiction is between the task's
  own acceptance criteria, not a mistake in execution — attempt 1 satisfied criterion 7 and
  failed criterion 2; attempts 2 and 3 satisfied criterion 2 and failed criterion 7. No 4th
  attempt can satisfy both. The fix is a wording change to criterion 7, spanning
  `M2-C12-01`..`-05` (all five sub-tasks carry the identical clause) — a specification decision,
  not a patch, and out of scope for an execution session to make unilaterally.
- **Substance is sound, independently re-verified:** every PrimeNG selector named in the batch
  exists in `primeng@22.1.0`; every `V.SMART/*.cs` citation is byte-unchanged from `master`; the
  axe accessibility criterion (weakened in attempt 2) was caught and restored in attempt 3;
  `git diff --stat master...HEAD -- . ':(exclude)docs'` is empty — no `V.SMART/`, no `frontend/`,
  no schema, no TypeScript touched.
- **Open question raised:** [**Q-70**](../open-questions.md) (`open-questions.md:42`) — how
  should criterion 7 be reworded, with a proposed (not applied) wording and the three-attempt
  history as evidence. **Owner: Vivek.**
- **Full record:** [`tasks/M2-C12-01.md` § Execution Record
  (2026-08-22)](tasks/M2-C12-01.md#execution-record-2026-08-22-session-close-out),
  [`task-tracker.md`](task-tracker.md) footnote ⁴², [`failure-log.md`](failure-log.md)
  (`M2-C12-01 · attempt 3 · independent validation · 2026-08-22 · FAIL (architecture)`),
  [`runner-state.md`](runner-state.md).

### To resume this task

1. Get the owner's ruling on **Q-70** (reword criterion 7, or accept the six/seven-path diff as
   compliant).
2. Apply that ruling to `M2-C12-01.md` **and** `M2-C12-02`..`-05` — the same clause is duplicated
   verbatim across all five sub-task files, so all of them fail identically until this is fixed.
3. Re-validate `M2-C12-01` against the corrected criterion 7. Do not attempt a 4th
   implementation pass before step 1 — three attempts already proved the criterion unsatisfiable
   as written.

### What this leaves selectable

`M2-C12-02`, `M2-C12-03`, `M2-C12-04` remain `Ready` in the tracker (same `depends_on`, same
priority) but carry the identical unsatisfiable criterion 7 — **not worth dispatching** until
Q-70 is answered, since each would fail validation the same way. `M2-C12-05` stays `Blocked`
behind all four by design (it owns the whole-tree tracker/`dependency-graph.md` restatement and
must run last).

Everything else in the tracker not touched by this close-out — `M2-A02` (Q-28, R-65), `M2-A04`
(unrecorded block, needs owner ruling), `M0-06` (branch already exists), `M0-11` (Product
Decision, owner-only), `M2-B05` (Blocked, awaiting re-specification onto R-66), `M2-B12-01`
(Blocked, escalation budget exhausted), `M0-01-03` (merged, `Needs Review`, awaiting a named
operator for runbook §7) — is unchanged by this session; see
[`task-tracker.md`](task-tracker.md) for current status on each.
