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
last_verified: 2026-08-12
dependencies: [KB-080, KB-081, KB-083]
---

# Review, Handoff and Done Templates

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
- [ ] No business logic added to React; no business service rewritten.
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

### Milestone

1. Every task Done (parents roll up from children).
2. **The exit gate passes**, with evidence recorded per criterion.
3. The milestone review is recorded.
4. Later-milestone estimates re-baselined if this milestone produced evidence that changes
   them.

### Project

Gate G6 passes: all tenants on React for all modules; one full financial period with zero
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
