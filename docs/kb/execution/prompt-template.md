---
doc_id: KB-083
title: Fresh-Session Execution Prompt — Canonical Template and Generation Rules
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-12
dependencies: [KB-080, KB-002, KB-003, KB-005]
---

# Fresh-Session Execution Prompt — Template and Generation Rules

Every task in [KB-080](README.md) carries a copy-paste prompt for an AI session that has
**never seen any prior conversation**. This document is the canonical template those prompts
are generated from, plus the rules that keep them honest.

## The operating model this serves

```
ONE TASK → ONE FRESH SESSION → READ KB → CHECK REGISTRY → INVESTIGATE ONLY IF NEEDED
        → IMPLEMENT ONLY THAT TASK → TEST → DOCUMENT → COMMIT → REVIEW → STOP
```

The executing session's persistent context is **the repository + `docs/kb/` + the task
prompt**. Nothing else. A prompt that cannot be executed from those three things alone is a
defective prompt.

## Generation rules

These are binding on whoever writes or regenerates a prompt.

1. **No placeholders.** Every `<…>` in the template is replaced with the task's real value.
   A prompt containing an unfilled placeholder is not shippable.
2. **Never invent a path.** Every file, directory, service, and command named in a prompt
   must exist in the repository at generation time, or be explicitly marked
   `TO BE CREATED`. Verify before writing.
3. **Cite `file:line` for behaviour claims.** "Login swallows exceptions" is not evidence;
   `V.SMART/V.SMART.Shared/Repository/MasterRepository/Admins/UserRepository.cs:44-48` is.
4. **Classify every factual claim** as `Confirmed` / `Inferred` / `Unknown`, per
   [KB-002](../source-of-truth-rules.md). Never state an inference as fact.
5. **Prompts are generated when their inputs exist.** M0 and M2 prompts are written now
   because their inputs are the current repository. Module-wave prompts (M3/M4) are
   generated **at the start of their wave**, after that wave's `INV-0xx` business-rule
   investigation completes — because the prompt's *Business Rules to Preserve* section is
   that investigation's output. Writing them earlier would mean inventing rules, which
   violates rule 4 and the project's core constraint. See
   [KB-080 §11](README.md#11-m3--core-modules).
6. **One task per prompt. One session per prompt.** The closing instruction forbidding the
   next task is not decoration — it is what makes each unit independently reviewable and
   reversible.
7. **Regenerate, don't patch.** If a task's scope changes, regenerate its whole prompt file
   and bump `last_verified`. Half-edited prompts drift from the task spec above them.

## Anti-repetition clause (mandatory in every prompt)

Every prompt must carry this verbatim, because the same repository will be worked on by many
independent sessions that cannot see each other:

> Before investigating the repository, search `docs/kb/investigation-registry.md` and the
> relevant knowledge-base documents via `docs/kb/INDEX.md`. If an investigation is
> **Complete** and not stale, reuse its findings and cite the `doc_id` — do not re-derive
> them. If it is **Partial**, investigate only the documented gap. If it is absent or
> contradicted by current code, investigate, then record the finding, its `file:line`
> evidence, and its confidence in the knowledge base so that future sessions do not repeat
> this work. Record negative results too — "grepped for X, found none" is a finding.

## Evidence format (mandatory for new findings)

```yaml
Finding:        <one sentence>
Evidence:       <path:line-range>
Business rule:  <BR-xxx-nnn or "n/a">
Confidence:     Confirmed | Inferred | Unknown
Last verified:  YYYY-MM-DD
```

---

## The template

Everything between the rules below is copied into each task file, with values substituted.

```text
============================================================
ERP MIGRATION — TASK EXECUTION PROMPT
============================================================

TASK ID:
<task id, e.g. M0-03>

TASK NAME:
<task name>

ROLE
You are an engineer working on the V.SMART / NexGen ERP
modernization project. You are executing exactly one task.

PROJECT OBJECTIVE
We are replacing the existing Blazor Server frontend with a new
React frontend while preserving the existing ERP business
behaviour, business services, database behaviour and business
rules wherever possible. The backend is extended, never
rewritten. Business logic currently trapped in Razor @code is
extracted into server-side services before any React screen
replaces it.

CURRENT ARCHITECTURE
Repository root: C:\Kumar\NexGen-ERP---2025-master\NexGen-ERP---2025-master
(this is the git repository root; the parent directory is not
part of the project)

  V.SMART/V.SMART.Shared   .NET 9 class library — ALL domain code:
                           196 EF entity sets, 285 business services,
                           ~190 repositories + UnitOfWork, 274 ViewModels,
                           333 Razor pages, 440 routes
  V.SMART/V.SMART.Web      Blazor Server host (live UI, stays running)
  V.SMART/V.SMART.Api      ASP.NET Core Web API (.NET 9) — the React
                           backend; currently 2 controllers / 6 endpoints
  V.SMART/V.SMART          .NET MAUI Blazor Hybrid host

SQL Server + EF Core 9, code-first, database-per-tenant.
<task-specific architecture context>

SOURCE OF TRUTH
The project knowledge base is at docs/kb/. Read it before
investigating the repository. Use docs/kb/INDEX.md for
question-to-document routing.

Required reading for this task:
<exact doc_ids and paths>

Authority order when sources conflict:
  1. Current source code
  2. Database schema / EF migrations (for storage)
  3. The knowledge base (for interpretation)
  4. Older prose documentation — hypothesis only
docs/ARCHITECTURE.md is superseded and contains known factual
errors. Do not rely on it.

INVESTIGATION REGISTRY
Before any repository investigation, search
docs/kb/investigation-registry.md.

Relevant investigations:
<INV ids with status>

If an investigation is Complete and not stale, reuse its
findings and cite the doc_id. If Partial, investigate only the
documented gap. If absent or contradicted by current code,
investigate and then record the finding with file:line evidence
and a Confirmed/Inferred/Unknown confidence rating. Record
negative results too.

CURRENT TASK
<complete task objective>

WHY THIS TASK EXISTS
<task-specific explanation, including the risk id (R-xx),
gap id (A1/B5/…) or gate it serves>

PREREQUISITES
<task ids, or "None">

CURRENT IMPLEMENTATION
<verified existing behaviour with file:line evidence and
confidence classification>

TARGET IMPLEMENTATION
<target end state>

RELEVANT FILES
<actual verified paths>

BUSINESS RULES TO PRESERVE
<BR ids + statement + file:line evidence, or "None — this task
does not touch business behaviour">

CONSTRAINTS
- Do not rewrite existing business services.
- Do not reimplement ERP business logic in React/TypeScript.
- Do not change unrelated modules.
- Do not make assumptions without checking source code.
- Do not repeat completed investigations.
- Do not modify the database schema unless this task explicitly
  authorizes it.
- Preserve existing API behaviour wherever practical.
- The server remains authoritative for validation, calculations,
  permissions and document numbering.
- Do not start another task after completing this one.
<task-specific constraints>

EXECUTION PROCEDURE
1. Read the referenced knowledge-base documents.
2. Search the investigation registry.
3. Confirm whether the required investigation already exists.
4. Inspect the actual source code.
5. Verify every assumption against code before acting on it.
6. Implement only this task.
7. Run the required verification commands and tests.
8. Review the full git diff.
9. Update documentation.
10. Update the investigation registry if required.

EXPECTED FILE CHANGES
Modified:  <files>
Created:   <files>
Must not change: <protected areas>

TESTS
<tests, with the exact commands>

ACCEPTANCE CRITERIA
<objectively verifiable criteria>

VERIFICATION COMMANDS
<real repository commands only>

DOCUMENTATION REQUIREMENTS
<exact docs to update, including frontmatter fields>

GIT
Branch:  <branch>
Commit:  <commit subject>
Do not merge. Do not push to master. Leave the branch for review.

FINAL RESPONSE
When finished, report:
1.  Task ID
2.  Task status (Completed / Needs Review / Blocked)
3.  What was implemented
4.  Files created
5.  Files modified
6.  Files deleted
7.  Tests executed
8.  Test results
9.  Documentation updated
10. Investigation registry updated
11. Architectural decisions taken
12. Unexpected findings
13. Assumptions made
14. Deviations from this task
15. Recommended next task

IMPORTANT:
EXECUTE ONLY THIS TASK.
DO NOT START THE NEXT TASK.
============================================================
```

## Verified repository commands

Only these have been confirmed to work in this repository as of 2026-08-12
(INV-029). Do not put an unverified command in a prompt.

| Purpose | Command | Verified result |
|---|---|---|
| Build the API and its dependencies | `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` | 0 errors, 6,695 warnings, ~3 min |
| Build the Blazor host | `dotnet build V.SMART/V.SMART.Web/V.SMART.Web.csproj` | not yet measured |
| Build the whole solution | `dotnet build NexGen-ERP---2025-master.sln` | **not verified** — includes the MAUI head, which needs workloads. Also see the solution-file warning below |
| Working-tree state | `git status --porcelain` | 37 entries as of 2026-08-12 |
| Search committed history | `git grep -l "<pattern>" HEAD` | works |

**Solution-file warning (Confirmed, INV-029).** The solution on disk,
`NexGen-ERP---2025-master.sln`, is **untracked**. The only `.sln` in `HEAD` is
`Bhargavi V.SMART ERP - 2025.sln`, which is **deleted** in the working tree. So the file
every build command names is not in source control, and a fresh clone gets a different one
whose validity is **Unknown**. Until **M0-00** resolves this, prefer per-project build
commands (`dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj`) over solution-level ones.

**Toolchain note (Confirmed, INV-029).** Projects target `net9.0`; only the .NET **10**
SDK is installed (10.0.300, 10.0.302). The build succeeds through SDK roll-forward. Any
prompt that pins an SDK version, or assumes `dotnet --version` reports 9.x, is wrong.

**Warning baseline (Confirmed).** 6,695 warnings, largely `MUD0002` MudBlazor analyzer
warnings. CI (M0-07) must record this baseline and fail on *new* warnings — it cannot use
`-warnaserror` until the baseline is cleared.

## Test commands — do not use yet

There is **no test project in the solution** (INV-023, Confirmed). `dotnet test` will find
nothing. The first test project is created by **M0-12**. Until M0-12 lands, no prompt may
list `dotnet test` as a verification command. After it lands, this table must be updated
here so that every later prompt inherits the correct command.
