---
doc_id: KB-084
title: Milestone Review, Task Handoff and Definition-of-Done Templates
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-16
dependencies: [KB-080, KB-081, KB-083, KB-088, KB-089]
---

# Review, Handoff and Done Templates

## Session close-out — do this before reporting

The repository, not the conversation, is what survives this session. Run this checklist
before writing the final report; each item is a place where knowledge is otherwise lost.

- [ ] **`tasks/<TASK-ID>.md`** — `## Execution Record (<date>)` appended: what was actually
      done, what the commands actually printed, what differed from the plan and why.
      Frontmatter `status` and `last_verified` updated.
- [ ] **[`task-tracker.md`](task-tracker.md)** (KB-081) — status row updated. If the honest
      status is `Needs Review` or `Blocked`, say which and why, with the owner named.
- [ ] **[`current-task.md`](current-task.md)** (KB-089) — **rewritten** for the next task, not
      appended to. Next task chosen by
      [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).
      Anything this task discovered that the next one needs is carried across.
- [ ] **Discoveries recorded where they belong**, per
      [KB-088 §4](workflow.md#4-which-documents-to-update) — investigation registry (including
      negative results), business rules (with `file:line`), risks, open questions, ADRs.
- [ ] **Nothing important exists only in this conversation.** Read back the session: any
      finding, decision, assumption or gotcha that a future session would need and cannot find
      in the repository is a gap to close now.
- [ ] **No document updated that did not actually change.** Documentation noise costs every
      future session real context.

Then write the Task Handoff below. **The handoff is written before anything else starts** — a session continuing to another task writes this first, so a reviewer can read the finished task without the next one already in flight against it. *(Updated 2026-08-20: this was "stop — do not start the next task".)*

## Task Handoff

Filled in by the executing session's final response, then pasted into the task's branch
description or PR body. It is the reviewer's entry point.

```markdown
## Task Handoff — <TASK-ID>

**Task:** <name>
**Status:** Completed | Needs Review | Blocked
**Branch:** migration/<TASK-ID>-<slug>
**Commit:** <TASK-ID>: <subject>

### What was implemented
<2–5 sentences. What changed in behaviour, not a list of files.>

### Files
| Action | Path |
|---|---|
| Created | |
| Modified | |
| Deleted | |

### Tests
| Test | Command | Result |
|---|---|---|

### Verification
| Command | Expected | Actual |
|---|---|---|

### Acceptance criteria
- [ ] <copied from the task file, ticked only if objectively met>

### Documentation updated
<doc_ids + what changed + whether last_verified was bumped>

### Investigation registry
<INV row added/amended, or "No change required">

### New findings
```yaml
Finding:
Evidence:        <path:line>
Business rule:   <BR id or n/a>
Confidence:      Confirmed | Inferred | Unknown
Last verified:   YYYY-MM-DD
```

### Architectural decisions taken
<any decision that constrains later tasks; if it is significant, it needs an ADR>

### Assumptions made
<each one, and how a reviewer can check it>

### Deviations from the task
<what differed and why — an empty section here is a claim, so only write it if true>

### Unexpected findings
<anything discovered that the plan does not know about — this is how the plan learns>

### Recommended next task
<TASK-ID> — <why>
```

### Reviewer checklist

- [ ] The diff contains **only** this task's scope. Two task ids in one branch is a reject.
- [ ] Every acceptance criterion is objectively met, not merely asserted.
- [ ] Verification commands were actually run, with output.
- [ ] No secret, connection string or credential added anywhere.
- [ ] No business logic added to the Angular app; no business service rewritten.
- [ ] No database schema change unless the task authorised one.
- [ ] Claims are classified Confirmed / Inferred / Unknown, with `file:line` evidence.
- [ ] Documentation and the investigation registry are updated, with `last_verified` bumped.
- [ ] Unexpected findings are recorded somewhere durable, not just in the PR body.

---

## Milestone Review

Run at every gate. **A milestone is not complete because its tasks are complete — it is
complete when its gate passes and this review is recorded.**

```markdown
# Milestone Review — <M#> <name>

**Gate:** <G#>   **Reviewed:** YYYY-MM-DD   **Chair:** <name>

## 1. Gate status
| Criterion | Met | Evidence |
|---|---|---|
| <copied verbatim from the gate in KB-080> | ✅/❌ | <link, command output, test run> |

**Gate verdict:** PASSED | FAILED | PASSED WITH EXCEPTIONS

Exceptions require a named owner and a date. "We'll get to it" is a failed gate.

## 2. Schedule
| | Planned | Actual | Variance |
|---|---|---|---|
| Duration | | | |
| Tasks | | | |

**Why the variance:** <the actual cause, not a euphemism>

## 3. Tasks
| Task | Planned est. | Actual | Status |
|---|---|---|---|

Tasks not completed: <id, why, where they moved to>

## 4. Outstanding risks
| Risk | Severity | Owner | Carried into |
|---|---|---|---|

New risks discovered during this milestone → add to KB-060 with an R-id.

## 5. Open questions
| Q | Answered? | Answer / still blocking |
|---|---|---|

## 6. Documentation
- [ ] KB documents updated, `last_verified` bumped
- [ ] Investigation registry rows added/amended
- [ ] New business rules have `file:line` evidence and BR ids
- [ ] As-is and proposal documents still strictly separated
- [ ] KB-081 tracker reflects reality

## 7. Estimate re-baselining
Does evidence from this milestone change later estimates? For M3 this is **mandatory**
(M3-9-01). State the new numbers or state explicitly that none changed.

## 8. Lessons learned
| What happened | What we will do differently | Change landed in |
|---|---|---|

A lesson that does not change a document or a task is not a lesson. Every row needs a
destination.

## 9. Decision to proceed
Proceed to <next milestone>: YES / NO / CONDITIONAL
Conditions: <…>
```

---

## Definition of Done

### Task

A task is Done when **all** of the following hold:

1. Acceptance criteria objectively met.
2. Verification commands run and passing.
3. Required tests written and passing — testing belongs to the task that introduces the
   behaviour, never deferred to M5.
4. Documentation updated; `last_verified` bumped on every document touched.
5. Investigation registry updated where the task investigated anything, **including negative
   results**.
6. Diff reviewed against the reviewer checklist.
7. Committed on its own branch, single-scope, independently revertible.
8. Tracker (KB-081) updated.
9. `current-task.md` (KB-089) handed over to the next task, and the session close-out
   checklist above is clear.

Reaching all nine leaves the task at `REVIEW`. `COMPLETED` additionally requires whatever
human step the task's *Completion Conditions* names — normally the merge
([KB-088 §3](workflow.md#who-may-set-completed)).

### Milestone

1. Every task Done (parents roll up from children).
2. **The exit gate passes**, with evidence recorded per criterion.
3. The milestone review is recorded.
4. Later-milestone estimates re-baselined if this milestone produced evidence that changes
   them.

### Project

Gate G6 passes: all tenants on Angular for all modules; one full financial period with zero
module-level fallbacks; a rollback drill executed successfully in production; Blazor routes
retired and the decommissioning decision recorded as a new ADR.

---

## Handling a failed gate

Gates are not renegotiated to fit a date.

1. Record the failing criteria in the milestone review.
2. Create remediation tasks with new ids in the milestone that failed — never fold them
   silently into the next milestone.
3. Re-run the gate.

The one legitimate escape is **PASSED WITH EXCEPTIONS**, which requires: a named owner per
exception, a date, and a written statement of what breaks if the exception is never closed.
Anything else is moving the gate, which defeats its purpose.
