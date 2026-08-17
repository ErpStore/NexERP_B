---
doc_id: KB-092
title: Validation Failure and Diagnosis Log
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-16
dependencies: [KB-081, KB-089, KB-091]
---

# Validation Failure and Diagnosis Log

**Append-only.** Every validation that returned `FAIL`, every diagnosis of that failure, and
every safety stop. One entry per attempt.

This exists so that the autonomous runner ([`autonomous-runner.md`](autonomous-runner.md),
KB-091) can answer three questions after the conversation that produced them is gone:

1. **Has this been tried?** A retry that repeats a recorded fix is a loop, not a retry.
2. **How many attempts has this task had?** The retry budget is meaningless if the count
   lives in a chat window.
3. **Why did it stop?** A `BLOCKED` task with no recorded reason costs the next session the
   whole diagnosis again.

## Rules

- **Append; never edit or delete an entry.** A wrong diagnosis that was later corrected is
  the most useful thing in this file — it stops the next session reaching for it again.
- Write the entry **before** the next attempt starts. An attempt that is only in memory is
  lost if the session dies.
- Cite `file:line` for evidence, per
  [`source-of-truth-rules.md`](../source-of-truth-rules.md) (KB-002). "The build failed" is
  not evidence; the error id and the failing line are.
- Record **safety stops** here too, not only failures. "Stopped: needs DBA access" is a
  result.
- A failure that reveals a business rule, a risk, or an open question is *also* recorded in
  its proper home (KB-030, KB-060, KB-004). This log records the **attempt**; those record
  the **knowledge**.
- When a task finally passes, the entries stay. The task file's `## Execution Record`
  summarises the outcome and links here for the detail.

## Entry format

````markdown
### <TASK-ID> · attempt <n> · <YYYY-MM-DD>

| Field | Value |
|---|---|
| Runner state | FAILED / DIAGNOSING / ESCALATED / BLOCKED |
| Model in use | haiku / sonnet / opus |
| Validator verdict | FAIL |
| Failure category | build / test / acceptance-criterion / regression / business-rule / architecture / environment |

**What failed** — the acceptance criterion or command, quoted, with what it actually printed.

**Root cause** — one sentence, or `unknown` (which is itself an escalation trigger, KB-091 §6.3).

**Evidence** — `path:line`, command output, or the legacy behaviour it contradicts.

**Disposition** — `fixed` / `retry` / `escalate` / `blocked`, and why.

**Next attempt routed to** — model, and which KB-091 §6.3 trigger applied, if any.
````

The `Failure category` is not decoration: `business-rule` and `architecture` escalate
immediately rather than being retried at the same model, because they are never "just a bug"
([KB-091 §6.3](autonomous-runner.md#63-escalation-triggers)).

---

## Log

*No entries. The autonomous runner has not yet executed a task.*

The three tasks completed before this framework existed — **M0-00**, **M0-01-01** and
**M0-01-02** — were executed under the human-in-the-loop workflow and are recorded in their
own task files' `## Execution Record` sections and in
[`task-tracker.md`](task-tracker.md) (KB-081). They are **not** backfilled here: this log
records attempts made by the runner, and inventing retrospective entries for work it did not
do would make it lie.
