---
doc_id: KB-086
title: Build and Toolchain Baseline
module: execution
source_files:
  - NexGen-ERP---2025-master.sln
  - V.SMART/V.SMART.Shared/V.SMART.Shared.csproj
  - V.SMART/V.SMART.Web/V.SMART.Web.csproj
  - V.SMART/V.SMART.Api/V.SMART.Api.csproj
  - V.SMART/V.SMART/V.SMART.csproj
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: complete
confidence: confirmed
last_verified: 2026-08-17
dependencies: [KB-080, KB-083, KB-010]
---

# KB-086 — Build and Toolchain Baseline

Produced by **M0-15**, closing the one gap **INV-029** left open: whether
`dotnet build NexGen-ERP---2025-master.sln` succeeds, and under what conditions. Every claim
below is tagged **Confirmed** (measured in this session), **Inferred** (reasoned from
Confirmed facts) or **Unknown** (could not be determined in this session, with the reason).

Execution machine: a Windows 11 developer workstation (OS Version 10.0.26200), with Visual
Studio 18.9 and its MAUI workloads installed — **not** a CI-like, workload-free environment.
This matters for §3 and §6.

## 1. Toolchain

**Confirmed**, measured 2026-08-17 (this session). This **supersedes** the 2026-08-12 figures
in INV-029 / the task file's "Current Implementation" section — the SDK list has drifted
since then.

`dotnet --list-sdks`:
```
10.0.300 [C:\Program Files\dotnet\sdk]
10.0.400 [C:\Program Files\dotnet\sdk]
```
(2026-08-12's INV-029 recorded `10.0.300, 10.0.302` — **10.0.302 is no longer present**;
`10.0.400` is now installed instead. No `.NET 9` SDK is installed, confirming INV-029's
original point still holds: the build only works through roll-forward.)

`dotnet --list-runtimes` (abridged — full runtime families only):
```
Microsoft.AspNetCore.App 8.0.30, 9.0.19, 10.0.8, 10.0.11
Microsoft.NETCore.App 8.0.30, 9.0.19, 10.0.8, 10.0.11
Microsoft.WindowsDesktop.App 8.0.30, 9.0.19, 10.0.8, 10.0.11
```

`dotnet workload list`:
```
Workload version: 10.0.400-manifests.b0ae88bd

Installed Workload Id      Manifest Version         Installation Source
-----------------------------------------------------------------------
android                    36.1.69/10.0.100         VS 18.9.12105.275
ios                        26.5.10301/10.0.100      VS 18.9.12105.275
maccatalyst                26.5.10301/10.0.100      VS 18.9.12105.275
maui-windows               10.0.20/10.0.100         VS 18.9.12105.275
```

`dotnet --info` selects `10.0.400` as the active SDK (highest installed, no `global.json` was
present at measurement time to pin a different one).

**Inferred.** The SDK-list drift between 2026-08-12 (10.0.300/10.0.302) and 2026-08-17
(10.0.300/10.0.400) on the *same* machine shows that this developer environment's SDK set
changes without anyone deliberately editing this repository — consistent with automatic
Visual Studio component updates. This is direct evidence for §7's pinning decision.

## 2. Target frameworks (re-verified with file:line, not copied from the task file)

- `V.SMART/V.SMART.Shared/V.SMART.Shared.csproj:4` —
  `<TargetFrameworks>net9.0-windows10.0.19041.0;net9.0</TargetFrameworks>` (multi-target)
- `V.SMART/V.SMART.Web/V.SMART.Web.csproj:4` — `<TargetFramework>net9.0</TargetFramework>`
- `V.SMART/V.SMART.Api/V.SMART.Api.csproj:4` — `<TargetFramework>net9.0</TargetFramework>`
- `V.SMART/V.SMART/V.SMART.csproj:4` —
  `<TargetFrameworks>net9.0-android;net9.0-ios;net9.0-maccatalyst</TargetFrameworks>`
- `V.SMART/V.SMART/V.SMART.csproj:5` — Windows-conditional addition:
  `<TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">$(TargetFrameworks);net9.0-windows10.0.19041.0</TargetFrameworks>`

**Confirmed** — line numbers match the task file's citations exactly; no drift here.

## 3. Per-project build results

Every project was built twice from a clean `bin`/`obj` (`rm -rf` of each project's `bin` and
`obj`, never `git clean -xdf` — `V.SMART.Api/` is untracked and `git clean` would delete it
entirely; confirmed by previewing with `git clean -xdn` first, which listed
`Would remove V.SMART/V.SMART.Api/`).

### `V.SMART.Api` — `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj`

| Run | Errors | Warnings | Wall-clock |
|---|---|---|---|
| 1 (cold, Shared + Api both dirty) | 0 | 6,695 | 2m 27s |
| 2 (clean Api + Shared only) | 0 | 6,695 | 1m 23s |

**Confirmed reproducible**: identical error and warning counts across both runs. This
confirms and re-measures the number INV-029 first reported (also 0 errors / 6,695 warnings on
2026-08-12), with a shorter *second*-run wall-clock time attributable to warmer OS/NuGet
caches, not to a different code path.

### `V.SMART.Web` — `dotnet build V.SMART/V.SMART.Web/V.SMART.Web.csproj` (previously
"not yet measured")

| Run | Errors | Warnings | Wall-clock |
|---|---|---|---|
| 1 (clean) | 0 | 6,698 | 1m 19s |
| 2 (clean) | 0 | 6,698 | 1m 20s |

**Confirmed reproducible.** 6,698 is 3 warnings higher than the Api build's 6,695 — Web pulls
in one additional Web-only `CS8600` (`V.SMART.Web/Services/WebFileUploadService.cs:157`) not
present when building Api alone, plus the Shared project's warnings are common to both.

### `V.SMART.Shared` — `dotnet build V.SMART/V.SMART.Shared/V.SMART.Shared.csproj`

| Run | Errors | Warnings | Wall-clock |
|---|---|---|---|
| 1 (clean) | 0 | 13,341 | 2m 17s |
| 2 (clean) | 0 | 13,341 | 2m 10s |

**Confirmed.** Both target frameworks build (`net9.0` and `net9.0-windows10.0.19041.0`,
verified by grepping the build log for each `TargetFramework=` tag — both appear). The
warning count for `Shared` alone (13,341) is roughly double the Api build's Shared-only share,
because both target frameworks are compiled and each one emits its own copy of every
Shared-code warning.

## 4. Whole-solution build — the question INV-029 left open

`dotnet build NexGen-ERP---2025-master.sln` was attempted three times.

**Run 1 (dirty `V.SMART`/MAUI `obj`, left over from earlier local MAUI build attempts on this
machine, unrelated to this session): FAILED.** 31.95s, 2 errors, 16 warnings:
- `Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(359,5): error : System.IO.IOException:
  The process cannot access the file '...\V.SMART\obj\Debug\net9.0-windows10.0.19041.0\
  win10-x64\compressed\publish\tzxjg6is5z-{0}-n8rndlt7dy-n8rndlt7dy.br' because it is being
  used by another process.` (`V.SMART.csproj::TargetFramework=net9.0-windows10.0.19041.0`)
- `Xamarin.Android.EmbeddedResource.targets(39,5): error XARLP7000: ... Renaming temporary
  file failed: Permission denied` (`V.SMART.csproj::TargetFramework=net9.0-android`)

Both errors are **file-lock/permission failures against stale artifacts in `V.SMART/obj`**,
not compilation errors and not a missing-workload failure — the workloads were installed and
were being exercised. **Confirmed** as a real, first-hand-observed outcome; **Inferred** cause:
stale `obj` output from an earlier, unrelated local MAUI build attempt on this machine
conflicting with file locks (likely antivirus or an editor holding a handle), not a defect in
the current code or a fresh-clone condition. This is itself a **finding worth recording**: the
solution build is **not safely re-runnable without a full clean** on this machine — a real
fragility a CI runner without that stale state would not necessarily hit, but any developer
machine with prior MAUI build attempts could.

**Run 2 and Run 3 (fully clean `bin`/`obj` for all four projects before each): SUCCEEDED,
identically.**

| Run | Errors | Warnings | Wall-clock |
|---|---|---|---|
| 2 (clean) | 0 | 13,367 | 4m 16s |
| 3 (clean) | 0 | 13,367 | 4m 7s |

**Confirmed, reproducible**: two clean runs, identical outcome. The solution — including the
MAUI head — builds successfully on **this** machine, given a clean `obj` and the MAUI
workloads already installed. Non-blocking warnings observed in the successful runs include
`NETSDK1202` ("the workload 'net9.0-android' is out of support") and `NU1608` (a
`Microsoft.CodeAnalysis.VisualBasic`/`Microsoft.CodeAnalysis.Common` version mismatch) — noted
here as findings, not fixed (out of this task's scope).

### Would it succeed on a runner without MAUI workloads?

**Unknown.** This session had no workload-free machine or container available to test it, and
the task explicitly permits recording this as Unknown rather than reasoning about it. What is
Confirmed instead: the MAUI workloads (`android`, `ios`, `maccatalyst`, `maui-windows`) are
present on this developer machine via a Visual Studio installation (§1) and a typical hosted
GitHub Actions runner does **not** have them preinstalled — so a local success here says
nothing about CI. Testing this would require either provisioning a workload-free container
(e.g. `mcr.microsoft.com/dotnet/sdk:10.0` with no `dotnet workload install` step) or measuring
`dotnet workload restore` time/footprint on a clean machine; neither was available in this
session.

## 5. Warning breakdown by code

**Confirmed**, from the Api build log (`dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj`,
both runs — the per-code counts were byte-for-byte identical between run 1 and run 2).

**Methodology note (a finding in its own right).** Grepping the raw build-log text for
` warning <CODE>` lines yields exactly **13,390** matches — precisely double the **6,695**
MSBuild reports in its own summary line (`6695 Warning(s)`), and this 2x ratio was exact and
identical in both runs. Individual diagnostics (traced by file/line) are frequently printed
twice per build with identical text — a normal MSBuild behaviour (e.g. once during the inner
build and once during an evaluation/analysis pass) — not evidence of two different warnings.
**All counts below are the raw grep count divided by 2**, which reconciles exactly to 6,695
and should be used, not the raw grep count, whenever parsing console output for a warning
tally.

| Warning code | Count | % of 6,695 |
|---|---|---|
| CS8602 (possibly-null dereference) | 1,579 | 23.6% |
| CS8629 (nullable value type may be null) | 984 | 14.7% |
| CS8618 (non-nullable field/property uninitialized) | 870 | 13.0% |
| CS8601 (possible null reference assignment) | 611 | 9.1% |
| CS8604 (possible null reference argument) | 599 | 8.9% |
| CS0108 (member hides inherited member; missing `new`) | 428 | 6.4% |
| CS0414 (field assigned but never used) | 210 | 3.1% |
| CS8603 (possible null reference return) | 161 | 2.4% |
| CS4014 (un-awaited async call) | 156 | 2.3% |
| CS8600 (null literal to non-nullable) | 141 | 2.1% |
| CS0168 (variable declared, never used) | 135 | 2.0% |
| **MUD0002 (MudBlazor analyzer — illegal component attribute)** | **130** | **1.94%** |
| CS8620 (nullability of reference in argument) | 99 | 1.5% |
| CS0169 (field never used) | 87 | 1.3% |
| CS8605 (unboxing a possibly-null value) | 76 | 1.1% |
| CS8981 (lowercase type name) | 73 | 1.1% |
| CA2200 (rethrow to preserve stack trace) | 51 | 0.8% |
| CS8625 (null literal to non-nullable reference type) | 49 | 0.7% |
| CA1416 (platform-compatibility) | 49 | 0.7% |
| CS0219 (variable assigned, never used) | 39 | 0.6% |
| CS8619 (nullability of reference types in value) | 32 | 0.5% |
| CS0472 (comparison to null always same result) | 25 | 0.4% |
| CS8613 (nullability differs, return types) | 18 | 0.3% |
| CS8714 (type doesn't satisfy nullable constraint) | 17 | 0.3% |
| CS0649 (field never assigned, always default) | 15 | 0.2% |
| CS0162 (unreachable code) | 15 | 0.2% |
| CS0105 (duplicate using) | 11 | 0.2% |
| RZ10012 (unrecognized/unknown component) | 10 | 0.15% |
| SYSLIB0021 (obsolete crypto API) | 8 | 0.12% |
| Remaining codes (NU1608, CS8765, CS8621, CS8073, CS1717, CS0618, CS0114, CS8767, CS8622, CS8609) | 17 | 0.25% |

Sum reconciles exactly to 6,695.

**Correction to the task file's premise.** The task's "Why This Task Exists" section
(2026-08-12, INV-029) described the baseline as "largely `MUD0002`". That is **not** what this
session's breakdown shows: `MUD0002` is only the 12th-largest code, at **130 occurrences
(1.94%)** — nullable-reference-analysis warnings (`CS86xx` family) dominate overwhelmingly.
`CS8602` alone (1,579, 23.6%) is more than 12x `MUD0002`'s count. This is recorded as a
correction, not an error report against the task file — it is exactly the kind of number this
task exists to produce.

## 6. `global.json` decision

**Decision: Pin.** A root `global.json` was created:

```json
{
  "sdk": {
    "version": "10.0.400",
    "rollForward": "latestFeature"
  }
}
```

**Reasoning.** §1 showed the installed SDK set on this single machine drifted from
`10.0.300/10.0.302` (2026-08-12) to `10.0.300/10.0.400` (2026-08-17) with no repository change
— an unmanaged, silent SDK change is exactly how the current `net9.0`-targets-vs-SDK-10
mismatch (INV-029's original finding) arose in the first place, and pinning is the direct fix
for "nobody would notice". `rollForward: latestFeature` is chosen over an exact pin
(`disable`) or `latestPatch` because it still lets this same drift happen *within* major.minor
10.0 (from feature band 3xx to 4xx, as already observed) without breaking the local build —
but it stops a future jump to a different **major** SDK (e.g. .NET 11, which is not proven
compatible with `net9.0` multi-targeting or the MAUI workload manifests currently installed)
from being silently adopted. CI (M0-07) should still pin the exact SDK version explicitly via
`actions/setup-dotnet` for full reproducibility on the hosted runner, independent of this
file — `global.json` and `actions/setup-dotnet` are complementary, not either/or.

## 7. Recommended CI build command

**Recommendation:** M0-07's CI build step should run the two projects the React
migration actually depends on, **not** the whole solution:

```
dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj
dotnet build V.SMART/V.SMART.Web/V.SMART.Web.csproj
```

**Justification.** §4 showed the solution build *can* succeed (Confirmed, reproducible twice)
— but only given (a) MAUI workloads installed and (b) a fully clean `obj` beforehand; a dirty
`obj` produced file-lock/permission failures unrelated to the code. §4 also could not confirm
(Unknown) whether a workload-free hosted runner — the default assumption for GitHub Actions —
would succeed at all. Building `Api` and `Web` explicitly avoids depending on either
uncertainty: both build cleanly, twice, in isolation, with 0 errors, in under 1.5 minutes each.
**Trade-off, stated explicitly:** this means the MAUI head (`V.SMART/V.SMART`) is **not**
built in CI under this recommendation — a regression there would not be caught until a
developer builds the solution locally. This is relevant to **Q-19-adjacent decision Q-11**
(the MAUI app's future — `docs/kb/open-questions.md`) and should be revisited once that
question is answered; if the MAUI app's future is "keep it", a follow-up task should add a
MAUI build to CI running on a runner with the workloads pre-installed (e.g.
`actions/setup-dotnet` plus an explicit `dotnet workload install` step), separately from this
recommendation.

## 8. Warning baseline for M0-07

**The number M0-07 gates against: 6,695 warnings**, from
`dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` — confirmed reproducible across two
clean runs in this session (§3) and unchanged from INV-029's original 2026-08-12 measurement.
If M0-07 adopts this task's §7 recommendation (building `Api` and `Web` both), the **Web**
project's own baseline is **6,698 warnings** (§3) — 3 higher than Api's, because Web pulls in
one Web-only warning not present in an Api-only build. M0-07 should gate each build command
against its own measured baseline, not a single shared number, if both commands run in CI.

## Summary table (for quick reference)

| Item | Value | Confidence |
|---|---|---|
| SDKs installed | 10.0.300, 10.0.400 | Confirmed (2026-08-17) |
| `global.json` | Created, pins `10.0.400`, `rollForward: latestFeature` | Confirmed (this task) |
| Api build | 0 errors, 6,695 warnings, ~1m23s–2m27s | Confirmed, reproducible x2 |
| Web build | 0 errors, 6,698 warnings, ~1m19s–1m20s | Confirmed, reproducible x2 |
| Shared build (both TFMs) | 0 errors, 13,341 warnings, ~2m10s–2m17s | Confirmed, reproducible x2 |
| Solution build (clean obj) | 0 errors, 13,367 warnings, ~4m7s–4m16s | Confirmed, reproducible x2 |
| Solution build (dirty obj) | FAILED — 2 file-lock/permission errors, not code errors | Confirmed (one-off observation) |
| Solution build without MAUI workloads | — | **Unknown** — no workload-free environment available this session |
| MUD0002 share of Api's 6,695 warnings | 130 (1.94%) | Confirmed |
| Dominant warning family | `CS86xx` nullable-reference warnings (not MUD0002) | Confirmed |
| Recommended CI command | `dotnet build` Api and Web separately, not the `.sln` | Recommendation, justified above |
