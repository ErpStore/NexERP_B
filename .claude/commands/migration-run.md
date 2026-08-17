---
description: Start the autonomous ERP migration runner — processes dependency-ready tasks until a stop condition is reached
argument-hint: "[tasks=N] [retries=N] — e.g. 'tasks=3'; defaults to one task"
allowed-tools: Workflow, Read, Grep, Glob, Bash
model: inherit
---

Start the autonomous migration runner. Arguments: `$ARGUMENTS`

The orchestration loop is **not** something you improvise here. It is
`.claude/workflows/migration-runner.js` — a deterministic workflow that owns task selection,
model routing, the retry/escalation decision and every stop condition, and that delegates the
actual work to the `migration-investigator` / `migration-implementer` / `migration-validator`
/ `migration-debugger` subagents.

Its policy — the routing table, the retry budget, the escalation triggers and the safety
limits — is specified in `docs/kb/execution/autonomous-runner.md` (KB-091) and implemented as
literals in the script. Read KB-091 if you need to explain or change a routing decision;
change both together, because `tools/check-agent-system.sh` asserts they agree.

## Launch it

Call the **Workflow** tool with `{ name: "migration-runner" }`.

Translate `$ARGUMENTS` into the `args` object, passing only what the user actually specified:

| The user typed | Pass |
|---|---|
| *(nothing)* | `{}` — one task, then stop |
| `tasks=3` | `{ "maxTasks": 3 }` |
| `retries=1` | `{ "maxRetries": 1 }` |
| `tasks=5 retries=3` | `{ "maxTasks": 5, "maxRetries": 3 }` |

Pass `args` as a real JSON object, not a string.

**Never widen a safety flag.** `allowMerge`, `allowPush`, `allowSchemaChange` and
`allowDestructiveDb` stay `false` unless the user has explicitly asked for that specific thing
in this conversation — and such approval covers one task, never the run.

## Before launching, check two things

1. **`docs/kb/execution/runner-state.md`** (KB-093) — if Status is `RUNNING`, a run may already
   be live. Say so and ask rather than starting a second one; two runners on one repository
   produce a merge, not progress.
2. **`git status --porcelain`** — report the working tree state. A task whose first step
   requires a clean tree will stop on a dirty one, and the user would rather know now.

Then launch and say plainly that the run has started, what it will do, and that
`/migration-status` shows progress. The workflow runs in the background and reports when it
finishes; do not poll it.

## When it returns

Report its `stopReason` verbatim, the tasks processed with their verdicts and attempt counts,
and what the user should do next. If it stopped `BLOCKED`, name who can unblock it.

Do not re-launch it automatically after a stop — a stop condition is a decision point, and
re-running past one is how a loop becomes a runaway.

## Do not

- Execute a migration task yourself in this session. Selection, classification, routing and
  validation all live in the workflow; doing it here bypasses the independent validation that
  makes the loop trustworthy.
- Re-execute a completed task (M0-00, M0-01-01, M0-01-02 are finished history).
- Rebuild the `docs/kb/` structure.
