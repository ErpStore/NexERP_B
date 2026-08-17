# CLAUDE.md — V.SMART / NexGen ERP

This file is the **invariant** context for every session. It never describes a specific
task. The active task lives in [`docs/kb/execution/current-task.md`](docs/kb/execution/current-task.md).

**The repository is the persistent memory of this migration. The conversation is only
temporary execution context.** Nothing you need should ever have to be pasted in.

---

## To start work

```
Read CLAUDE.md and docs/kb/execution/current-task.md.
Execute the current task according to the repository's migration workflow.
Do not start the next task.
```

That prompt is complete. The procedure it refers to is
[`docs/kb/execution/workflow.md`](docs/kb/execution/workflow.md) (KB-088) — read it once at
the start of any execution session.

---

## Repository root — read this before using any path

The Claude Code working directory is usually `C:\Kumar\NexGen-ERP---2025-master`, a
**wrapper**. The git repository is the nested folder:

```
C:\Kumar\NexGen-ERP---2025-master\NexGen-ERP---2025-master\
```

Every path in this file and in `docs/kb/` is relative to that **nested** root. If `git` or
`dotnet` behaves as though the project does not exist, this is why.

---

## What this project is

Replace the Blazor Server frontend with a React 19 + TypeScript SPA talking to a versioned
ASP.NET Core Web API, **preserving existing ERP business behaviour**.

```
Existing Blazor ERP → understand behaviour → identify business rules
   → React frontend + ASP.NET Core API + existing database/business rules
   → incremental migration, Blazor stays live until each replacement is verified
```

This is **not** a UI translation exercise. Business logic currently trapped in Razor
`@code` is *extracted into server-side services* before any React screen replaces it — it
is never reimplemented in TypeScript.

| Project | Role |
|---|---|
| `V.SMART/V.SMART.Shared` | .NET 9 class library — **all** domain code: ~196 EF entity sets, 285 business services, ~190 repositories + UnitOfWork, 274 ViewModels, 333 Razor pages, 440 routes |
| `V.SMART/V.SMART.Web` | Blazor Server host — the live UI, stays running throughout |
| `V.SMART/V.SMART.Api` | ASP.NET Core Web API (.NET 9) — the React backend. **Already exists**, ~10% built. Extended, never created and never rewritten |
| `V.SMART/V.SMART` | .NET MAUI Blazor Hybrid host — shares the domain layer |

SQL Server + EF Core 9, code-first, **database-per-tenant**.

---

## Authority order when sources conflict

1. **Current source code** — always wins.
2. Database schema / EF migrations — authoritative for storage.
3. The knowledge base `docs/kb/` — authoritative for interpretation.
4. Older prose docs — **hypotheses only**. `docs/ARCHITECTURE.md` is an unfinished template
   with known factual errors; it is superseded, not authoritative.

Full rules: [`docs/kb/source-of-truth-rules.md`](docs/kb/source-of-truth-rules.md) (KB-002).

Every factual claim you write is tagged **Confirmed** (traced to `file:line`), **Inferred**
(reasoned, with the reasoning shown), or **Unknown** (recorded in `open-questions.md`).
Never write an inference so that it reads as fact.

A task file's *Current Implementation* section is a **hypothesis**, not fact — task files
go stale as other tasks land. Re-verify line numbers against the code before citing them.

---

## Context discipline — this is load-bearing

The knowledge base is large. Reading it all costs more than the work.

**Read exactly this, and nothing else, unless the work demands it:**

1. This file.
2. `docs/kb/execution/current-task.md` — small by design.
3. `docs/kb/execution/workflow.md` — the procedure.
4. Only the documents the current task's *Relevant Documentation* section names.
5. Only the code its *Relevant Existing Code* section names, plus what those lead to.

**Do not**: read the whole `docs/kb/` tree; read `execution/README.md` (KB-080) end to end —
it is 55 KB, deep-link to the section you need; read other tasks' files; re-derive facts the
KB already records; reproduce large files into the conversation when a `grep` answers the
question.

**Before investigating anything**, search
[`docs/kb/investigation-registry.md`](docs/kb/investigation-registry.md) (KB-003) and route
via [`docs/kb/INDEX.md`](docs/kb/INDEX.md) (KB-005). If an investigation is `Complete` and
not stale, **reuse it and cite the `doc_id`** — do not re-derive it. If `Partial`,
investigate only the documented gap. If absent or contradicted by code, investigate, then
record the finding with `file:line` evidence and a confidence rating so no future session
repeats the work. **Record negative results too** — "grepped for X, found none" is a finding.

---

## Task lifecycle

```
PLANNED → READY → IN_PROGRESS → IMPLEMENTATION → TESTING → REVIEW → COMPLETED
```

`BLOCKED` is an orthogonal flag, not a phase — a task in any pre-COMPLETED phase can be
blocked by a missing prerequisite or a missing human decision.

**A task is never COMPLETED because the code was written.** Its acceptance criteria must be
objectively met and its required validation actually run. Definitions and transition rules:
[`docs/kb/execution/workflow.md`](docs/kb/execution/workflow.md).

---

## Standing constraints — non-negotiable

- **Stay inside the current task's scope.** No opportunistic refactoring of unrelated code.
- **Do not start the next task.** One task, one session. This is what makes each unit
  independently reviewable and revertible.
- Do not rewrite an existing business service wholesale to "clean it up".
- Do not reimplement ERP business logic in React/TypeScript — call the API.
- Do not change the database schema unless the task explicitly authorises it.
- Do not change an architecture decision without recording it (new ADR, or an update to the
  relevant decision document).
- **Do not invent business rules.** A rule without `file:line` evidence is not a rule.
- If a requirement is unclear, record it in
  [`docs/kb/open-questions.md`](docs/kb/open-questions.md) — do not guess.
- If implementation reveals a business rule or a significant technical decision, record it.
- The server stays authoritative for validation, calculations, permissions and document
  numbering. Always.
- **Never merge or push** without an explicit instruction in the current conversation.
  Approval for one task does not carry to the next.
- Never claim a command's result you did not observe. If something cannot be verified, say
  so.

---

## Git

| Rule | |
|---|---|
| Branch | `migration/<TASK-ID>-<slug>` |
| Commit | `<TASK-ID>: <imperative summary>` |
| Scope | One task per branch. Two task ids in one branch is a reject. |
| Merge | Left for review. Never merged or pushed from an execution session. |

Remote `https://github.com/ErpStore/NexERP_B.git`, default branch `master`.

---

## Build and test commands

**Do not invent a build command.** The verified list — and what is explicitly *not* yet
verified — is the single table in
[`docs/kb/execution/prompt-template.md` § Verified repository commands](docs/kb/execution/prompt-template.md#verified-repository-commands).
It is updated by the tasks that measure it, so it is always current; copying commands out of
it into other documents is how it goes stale.

At present: prefer per-project `dotnet build <path>.csproj` over solution-level builds, and
**`dotnet test` finds nothing** — no test project exists until M0-12-01 creates one.

---

## Known traps — do not rediscover these

- **Untracked-directory checkout trap.** `V.SMART.Api/` is largely untracked; some branches
  track only `Program.cs` / `V.SMART.Api.csproj` from it. Switching branches can silently
  **delete** those files from disk. A build failing on a missing `Main` right after a branch
  switch is this, not corruption — restore with `git show <branch>:<path> > <path>`.
- Use `git grep --untracked`, not plain `git grep` — plain `git grep` silently skips
  `V.SMART.Api/`.
- The root `NexGen-ERP---2025-master.sln` is untracked; the only tracked `.sln` is deleted in
  the working tree.
- Build warning baseline is ~6,695, largely MudBlazor `MUD0002`. CI must fail on *new*
  warnings; it cannot use `-warnaserror` until that is cleared.
- `MauiAppBuilder.Configuration` does **not** include environment variables by default,
  unlike `WebApplicationBuilder` — it needs an explicit `AddEnvironmentVariables()`.
- PowerShell `Write-Error` under `$ErrorActionPreference = 'Stop'` terminates immediately
  here; use explicit flow control in ops scripts.

---

## Map

| You need | Read |
|---|---|
| The active task | `docs/kb/execution/current-task.md` |
| The execution procedure, lifecycle, completion protocol | `docs/kb/execution/workflow.md` (KB-088) |
| Autonomous execution: agents, model routing, retries, escalation, safety stops | `docs/kb/execution/autonomous-runner.md` (KB-091) |
| Why a task failed validation, and what was already tried | `docs/kb/execution/failure-log.md` (KB-092) |
| Is an autonomous run live, on what, and why it stopped | `docs/kb/execution/runner-state.md` (KB-093) |
| Status of every task | `docs/kb/execution/task-tracker.md` (KB-081) |
| What blocks what, critical path, next-task selection | `docs/kb/execution/dependency-graph.md` (KB-082) |
| Which KB document answers a question | `docs/kb/INDEX.md` (KB-005) |
| Whether something was already investigated | `docs/kb/investigation-registry.md` (KB-003) |
| A specific task's full specification | `docs/kb/execution/tasks/<TASK-ID>.md` |
| Milestones, gates, the whole roadmap | `docs/kb/execution/README.md` (KB-080) — deep-link, do not read whole |
| Handoff / review / definition-of-done templates | `docs/kb/execution/review-templates.md` (KB-084) |
| How to write a new task file | `docs/kb/execution/task-template.md` (KB-090) |
