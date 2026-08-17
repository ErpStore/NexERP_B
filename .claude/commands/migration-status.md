---
description: Report the migration's current state — active task, run state, next candidates, blockers — changing nothing
allowed-tools: Read, Grep, Glob, Bash
model: inherit
---

Report the current state of the V.SMART / NexGen ERP migration. **Read-only — change no
file, dispatch no agent, start no task.**

Repository root: `C:\Kumar\NexGen-ERP---2025-master\NexGen-ERP---2025-master` (the nested
folder; the outer directory is a wrapper).

Read:

- `docs/kb/execution/current-task.md` (KB-089) — the active task and its `## Run State`.
- `docs/kb/execution/task-tracker.md` (KB-081) — the status authority.
- `docs/kb/execution/failure-log.md` (KB-092) — attempts, diagnoses and safety stops.
- `docs/kb/execution/dependency-graph.md` (KB-082) § *Ready-task selection rule* — only to
  rank the candidates, not to change anything.

Then report, compactly:

1. **Active task** — id, title, lifecycle status, and the runner sub-state with its attempt
   count if a run is in progress.
2. **Completed** — how many, which ones. Note that `Needs Review` is *not* `Completed` in
   this project: it means committed and unmerged, and it still blocks merge-dependent
   successors.
3. **Blocked** — each blocked task, what it is blocked on, and **who can unblock it**.
   Distinguish blocked-on-a-task from blocked-on-a-human; they are different problems.
4. **Next candidates** — the ranked ready set with the reason the top one ranks first. If two
   rank equally and are genuinely independent, say so rather than picking.
5. **Open failures** — anything in the failure log without a resolution.
6. **Working tree** — `git status --porcelain` and the current branch, so the reader knows
   whether the tree is clean enough to start a run.

State plainly if the tracker and `current-task.md` disagree. KB-081 is authoritative on
status and `current-task.md` is the one to correct — but correcting it is a *run's* job, not
this command's.
