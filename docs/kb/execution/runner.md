---
doc_id: KB-091
title: Autonomous Migration Runner — Agents, State Machine and Safety Limits
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-16
dependencies: [KB-081, KB-082, KB-088, KB-089, KB-092, KB-093]
---

# Autonomous Migration Runner

The runner executes migration tasks end to end — select, classify, plan, investigate,
implement, validate, and either complete or diagnose — without a human re-supplying context
between tasks.

**The repository is the state. The conversation is a scratchpad.** Every transition below is
written to disk before the next begins, so a runner killed mid-task resumes from the
repository, not from memory.

This document is the specification the agents in `.claude/agents/` implement. Read it once
at the start of a run.

> **The runner does not re-execute completed work.** `M0-00`, `M0-01-01` and `M0-01-02` are
> `COMPLETED` and their task files are immutable history. The same applies to `M1-01`…`M1-05`
> (the analysis pass that produced this knowledge base). A selection step that returns a
> `COMPLETED` task is a bug — see §7.

---

## 1. The loop

```
        ┌──────────────────────────────────────────────────────────┐
        │                     SELECT TASK                          │
        │  KB-081 tracker + KB-082 selection rule → one task        │
        └───────────────────────────┬──────────────────────────────┘
                                    ▼
                             CLASSIFY TASK              complexity + risk (§3)
                                    ▼
                             SELECT MODEL               KB-092 routing (§4)
                                    ▼
                                  PLAN                  orchestrator, in-session
                                    ▼
                             INVESTIGATE                migration-investigator
                                    ▼
                              IMPLEMENT                 migration-implementer
                                    ▼
                               VALIDATE                 migration-validator
                                    ▼
                          ┌─────────────────┐
                          │     PASS?       │
                          └────┬───────┬────┘
                        YES    │       │    NO
                               ▼       ▼
                         COMPLETE    DIAGNOSE           migration-debugger
                               │       │
                        UPDATE KB      FIX ──► VALIDATE ──┐
                               │       │                  │
                               │       └── retries left? ──┤
                               ▼                           ▼
                          NEXT TASK                 ESCALATE → BLOCKED
```

Each box is a distinct agent invocation with its own context. The orchestrator holds only
the loop state — it never carries an investigation's raw output into the implementer's
context, only the written artefact.

## 2. States

```
PLANNED → READY → IN_PROGRESS → INVESTIGATING → IMPLEMENTING → VALIDATING → REVIEW → COMPLETED
```

On validation failure:

```
VALIDATING → FAILED → DIAGNOSING → RETRYING → IMPLEMENTING → VALIDATING
```

When diagnosis exceeds the current model's reach:

```
FAILED → ESCALATED → (investigate + implement at the escalation model) → VALIDATING
```

When retries are exhausted or a safety limit trips:

```
→ BLOCKED
```

| State | Meaning | Written where |
|---|---|---|
| `PLANNED` | In the plan; prerequisites unmet | KB-081, task frontmatter |
| `READY` | Prerequisites met; selectable now | KB-081 |
| `IN_PROGRESS` | Selected, classified, model chosen, plan written | KB-089 Runner State |
| `INVESTIGATING` | Investigator running | KB-089 |
| `IMPLEMENTING` | Implementer running | KB-089 |
| `VALIDATING` | Validator running | KB-089 |
| `FAILED` | Validator returned FAIL | KB-089 + KB-093 |
| `DIAGNOSING` | Debugger analysing root cause | KB-089 |
| `RETRYING` | Fix applied; attempt counter incremented | KB-089 + KB-093 |
| `ESCALATED` | Re-running at the escalation model | KB-089 + KB-093 |
| `REVIEW` | Validation passed, committed, awaiting human merge | KB-081, task file |
| `COMPLETED` | Merged / integrated | KB-081, task file |
| `BLOCKED` | Stopped. Needs a human | KB-081 + KB-093 |

`REVIEW` is not ceremony. An autonomous runner may not merge, so `REVIEW` is the honest
terminal state for most tasks — see [KB-088 § Who may set COMPLETED](workflow.md#who-may-set-completed).
The runner marks `COMPLETED` only when a task's *Completion Conditions* contain no human step.

Earlier drafts of [KB-088](workflow.md) named two of these `IMPLEMENTATION` and `TESTING`.
The names here are canonical; those are accepted as aliases.

## 3. Classification

Deterministic, so two runs classify alike. Read the task's frontmatter first — if it declares
`complexity`, that wins and no inference is needed.

Otherwise infer, taking the **highest** band any rule triggers:

| Band | Triggers |
|---|---|
| **LOW** | Documentation or DevOps task; single file or a doc-only diff; `business_rules: []`; no schema change; no API surface; acceptance criteria are all mechanical checks |
| **MEDIUM** | Single module; Backend/Frontend/Testing task against an already-extracted contract; business rules already recorded with `file:line` evidence; a known-good pattern exists to follow |
| **HIGH** | Business-rule extraction from Razor `@code`; Security or Architecture task; touches ≥2 modules; DB schema or stored procedures; legacy behaviour not yet recorded in the KB; a Product Decision; anything gating a milestone gate; **any task already at attempt ≥ 2** |

Risk is recorded separately (`risk: low|medium|high`) and does **not** lower a band — a
low-complexity change to a high-risk surface still gets high-risk handling: narrower scope,
stricter validation, and no auto-merge.

## 4. Model selection

Policy lives in exactly one place: [`model-routing.md`](model-routing.md) (KB-092). Do not
hardcode a model anywhere else, and do not put a model name in an agent's frontmatter — every
worker declares `model: inherit` and is given its model by the orchestrator at call time.

## 5. Agents

| Agent | Role | Writes code? |
|---|---|---|
| [`migration-orchestrator`](../../../.claude/agents/migration-orchestrator.md) | Owns the loop and the state file. Selects, classifies, routes, coordinates, decides retry vs escalate vs block | No |
| [`migration-investigator`](../../../.claude/agents/migration-investigator.md) | Finds legacy behaviour, business rules, dependencies, risks | **No — read-only** |
| [`migration-implementer`](../../../.claude/agents/migration-implementer.md) | Implements exactly the current task | Yes |
| [`migration-validator`](../../../.claude/agents/migration-validator.md) | Independently proves PASS/FAIL against acceptance criteria | No |
| [`migration-debugger`](../../../.claude/agents/migration-debugger.md) | Root-causes a failure; fixes when safe, escalates when not | Yes, narrowly |

**The orchestrator does not do the work.** If it finds itself reading business services or
editing code, it has absorbed a worker's job and the separation that makes validation
independent is gone.

**The validator must not trust the implementer.** It re-derives evidence from the repository —
running commands, reading diffs, checking criteria one by one. A validator that reads the
implementer's summary and agrees has validated nothing. It is invoked with no access to the
implementer's reasoning, only to the repository and the acceptance criteria.

### Where the orchestrator runs

Run the loop in the **main session** (`/migrate`), which spawns the four workers as
subagents. Claude Code does not guarantee that a subagent may spawn further subagents, so
delegating the whole loop to `migration-orchestrator` as a subagent may leave it unable to
call its workers. The agent file exists for that case and declares the `Agent` tool; if
nesting is unavailable in your version, it degrades to advisory — it reports the plan and the
main session executes it.

## 6. Retry and escalation

Attempt counting is per task, persisted in [`current-task.md`](current-task.md) and detailed
in [`failure-log.md`](failure-log.md).

| Setting | Default | Override |
|---|---|---|
| `max_retries` | **2** (3 attempts total) | task frontmatter |
| Escalate after | attempt **2** fails | `escalate_after` in frontmatter |
| Escalation model | Opus | `escalation_model` |

**Escalate immediately, without spending a retry, when:**

- business rules are unclear or contradict the KB;
- an architectural decision is required;
- more than one module is implicated;
- legacy behaviour cannot be determined from the code;
- the debugger cannot identify a root cause;
- the same failure signature repeats — a repeat means the diagnosis was wrong, and another
  attempt at the same level will reproduce it.

**Never retry** a failure whose cause is environmental (missing credential, unreachable
database, locked build tool) or a genuine unknown (an unanswered `Q-nn`). Retrying cannot fix
either. Go to `BLOCKED` and say what is needed.

Every attempt appends to `failure-log.md` **before** the next begins. An attempt that is not
written down did not happen, and the next session will repeat it.

## 7. Safety limits — hard stops

The runner stops, sets `BLOCKED`, records the reason with an owner, and does **not** proceed
to another task when:

1. `max_retries` is exceeded.
2. A required business decision is unknown — raise it in
   [`open-questions.md`](../open-questions.md) with a `Q-nn`.
3. An architectural decision is required — it needs an ADR and human approval.
4. A destructive database operation would be required.
5. Credentials, DB access, or the environment needed to validate are unavailable.
6. **Tests cannot be run reliably.** Unverifiable is not the same as passing.
7. The change would alter business behaviour without a decision authorising it.
8. Selection returns a `COMPLETED` task, or any of `M0-00`, `M0-01-01`, `M0-01-02`,
   `M1-01`…`M1-05` — these are finished history. Stop and report a selection bug.
9. Git state is not what the task expects — wrong branch, dirty tree, or a branch cut from
   somewhere unexpected.
10. The runner would need to merge, push, rotate a credential, or touch production.

**Never silently guess.** A recorded `BLOCKED` with a named owner is a good outcome; a guess
that passes validation is the worst one, because it is invisible.

## 8. What the runner may never do

- Merge or push. Branch and commit only.
- Re-execute a `COMPLETED` task, or edit its task file except to correct a factual error.
- Start a second task in one run unless `continue_on_success` is explicitly enabled (§9).
- Rewrite an existing business service to "clean it up".
- Reimplement ERP business logic in TypeScript.
- Invent a business rule, or record one without `file:line` evidence.
- Change the DB schema unless the task authorises it.
- Suppress a failing test, widen an acceptance criterion, or downgrade a criterion to make a
  task pass. If a criterion is wrong, that is a finding for a human, not an edit.

## 9. Run configuration

Defaults live here; a run may override them in the start prompt.

| Key | Default | Effect |
|---|---|---|
| `continue_on_success` | **false** | When false, the runner stops after one task, having set the next as current. When true, it proceeds until a stop condition. |
| `max_tasks_per_run` | 1 | Upper bound even when continuing. |
| `max_retries` | 2 | Per task. |
| `auto_commit` | true | Commit single-scope on the task branch. Never merge. |
| `require_human_for` | `HIGH` risk, Security, Product Decision, schema | Task classes that stop at `REVIEW` regardless of validation. |

## 10. Per-task procedure

1. **Select** — KB-081 + [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).
   Verify the task is not `COMPLETED` (§7.8).
2. **Classify** (§3) and **route** (§4). Write both into `current-task.md`.
3. **Plan** — a short plan in `current-task.md`. Not a document.
4. **Investigate** — spawn `migration-investigator` with the task's *Relevant Existing Code*
   and open questions. Its findings go into the KB (registry, business rules) — cite existing
   `INV-0xx` rather than re-deriving. Skip only if the task declares no investigation need
   **and** the registry already covers it.
5. **Implement** — spawn `migration-implementer` with the task file plus the investigation
   artefact by path.
6. **Validate** — spawn `migration-validator` with the acceptance criteria and the diff.
   Independent by construction.
7. **On PASS** → commit, update the task file's Execution Record, KB-081, and any KB document
   that actually changed (KB-088 §4), select the next task, rewrite `current-task.md`, stop.
8. **On FAIL** → append to `failure-log.md`, spawn `migration-debugger`, then retry or
   escalate or block per §6.

Between every step the state file is written first. That is what makes the loop resumable.
