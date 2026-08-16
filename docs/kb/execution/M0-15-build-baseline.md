---
doc_id: KB-104
title: Build and Toolchain Baseline
module: execution
source_files:
  - NexGen-ERP---2025-master.sln
  - V.SMART/V.SMART.Shared/V.SMART.Shared.csproj
  - V.SMART/V.SMART.Web/V.SMART.Web.csproj
  - V.SMART/V.SMART/V.SMART.csproj
  - tools/measure-build-baseline.ps1
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: partial
confidence: mixed
last_verified: 2026-08-13
dependencies: [KB-080, KB-083, KB-010]
---

# Build and Toolchain Baseline

Task [M0-15](tasks/M0-15.md). **Status: half A complete, half B pending a measurement run.**

This task splits the way [M0-01-02](tasks/M0-01-02.md) does, for the same reason: part of it
is repository analysis, and part needs a capability the authoring environment lacks. Here the
missing capability is **a .NET SDK** — `dotnet` is not installed in the session that wrote
this (`dotnet: command not found`), so nothing in §3–§5 could be measured.

| Half | Who | What | Status |
|---|---|---|---|
| **A — repository analysis** | AI session | Target frameworks, build-config inventory, solution composition, the measurement script | **Complete** (§1, §2, §6) |
| **B — measurement** | Anyone with the .NET SDK | Run `tools/measure-build-baseline.ps1`, paste back `SUMMARY.md` | **Pending** (§3, §4, §5) |
| **C — write-up** | AI session | Fill §3–§5, finalise the `global.json` decision and the CI command | Pending B |

**Do not treat this document as a baseline yet.** M0-07 consumes §5's recommended command
and §4's warning number; neither is real until half B runs.

---

## 1. Project composition — **Confirmed**

### 1.1 The solution declares four projects; source control contains three

`NexGen-ERP---2025-master.sln` declares four projects (`:6`, `:8`, `:10`, `:12`):

| Solution entry | `.csproj` path | In source control? |
|---|---|---|
| `V.SMART.Shared` | `V.SMART\V.SMART.Shared\V.SMART.Shared.csproj` | **Yes** |
| `V.SMART` (MAUI) | `V.SMART\V.SMART\V.SMART.csproj` | **Yes** |
| `V.SMART.Web` | `V.SMART\V.SMART.Web\V.SMART.Web.csproj` | **Yes** |
| `V.SMART.Api` | `V.SMART\V.SMART.Api\V.SMART.Api.csproj` | **NO** |

**`V.SMART/V.SMART.Api/` is absent from both the working tree and the git index**, and it is
**not gitignored** (`git check-ignore` exits non-zero). It was deliberately deferred by M0-00
group G2 to [M0-03-01](tasks/M0-03-01.md), because its `appsettings.json` carries a JWT
signing secret. `git ls-files '*.csproj'` returns exactly three paths. **Confirmed 2026-08-13.**

### 1.2 Consequence: the solution build cannot succeed from source control alone

This partially answers the open question INV-029 left — and the answer is neither of the two
outcomes M0-15 anticipated (success, or failure on missing MAUI workloads):

> **Finding.** On a **fresh clone**, `dotnet build NexGen-ERP---2025-master.sln` fails before
> compilation begins, because MSBuild cannot load a solution that references a `.csproj` which
> does not exist on disk. The failure is structural, not toolchain-related — installing every
> MAUI workload would not fix it.
> **Evidence:** `NexGen-ERP---2025-master.sln:12` references
> `V.SMART\V.SMART.Api\V.SMART.Api.csproj`; `git ls-files V.SMART/V.SMART.Api` returns
> nothing; the path does not exist on disk in a fresh checkout.
> **Confidence:** **Confirmed** for a fresh clone. **Inferred** that a developer machine which
> still holds the untracked directory will get past this point and hit the *separate*
> MAUI-workload question — which is exactly what half B measures.

**This is a G0 finding, not just an M0-15 one.** G0's first exit criterion is that a fresh
environment can be rebuilt *from source control alone*. Today it cannot: the solution does not
even load. The fix is owned by **M0-03-01** (commit `V.SMART.Api/` with its secrets
externalised), not by this task — recorded here so M0-07 does not attempt a solution-level CI
build before that lands.

### 1.3 Target frameworks — **Confirmed**, cited by line

| Project | Declaration | File:line |
|---|---|---|
| `V.SMART.Shared` | `<TargetFrameworks>net9.0-windows10.0.19041.0;net9.0</TargetFrameworks>` — **multi-target** | `V.SMART/V.SMART.Shared/V.SMART.Shared.csproj:4` |
| `V.SMART.Web` | `<TargetFramework>net9.0</TargetFramework>` — single | `V.SMART/V.SMART.Web/V.SMART.Web.csproj:4` |
| `V.SMART` (MAUI) | `<TargetFrameworks>net9.0-android;net9.0-ios;net9.0-maccatalyst</TargetFrameworks>` | `V.SMART/V.SMART/V.SMART.csproj:4` |
| `V.SMART` (MAUI), Windows only | `<TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">$(TargetFrameworks);net9.0-windows10.0.19041.0</TargetFrameworks>` | `V.SMART/V.SMART/V.SMART.csproj:5` |
| `V.SMART.Api` | **not determinable** — project absent (§1.1) | — |

`V.SMART/V.SMART/V.SMART.csproj:7` also carries a commented-out `net9.0-tizen` target; it is
inactive and listed only so a future reader does not "restore" it by accident.

**The MAUI head's target list is OS-conditional.** On Windows it builds four TFMs; on Linux or
macOS it builds three, and `net9.0-windows10.0.19041.0` silently disappears. A CI runner's OS
therefore changes what the solution build even attempts — relevant to §5.

### 1.4 Root build configuration — **Confirmed negative result**

Checked at the repository root; **none of these exist**:

| File | Present? | Consequence |
|---|---|---|
| `global.json` | **No** | Nothing pins the SDK. Every machine uses whatever it has, via roll-forward. See §5.1. |
| `Directory.Build.props` | **No** | No shared MSBuild properties; every setting is per-project and undiscoverable. |
| `Directory.Packages.props` | **No** | No central package-version management; versions drift per project. |
| `NuGet.config` | **No** | Restore uses whatever feeds the machine has configured — a supply-chain and reproducibility gap. |
| `.editorconfig` (root) | **No** | No shared analyzer severity or style configuration. |

This is the reproducibility hole M0-15 exists to characterise: **nothing in the repository
constrains the build environment.** Two machines can produce different results and neither
would notice.

### 1.5 The analyzer behind `MUD0002` — **Confirmed**

`V.SMART/V.SMART.Shared/V.SMART.Shared.csproj:56` references `MudBlazor` version **8.11.0**.
MudBlazor ships a Roslyn analyzer, and `MUD0002` is expected to dominate the warning count —
KB-083 records ~6,695 warnings for the API project, "largely MUD0002". §4 quantifies that
properly; until then the attribution is **Inferred**, carried from INV-029.

---

## 2. Measurement methodology — **Confirmed** (the script exists; it has not been run)

`tools/measure-build-baseline.ps1` (**UNVERIFIED** — never executed; see its header banner)
performs every measurement M0-15 requires:

1. Captures `dotnet --info`, `--list-sdks`, `--list-runtimes`, `workload list` verbatim.
2. Builds each **present** project **twice**, each time after deleting `bin/`+`obj/`, recording
   wall-clock seconds, error count and warning count. Twice, because *a warning count that is
   not reproducible cannot be a CI gate*, and discovering that here is far cheaper than
   discovering it in M0-07.
3. Attempts the solution build and captures the **first three error lines verbatim** — which is
   what distinguishes "missing project file" (§1.2) from "missing MAUI workload" from anything
   else.
4. Breaks warnings down by code and computes each code's share.

**Two counting methods are recorded, deliberately.** MSBuild's own trailing `N Warning(s)`
summary and a distinct count of log lines matching `: warning XXnnnn:` legitimately differ —
multi-targeting emits the same diagnostic once per TFM, and MSBuild de-duplicates differently
than a line count does. The script reports both rather than picking one silently. **M0-07 must
state which number its gate uses.**

**Safety properties** (relevant because this repository has already had one build-output
incident, R-14): the script deletes only `bin/` and `obj/` under `V.SMART/`, never runs
`git clean`, modifies no source file, and writes all output to `%TEMP%\m0-15-baseline\` —
outside the repository, so it cannot dirty the tree or trip
`tools/check-no-build-output.sh`.

**Platform note.** The script is PowerShell because every environment signal in this
repository points to Windows development (`C:\Kumar\…` paths, `robocopy` in the M0-00 log,
Windows Git Credential Manager in INV-034, a `DESKTOP-…\SQLEXPRESS` connection string). No
bash equivalent was written: writing a second untested script in a language nobody has
confirmed they need doubles the surface for an unverified tool to be wrong in.

---

## 3. Toolchain as measured — **PENDING HALF B**

> Fill from `SUMMARY.md` §1. Expected per INV-029 (2026-08-12, **not** re-verified):
> SDKs `10.0.300` and `10.0.302` only — i.e. **no .NET 9 SDK**, so every `net9.0` target builds
> through SDK roll-forward; workloads `android`, `ios`, `maccatalyst`, `maui-windows` present
> on the original developer machine.

| Item | Value |
|---|---|
| `dotnet --list-sdks` | *pending* |
| `dotnet --list-runtimes` | *pending* |
| `dotnet workload list` | *pending* |
| OS / machine class (developer vs CI-like) | *pending* |

## 4. Build measurements — **PENDING HALF B**

| Project | Run | Seconds | Errors | Warnings (summary) | Warning lines |
|---|---|---|---|---|---|
| `V.SMART.Api` | — | — | — | *absent from source control (§1.1)* | — |
| `V.SMART.Web` | 1 / 2 | *pending* | *pending* | *pending* — **never measured before** | *pending* |
| `V.SMART.Shared` | 1 / 2 | *pending* | *pending* | *pending* | *pending* |
| **Solution** | 1 | *pending* | *pending* | *pending* | *pending* |

**Blocking condition:** if any project reports a **non-zero error count**, that is a blocking
finding, not a measurement — stop and report it rather than measuring around it.

**Warning breakdown / `MUD0002` share:** *pending.*

**Reproducibility verdict** (do run 1 and run 2 agree?): *pending.* If they disagree, the
warning count cannot be used as a CI gate as-is, and M0-07 must gate on *new* warnings against
a stored baseline file instead of on an absolute number.

## 5. Decisions — **PROVISIONAL, pending half B**

Both decisions below are reasoned from §1, which is Confirmed. Neither is final until the
measured SDK list exists, because both depend on *which* SDK versions are actually installed.

### 5.1 `global.json` — provisional recommendation: **pin, with `rollForward: latestFeature`**

**Reasoning from confirmed facts.** Every project targets `net9.0`; INV-029 reports only .NET
**10** SDKs installed, so the build works purely through roll-forward, and **nothing in the
repository records that** (§1.4 — no `global.json`, no `Directory.Build.props`, no
`NuGet.config`). A CI runner provisioned with a different SDK band would build something
subtly different from a developer machine and nobody would notice. That is precisely the
failure mode M0-07's gate is supposed to make impossible.

**Recommended shape** (exact version to be filled from §3):

```json
{ "sdk": { "version": "<measured SDK>", "rollForward": "latestFeature" } }
```

`latestFeature` rather than `disable`: pinning exactly would break every developer whose
patch band differs, for no reproducibility gain that matters at this stage; `latestPatch` is
the stricter alternative if the team prefers it. **The decision that matters is that the
choice is recorded at all** — an unrecorded default is exactly how the current
`net9.0`-targets-on-an-SDK-10-machine mismatch arose unnoticed.

**Explicitly deferred to M0-07 as well, not instead:** even with `global.json`, the CI
workflow should pin via `actions/setup-dotnet`, so the runner fails loudly on a missing SDK
rather than silently rolling forward.

### 5.2 Recommended CI build command — provisional: **per-project, not the solution**

```
dotnet build V.SMART/V.SMART.Web/V.SMART.Web.csproj --no-incremental
```
…plus `V.SMART.Api` **once M0-03-01 puts it in source control**, at which point it becomes the
primary CI target (it is the backend the React app is built on).

**Justification — two independent reasons, both Confirmed:**
1. **The solution build cannot work in CI today at all** (§1.2): it references a `.csproj`
   that is not in source control, so a runner cloning the repository cannot load it. This is
   decisive on its own.
2. **Even after that is fixed**, the solution includes the MAUI head, whose target frameworks
   are OS-conditional (§1.3) and which requires the `android`/`ios`/`maccatalyst` workloads. A
   hosted runner has none of them. Requiring them would add a multi-minute
   `dotnet workload restore` to every CI run to build a head that no M2–M6 task touches.

**The trade-off, stated rather than buried:** with this command **the MAUI head is not built
in CI**, so a change to `V.SMART.Shared` that breaks the MAUI project would not be caught. That
is an accepted, recorded cost — and it is directly relevant to **Q-11** (the MAUI app's future,
[open-questions.md](../open-questions.md)). If the answer to Q-11 is "MAUI stays", CI needs a
periodic or on-demand MAUI job on a Windows runner with workloads installed. If the answer is
"MAUI is decommissioned", this trade-off costs nothing. **M0-07 should not silently assume
either.**

## 6. Handoff

| Consumer | What it takes from here |
|---|---|
| **M0-07** (CI pipeline) | §5.2's command, §4's warning number *and* which of the two counting methods it uses, §5.1's SDK pin, plus the Q-11 trade-off in §5.2 |
| **M0-08** (already Completed) | Its CI guard `tools/check-no-build-output.sh` is a separate CI step; it needs no build and runs in under a second |
| **M0-03-01** | §1.1/§1.2 — committing `V.SMART.Api/` is what makes a solution-level build possible at all, and is a G0 blocker |
| **M0-12-01** | Adds `tests/V.SMART.Shared.Tests/` to the same CI workflow; will need this document's build command as its template |

### To complete half B

```powershell
# from the repository root, on a machine with the .NET SDK:
powershell -ExecutionPolicy Bypass -File tools\measure-build-baseline.ps1
```

It prints the path to `SUMMARY.md` when it finishes. Paste that file back; §3–§5 get filled
in and this document's `status` moves from `partial` to `complete`.
