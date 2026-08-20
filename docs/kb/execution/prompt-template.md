---
doc_id: KB-083
title: Execution Prompt, Generation Rules and Verified Commands
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-20
dependencies: [KB-080, KB-002, KB-003, KB-005, KB-088, KB-090]
---

# Execution Prompt, Generation Rules and Verified Commands

This document holds three things: **the session prompt**, the **generation rules** that keep
task files honest, and the **verified repository commands** table — the single authoritative
list of what actually builds and tests in this repository.

## The execution prompt

The whole prompt for a fresh session is:

```text
Read CLAUDE.md and docs/kb/execution/current-task.md.
Execute the current task according to the repository's migration workflow.
When it closes, pick the next task that can actually be done — see CLAUDE.md § Standing constraints for the five-part test.
```

Nothing is pasted. No previous prompt, no conversation history, no architecture recap. If a
session cannot proceed from those three lines, the **repository** is missing something, and
fixing that is part of the work.

- Invariant context — architecture, authority order, standing constraints:
  [`CLAUDE.md`](../../../CLAUDE.md) at the repository root.
- The active task: [`current-task.md`](current-task.md) (KB-089).
- The procedure: [`workflow.md`](workflow.md) (KB-088).

### The superseded model

Every task file under `tasks/` written before 2026-08-16 ends with a **"Fresh-Session
Execution Prompt"** block — ~150 lines restating the project objective, architecture,
source-of-truth rules, anti-repetition clause, constraints, execution procedure and final
report format, identically, in all 105 files.

**Those blocks are obsolete.** `CLAUDE.md` and `current-task.md` now supply everything they
restated, once, instead of 105 times. When opening an existing task file, read its
specification sections and **skip the trailing prompt block**. New task files are written to
[`task-template.md`](task-template.md) (KB-090), which has no such block.

The existing files are not being rewritten for this alone — the specifications above the
block remain accurate and several carry Execution Records worth keeping.

## The operating model this serves

```
ONE TASK → ONE FRESH SESSION → READ CLAUDE.md + current-task.md → CHECK REGISTRY
        → INVESTIGATE ONLY IF NEEDED → IMPLEMENT ONLY THAT TASK → TEST → DOCUMENT
        → COMMIT → HAND OVER current-task.md → STOP
```

The executing session's persistent context is **the repository**. Nothing else. A task that
cannot be executed from the repository alone is a defective task.

## Generation rules

These are binding on whoever writes or regenerates a **task file**
([`task-template.md`](task-template.md)) — and were binding on the prompts that preceded them.

1. **No placeholders.** Every `<…>` in the template is replaced with the task's real value.
   A task file containing an unfilled placeholder is not shippable.
2. **Never invent a path.** Every file, directory, service, and command named in a task file
   must exist in the repository at generation time, or be explicitly marked
   `TO BE CREATED`. Verify before writing.
3. **Cite `file:line` for behaviour claims.** "Login swallows exceptions" is not evidence;
   `V.SMART/V.SMART.Shared/Repository/MasterRepository/Admins/UserRepository.cs:44-48` is.
4. **Classify every factual claim** as `Confirmed` / `Inferred` / `Unknown`, per
   [KB-002](../source-of-truth-rules.md). Never state an inference as fact.
5. **Task files are generated when their inputs exist.** M0 and M2 task files are written
   now because their inputs are the current repository. Module-wave files (M3/M4) are
   generated **at the start of their wave**, after that wave's `INV-0xx` business-rule
   investigation completes — because the *Business Rules* section is that investigation's
   output. Writing them earlier would mean inventing rules, which violates rule 4 and the
   project's core constraint. See [KB-080 §11](README.md#11-m3--core-modules).
6. **One task per file. One task, one branch.** *(Updated 2026-08-20: a session may now run several tasks; each still gets its own branch from `master`.)* Branch-per-task is what
   makes each unit independently reviewable and reversible — not the one-session limit that preceded it.
7. **Regenerate, don't patch.** If a task's scope changes, regenerate the whole file and bump
   `last_verified`. Half-edited files drift from the task they describe.
8. **Reference, never duplicate.** A task file that restates a business rule, an ADR or an
   architecture section instead of linking to it goes stale silently — and then it lies. This
   rule is why the per-task prompt preamble was removed: 105 copies of the same paragraph
   cannot be kept true.

## Anti-repetition clause

This is now stated once, in [`CLAUDE.md`](../../../CLAUDE.md) and
[KB-088 §2](workflow.md#2-starting-a-session), rather than copied into every task. It binds
every session, because the same repository is worked on by many independent sessions that
cannot see each other:

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

## The legacy prompt template — historical

> **Superseded 2026-08-16.** This block is retained so that the ~105 task files containing a
> copy of it remain interpretable, and so the reasoning behind each section is not lost. **Do
> not generate new prompts from it.** New task files use
> [`task-template.md`](task-template.md) (KB-090); the session prompt is the three lines at
> the top of this document.
>
> Where each section went: the *Role* / *Project Objective* / *Current Architecture* /
> *Source of Truth* / *Constraints* / *Execution Procedure* preamble is now
> [`CLAUDE.md`](../../../CLAUDE.md); *Current Task* … *Acceptance Criteria* is now
> [`current-task.md`](current-task.md) plus the task file; the *Final Response* format is
> [KB-084](review-templates.md).

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
- When this task closes, pick the next task that can actually be done (CLAUDE.md § Standing constraints defines the test). Never merge or push.
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
WHEN THIS TASK CLOSES, PICK THE NEXT TASK THAT CAN ACTUALLY BE DONE — prerequisites Completed AND MERGED, not a Product Decision, not blocked on an open question, task file not marked superseded, no sibling branch on the same files. NEVER MERGE OR PUSH.
============================================================
```

## Verified repository commands

Confirmed as of 2026-08-17 (M0-15; see [KB-086](M0-15-build-baseline.md) for full detail,
methodology and reproducibility evidence). Do not put an unverified command in a prompt.

| Purpose | Command | Verified result |
|---|---|---|
| Build the API and its dependencies | `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` | 0 errors, 6,695 warnings, ~1m23s–2m27s (reproducible x2, KB-086 §3). Re-measured 2026-08-19 (M2-B07 attempt 2, on that task's branch, `--no-incremental`): **0 errors, 6,694 warnings, 1m04s** |
| Build the Blazor host | `dotnet build V.SMART/V.SMART.Web/V.SMART.Web.csproj` | 0 errors, 6,698 warnings, ~1m19s–1m20s (reproducible x2, KB-086 §3). Re-measured 2026-08-18 (M0-03-03, on that task's branch, `--no-incremental`): **0 errors, 6,697 warnings, 1m18s** (second run 1m01.86s, same counts). A warm incremental run of the same command took ~6s and reported only the 5 warnings belonging to `V.SMART.Web` itself, the rest coming from `V.SMART.Shared`, which was already built. Read a low warning count as "incremental", not "improved". Re-measured 2026-08-19 (M2-B07 attempt 2, `--no-incremental`): **0 errors, 6,697 warnings, 1m04s** — same count as 2026-08-18, so this is a reproduced baseline, not "not yet measured" |
| Build the whole solution | `dotnet build NexGen-ERP---2025-master.sln` | 0 errors, 13,367 warnings, ~4m7s–4m16s **on this machine, from a clean `obj`** (reproducible x2). A dirty `obj` produced 2 file-lock/permission errors unrelated to code. Whether it succeeds on a workload-free CI runner is **Unknown** — untested (KB-086 §4). **Not recommended for CI** — see KB-086 §7. |
| Build the MAUI head | `dotnet build V.SMART/V.SMART/V.SMART.csproj` | **0 errors, 6,671 warnings, 1m57s** (measured 2026-08-19, M2-B07 attempt 2, on that task's branch, warm/incremental — **not** `--no-incremental`). Four target frameworks: `net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`, `net9.0-windows10.0.19041.0`. This is a **single** observation, on a machine with the MAUI workloads installed; it does not establish that a workload-free CI runner can build it. An earlier attempt on 2026-08-19 failed with `MSB6006: "java.exe" exited with code 143` on the Android target — code 143 is SIGTERM, i.e. that session's own 180s timeout killing the D8 dexing step, not a code defect. Give it 5+ minutes cold |
| **The CI build command** (M0-07) | `dotnet restore <csproj>` then `dotnet build <csproj> --no-restore --no-incremental -v normal -nologo -bl:<path>.binlog`, run for `V.SMART.Api` and `V.SMART.Web` | Api: 0 errors, **6,693** warnings; Web: 0 errors, **6,695** warnings (measured 2026-08-17, M0-07, locally — 2 and 3 lower than the plain-`dotnet build` figures above because separating restore moves the `NU1608` restore warnings out of the build log; see [KB-087](ci-pipeline.md) §4). `--no-incremental` and `-v normal` are **required** by the warning gate, not stylistic |
| Analyzer warning gate (M0-07) | `pwsh tools/compare-warnings.ps1 -LogPath <log> -BaselinePath ci/warning-baseline.json -Target <V.SMART.Api\|V.SMART.Web>` (POSIX sibling: `tools/compare-warnings.sh <log> <baseline> <target>`) | exit 0 = at/below baseline, 1 = gate failure, 2 = the measurement could not be trusted. Verified locally 2026-08-17 on four logs, both variants agreeing. See [KB-087](ci-pipeline.md) |
| **Run the tests** (M0-12-01) | `dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj` | **11 tests discovered, 11 passed, 0 failed, 0 skipped**; test execution 9s, whole command ~15s warm (measured 2026-08-19, M0-12-01, locally, SDK 10.0.400). Do **not** add `--no-build` or `--no-restore`: this is the only project that restores it. It is no longer the repository's only test project (`tests/V.SMART.Api.Tests` was added by M2-A06, 2026-08-20), and it no longer reports 11: **84 tests discovered, 84 passed, 0 failed** when re-run on 2026-08-20, the growth coming from tasks that landed after M0-12-01. Treat the 11 as the M0-12-01 measurement, not as a current expectation |
| **Run the API tests** (M2-A06) | `dotnet test tests/V.SMART.Api.Tests/V.SMART.Api.Tests.csproj` | **21 tests discovered, 21 passed, 0 failed, 0 skipped**; test execution ~0.1s, whole command ~20s warm (measured 2026-08-20, M2-A06, locally, on branch `migration/M2-A06-problem-details`). Project created by M2-A06, not by M2-A02 as earlier planned — M2-A02 had not landed. No database and no host: the error contract is exercised over `DefaultHttpContext` and controllers with stand-in services. Like the Shared suite, do **not** pass `--no-build`/`--no-restore`. **Not yet wired into `NexGen-ERP---2025-master.sln` or `.github/workflows/ci.yml`** — M2-A06 kept its diff to the error contract; whoever next touches the CI test step should add this project beside the Shared one, or it runs only locally. Re-measured 2026-08-20 on `migration/M2-A01-02-require-screen-right`: **104 tests discovered, 104 passed, 0 failed, 0 skipped**, execution 123 ms — the growth is M2-A01-02's filter and startup-validator suites. Re-measured 2026-08-20 on `migration/M2-A01-03-rights-cache` after the attempt-1 repair: **117 tests discovered, 117 passed, 0 failed, 0 skipped**, execution ~1 s — the growth is M2-A01-03's 13 cache tests, and the ~1 s is the one deliberate wall-clock test that watches a 1-second TTL expire. Re-measured 2026-08-20 on `migration/M2-A07-me-endpoint`: **148 tests discovered, 148 passed, 0 failed, 0 skipped**, execution 1 s — the growth is M2-A07's 31 `/api/v1/me` tests. **Still controller-level only**: the project has no `Microsoft.AspNetCore.Mvc.Testing` reference and no host, so nothing here asserts an HTTP status over the wire (R-43) |
| Working-tree state | `git status --porcelain` | 0 entries (or only the by-design-untracked `V.SMART/V.SMART.Api/`) as of 2026-08-17, after M0-00 |
| Search committed history | `git grep -l "<pattern>" HEAD` | works |

**Solution-file note (Confirmed, M0-00/M0-15).** `NexGen-ERP---2025-master.sln` is now
**tracked** (resolved by M0-00, commit `d83e2ea`) and lists exactly 4 projects: `V.SMART.Shared`,
`V.SMART` (MAUI), `V.SMART.Web`, `V.SMART.Api`. The earlier untracked-`.sln` risk this note
used to describe is resolved.

**Toolchain note (Confirmed, M0-15, 2026-08-17).** Projects target `net9.0`; the installed
SDKs are `10.0.300` and `10.0.400` (drifted from `10.0.300`/`10.0.302` recorded by INV-029 on
2026-08-12, on the same machine, with no repository change — see KB-086 §1). The build
succeeds through SDK roll-forward. A root `global.json` now pins the SDK to `10.0.400` with
`rollForward: latestFeature` (KB-086 §6) — a prompt may assume this pin exists, but should
still not assume `dotnet --version` reports 9.x.

**Local dev database exists on this workstation (Confirmed, M2-B07 close-out, 2026-08-19) —
three prior sessions wrongly concluded otherwise.** SQL Server Express instance
`DESKTOP-FIIBE97\SQLEXPRESS` carries a `NexGenErpDb_Master` master database and at least one
197-table tenant database, resolvable via the master DB's `Tenants` table by
`Hostname='localhost'`. Pointing `ConnectionStrings__MasterDb` at it and starting
`V.SMART.Web` renders `/` at `200` with zero DI resolution errors. Do **not** put the `sa` or
any other credential in this file, in a prompt, or anywhere in the repository — the password
stays in the database/OS credential store only. A session needing this database should ask a
human to confirm the connection string is still valid before relying on it; this note records
the *coordinates* found working on one date, not a guarantee they still are.

**Warning baseline (Confirmed, KB-086 §5).** 6,695 warnings on the Api build. The dominant
codes are the `CS86xx` nullable-reference-analysis family (`CS8602` alone is 23.6% of the
total) — **not** `MUD0002` as previously described; `MUD0002` is 130 occurrences, 1.94% of the
total. CI (M0-07) must record this baseline and fail on *new* warnings — it cannot use
`-warnaserror` until the baseline is cleared.

## Verified frontend commands

> **⚠ These rows describe the React app, which [`ADR-007`](../decisions/ADR-007-angular-stack.md)
> superseded on 2026-08-20.** They are **still factually correct** — every command below was
> measured and still runs today — so they are kept rather than deleted, per this table's own rule
> that it records observed results and never predictions.
>
> **Do not treat them as the frontend contract.** The stack is Angular + PrimeNG. When the
> re-scoped `M2-C01` scaffolds the Angular app, it replaces this whole section with measured
> Angular CLI commands and deletes these rows in the same change — **not before**, because until
> then removing them would make this document silently wrong in the other direction.
>
> The Angular pilot at `frontend/vsmart-erp/` still has no verified command here; `M2-C11`
> (re-scoped from *archive* to *adopt as baseline*) is where it acquires one.

Added by **M2-C01**, which created `frontend/nexgen-web/` — the repository's first React
project. Before it, this document had **no** frontend row of any kind; the only other
JavaScript tree, `frontend/vsmart-erp/`, was then the archived Angular pilot and has never had a
verified command here.

Every command below was **run locally on 2026-08-19** on this Windows workstation
(`node v24.19.0`, `npm 11.17.0`) from `frontend/nexgen-web/`, after `rm -rf node_modules`
followed by `npm ci`. Exit codes were observed, not assumed.

| Purpose | Command (from `frontend/nexgen-web/`) | Verified result |
|---|---|---|
| Install exactly what the lockfile pins | `npm ci` | exit 0; **554 packages in 23s** from an empty `node_modules/`. Two `allow-scripts` warnings (`esbuild`, `msw` postinstall not auto-approved by npm 11) — **not** failures: the platform binary `@esbuild/win32-x64` is a normal optional dependency and is present, and every build/test below passes without approving them |
| Typecheck | `npm run typecheck` | exit 0, no output. Runs `tsc --noEmit` twice — `tsconfig.json` (`src/` + `e2e/`) and `tsconfig.node.json` (root config files) — because the two need different `lib`/`types` |
| Lint | `npm run lint` | exit 0, no output. `eslint . --max-warnings=0`, type-aware `typescript-eslint`, `react-hooks`, `jsx-a11y`, `simple-import-sort`, plus the two ADR-003 `no-restricted-imports` rules |
| Format check | `npm run format:check` | exit 0 — "All matched files use Prettier code style!" Re-observed 2026-08-19 after the correction noted below; the row as first written was not observed |
| Unit tests | `npm run test -- --run` | exit 0 — **1 test file, 1 test passed**, ~30s cold (jsdom environment setup dominates: 20.4s), ~3.7s warm |
| Coverage | `npm run coverage` | exit 0 — statements **82.89 %**, branches **100 %**, functions **80 %**, lines **82.89 %**. `vitest.config.ts` thresholds are set to the floor of those numbers, so they can only be raised. **Re-observed 2026-08-20** on `migration/M2-C04-01-design-tokens` after the theme layer landed: exit 0 — statements **95.90 %**, branches **100 %**, functions **86.95 %**, lines **95.90 %**, 150 tests. Thresholds unchanged; the gate held and the measured numbers rose |
| Production build | `npm run build` | exit 0 — typecheck then `vite build`, 830 modules, **3.56s**. Entry chunk `assets/index-*.js` 289.69 kB raw / **90.90 kB gzip**; vendor `react` chunk 102.50 kB / 34.48 kB gzip; Mantine CSS 201.38 kB / 29.30 kB gzip. Initial JS gzip **125.38 kB** against KB-050's `< 250 KB gzip` budget |
| E2E smoke | `npm run e2e` | exit 0 — **1 passed (6.2s)**, chromium. Playwright starts the Vite dev server itself. Requires `npx playwright install chromium` once per machine; that download succeeded here |

**Correction, 2026-08-19 (M2-C01 attempt 1 → attempt 2).** The `Format check` row above was
originally committed (`4ac7241`) claiming `exit 0` when the command actually exited **1** on
that tree: `frontend/nexgen-web/README.md` was edited after the last `npm run format` and
carried un-normalised markdown (`*emphasis*` rather than `_emphasis_`, unaligned table pipes).
Validation caught it, `npm run format` was run, and the row was then re-observed as stated.
Recorded here rather than silently overwritten, because this table's only value is that its
rows were run — see [`failure-log.md`](failure-log.md) (KB-092), M2-C01 attempt 1.

**`npm ci`, never `npm install`, in CI.** `package-lock.json` is committed, and it carries the
Linux optional binaries for `rollup` and `esbuild` as well as the Windows ones — so moving the
CI job to `ubuntu-latest` later is available, though M2-C01 deliberately did not take it (the
job's first ever run should not also be its first run on an unverified platform).

**Node version, stated plainly (M2-C01 deviation).** `.nvmrc` pins **22** and CI resolves from
it, but `package.json` declares `"engines": { "node": ">=22" }` with an **open upper bound**,
not the `>=22 <23` the task file asked for. The only Node on this workstation is **24.19.0**,
and this task's definition of done is that the commands above are *verified* — which can only
be done on the Node actually installed. A closed `<23` range would have made every local
`npm` command emit `EBADENGINE`, and an outright failure under `engine-strict`. Nothing above
was verified on Node 22.

**Not yet verified (do not put these in a prompt):** `npm run dev` and `npm run preview` as
CI-checkable commands (they are long-running servers; only the Playwright `webServer` path
exercises `dev`), and any frontend command on a GitHub-hosted runner — the `frontend` and
`frontend-e2e` jobs added to `.github/workflows/ci.yml` by M2-C01 have **never executed**,
because an execution session may not push.

## Test commands

Superseded 2026-08-19 by **M0-12-01**, which created the repository's first test project.
The command is now in the *Verified repository commands* table above; the row is the
authority, this section only explains it.

```bash
dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj
```

**Measured 2026-08-19 (M0-12-01, locally, SDK 10.0.400):** 11 tests discovered, 11 passed,
0 failed, 0 skipped. Test execution 9s; the whole command ~15s warm. It restores and builds
the test project itself — **do not** pass `--no-build` or `--no-restore`, because neither the
CI restore step nor any other project restores this one.

**What the 11 tests actually cover, so no prompt overclaims them (Confirmed):** smoke and
harness only. One trivially-true discovery test, one assembly-loadability test, one
`CalculationService.UpdateTotalsAsync` call asserting only that `GrandTotal` moved off its
default, two test-double construction tests, and six EF-fixture tests that pin the INV-031
findings. **There is no business-behaviour coverage yet.** Real assertions arrive with
M0-12-02 (`CalculationService`) and M0-13 (`StockManagerService`). Risk R-05
([KB-060](../risks/technical-debt-register.md)) is *not* closed by this.

The scope is `tests/V.SMART.Shared.Tests` only. `dotnet test NexGen-ERP---2025-master.sln`
is **unverified** and inherits the solution build's MAUI-workload problem — do not use it.
