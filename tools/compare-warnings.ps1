<#
.SYNOPSIS
    Analyzer warning gate for task M0-07. Compares the warning population of an
    MSBuild log against the committed baseline in ci/warning-baseline.json.

.DESCRIPTION
    THE COUNTING RULE (this is the load-bearing part -- see
    docs/kb/execution/ci-pipeline.md "Counting rule"):

    An MSBuild console log at -v normal prints every diagnostic TWICE: once
    inline while the project builds (prefixed with a node id such as "2>") and
    once again in the warning summary block that follows the "Build succeeded."
    / "Build FAILED." marker. Confirmed 2026-08-17 on this repository: the Api
    build log contained 13,386 raw " warning <CODE>" lines and MSBuild's own
    summary reported 6,693 Warning(s) -- exactly 2x.

    This script therefore counts ONLY the lines that appear AFTER the
    "Build succeeded." / "Build FAILED." marker, i.e. MSBuild's own warning
    summary block. That set is authoritative by construction: its size equals
    the "<n> Warning(s)" figure MSBuild prints. The script asserts that
    equality and FAILS LOUDLY (exit 2) if it does not hold, rather than
    silently gating on a number it does not understand.

    Deliberately NOT used as the counting rule:
      * raw line count            -- exactly 2x inflated (see above);
      * "divide the raw count by 2" -- reconciles today but is a magic number
                                     that breaks the moment verbosity, node
                                     count or logger configuration changes;
      * de-duplicating identical diagnostic text -- WRONG. Measured on this
                                     repository, some diagnostics legitimately
                                     occur 2-3 times with byte-identical text
                                     (13,310 distinct lines vs 13,386 total),
                                     so text de-duplication under-counts.

    The gate is a RATCHET:
      total >  baseline total          -> FAIL
      a warning code absent from the baseline appears -> FAIL
      total <  baseline total          -> PASS, with an explicit instruction to
                                          lower the committed baseline
      total == baseline total          -> PASS

    It never uses -warnaserror / TreatWarningsAsErrors, and it never edits a
    .csproj, .cs or .razor file.

.PARAMETER LogPath
    Path to the captured MSBuild console log.

.PARAMETER BaselinePath
    Path to ci/warning-baseline.json.

.PARAMETER Target
    Key under "targets" in the baseline file, e.g. "V.SMART.Api".

.PARAMETER UpdateBaseline
    Print the measured JSON block to stdout instead of comparing. Used to
    regenerate the baseline from a runner log; the output is pasted into
    ci/warning-baseline.json by a human, never committed automatically.

.OUTPUTS
    Exit 0 = pass, 1 = gate failure, 2 = the script could not trust its own
    measurement (unreadable log, missing summary block, or a total that does
    not reconcile with MSBuild's own "<n> Warning(s)" line).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$LogPath,
    [Parameter(Mandatory = $true)][string]$BaselinePath,
    [Parameter(Mandatory = $true)][string]$Target,
    [switch]$UpdateBaseline
)

$ErrorActionPreference = 'Stop'

function Write-Section([string]$text) { Write-Host ""; Write-Host "=== $text ===" }

if (-not (Test-Path -LiteralPath $LogPath)) {
    Write-Host "compare-warnings: build log not found: $LogPath"
    exit 2
}

$lines = Get-Content -LiteralPath $LogPath

# --- locate MSBuild's own warning summary block -----------------------------
$markerIndex = -1
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match '^\s*Build (succeeded|FAILED)\.\s*$') { $markerIndex = $i; break }
}
if ($markerIndex -lt 0) {
    Write-Host "compare-warnings: no 'Build succeeded.' / 'Build FAILED.' marker in $LogPath."
    Write-Host "The log was probably captured at a verbosity below 'normal', or truncated."
    Write-Host "The counting rule depends on that block; refusing to guess."
    exit 2
}

$summary = $lines[($markerIndex + 1)..($lines.Count - 1)]

# --- count -----------------------------------------------------------------
$counts = [ordered]@{}
$total = 0
foreach ($line in $summary) {
    foreach ($m in [regex]::Matches($line, ': warning ([A-Za-z]+[0-9]+):')) {
        $code = $m.Groups[1].Value
        if ($counts.Contains($code)) { $counts[$code] = $counts[$code] + 1 } else { $counts[$code] = 1 }
        $total++
    }
}

# --- reconcile against MSBuild's own total ---------------------------------
$reported = $null
foreach ($line in $summary) {
    if ($line -match '^\s*(\d+)\s+Warning\(s\)\s*$') { $reported = [int]$Matches[1] }
}
if ($null -eq $reported) {
    Write-Host "compare-warnings: could not find MSBuild's '<n> Warning(s)' line; refusing to gate on an unverified count."
    exit 2
}
if ($reported -ne $total) {
    Write-Host "compare-warnings: COUNTING RULE BROKEN."
    Write-Host "  parsed from the summary block : $total"
    Write-Host "  MSBuild's own Warning(s) line : $reported"
    Write-Host "These must be equal. Something about the log format changed."
    Write-Host "Fix the counting rule in tools/compare-warnings.ps1 and re-document it in"
    Write-Host "ci/warning-baseline.json before trusting this gate again."
    exit 2
}

$sorted = [ordered]@{}
foreach ($k in ($counts.Keys | Sort-Object { -$counts[$_] }, { $_ })) { $sorted[$k] = $counts[$k] }

if ($UpdateBaseline) {
    Write-Host "`"total`": $total,"
    Write-Host "`"codes`": {"
    $keys = @($sorted.Keys)
    for ($i = 0; $i -lt $keys.Count; $i++) {
        $comma = if ($i -lt $keys.Count - 1) { ',' } else { '' }
        Write-Host ("  `"{0}`": {1}{2}" -f $keys[$i], $sorted[$keys[$i]], $comma)
    }
    Write-Host "}"
    exit 0
}

# --- load the baseline ------------------------------------------------------
if (-not (Test-Path -LiteralPath $BaselinePath)) {
    Write-Host "compare-warnings: baseline not found: $BaselinePath"
    exit 2
}
$baseline = Get-Content -LiteralPath $BaselinePath -Raw | ConvertFrom-Json
if (-not $baseline.targets.PSObject.Properties.Name.Contains($Target)) {
    Write-Host "compare-warnings: baseline has no target '$Target'. Known targets: $($baseline.targets.PSObject.Properties.Name -join ', ')"
    exit 2
}
$baseTarget = $baseline.targets.$Target
$baseTotal = [int]$baseTarget.total
$baseCodes = @{}
foreach ($p in $baseTarget.codes.PSObject.Properties) { $baseCodes[$p.Name] = [int]$p.Value }

Write-Section "Analyzer warning gate -- $Target"
Write-Host "log        : $LogPath"
Write-Host "baseline   : $BaselinePath (total $baseTotal)"
Write-Host "measured   : $total"

# --- evaluate ---------------------------------------------------------------
$failed = $false

$newCodes = @($sorted.Keys | Where-Object { -not $baseCodes.ContainsKey($_) })
if ($newCodes.Count -gt 0) {
    $failed = $true
    Write-Section "FAIL -- warning code(s) not present in the baseline"
    foreach ($c in $newCodes) { Write-Host ("  {0}  x{1}   (baseline: absent)" -f $c, $sorted[$c]) }
    Write-Host ""
    Write-Host "A new warning CODE is a new class of defect, not more of an old one."
    Write-Host "Fix it, or -- if it is genuinely acceptable -- add it to $BaselinePath"
    Write-Host "in its own reviewed commit that says why."
}

if ($total -gt $baseTotal) {
    $failed = $true
    Write-Section "FAIL -- warning total rose above the baseline"
    Write-Host ("  baseline {0}, measured {1}, delta +{2}" -f $baseTotal, $total, ($total - $baseTotal))
    Write-Host ""
    Write-Host "Codes that increased:"
    $anyIncrease = $false
    foreach ($k in $sorted.Keys) {
        $was = if ($baseCodes.ContainsKey($k)) { $baseCodes[$k] } else { 0 }
        if ($sorted[$k] -gt $was) {
            $anyIncrease = $true
            Write-Host ("  {0}  {1} -> {2}  (+{3})" -f $k, $was, $sorted[$k], ($sorted[$k] - $was))
        }
    }
    if (-not $anyIncrease) {
        Write-Host "  (none -- every per-code count matches the baseline, so the baseline's"
        Write-Host "   'total' field disagrees with the sum of its own 'codes'. Regenerate it"
        Write-Host "   with -UpdateBaseline rather than hand-editing the total.)"
    }
}

if ($failed) {
    Write-Section "Gate: FAILED"
    Write-Host "This gate never uses -warnaserror. It compares against a committed baseline."
    Write-Host "See docs/kb/execution/ci-pipeline.md -> 'How to read a failure'."
    exit 1
}

if ($total -lt $baseTotal) {
    Write-Section "PASS -- and the baseline is now stale (ratchet)"
    Write-Host ("  baseline {0}, measured {1}, delta -{2}" -f $baseTotal, $total, ($baseTotal - $total))
    Write-Host ""
    Write-Host "ACTION REQUIRED: lower the committed baseline so the improvement cannot be undone."
    Write-Host "  1. Download the build log artefact from this run."
    Write-Host "  2. pwsh tools/compare-warnings.ps1 -LogPath <log> -BaselinePath ci/warning-baseline.json -Target $Target -UpdateBaseline"
    Write-Host "  3. Paste the printed 'total' and 'codes' into ci/warning-baseline.json under targets.$Target,"
    Write-Host "     update generated_on/commit/date, and commit."
    Write-Host "Warnings are allowed to fall. They are never allowed to rise."
    Write-Host ""
    Write-Section "Gate: PASSED (below baseline)"
    exit 0
}

Write-Section "Gate: PASSED (equal to baseline)"
exit 0
