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
last_verified: 2026-08-25
dependencies: [KB-081, KB-082, KB-088, KB-091, KB-092, KB-093, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## `M0-04`: Rotate the exposed credentials — Blocked, session close-out 2026-08-25

**Run State:** Attempt **1 of 4** executed on `migration/M0-04-credential-rotation-runbook`
(tip `e437fe5`). Status **`BLOCKED`** — the implementer delivered all four AI-deliverable
artefacts and stopped correctly, because rotating any credential requires production access
no AI session has or should have. **This is the task's own designed terminal state** (its
*Target Result* says so explicitly), not a validation failure — no validator ran
(`"verdict": "none"`; nothing here is gated by tests). Escalations this attempt: 0. A human
or a later run should **resume from here — not restart** the document work, which is
complete and committed.

**Full spec:** [`tasks/M0-04.md`](tasks/M0-04.md) § Execution Record (2026-08-25). Tracker:
`task-tracker.md` row 67, footnotes ⁴, ⁷⁰, ⁷¹. Runner bookkeeping:
[`runner-state.md`](runner-state.md) `Status` row.

### What is blocked, and on whom

Four named human roles, none of them a task that can land on `master`:

| Role | Unblocks |
|---|---|
| `sysadmin` on `154.61.76.112,1533` and the dev SQLEXPRESS instance | C-1, C-2 |
| Whoever can write to the master database's `Tenants` table | C-3 |
| Whoever owns `V.SMART.Api`'s deployment configuration | C-4 |
| Whoever owns the GST gateway / Bhargavi Soft-Tech licensing relationship | C-5, C-7 |

Top escalation, in priority order (full detail in `task-tracker.md` footnote ⁷¹ and
`tasks/M0-04.md`'s Execution Record):

1. The repository is still public — re-verified today (unauthenticated REST → `200`); Q-19
   already answered, owner's deliberate decision, so this is exposure, not a question.
2. **New (`C-7`, not in any prior register):** the AES key/IV protecting every tenant's GST
   gateway credential is hardcoded and public at
   `V.SMART/V.SMART.Shared/E_Invoice/LicenseProductKey.cs:28-29`. Rotating the gateway
   credential alone does not restore confidentiality — the vendor must re-key. No owner yet.
3. The SA password, the plaintext production password
   (`docs/kb/risks/technical-debt-register.md:44`), and the default `Jwt:Secret` are all still
   at `HEAD` inside `docs/kb/` (5 files) — `M0-05`'s history purge alone will not remove them.
   **Q-84** asks the owner who redacts them and when.

### Delivered and committed on this branch

`docs/runbooks/credential-rotation.md` (C-1…C-7, owner/blast-radius/window/procedure/
rollback/verification per credential, plus the unsigned §8 human checklist);
`docs/kb/investigation-registry.md` **INV-052** (credential-usage inventory, Complete);
`docs/kb/open-questions.md` **Q-84**; `docs/kb/risks/technical-debt-register.md` (R-01/R-02
stated explicitly to remain open until §8 is signed). **Not done:** the rotation itself; the
`Tenants` row count and licence-key blast radius (need database access); any redaction of
`docs/kb/` (Q-84, owner decision).

**Do not re-run this task from scratch.** The documents are the deliverable and they exist.
What is missing is a human performing the rotation and signing the checklist — that is not
something a future session can do either; it is where a human picks up.

### Historical selection reasoning (superseded by the outcome above, kept for provenance)

Selected 2026-08-25 (select-only pass, tip `beee0f9` on `master`, tree clean).

### Why this task, and why now

`M0-04` was misclassified as blocked for three prior Select passes because the *rotation*
needs production SQL Server / GST e-Invoice gateway access the owner confirmed is
unavailable (2026-08-19 deferral). **That deferral describes the rotation, not the task.**
Every one of `M0-04`'s acceptance criteria is a document — a rotation runbook, a complete
credential-consumption inventory, a human verification checklist, and Q-19 raised. Its own
*Target Result* states the task closes **`Blocked`**, not `Completed`, if no named human
performs the actual rotation during the session (Step 10: *"Do not report Completed because
the documents were written"*). Q-19 is already `ANSWERED` (2026-08-12), clearing the one
prerequisite gating the document work.

**Part 5 of the five-part "can actually be done" test** — no sibling branch on this task's
files — was cleared by renaming the stale, 324-commits-behind prior attempt
(`migration/M0-04-credential-rotation-runbook`, one commit) to
`archive/M0-04-runbook-stale-lineage` on 2026-08-25, preserving its 269-line runbook at
`1f905db`. **That runbook is a starting point, not a merge candidate** — its procedures
(least-privilege login alongside `sa`, update `Tenants` connection strings, verify before
disabling, disable rather than drop) are sound and stack-independent, but its cited evidence
is stale: it names `Jwt:Secret` at `appsettings.json:12` (now `:37`, value `""`, externalised
by `M0-03`) and claims the Api project is untracked (tracked since `623b1e1`). Every
Confirmed row in the new inventory must carry a `file:line` re-derived against current
`master`, not copied from the archived branch.

### What this task will actually produce this session

1. A rotation runbook — ordered, executable steps for a human with production access.
2. A complete credential inventory across `M0-04`'s ten `source_files` (see `tasks/M0-04.md`
   frontmatter) plus any non-file consumers (env vars, CI secrets, tenant DB rows).
3. A verification checklist, per credential, objective and tickable.
4. Confirmation that Q-19 is recorded as answered (it already is — 2026-08-12).
5. **The actual rotation only if a human with production SQL / GST gateway access
   participates in this session.** If nobody does, the honest close is `Blocked`, named to
   an owner (Vivek) — not `Completed`.

### Classification (KB-091 §4)

`task_type: Security` → base **HIGH** (§4.1, no further raise needed). Risk **HIGH** (§4.3 —
`task_type: Security`, and the task concerns credentials directly). Per §5.1/§5.2: Investigate
`opus`, Implement `opus`, Validate `opus` (risk HIGH forces `opus` for validation regardless
of complexity).

### Carried forward — still true, unaffected by this selection

- **`M0-06`** (`Ready`, P1) still fails part 5: sibling branch
  `migration/M0-06-remove-default-admin` still exists, unmerged.
- **`M0-11`** (`Ready`, P0) still fails part 2: `task_type: Product Decision`, owner-only,
  never self-selectable.
- **`M2-A03`** (`Blocked`) still needs a human to mark the `api-contract`/`build` CI job a
  *required* status check on `master`, or accept it as a standing manual gate. Owner: Vivek.
- **Q-71** (open-questions.md) is still open — untouched by this selection.
- **R-43** (no `WebApplicationFactory` host in `tests/V.SMART.Api.Tests`) is still open.
- Outstanding owner decisions unrelated to `M0-04` (`Q-38`, `Q-82`, merging the several
  `Needs Review` branches — `M2-A02`, `M2-A09`, `M2-A10`, `M2-B10` at last check) are
  unchanged — see `task-tracker.md` § Current state.
