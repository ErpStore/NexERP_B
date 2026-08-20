---
doc_id: KB-091
title: Autonomous Runner — Agents, Model Routing and the Execution State Machine
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-16
dependencies: [KB-081, KB-082, KB-083, KB-084, KB-088, KB-089, KB-090, KB-092]
---

# Autonomous Runner — Agents, Model Routing and the Execution State Machine

This document is the **single source of truth** for how the migration executes itself: which
agent does what, which model each one runs on, how a failed validation is retried or
escalated, and when the runner must stop and ask a human.

It **adds a mechanism**; it changes no rule. [`workflow.md`](workflow.md) (KB-088) remains
authoritative for the lifecycle and the completion protocol,
[`task-tracker.md`](task-tracker.md) (KB-081) for status, and
[`dependency-graph.md`](dependency-graph.md) (KB-082) for what may run next. Where this
document and those ever disagree, **they win and this one is corrected**.

> **The repository is the persistent memory of this migration. The conversation is only
> temporary execution context.** The runner exists to make that literally true: no state the
> loop depends on may live in a conversation.

---

## 1. What this adds, and what it deliberately does not

| Added | Not added |
|---|---|
| Five specialised agents under `.claude/agents/` | A second task-management system — KB-081 stays the only status authority |
| A model-routing policy keyed on task complexity and risk | New task ids, a new tracker, or a parallel backlog |
| Explicit failure sub-states (`FAILED`, `DIAGNOSING`, `RETRYING`, `ESCALATED`) | A new lifecycle — these refine KB-088 §1, they do not replace it |
| A bounded retry/escalation policy | Unbounded retrying. Retries are capped and the cap is configurable |
| A durable failure record, [`failure-log.md`](failure-log.md) (KB-092) | Duplication of anything the task file or tracker already records |
| Optional task frontmatter (`complexity`, `risk`, …) | A requirement to edit the 105 existing task files — every field is **derived** when absent |

**No completed task is re-executed.** The runner only ever opens a task that
[KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) returns
as the next candidate, and that rule reads status from KB-081.

---

## 2. The agent roster

Defined in `.claude/agents/`. Each agent's file states its own responsibilities; this table
is the contract between them.

| Agent | Reads | Writes | Returns |
|---|---|---|---|
| `migration-orchestrator` | KB-089, KB-081, KB-082, this file | KB-089 § Run State, KB-081, KB-092 | The loop's decisions |
| `migration-investigator` | Legacy code, KB-003, KB docs | **Nothing in the repository** | An investigation result |
| `migration-implementer` | Task file, investigation result, named KB docs | Application code + the task's own documentation | What it changed, and what it could not |
| `migration-validator` | Acceptance criteria, the diff, the legacy behaviour | **Nothing except its own verdict evidence** | `PASS` or `FAIL` with evidence |
| `migration-debugger` | The failure, previous attempts (KB-092), the code | A fix, when the fix is safe and in scope | A root cause + a disposition |

Two invariants make this worth the extra hops:

- **The validator is not the implementer.** It starts from the acceptance criteria and the
  observed behaviour, never from the implementer's account of its own work. A validator that
  assumes the implementer was right adds cost and no signal.
- **The investigator is read-only.** Investigation that edits code cannot be reused, cached,
  or trusted as a description of the *existing* system.

The pre-existing `erp-migration-executor` agent is unchanged in purpose: it is the
**single-task, human-in-the-loop** executor. Use it when you want one task done under
supervision. Use the orchestrator when you want the loop.

---

## 3. Runner configuration

The one configuration block. Change it here; nothing else reads a hard-coded value.

```yaml
runner:
  # How many tasks one autonomous run may complete before stopping.
  # Was 1 until 2026-08-20, preserving the then-standing "one task, one session"
  # constraint. The owner replaced that with "pick the next task that can actually
  # be done"; see CLAUDE.md § Standing constraints for the five-part test.
  # 5 is a SAFETY BOUND, not a target -- the run also stops on no-ready-task or
  # budget_reserve, whichever comes first.
  max_tasks_per_run: 5
  continue_on_complete: true       # each task still gets its own branch FROM MASTER;
                                   # allow_merge/allow_push stay false, so continuing
                                   # produces more branches to review, never anything
                                   # on master.
                                   # A finished task is `Needs Review`, so its dependents
                                   # are NOT selectable -- consecutive tasks are
                                   # INDEPENDENT, and a dependency chain still advances
                                   # only through a human merge. Deliberate, not a defect.

  # Retry budget for a task whose validation FAILED.
  max_retries: 2                   # 2 retries = up to 3 implementation attempts total
  escalate_after_failures: 2       # the 2nd FAIL on one task forces the escalation model
  max_escalations: 1               # one Opus escalation per task, then BLOCKED

  # Validation
  require_independent_validation: true   # the implementer may never validate its own work
  validator_min_model: sonnet            # never route validation to the cheapest model

  # Safety
  stop_on_blocked: true
  allow_merge: false               # never merge or push without an in-conversation instruction
  allow_push: false
  allow_schema_change: false       # only a task that explicitly authorises it may override
  allow_destructive_db: false
```

A run may narrow these (fewer tasks, fewer retries). **A run may not widen `allow_*` —
those are lifted only by an explicit human instruction in the conversation**, per
[`CLAUDE.md`](../../../CLAUDE.md) § Standing constraints.

---

## 4. Classifying a task

Complexity and risk drive model selection. Both are **derived** from metadata every existing
task file already has, so the 105 task files written before this document work unmodified. An
explicit frontmatter value always wins over the derivation.

### 4.1 Base complexity from `task_type`

| `task_type` | Base |
|---|---|
| Documentation | LOW |
| DevOps, Investigation, Testing | MEDIUM |
| Backend, Frontend, Database | MEDIUM |
| Security, Migration, Architecture | HIGH |
| Product Decision | HIGH — **and never autonomous**, see §8 |

### 4.2 Raise one level for each of these that is true

- `estimate` is 3 d or more.
- `depends_on` names 3 or more tasks, **or** 3 or more tasks name this one.
- `business_rules` is non-empty — behaviour preservation is at stake.
- `source_files` spans two or more of the four projects (`V.SMART.Shared`, `V.SMART.Web`,
  `V.SMART.Api`, `V.SMART`).
- The task touches authentication, authorisation, tenancy resolution, document numbering, or
  calculation logic.
- `risk` is HIGH.

LOW + two raises is HIGH. There is no level above HIGH; further raises are recorded as
escalation triggers instead (§6.3).

### 4.3 Risk

| Risk | When |
|---|---|
| HIGH | Security or Product Decision type; database schema; secrets or credentials; `Program.cs` or `appsettings*.json`; anything with `business_rules` populated; anything that changes behaviour a live Blazor user can observe |
| MEDIUM | Default |
| LOW | Documentation-only, or a read-only investigation that writes nothing but KB prose |

Risk does not by itself pick a model — it sets **floors** (§5.2) and it decides how hard the
validator must look.

---

## 5. Model routing

Centralised here. Agents declare a sensible default in their own frontmatter; the
orchestrator overrides it per call using this table. Use Claude Code's **model aliases**
(`haiku`, `sonnet`, `opus`, `inherit`) — never a dated model id, which goes stale.

### 5.1 The routing table

| Role | LOW | MEDIUM | HIGH |
|---|---|---|---|
| Investigate | `haiku` | `sonnet` | `opus` |
| Implement | `sonnet` | `sonnet` | `opus` |
| Validate | `sonnet` | `sonnet` | `opus` |
| Diagnose (first failure) | `sonnet` | `sonnet` | `opus` |
| Diagnose (escalated) | `opus` | `opus` | `opus` |

Orchestration itself runs on whatever model the session was started with (`inherit`). The
loop's own decisions are cheap; the work inside it is not.

### 5.2 Floors that override the table

1. **Validation is never routed to `haiku`.** Independent validation is the mechanism that
   stops a plausible-but-wrong implementation from being recorded as done.
2. **`risk: HIGH` forces `opus` for validation**, whatever the complexity says.
3. **`haiku` never writes application code.** At LOW complexity it investigates and it may
   edit documentation; `.cs`, `.razor`, `.ts`, `.tsx` and `.csproj` edits require `sonnet` or
   better.
4. **An escalated attempt runs on `opus`** for both diagnosis and the implementation that
   follows it.

### 5.3 Per-task overrides

Any task file may set `preferred_model` / `escalation_model` in frontmatter (§9). A task that
does so states *why* in its Objective — otherwise the next person to read it cannot tell
whether the override is a considered decision or a leftover.

**Opus is not the default.** Most of M0 is DevOps and Documentation work where an Opus run
buys nothing. Escalation exists precisely so that the expensive model is spent on the cases
in §6.3 rather than uniformly.

---

## 6. The execution state machine

### 6.1 States

```
PLANNED → READY → IN_PROGRESS → INVESTIGATING → IMPLEMENTING → VALIDATING → REVIEW
                                                                   │
                                                              FAIL │
                                                                   ↓
                                                                FAILED
                                                                   ↓
                                                             DIAGNOSING
                                                          ┌────────┴────────┐
                                                 fixable  │                 │ too complex
                                                          ↓                 ↓
                                                      RETRYING          ESCALATED
                                                          │                 │
                                                          │            (Opus investigation)
                                                          └────────┬────────┘
                                                                   ↓
                                                            IMPLEMENTING
                                                                   ↓
                                                             VALIDATING
                                                                   │
                                                retries exhausted  ↓
                                                              BLOCKED
```

### 6.2 Mapping onto the canonical lifecycle

The runner's states are **finer-grained than KB-088 §1, not different from it.** Only the
canonical name is ever written to KB-081 or to a task file's frontmatter — the runner's
sub-state lives in [`current-task.md` § Run State](current-task.md) and nowhere else. This is
what keeps one vocabulary in the tracker.

| Runner state | Canonical ([KB-088 §1](workflow.md#1-task-lifecycle)) |
|---|---|
| `INVESTIGATING` | `IN_PROGRESS` |
| `IMPLEMENTING`, `RETRYING` | `IMPLEMENTATION` |
| `VALIDATING`, `FAILED`, `DIAGNOSING`, `ESCALATED` | `TESTING` |
| `REVIEW` | `REVIEW` |
| `BLOCKED` | `BLOCKED` (a flag, not a phase) |

**The runner's success terminal state is `REVIEW`, not `COMPLETED`.** This project requires
integration before `COMPLETED` — see
[KB-088 § Who may set COMPLETED](workflow.md#who-may-set-completed). The runner may set
`COMPLETED` only for a task whose completion conditions contain no human step, and it must
say which condition it relied on. Never report `COMPLETED` for work a human still has to do.

### 6.3 Escalation triggers

Any one of these moves the task to `ESCALATED` and routes the next attempt to `opus`:

1. A business rule governing the change is unclear, undocumented, or contradicted by code.
2. An architecture decision is required — anything that would need a new or superseding ADR.
3. Two or more modules, or two or more of the four projects, are genuinely involved.
4. Legacy Blazor behaviour cannot be determined from the source with confidence.
5. **Validation has failed twice on this task** (`escalate_after_failures`).
6. The Sonnet-level diagnosis returned root cause `unknown`, or contradicted the previous
   diagnosis.
7. The validator's `FAIL` category is `business-rule` or `architecture` — those are never
   "just a bug".

### 6.4 Retry rules

- Attempt 1 fails → `DIAGNOSING` at the routed model → fix → `VALIDATING`.
- Attempt 2 fails → `ESCALATED` → Opus investigation → fix → `VALIDATING`.
- Attempt 3 fails → **`BLOCKED`**. Stop. Write the record. Do not attempt a fourth.
- A retry that would repeat a fix already recorded in KB-092 for this task is **not a retry**
  — it is a loop. Escalate instead.
- Every attempt is appended to [`failure-log.md`](failure-log.md) **before** the next one
  starts, so a crashed session loses no attempt history.

---

## 7. Persistent state — what is written where

The loop must survive losing the conversation entirely. Everything it needs is on disk:

| State | Lives in | Written by |
|---|---|---|
| Is a run live, on what, at which attempt, and why it stopped | [`runner-state.md`](runner-state.md) (KB-093) | The workflow's agents, every transition |
| Which task is active, and its classification | [`current-task.md`](current-task.md) § Run State (KB-089) | Orchestrator, at selection and completion |
| Status of every task | [`task-tracker.md`](task-tracker.md) (KB-081) | Orchestrator, at completion |
| What was actually done | `tasks/<TASK-ID>.md` § Execution Record | Implementer / orchestrator |
| Every failed validation and diagnosis | [`failure-log.md`](failure-log.md) (KB-092) | Validator (verdict), debugger (diagnosis) |
| Findings, negative results included | [`investigation-registry.md`](../investigation-registry.md) (KB-003) | Investigator |
| Unanswered questions | [`open-questions.md`](../open-questions.md) (KB-004) | Any agent — **raise, never guess** |
| Business rules discovered | KB-030, with `file:line` evidence | Implementer / investigator |
| Risks | [`technical-debt-register.md`](../risks/technical-debt-register.md) (KB-060) | Any agent |

A finding that exists only in an agent's return value is lost the moment the run ends. The
orchestrator's last act before stopping is the session close-out in
[`review-templates.md`](review-templates.md) (KB-084).

---

## 8. Safety limits — the runner stops and asks

**Never silently guess.** The runner halts, records why in
[`failure-log.md`](failure-log.md), sets the task `BLOCKED` with a named owner, and reports —
when any of these is true:

1. A task reaches `BLOCKED`, or its retry budget is exhausted.
2. A required **business decision** is unknown. `Product Decision` tasks (M0-11 and its kin)
   are never executed autonomously — surfacing them to their owner *is* the useful action.
3. An **architectural decision** needs human approval, or an existing ADR would have to be
   superseded.
4. A **destructive or schema-changing database operation** would be required and the task
   does not explicitly authorise it.
5. **Credentials, DBA access, or an environment** the task needs are unavailable. M0-02 and
   the M0-01 series are the standing examples.
6. **Tests cannot be reliably executed.** `dotnet test` finds no test project until M0-12-01
   creates one and **must not be used** before then — a green run that executed nothing is
   worse than a red one. The verified command list is
   [KB-083 § Verified repository commands](prompt-template.md#verified-repository-commands);
   a command not on it is not a validation.
7. The change would touch **secrets, credentials, or git history**, or would require a merge
   or a push. `allow_merge` / `allow_push` are `false` and only a human lifts them.
8. The runner detects a **potentially unsafe migration** — behaviour a live Blazor user could
   observe changing without an explicit decision, ERP business logic being reimplemented in
   TypeScript, or a business rule being invented without `file:line` evidence.
9. Two candidate tasks rank equally and are genuinely independent — KB-082 step 4 says say so
   and let the owner choose, and that applies to the runner too.

A stop is a **successful outcome** of the loop. Reporting a blocker accurately is worth more
than an unreviewable change.

---

## 9. Optional task metadata

The existing frontmatter (KB-090) is unchanged and remains sufficient. These keys are
**optional additions**; when absent, §4 and §5 derive them.

| Key | Values | Default when absent |
|---|---|---|
| `complexity` | `LOW` / `MEDIUM` / `HIGH` | Derived per §4 |
| `risk` | `LOW` / `MEDIUM` / `HIGH` | Derived per §4.3 |
| `preferred_model` | `haiku` / `sonnet` / `opus` | Routing table §5.1 |
| `escalation_model` | `opus` | `opus` |
| `max_retries` | integer | `runner.max_retries` (§3) |

`depends_on` already exists and is authoritative for dependencies.

**Acceptance criteria and validation requirements are not moved into frontmatter.** Every
task file already carries `## Acceptance Criteria` and `## Testing Requirements`, and those
sections are binding ([KB-088 §5](workflow.md#5-validation)). Copying them into YAML would
create a second copy that drifts — the failure mode this knowledge base exists to prevent.

---

## 10. The loop

```
 1. SELECT     Read current-task.md. If it holds an unfinished task, resume it from its
               Run State. Otherwise apply KB-082's ready-task selection rule against KB-081.
 2. CLASSIFY   Derive complexity and risk (§4). Record both in Run State.
 3. ROUTE      Pick the model per role from §5. Record the choice.
 4. PLAN       State the implementation plan in a few sentences (KB-088 §2 step 8).
 5. INVESTIGATE  Check KB-003 first — reuse a Complete finding by doc_id rather than
               re-deriving it. Only then dispatch migration-investigator for the gap.
 6. IMPLEMENT  Dispatch migration-implementer with the task file and the investigation result.
 7. VALIDATE   Dispatch migration-validator independently. PASS or FAIL, with evidence.
 8a. PASS   →  Record the verdict, append the Execution Record, update KB-081, run the
               KB-084 close-out, select the next task, rewrite current-task.md, and STOP
               unless continue_on_complete is true.
 8b. FAIL   →  Append to failure-log.md. Dispatch migration-debugger. Retry or escalate per
               §6.4. On exhaustion: BLOCKED, record, stop.
```

Step 5's "check the registry first" is not an optimisation. Re-deriving a finding the KB
already holds costs a model call and produces a second answer that may not match the first.

---

## 11. Entry points

| You want | Use |
|---|---|
| One task, autonomously, then stop | `/migration-run` |
| Several tasks in one run | `/migration-run tasks=3` |
| Stop a live run safely | `/migration-stop` |
| Just the state, changing nothing | `/migration-status` |
| One task with a human in the loop | The `erp-migration-executor` agent, as before |

The slash commands live in `.claude/commands/`. They are thin: they carry no policy of their
own, and defer to this document and to the runner script so that routing and retry rules exist
in exactly one place.

---

## 12. The orchestration mechanism

Agent definition files are **declarative**. Writing five of them creates five workers and no
loop — nothing in `.claude/agents/` selects a task, routes a model, or decides to retry. The
loop is a separate artefact:

### `.claude/workflows/migration-runner.js`

A **Claude Code workflow** — a deterministic JavaScript orchestration script. It owns exactly
the decisions §2 assigns to the orchestrator, and delegates all the work:

| The script decides | The agents do |
|---|---|
| Which task, and whether it is safe to start | Investigate, implement, validate, diagnose |
| Complexity → model, per role, per attempt | Read and write the repository |
| PASS → complete; FAIL → retry, escalate or block | Record findings where they belong |
| When the run stops, and why | — |

Why a script rather than a prompt telling a model to loop: control flow that must be
**deterministic** — a bounded retry counter, an escalation threshold, a stop condition — is
not something to re-derive by inference on every iteration. The script cannot forget the retry
cap, and two runs of the same state take the same path.

**It calls the agents in `.claude/agents/` directly**, by `agentType`. There are not two
definitions of a worker; the workflow supplies the model and the task-specific prompt, the
agent file supplies the role, its constraints and its tool restrictions — including the
read-only tool sets that make the investigator and validator trustworthy.

### Executable authority

A workflow script has **no filesystem access** — it cannot read this document at runtime.
So the routing table (§5.1), the floors (§5.2), the retry budget (§6.4) and the stop
conditions (§8) exist as literals in the script, and this document is their specification.

That is a real duplication, and it is handled by **verification rather than trust**:
`tools/check-agent-system.sh` asserts that the script's `ROUTING` constants, retry defaults and
`agentType` values match this document and the agents that exist on disk. If someone changes
one and not the other, the check fails. Do not "fix" a mismatch by editing only the script.

### Between tasks, nothing is carried in memory

The script passes only compact structured summaries between stages — a validator verdict, a
diagnosis — never a transcript. Every agent bootstraps from `CLAUDE.md` and the repository.
This is what makes "start it once" safe: a run killed at any point resumes from
[`runner-state.md`](runner-state.md), because each transition is written before the next
begins.

Consistency of the whole arrangement is checked by `tools/check-agent-system.sh`, alongside
`tools/check-kb-execution-framework.sh` which checks the framework this one builds on.
