<#
.SYNOPSIS
    M0-15 half A — collects every build measurement the toolchain baseline needs.

.DESCRIPTION
    ############################################################################
    # THIS SCRIPT IS UNVERIFIED. The session that wrote it has no .NET SDK and
    # no PowerShell runtime, so it has never been executed. Read it before you
    # run it. It is deliberately read-only with respect to the repository:
    # the ONLY thing it deletes is bin/ and obj/ directories under V.SMART/,
    # which are reproducible build output and are already gitignored.
    #
    # It never runs `git clean`, never touches docs/ or frontend/, never
    # modifies a source file, and writes all of its own output OUTSIDE the
    # repository (into your TEMP directory), so it cannot dirty the working
    # tree or trip tools/check-no-build-output.sh.
    ############################################################################

    Runs from the repository root, takes no arguments. Produces:
      <TEMP>\m0-15-baseline\SUMMARY.md   <- paste this back
      <TEMP>\m0-15-baseline\*.log        <- full build logs, kept for evidence

    What it measures, per docs/kb/execution/tasks/M0-15.md:
      1. Toolchain: dotnet --info / --list-sdks / --list-runtimes / workload list
      2. Each buildable project, built TWICE from a clean bin/obj, recording
         wall-clock time, error count and warning count. Twice, because a
         warning count that is not reproducible cannot become a CI gate, and
         finding that out here is far cheaper than finding it out in M0-07.
      3. The whole-solution build attempt — the open question INV-029 left.
      4. Warning breakdown by warning code, with MUD0002's share.

    It does NOT decide anything. The global.json pin decision and the CI build
    command recommendation are written up from this output afterwards.

.NOTES
    Requires: the .NET SDK on PATH. Nothing else.
    Safe to re-run. Each run overwrites the previous output directory.
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Continue'   # a failing build is DATA, not a crash

# --- 0. Preconditions -------------------------------------------------------

if (-not (Test-Path 'V.SMART')) {
    Write-Error "Run this from the repository root (V.SMART\ not found)."
    exit 2
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Error "The .NET SDK is not on PATH. Install it, or run this on a machine that has it."
    exit 2
}

$outDir = Join-Path $env:TEMP 'm0-15-baseline'
if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }
New-Item -ItemType Directory -Path $outDir -Force | Out-Null

Write-Host ""
Write-Host "M0-15 build baseline" -ForegroundColor Cyan
Write-Host "Output -> $outDir"
Write-Host ""

# --- helpers ----------------------------------------------------------------

function Invoke-Capture {
    param([string]$Label, [string[]]$DotnetArgs)
    $log = Join-Path $outDir "$Label.log"
    & dotnet @DotnetArgs *>&1 | Out-File -FilePath $log -Encoding utf8
    return $log
}

function Get-BuildStats {
    # Parses an MSBuild log for errors, warnings and a per-code breakdown.
    # Two counts are reported per category on purpose:
    #   - "Summary"  = MSBuild's own trailing "N Warning(s)" / "N Error(s)"
    #   - "Lines"    = distinct log lines matching ': warning XXnnnn:'
    # They legitimately differ (multi-targeting repeats diagnostics per TFM,
    # and MSBuild de-duplicates differently), so BOTH are recorded rather than
    # picking one and hoping. The CI gate in M0-07 must state which it uses.
    param([string]$LogPath)

    $warnCodes   = @{}
    $warnLines   = 0
    $errLines    = 0
    $sumWarnings = $null
    $sumErrors   = $null

    foreach ($line in (Get-Content -LiteralPath $LogPath)) {
        if ($line -match ':\s+warning\s+([A-Za-z]+[0-9]+)\s*:') {
            $warnLines++
            $code = $Matches[1]
            if ($warnCodes.ContainsKey($code)) { $warnCodes[$code]++ } else { $warnCodes[$code] = 1 }
        }
        elseif ($line -match ':\s+error\s+([A-Za-z]+[0-9]+)\s*:') {
            $errLines++
        }
        if ($line -match '^\s*(\d+)\s+Warning\(s\)')  { $sumWarnings = [int]$Matches[1] }
        if ($line -match '^\s*(\d+)\s+Error\(s\)')    { $sumErrors   = [int]$Matches[1] }
    }

    return [PSCustomObject]@{
        SummaryWarnings = $sumWarnings
        SummaryErrors   = $sumErrors
        WarningLines    = $warnLines
        ErrorLines      = $errLines
        ByCode          = $warnCodes
    }
}

function Clear-BuildOutput {
    # Deletes ONLY bin/ and obj/ under V.SMART/. Never `git clean`.
    Get-ChildItem -Path 'V.SMART' -Include bin,obj -Recurse -Directory -ErrorAction SilentlyContinue |
        ForEach-Object { Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }
}

# --- 1. Toolchain -----------------------------------------------------------

Write-Host "[1/4] Recording toolchain..." -ForegroundColor Yellow
Invoke-Capture -Label '01-dotnet-info'          -DotnetArgs @('--info')          | Out-Null
Invoke-Capture -Label '02-dotnet-list-sdks'     -DotnetArgs @('--list-sdks')     | Out-Null
Invoke-Capture -Label '03-dotnet-list-runtimes' -DotnetArgs @('--list-runtimes') | Out-Null
Invoke-Capture -Label '04-dotnet-workloads'     -DotnetArgs @('workload','list') | Out-Null

# --- 2. Per-project builds, twice each, from clean --------------------------

# V.SMART.Api is deliberately probed rather than assumed: it is referenced by
# the .sln but is NOT in source control (untracked, not gitignored), so it
# exists on the original developer machine and is absent from a fresh clone.
# Which of those you are on materially changes the solution-build result.
$candidates = @(
    @{ Name = 'Api';    Path = 'V.SMART\V.SMART.Api\V.SMART.Api.csproj' },
    @{ Name = 'Web';    Path = 'V.SMART\V.SMART.Web\V.SMART.Web.csproj' },
    @{ Name = 'Shared'; Path = 'V.SMART\V.SMART.Shared\V.SMART.Shared.csproj' }
)

$results = @()

Write-Host "[2/4] Building each project twice from clean..." -ForegroundColor Yellow
foreach ($proj in $candidates) {
    if (-not (Test-Path $proj.Path)) {
        Write-Host ("  {0,-7} SKIPPED - not present on disk: {1}" -f $proj.Name, $proj.Path) -ForegroundColor DarkYellow
        $results += [PSCustomObject]@{
            Project = $proj.Name; Run = '-'; Seconds = $null
            Errors = $null; Warnings = $null; Stats = $null; Present = $false
        }
        continue
    }

    foreach ($run in 1, 2) {
        Clear-BuildOutput
        $label = "10-build-$($proj.Name)-run$run"
        Write-Host ("  {0,-7} run {1} ..." -f $proj.Name, $run) -NoNewline

        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $log = Invoke-Capture -Label $label -DotnetArgs @('build', $proj.Path, '--no-incremental')
        $sw.Stop()

        $stats = Get-BuildStats -LogPath $log
        $results += [PSCustomObject]@{
            Project  = $proj.Name
            Run      = $run
            Seconds  = [math]::Round($sw.Elapsed.TotalSeconds, 1)
            Errors   = $stats.SummaryErrors
            Warnings = $stats.SummaryWarnings
            Stats    = $stats
            Present  = $true
        }
        Write-Host (" {0}s, {1} errors, {2} warnings" -f `
            [math]::Round($sw.Elapsed.TotalSeconds,1), $stats.SummaryErrors, $stats.SummaryWarnings)
    }
}

# --- 3. Solution build — the open question ---------------------------------

Write-Host "[3/4] Attempting the SOLUTION build (all outcomes are valid findings)..." -ForegroundColor Yellow
Clear-BuildOutput
$slnSw  = [System.Diagnostics.Stopwatch]::StartNew()
$slnLog = Invoke-Capture -Label '20-build-solution' -DotnetArgs @('build','NexGen-ERP---2025-master.sln','--no-incremental')
$slnSw.Stop()
$slnStats = Get-BuildStats -LogPath $slnLog

# First error line, verbatim — this is what identifies WHY it failed
# (missing project file vs missing MAUI workload vs something else).
$firstError = (Select-String -Path $slnLog -Pattern ':\s+error\s+' | Select-Object -First 3 | ForEach-Object { $_.Line.Trim() }) -join "`n"
if (-not $firstError) { $firstError = '(no error lines found)' }

Write-Host ("  solution: {0}s, {1} errors, {2} warnings" -f `
    [math]::Round($slnSw.Elapsed.TotalSeconds,1), $slnStats.SummaryErrors, $slnStats.SummaryWarnings)

# --- 4. Write the summary ---------------------------------------------------

Write-Host "[4/4] Writing SUMMARY.md..." -ForegroundColor Yellow

$sb = New-Object System.Text.StringBuilder
function Add-Line { param([string]$t = '') [void]$sb.AppendLine($t) }

Add-Line "# M0-15 raw measurements"
Add-Line ""
Add-Line "Generated by ``tools/measure-build-baseline.ps1``. Paste this whole file back."
Add-Line ""
Add-Line ("Machine OS: " + [System.Environment]::OSVersion.VersionString)
Add-Line ("PowerShell: " + $PSVersionTable.PSVersion.ToString())
Add-Line ""

Add-Line "## 1. Toolchain"
Add-Line ""
foreach ($pair in @(
    @('dotnet --list-sdks',     '02-dotnet-list-sdks'),
    @('dotnet --list-runtimes', '03-dotnet-list-runtimes'),
    @('dotnet workload list',   '04-dotnet-workloads'))) {
    Add-Line ("### ``{0}``" -f $pair[0]); Add-Line '```'
    Get-Content (Join-Path $outDir ($pair[1] + '.log')) | ForEach-Object { Add-Line $_ }
    Add-Line '```'; Add-Line ''
}
Add-Line "### ``dotnet --info`` (first 30 lines)"
Add-Line '```'
Get-Content (Join-Path $outDir '01-dotnet-info.log') -TotalCount 30 | ForEach-Object { Add-Line $_ }
Add-Line '```'; Add-Line ''

Add-Line "## 2. Per-project builds (twice each, from clean bin/obj)"
Add-Line ""
Add-Line "| Project | Run | Seconds | Errors | Warnings (MSBuild summary) | Warning lines |"
Add-Line "|---|---|---|---|---|---|"
foreach ($r in $results) {
    if (-not $r.Present) {
        Add-Line ("| {0} | — | — | — | **NOT PRESENT ON DISK** | — |" -f $r.Project)
    } else {
        Add-Line ("| {0} | {1} | {2} | {3} | {4} | {5} |" -f `
            $r.Project, $r.Run, $r.Seconds, $r.Errors, $r.Warnings, $r.Stats.WarningLines)
    }
}
Add-Line ""
Add-Line "**Reproducibility check:** run 1 and run 2 warning counts must match for the number to be usable as a CI gate."
Add-Line ""

Add-Line "## 3. Solution build"
Add-Line ""
Add-Line ("- Command: ``dotnet build NexGen-ERP---2025-master.sln --no-incremental``")
Add-Line ("- Wall clock: {0}s" -f [math]::Round($slnSw.Elapsed.TotalSeconds,1))
Add-Line ("- Errors: {0}  |  Warnings: {1}" -f $slnStats.SummaryErrors, $slnStats.SummaryWarnings)
Add-Line ""
Add-Line "First error lines, verbatim:"
Add-Line '```'
Add-Line $firstError
Add-Line '```'
Add-Line ""

Add-Line "## 4. Warning breakdown by code"
Add-Line ""
foreach ($r in $results) {
    if (-not $r.Present -or $r.Run -ne 2) { continue }
    Add-Line ("### {0} (run 2)" -f $r.Project)
    Add-Line ""
    $total = ($r.Stats.ByCode.Values | Measure-Object -Sum).Sum
    if (-not $total) { $total = 0 }
    Add-Line ("Total matched warning lines: {0}" -f $total)
    Add-Line ""
    Add-Line "| Code | Count | % of total |"
    Add-Line "|---|---|---|"
    $r.Stats.ByCode.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First 15 | ForEach-Object {
        $pct = if ($total -gt 0) { [math]::Round(100.0 * $_.Value / $total, 1) } else { 0 }
        Add-Line ("| {0} | {1} | {2}% |" -f $_.Key, $_.Value, $pct)
    }
    Add-Line ""
}

Add-Line "## 5. Files kept for evidence"
Add-Line ""
Add-Line '```'
Get-ChildItem $outDir -File | ForEach-Object { Add-Line $_.Name }
Add-Line '```'

$summaryPath = Join-Path $outDir 'SUMMARY.md'
[System.IO.File]::WriteAllText($summaryPath, $sb.ToString(), [System.Text.UTF8Encoding]::new($false))

# --- done -------------------------------------------------------------------

Clear-BuildOutput

Write-Host ""
Write-Host "Done." -ForegroundColor Green
Write-Host "Paste this file back:" -ForegroundColor Cyan
Write-Host "  $summaryPath"
Write-Host ""
Write-Host "Nothing in the repository was modified. Confirm with: git status --porcelain"
Write-Host ""
