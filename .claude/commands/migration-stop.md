---
description: Stop the autonomous migration runner safely at the next task boundary, or report that none is running
allowed-tools: Read, Write, Edit, Grep, Glob, Bash, TaskList, TaskStop
model: inherit
---

Stop the autonomous migration runner **safely**.

## The safe stop — prefer this

Edit `docs/kb/execution/runner-state.md` (KB-093):

- Set **Status** to `STOP_REQUESTED`.
- Record who asked, when, and why.

The runner reads that file at the start of every task, so the request takes effect at the
**next task boundary**: the in-flight task finishes its implement → validate → record cycle
and the run exits cleanly with the reason preserved.

This is preferred because a task abandoned mid-implementation leaves a half-finished diff on
its branch that a human then has to reconcile. Waiting one task boundary costs minutes and
avoids that entirely.

Tell the user the request is recorded and that the current task will finish first.

## The immediate stop — only when asked for

If the user explicitly wants the run killed **now**, accepting a possibly half-implemented
task on its branch:

1. `TaskList` to find the running `migration-runner` workflow.
2. `TaskStop` on its task id.
3. Then update `runner-state.md`: Status `STOPPED`, the reason, the task that was in flight,
   and an explicit note that its branch may hold an incomplete change needing review.

The repository stays consistent either way — every transition is written to disk before the
next begins — but only the safe stop leaves a task at a clean boundary.

## If nothing is running

Say so, and report what `runner-state.md` records: the last status, the last stop reason and
the current task. Change nothing. A stale `RUNNING` status with no live workflow means a
previous run was killed — point that out and offer to correct the file to `STOPPED` with the
reason "run terminated without recording a stop".

## Never

Do not mark a task `COMPLETED`, do not merge or push, and do not "tidy up" a partial
implementation as part of stopping. Stopping records state; it does not finish work.
