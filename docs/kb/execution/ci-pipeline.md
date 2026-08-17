---
doc_id: KB-087
title: CI Pipeline
module: execution
source_files:
  - .github/workflows/ci.yml
  - ci/warning-baseline.json
  - tools/compare-warnings.sh
  - tools/compare-warnings.ps1
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: complete
confidence: confirmed
last_verified: 2026-08-17
dependencies: [KB-080, KB-083, KB-086, KB-060]
---

# KB-087 — CI Pipeline

Created by **M0-07**, the repository's first continuous-integration pipeline. It closes the CI
half of **R-05** ([KB-060](../risks/technical-debt-register.md#r-05--no-automated-tests-no-ci))
and is the enforcement point every task after it relies on.

> **Read this before editing `.github/workflows/ci.yml`.** Two things in that file look like
> style and are not: `--no-incremental` and `-v normal`. Removing either silently breaks the
> warning gate — see [Counting rule](#counting-rule).

---

## 1. What runs, and when

| | |
|---|---|
| File | `.github/workflows/ci.yml` |
| Triggers | `push` to **every** branch (`branches: ['**']`), and `pull_request` targeting `master` |
| Concurrency | `ci-<ref>`, `cancel-in-progress: true` — a superseded push cancels its predecessor rather than queueing behind it |
| Runner | `windows-latest` (see §2) |
| Job timeout | 45 minutes |
| Permissions | `contents: read` — the workflow reads the repository and nothing else |
| Secrets | **none.** CI does not need a connection string to compile |

Push-on-every-branch is deliberate: `migration/*` branches must be checked **before** review,
not at it.

Steps, in order:

1. **Checkout** (`fetch-depth: 1` — nothing here inspects history).
2. **Hygiene guard** — `bash tools/check-no-build-output.sh` (M0-08's script; this workflow
   only calls it). Runs first because it is instant and there is no point building a tree
   with `bin/` committed into it. Non-zero exit fails the job.
3. **Set up .NET SDK** — `actions/setup-dotnet@v4` with `global-json-file: global.json`.
4. **Report toolchain** — `dotnet --version`, `dotnet --list-sdks`, so every run records what
   actually built it.
5. **Cache NuGet** — keyed on `hashFiles('**/*.csproj', 'global.json')`.
6. **Restore** — its own step, so a NuGet-feed failure is diagnosable at a glance.
7. **Build `V.SMART.Api`** — `--no-restore --no-incremental -v normal`, log captured, `-bl`
   binary log emitted.
8. **Build `V.SMART.Web`** — same flags.
9. **Analyzer warning gate — Api**, then **— Web**. Each against its *own* baseline.
10. *(commented placeholder — M0-12-01's `dotnet test` step goes here, and nowhere else)*
11. **Upload artefacts** (`if: always()`) — both `.log` and `.binlog` files, 14-day retention.

### The exact build commands

```
dotnet restore V.SMART/V.SMART.Api/V.SMART.Api.csproj
dotnet restore V.SMART/V.SMART.Web/V.SMART.Web.csproj

dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj --no-restore --no-incremental -v normal -nologo -bl:artifacts/api.binlog
dotnet build V.SMART/V.SMART.Web/V.SMART.Web.csproj --no-restore --no-incremental -v normal -nologo -bl:artifacts/web.binlog
```

These are [KB-086](M0-15-build-baseline.md) §7's recommended commands (Api and Web separately,
**not** the `.sln`), plus the flags the gate requires:

- `--no-restore` — restore already happened, as its own visible stage.
- `--no-incremental` — **required, not stylistic.** An incremental build reports *fewer*
  warnings than a cold one, so a gate reading an incremental log silently under-reports and
  passes builds it should fail.
- `-v normal` — **required.** The counting rule reads MSBuild's own warning summary block,
  which lower verbosities do not print.
- `-bl` — a binary log, uploaded as an artefact, for the first CI failure nobody can
  reproduce locally.

---

## 2. Runner choice — `windows-latest`

**Decision: `windows-latest`.** Recorded here rather than in an ADR because it is a build-
infrastructure choice, reversible in one line, not an architecture decision.

**Why.** `V.SMART.Shared` multi-targets `net9.0-windows10.0.19041.0;net9.0`
(`V.SMART/V.SMART.Shared/V.SMART.Shared.csproj:4`, Confirmed via KB-086 §2) and the MAUI head
adds a Windows-conditional target (`V.SMART/V.SMART/V.SMART.csproj:5`). The entire warning
baseline in [KB-086](M0-15-build-baseline.md) was measured on Windows. A Linux runner would
produce a different warning population from the same source, which would make the committed
baseline meaningless on day one, and would permanently exclude the Windows target framework
from ever being buildable in CI.

**Cost, stated plainly.** GitHub bills Windows runner minutes at roughly 2x Linux. A cold job
here is minutes, not seconds. If that cost becomes a problem, the honest fix is to move to
Linux *and re-measure the baseline there in the same commit* — never to keep the Windows
baseline and gate a Linux build against it.

**What the runner choice does not fix.** It does not make the MAUI head buildable: hosted
runners ship no MAUI workloads (KB-086 §4, Unknown-by-measurement). See §3.

---

## 3. Known exclusions

| Excluded | Why | Owner / revisit |
|---|---|---|
| `V.SMART/V.SMART` (the MAUI head), and therefore `dotnet build NexGen-ERP---2025-master.sln` | KB-086 §4: the solution build succeeds on a developer workstation **only** with MAUI workloads installed *and* a fully clean `obj/`; a dirty `obj/` produced file-lock errors. Whether a workload-free hosted runner can build it at all is recorded as **Unknown** — untested. **Cost: a regression in the MAUI head is not caught until someone builds the solution locally.** | Revisit when **Q-11** (the MAUI app's future, [KB-004](../open-questions.md)) is answered. If the answer is "keep it", a follow-up task adds a separate job with an explicit `dotnet workload install`. |
| `dotnet test` | There is **no test project in the solution** (INV-023, Confirmed). A test step now is either red or vacuously green, and both teach the team to ignore CI. | **M0-12-01**, at the commented placeholder in `ci.yml`. |
| Any frontend job | No React code exists yet; the Angular pilot at `frontend/vsmart-erp/` is archived by M2-C11. | **M2-C01**. |
| Any deploy / publish / release step | This pipeline compiles; it ships nothing. Deployment topology is **Q-16**, unanswered. | Out of scope until Q-16 is answered. |

---

## 4. The warning baseline

The repository carries ~6,700 build warnings. An analyzer gate on that number has only two
settings unless it is baselined: permanently red, or permanently useless. So the gate compares
against a **committed baseline** and is a **ratchet**.

`ci/warning-baseline.json` records, per build target: the exact command, the error count, the
warning total, and the count for every warning code — plus the counting rule, the runner OS,
the SDK version, the commit sha and the date that produced them.

| Target | Total | Errors |
|---|---|---|
| `V.SMART.Api` | 6,693 | 0 |
| `V.SMART.Web` | 6,695 | 0 |

Web is 2 higher than Api: one Web-only `CS8600`
(`V.SMART/V.SMART.Web/Services/WebFileUploadService.cs:157`, per KB-086 §3) and one `NU1608`.

### Why these differ from KB-086's 6,695 / 6,698

**Confirmed, measured 2026-08-17 in this session, and fully explained** — this is not drift and
not a discrepancy to chase.

KB-086 measured a plain `dotnet build <csproj>`, which *includes* restore. This pipeline runs
restore as its own step and builds with `--no-restore`, which moves the restore-time `NU1608`
warnings out of the build log: Api loses 2 (6,695 → 6,693), Web loses 2 of its 3 (6,698 →
6,695; one `NU1608` is still emitted during Web's build itself). The arithmetic reconciles
exactly: KB-086 §5 buckets ten low-frequency codes including `NU1608` as "remaining, 17"; the
same codes measured here without `NU1608` sum to 15. Every other code matches KB-086 §5
count-for-count.

### Gate semantics (the ratchet)

| Measured vs baseline | Result |
|---|---|
| total **>** baseline total | **FAIL** — prints the delta and every code that increased |
| a warning **code** appears that is absent from the baseline | **FAIL** — prints the code and its count, even if the total did not rise |
| total **<** baseline total | **PASS**, with an explicit `ACTION REQUIRED` instruction to lower the committed baseline |
| total **=** baseline total | **PASS** |

It never uses `-warnaserror`, `/warnaserror` or `<TreatWarningsAsErrors>`, and it never edits a
`.csproj`, `.cs` or `.razor` file. A new *code* fails independently of the total because a new
code is a new **class** of defect, not more of an old one.

### Counting rule

This is the part that makes every future comparison mean something, so it is documented in
three places that must stay in agreement: here, in `ci/warning-baseline.json`
(`counting_rule`), and in the header of both comparison scripts.

**The rule: count only the `: warning <CODE>:` lines that appear *after* MSBuild's
`Build succeeded.` / `Build FAILED.` marker — MSBuild's own warning summary block — and assert
that the resulting total equals the `<n> Warning(s)` figure MSBuild prints in that same block.
If the two disagree, the script exits 2 and fails loudly rather than gate on a number it
cannot explain.**

Rules deliberately rejected, each with the measurement that rejected it (Api build log,
2026-08-17):

| Rejected rule | Why |
|---|---|
| Raw count of ` warning <CODE>` lines | **13,386** — exactly 2x MSBuild's own 6,693. Every diagnostic is printed twice: once inline while the project builds (node-prefixed, e.g. `2>`) and once in the summary block. Confirmed by splitting the log: 6,693 prefixed + 6,693 unprefixed. |
| "Divide the raw count by 2" (KB-086 §5's methodology note) | Reconciles today, but is a magic number: it breaks silently the moment verbosity, node count or logger configuration changes. Correct as an *analysis* technique in KB-086; not safe as a *gate*. |
| De-duplicate by diagnostic text | **Measured wrong.** The same log had **13,310 distinct** warning lines against 13,386 total — some diagnostics legitimately repeat with byte-identical text (up to 3x). Text de-duplication would under-count and let real regressions through. |

---

## 5. How to update the baseline

The baseline is **committed**, so changing it is a reviewed act, never an automatic one. CI
never rewrites it.

**When the gate says the total fell** (the ratchet notice):

1. Download the `build-logs-<run_id>` artefact from the run.
2. Regenerate the block:
   ```
   pwsh tools/compare-warnings.ps1 -LogPath <log> -BaselinePath ci/warning-baseline.json -Target V.SMART.Api -UpdateBaseline
   ```
   or, on Linux/macOS/git-bash:
   ```
   tools/compare-warnings.sh <log> ci/warning-baseline.json V.SMART.Api --update
   ```
3. Paste the printed `total` and `codes` into `ci/warning-baseline.json` under
   `targets.<target>`, and update `provenance` (commit sha, date, SDK, runner OS).
4. Commit on its own, with a message that says which change removed the warnings.

**When a new warning code is genuinely acceptable** (rare — a new analyzer package, a
deliberate SDK upgrade): add the code to `codes` and raise `total` **in its own reviewed
commit whose message explains why**. Never bundle it with the change that introduced it; that
is exactly the review the gate exists to force.

**Formatting is load-bearing.** `tools/compare-warnings.sh` parses the JSON with `awk`, not
`jq`, so it has no dependency beyond a POSIX shell: keep exactly one `"CODE": N` pair per line
inside each `codes` object. `tools/compare-warnings.ps1` uses a real JSON parser and does not
care; the workflow calls the `.ps1`.

---

## 6. How to read a failure

| Failing step | What it means | What to do |
|---|---|---|
| **Hygiene guard** | Build output, IDE state or a dependency directory is **tracked by git**. The script lists every offending path. | `git rm --cached -r <path>` — never a bare `git rm`, the content must stay on disk. See KB-060 R-14. |
| **Restore** | A NuGet feed, a package version conflict, or an SDK the runner could not resolve. | Read the step's own output; it is isolated from build noise on purpose. |
| **Build** | A genuine compile error. `0 errors` is the baseline, so any error is a regression. | Download the `.binlog` artefact and open it with the MSBuild Structured Log Viewer. |
| **Analyzer gate — `FAIL: warning code(s) not present in the baseline`** | Your change introduced a *kind* of warning the codebase did not have. | Fix it. Do not add the code to the baseline to get past the gate unless you can justify it in a review. |
| **Analyzer gate — `FAIL: warning total rose above the baseline`** | More of an existing warning kind. The output lists every code that increased and by how much. | Fix the new occurrences. The delta is usually small and points straight at the diff. |
| **Analyzer gate — exit 2, `COUNTING RULE BROKEN`** | The script's parsed total disagrees with MSBuild's own `<n> Warning(s)`. Something about the log format changed (verbosity, logger, SDK). | **Do not "fix" it by loosening the gate.** Re-establish the counting rule, update it in the script *and* in `ci/warning-baseline.json`, and say so in the commit. |
| **Analyzer gate — `PASS ... ACTION REQUIRED`** | Warnings *fell*. The build is green. | Lower the committed baseline (§5) so the improvement cannot be silently undone. |

---

## 7. Evidence

Every result below was **observed** in the M0-07 execution session on 2026-08-17, on the
developer workstation (Windows 11, OS Version 10.0.26200, SDK 10.0.400, commit
`8b67f3de0752814d3dee25a687d02d4cadeb0f46`). What was **not** observed is recorded in §8 —
read both.

### The gate fails on a deliberately introduced warning

`#warning M0-07 gate test -- temporary, reverted in the same session` was inserted as the
first line of `V.SMART/V.SMART.Api/Program.cs`, the project rebuilt with the workflow's exact
flags, and the gate run against the resulting log:

```
=== Analyzer warning gate -- V.SMART.Api ===
baseline   : ci/warning-baseline.json (total 6693)
measured   : 6694

=== FAIL -- warning code(s) not present in the baseline ===
  CS1030  x1   (baseline: absent)

=== FAIL -- warning total rose above the baseline ===
  baseline 6693, measured 6694, delta +1

Codes that increased:
  CS1030  0 -> 1  (+1)

=== Gate: FAILED ===
```

Exit code **1**, from both `compare-warnings.sh` and `compare-warnings.ps1`. The failure
**names the new code** (`CS1030`) and its count. The `#warning` line was then removed and
`git status` confirmed `V.SMART/V.SMART.Api/Program.cs` byte-identical to `HEAD`; a rebuild
returned to 6,693 and the gate passed.

### The ratchet tolerates a decrease

With the committed baseline total temporarily raised to 6,694 against a measured 6,693, both
scripts exited **0** and printed:

```
=== PASS -- and the baseline is now stale (ratchet) ===
  baseline 6694, measured 6693, delta -1

ACTION REQUIRED: lower the committed baseline so the improvement cannot be undone.
```

### A rise in the total fails even with no new code

Baseline total temporarily lowered to 6,688 against a measured 6,693: exit **1**,
`FAIL -- warning total rose above the baseline ... delta +5`.

### Idempotence (locally)

Two consecutive `--no-incremental` builds of `V.SMART.Api` on the same commit reported
**6,693 warnings, 0 errors** both times, and the gate passed both times. KB-086 §3 independently
observed the same reproducibility across two clean runs.

### Both script variants agree

`compare-warnings.sh` and `compare-warnings.ps1` were run against the same four logs and
produced identical verdicts and exit codes in every case.

---

## 8. What has NOT been verified — read this before trusting the numbers

The M0-07 execution session **could not push a branch** (pushing is not permitted from an
execution session), so **no GitHub Actions run has ever executed**. Everything in §7 was
verified by running the workflow's exact commands and the exact gate scripts locally. That
proves the logic. It does not prove the runner.

Consequently, and honestly:

| Not verified | Consequence | Resolves when |
|---|---|---|
| The workflow has never run on a GitHub-hosted runner | Syntax parses (validated with a YAML parser) and the commands are the measured ones, but no run URL exists | The branch is pushed |
| `ci/warning-baseline.json` was produced **on a developer workstation**, not on the runner | It is marked `"provisional": true`, `"must_be_regenerated_on_runner": true`. M0-07 specifies the runner's number is the one CI gates against | First green run — replace the `targets` blocks, set `provisional: false`, and record the local-vs-runner delta here and in INV-029 |
| Whether a workload-free hosted runner can build `V.SMART.Api` / `V.SMART.Web` at all | KB-086 §4 records this as **Unknown**. Neither project targets a MAUI framework, so it is *expected* to work — expected, not confirmed | First run |
| Whether the runner reproduces 6,693 / 6,695 | If it differs, **the runner's number becomes the baseline** and the delta is a finding | First run |
| CI green on `master` | `master` has no workflow until this branch is merged | Merge (not done by an execution session) |
| The CI check as a **required status check** on `master` | Needs GitHub admin rights, which an execution session does not have. M0-00 deliberately left this open because no check existed to require; it stays open until an admin adds `Restore, build and gate analyzer warnings` to `master`'s protection **after** the first green run | A human with admin rights |

**Rollback ordering, if this ever has to be undone:** remove the required status check
**before** deleting the workflow. Do it the other way round and `master` becomes unmergeable,
waiting forever for a check that can never report.

---

## 9. Who extends this workflow next

In order:

| Task | What it adds |
|---|---|
| **M0-12-01** | The first test project, and the `dotnet test` step at the commented placeholder. Also updates KB-083's "Test commands — do not use yet" section in the same commit |
| **M2-B10** | OpenAPI polish and TypeScript client generation, verified in CI |
| **M2-A03** | The permission-matrix harness, as a merge-blocking gate |
| **M2-C01** | A frontend job — Vite + React, lint and test |

Anyone adding a job should keep the existing invariants: no secrets, no deployment, no
`-warnaserror`, and every new gate baselined before it is enforced.
