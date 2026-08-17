---
doc_id: KB-088
title: Repository-Driven Execution Workflow
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-16
dependencies: [KB-080, KB-081, KB-082, KB-083, KB-084, KB-089, KB-090, KB-091]
---

# Repository-Driven Execution Workflow

**The repository is the persistent memory of this migration. The conversation is only
temporary execution context.**

A new session must be able to continue the migration with no pasted history. If a session
cannot proceed without information that only exists in a previous conversation, that is a
**defect in the repository**, and closing it is part of the work — not an inconvenience to
route around.

This document defines the lifecycle, the session procedure, and the completion protocol.
The invariant project context lives in [`CLAUDE.md`](../../../CLAUDE.md) at the repository
root; the active task lives in [`current-task.md`](current-task.md).

---

## 1. Task lifecycle

```
PLANNED → READY → IN_PROGRESS → IMPLEMENTATION → TESTING → REVIEW → COMPLETED
```

| State | Means | Entered when |
|---|---|---|
| `PLANNED` | The task exists in the plan. Prerequisites are not all met. | A task file is written, or a wave is scheduled |
| `READY` | Every prerequisite is `COMPLETED` and no external answer is missing. It can be opened now. | The last prerequisite closes |
| `IN_PROGRESS` | A session has opened it: read the KB, verified the current code state, produced a plan. Nothing has been written yet. | A session sets `current-task.md` status and starts |
| `IMPLEMENTATION` | Code and/or documents are being written. | First edit |
| `TESTING` | Implementation is complete; validation is being run. | Last edit |
| `REVIEW` | Validation passed, acceptance criteria self-checked, committed single-scope on its branch. Awaiting a human reviewer / merge. | Commit |
| `COMPLETED` | Reviewed and integrated. The record of what was done stands permanently. | Merge, per §6 |

`BLOCKED` is an **orthogonal flag, not a phase**. Any task before `COMPLETED` can be
blocked — by an incomplete prerequisite, or by a missing human input (a credential, a
product decision, DBA access). Record *what* it is blocked on and *who* can unblock it. A
task blocked on a human is a different problem from one blocked on a task, and the tracker
must say which.

**A task must not reach `COMPLETED` until its acceptance criteria and required validation
have been satisfied.** Code being written is not completion. Nor is a passing build, unless
the acceptance criteria say it is.

### Legacy status names

The tracker (KB-081) and the 105 existing task files use an older vocabulary. Both are
valid; these are the same states under different names. New writes use the canonical names.

| Canonical | Legacy equivalent in KB-081 / task frontmatter |
|---|---|
| `PLANNED` | `Not Started` |
| `READY` | `Ready` |
| `IN_PROGRESS` / `IMPLEMENTATION` / `TESTING` | `In Progress` |
| `REVIEW` | `Needs Review` |
| `COMPLETED` | `Completed` |
| *(flag)* `BLOCKED` | `Blocked` |

Do not mass-rewrite the existing files to change vocabulary — that is documentation churn
with no reader benefit. Update a task's status word when you touch that task anyway.

---

## 2. Starting a session

The whole prompt is:

```
Read CLAUDE.md and docs/kb/execution/current-task.md.
Execute the current task according to the repository's migration workflow.
Do not start the next task.
```

Nothing else is pasted. Ever.

### The procedure

1. **Read `CLAUDE.md`** — invariant context: architecture, authority order, standing
   constraints, repository-root trap.
2. **Read `current-task.md`** — which task, what it needs, what "done" means.
3. **Read this file.**
4. **Read only the documents `current-task.md` names** under *Relevant Documentation*. Then
   its full task file, `tasks/<TASK-ID>.md`, which is binding.
5. **Check the investigation registry** (KB-003) before investigating anything. Reuse
   `Complete` findings by `doc_id`. Investigate only documented gaps.
6. **Inspect the relevant existing code** — only what the task names, plus what that leads
   to. Treat the task file's *Current Implementation* as a hypothesis and re-verify line
   numbers; task files go stale as other tasks land.
7. **Confirm the prerequisites are actually satisfied**, not merely listed as satisfied.
   Check the tracker, and check reality. A prerequisite at `REVIEW` (committed, unmerged) is
   generally still blocking for a merge-dependent successor — flag that rather than silently
   treating it as done.
8. **State the implementation plan briefly** — a few sentences, not a document.
9. **Implement only this task.** Nothing adjacent, however tempting.
10. **Run the validation** the task requires (§5).
11. **Review the implementation against the acceptance criteria**, one by one, honestly.
12. **Update the KB documentation** that actually changed (§4).
13. **Update the tracker** (KB-081).
14. **Update `current-task.md`** — hand over to the next task (§3).
15. **Report** with the standard final report ([KB-084](review-templates.md)).
16. **Stop.**

**Do not automatically implement the next task.** One task, one session. This is what makes
each unit independently reviewable and independently revertible.

### Context discipline

The knowledge base is far larger than any one task needs. Reading it all costs more than the
work does.

**Never**: copy a previous prompt into a new session; restate the architecture, milestone
descriptions or business rules that the KB already holds; read the whole repository when a
subset is relevant; read every KB document for every task; reproduce a large file into the
conversation when `grep` answers the question; keep conversation history alive for something
that can be written down.

**Instead**: reference by path and `doc_id`, and read on demand.

```
See:
docs/kb/architecture/backend-architecture.md
docs/kb/business-rules/business-rule-inventory.md
docs/kb/decisions/ADR-003-react-stack.md
```

`docs/kb/execution/README.md` (KB-080) is ~55 KB — always deep-link to a section, never read
it end to end.

---

## 3. Completing a task

In this order.

1. **Verify** every acceptance criterion against observed evidence. A criterion you did not
   check is not met.
2. **Record the actual outcome** in the task file: append an `## Execution Record (<date>)`
   section — what was really done, what the commands actually printed, what differed from the
   plan and why. This is the durable record; the conversation is not.
3. **Record discoveries** (§4). A finding that lives only in a chat message is lost.
4. **Update the task's status** in its own frontmatter and in the tracker (KB-081).
5. **Commit** single-scope on `migration/<TASK-ID>-<slug>`. Do not merge, do not push.
6. **Select the next task** (§7).
7. **Rewrite `current-task.md`** for that task — replacing it, not appending to it. Set its
   status to `READY`, and carry over anything the completed task discovered that the next one
   needs to know.
8. **Do not implement it.**

The next session then continues from step 1 of §2 with no human context transfer.

### Who may set COMPLETED

An execution session's honest end state is normally `REVIEW`: the work is done, validated and
committed, but unmerged and unreviewed. This project's convention is that `COMPLETED`
requires integration — see KB-081's own notes on why M0-03 and M0-08 sit at `Needs Review`
rather than `Completed` despite meeting every criterion.

A session may set `COMPLETED` only when the task's completion conditions genuinely include no
human step. When they do include one — a merge, a credential rotation, a product decision, a
DBA drill — say so plainly and leave the status at `REVIEW` or `BLOCKED` with the named owner.

**Never report `COMPLETED` for work a human still has to do.**

---

## 4. Which documents to update

Update a document **only when something actually changed**. Documentation noise is a real
cost: it makes the next session read more to learn the same amount.

| Document | Update when |
|---|---|
| [`current-task.md`](current-task.md) (KB-089) | **Always**, at the end of every session |
| [`task-tracker.md`](task-tracker.md) (KB-081) | **Always** — status changed |
| `tasks/<TASK-ID>.md` | **Always** — frontmatter status + `## Execution Record` |
| [`dependency-graph.md`](dependency-graph.md) (KB-082) | A real dependency was found, removed, or reclassified. Not for status changes |
| [`INDEX.md`](../INDEX.md) (KB-005) | A document was created, or a `doc_id` claimed |
| [`investigation-registry.md`](../investigation-registry.md) (KB-003) | Anything was investigated — **including negative results** |
| [`open-questions.md`](../open-questions.md) (KB-004) | A question was raised, or answered. Raise rather than guess |
| `architecture/*` | Observed behaviour of the **existing** system contradicts what is recorded, or fills a stated gap |
| `business-rules/business-rule-inventory.md` (KB-030) | A business rule was discovered — with `file:line` evidence and a BR id. Never invented |
| `decisions/ADR-*` | A decision was taken that constrains later tasks. ADRs are immutable once accepted — supersede, never edit |
| `risks/technical-debt-register.md` (KB-060) | A risk was found, closed, or its severity changed |
| `migration/*`, `frontend-new/*` | The plan itself changed — not merely progressed |

Bump `last_verified` on every document you touch. When code contradicts a document, **the
code wins**: correct the document, note the delta, bump the date, update the registry row.

**As-is and proposal documents are never mixed.** `architecture/`, `modules/`,
`business-rules/`, `api/api-overview.md` describe what exists. `frontend-new/`, `migration/`,
`api/api-readiness-assessment.md` describe what is proposed. Answering "how does X work?"
from a proposal document is this knowledge base's worst failure mode.

### Recording a finding

```yaml
Finding:        <one sentence>
Evidence:       <path:line-range>
Business rule:  <BR-xxx-nnn or "n/a">
Confidence:     Confirmed | Inferred | Unknown
Last verified:  YYYY-MM-DD
```

Cite `file:line`, never just a file — line numbers are what make a claim re-verifiable and
staleness detectable. Prefer a declaration line plus a symbol name over a bare range.

---

## 5. Validation

**Do not claim a task is complete merely because the code was written.**

Every task specifies its own validation. Depending on the task, it may include: build, unit
tests, integration tests, API tests, frontend tests, type checking, linting, database
validation, or manual verification. The task file's *Testing Requirements* and *Verification
Commands* sections are binding.

Rules:

- **Only use commands verified to work in this repository.** The authoritative list is
  [KB-083 § Verified repository commands](prompt-template.md#verified-repository-commands).
  Do not copy that table into other documents — it is maintained in one place precisely so it
  cannot go stale in three.
- A task that measures a command's behaviour puts that command in its *Implementation Steps*,
  not its *Verification Commands*. You cannot verify with the thing you are characterising.
- Testing belongs to the task that introduces the behaviour. It is never deferred to M5.
- If something genuinely cannot be verified in this environment — a locked build tool, no DB
  access, no production credentials — **say so explicitly and name what would verify it**.
  Never report a result you did not observe.

---

## 6. Milestones and gates

Milestones close on their **gate**, never on their task list and never on their estimate.
Every task being `COMPLETED` is explicitly *not* sufficient.

At each gate, run the milestone review in
[KB-084 § Milestone Review](review-templates.md#milestone-review) and record it with evidence
per criterion. A failed gate is not renegotiated: record the failing criteria, create
remediation tasks **with new ids in the milestone that failed**, and re-run the gate. The one
legitimate escape is `PASSED WITH EXCEPTIONS`, which requires a named owner per exception, a
date, and a written statement of what breaks if it is never closed.

---

## 7. Selecting the next task

Deterministic, so that two sessions reach the same answer. Full rule and tie-breaks:
[`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).

In short: from KB-081, take every task whose prerequisites are all genuinely satisfied and
which is not blocked on a human; drop anything sharing a file with in-flight work
(KB-082 § Same-file conflicts); prefer P0, then the task unblocking the most downstream work,
then the critical path. If two are equally ranked and independent, say so and let the owner
choose rather than picking silently.

Then write it into `current-task.md` and **stop**.

---

## 8. Task boundaries

- Stay within the task's scope. No opportunistic refactoring of unrelated code.
- Do not start the next task.
- Do not change an architecture decision without recording it.
- Do not invent business rules.
- If a requirement is unclear, record it in `open-questions.md` rather than guessing.
- If implementation reveals an important business rule, record it with evidence.
- If implementation reveals a significant technical decision, record it as an ADR or update
  the appropriate decision document.

The migration is not a UI translation. Preserve existing business behaviour unless an
explicit architectural decision changes it — and if one does, that decision is written down
before the behaviour is.

---

## 9. Writing new task files

Use [`task-template.md`](task-template.md) (KB-090). Generation rules — no placeholders, never
invent a path, cite `file:line`, classify every claim — are in
[KB-083 § Generation rules](prompt-template.md#generation-rules) and remain binding.

M3/M4 wave task files are still generated **at the start of their wave**, not in advance: a
task's *Business Rules* section is the output of that wave's `INV-0xx` investigation, and
writing it earlier would mean inventing rules. See
[KB-080 §11](README.md#11-m3--core-modules).

A task file must contain enough to execute independently, and **must not** contain the
history of previous tasks. It references the knowledge base; it never duplicates it.

---

## 10. Autonomous execution

Everything above describes the procedure regardless of who runs it. A **human-in-the-loop**
session runs it directly, using the `erp-migration-executor` agent or no agent at all.

An **autonomous** run executes the same procedure through a set of specialised agents — an
orchestrator that selects and routes, a read-only investigator, an implementer, an
independent validator, and a debugger that handles failed validation. The mechanism is
defined once, in [`autonomous-runner.md`](autonomous-runner.md) (KB-091): the agent roster,
the runner configuration, how a task's complexity and risk are derived, which model each role
is routed to, the failure sub-states, the retry and escalation budget, and the conditions
under which the runner must stop and ask a human.

Three properties of that mechanism matter to this document:

- **It refines §1, it does not replace it.** The runner's `INVESTIGATING`, `IMPLEMENTING`,
  `VALIDATING`, `FAILED`, `DIAGNOSING`, `RETRYING` and `ESCALATED` states each map onto a
  canonical state, and **only the canonical name is ever written** to KB-081 or to a task
  file. One vocabulary in the tracker.
- **It does not weaken §3.** `REVIEW` remains the honest end state of a run; `COMPLETED` still
  requires integration, and the runner may not set it for work a human has yet to do.
- **It does not weaken §5.** Validation is independent of implementation, uses only the
  verified commands in [KB-083](prompt-template.md#verified-repository-commands), and an
  unverifiable check is reported as unverifiable — never as a pass.

Start a run with `/migration-run`; inspect state without changing it with `/migration-status`.
By default a run executes **one** task and stops, exactly as a human session does.
