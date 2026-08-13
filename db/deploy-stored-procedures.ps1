<#
================================================================================
 db/deploy-stored-procedures.ps1
================================================================================

 STATUS: UNVERIFIED. Written by an AI session with no database credentials and
 no network path to any SQL Server instance (task M0-01-03 / R-01, R-02 --
 docs/kb/risks/technical-debt-register.md). It has NEVER been executed against
 a real database by anyone as of this writing. Do not treat a clean read of
 this file as proof it works -- the first person to run it (the rebuild drill,
 db/RUNBOOK-rebuild-tenant-database.md) must record what actually happened in
 db/REBUILD-DRILL-LOG.md, including every failure, before this status changes.

 What this script does:
   1. Enumerates every db/stored-procedures/**/*.sql file (recursive -- this
      covers both the flat directory, where M0-01-02's 78 captured "missing"
      procedures live, and db/stored-procedures/relocated-legacy/, where
      M0-01-03 relocated the 13 procedures that used to live in
      "Existing Store Procedures/StoredProcedures/"). manifest.csv,
      README.md and CAPTURE-STATUS.md are never matched -- the filter is
      literally *.sql.
   2. BEFORE connecting to anything: cross-checks that file set against
      db/stored-procedures/manifest.csv (every row, not just 'missing' rows)
      and refuses to run if a referenced procedure has no corresponding file,
      unless -SkipCompletenessCheck is passed (which prints a loud warning and
      proceeds anyway). A manifest row whose procedure name is separately
      recorded in CAPTURE-STATUS.md as a documented capture gap (the 4 names
      still genuinely absent from the source tenant -- see that file) is a
      warning here, not a hard block, mirroring db/tools/verify-capture.sh's
      own "documented, owned exception" logic.
   3. Applies each file, in a stable sorted order (see "Ordering" below), as
      CREATE OR ALTER PROCEDURE -- every file already satisfies that
      convention (db/stored-procedures/README.md), so re-running this script
      against a database that already has every procedure deployed is a
      no-op every time. That is what makes this a deployment STEP, not a
      one-shot migration.
   4. Stops immediately, non-zero exit code, naming the file, the moment any
      single file fails to apply. It does not "keep going and report
      everything at the end" -- a half-applied deployment that looks like a
      full one is exactly the failure mode this task exists to prevent.
   5. Prints a summary (applied / skipped / failed counts, target database
      NAME only) at the end, or as much of one as was reached before an abort.

 What this script deliberately does NOT do:
   - It does not read V.SMART/**/appsettings*.json, and it does not fall back
     to the hardcoded credentials in
     V.SMART/V.SMART.Shared/Data/MigrationData/ApplicationDbContextFactory.cs
     or MasterDbContextFactory.cs. Every connection detail comes from
     -ServerInstance/-Database plus one of the three authentication parameter
     sets below, supplied by the caller at run time. If M0-03 (externalised
     configuration) has landed in your environment, source those parameters
     from its configuration surface; if it has not, pass them explicitly on
     the command line or via environment variable, per the parameters below
     -- never by editing this script.
   - It does not modify any .sql file, manifest.csv, README.md, or
     CAPTURE-STATUS.md. It only reads them.
   - It does not attempt to apply EF Core migrations. That is a separate step
     -- see db/RUNBOOK-rebuild-tenant-database.md, which also flags Q-02
     (docs/kb/open-questions.md) as still open: there is no
     Migrate()/MigrateAsync() call anywhere under V.SMART/, and this script
     is not that call either.

 Ordering (binding requirement #5):
   Files are applied in a single stable sort by relative path (ordinal,
   case-sensitive-off comparison) -- not directory-listing order, not
   manifest-row order. The working assumption is that order does not matter
   at all: SQL Server uses deferred name resolution for stored procedures, so
   CREATE OR ALTER PROCEDURE A, whose body references object B, succeeds even
   if B does not exist yet at the moment A is created -- name resolution for
   the body's references happens at EXECUTE time, not CREATE time. This is
   documented SQL Server behaviour -- Inferred for this specific procedure
   set, NOT verified against a real deployment in this session (no database
   access -- see STATUS above). If the rebuild drill (item 7 in
   db/RUNBOOK-rebuild-tenant-database.md) finds an ordering dependency --
   some procedure's CREATE fails because of something order-sensitive this
   comment did not anticipate -- record that finding in
   db/REBUILD-DRILL-LOG.md and in docs/kb/risks/technical-debt-register.md
   (R-04), then add an explicit order file here. Nothing has forced that yet.

 Known findings carried into this deployment, not resolved here (see
 docs/kb/execution/tasks/M0-01-03.md for the escalation to a human):
   - Sp_Print_PurchaseOrder.sql (db/stored-procedures/relocated-legacy/) is
     retained-but-unreferenced: no C#/Razor call site anywhere under
     V.SMART/ calls Sp_Print_PurchaseOrder (manifest status: unreferenced).
     This script deploys it like any other file -- deleting it is a human
     decision this task was explicitly told not to make.
   - Sp_Print_MFGDC.sql (db/stored-procedures/relocated-legacy/) declares
     [dbo].[Sp_Print_MFGDC]; the application calls Sp_Print_MfgDC -- a
     case-only difference (manifest status: case_mismatch). Deployed under
     its declared spelling, unchanged; resolving under a case-insensitive
     collation is Inferred, not Confirmed against a live server.
   - 4 manifest 'missing' names have no file at all and are not deployed by
     this script: Sp_BomAnalysis, Sp_Print_Estimation, Sp_Print_Receipts,
     Sp_Print_SingleProcessInspection. See
     db/stored-procedures/CAPTURE-STATUS.md, "Findings requiring a human
     decision" -- each is escalated as dead-code-or-latent-defect, per name,
     not resolved here either.

================================================================================
#>

#Requires -Version 5.1
[CmdletBinding(SupportsShouldProcess = $true, DefaultParameterSetName = 'TrustedConnection')]
param(
    # --- Target -----------------------------------------------------------
    [Parameter(Mandatory = $true)]
    [string]$ServerInstance,

    [Parameter(Mandatory = $true)]
    [string]$Database,

    # --- Authentication (choose exactly one style) -------------------------
    # Default / preferred: Windows Authentication. No parameter needed --
    # this is what happens if neither -Credential nor
    # -SqlUserName/-SqlPasswordEnvVar is supplied.
    [Parameter(ParameterSetName = 'SqlCredential')]
    [System.Management.Automation.PSCredential]$Credential,

    # Alternative SQL-auth path: username on the command line (not a secret),
    # password read from an environment variable at run time (never a script
    # parameter, never echoed, never written anywhere).
    [Parameter(ParameterSetName = 'SqlEnvVar')]
    [string]$SqlUserName,

    [Parameter(ParameterSetName = 'SqlEnvVar')]
    [string]$SqlPasswordEnvVar,

    # See db/tools/Export-StoredProcedures.ps1's header comment for why this
    # switch exists -- most on-prem / local SQL Server instances (including
    # every tenant connection string this application already uses) run with
    # a self-signed certificate, so strict validation fails with an SSL trust
    # error unless this is set. It does not weaken authentication.
    [switch]$TrustServerCertificate,

    # --- Source / worklist ---------------------------------------------------
    [string]$SourceDirectory = (Join-Path $PSScriptRoot 'stored-procedures'),
    [string]$ManifestPath = (Join-Path $PSScriptRoot 'stored-procedures\manifest.csv'),
    [string]$CaptureStatusPath = (Join-Path $PSScriptRoot 'stored-procedures\CAPTURE-STATUS.md'),

    # Binding requirement #4: refuse to run by default if the manifest
    # references a procedure with no corresponding file. Pass this to
    # proceed anyway -- a loud warning is still printed, every gap is still
    # named, this is not a silent override.
    [switch]$SkipCompletenessCheck,

    [int]$CommandTimeoutSeconds = 300,
    [int]$ConnectionTimeoutSeconds = 30,

    # Report what would be applied, in what order, without connecting to
    # anything or running the completeness check's database-free logic
    # differently -- still runs the completeness check (it needs no
    # database), just skips Invoke-Sqlcmd.
    [switch]$WhatIfDeploy
)

$ErrorActionPreference = 'Stop'
$exitCode = 0

function Write-Section {
    param([string]$Title)
    Write-Host ""
    Write-Host "==================== $Title ====================" -ForegroundColor Cyan
}

# -----------------------------------------------------------------------
# Preconditions
# -----------------------------------------------------------------------
if (-not (Test-Path -LiteralPath $SourceDirectory)) {
    throw "Source directory not found: '$SourceDirectory'."
}
if (-not (Test-Path -LiteralPath $ManifestPath)) {
    throw "Manifest not found at '$ManifestPath'. Run M0-01-01 first, or pass -ManifestPath explicitly."
}
if ($PSCmdlet.ParameterSetName -eq 'SqlEnvVar') {
    if (-not $SqlUserName -or -not $SqlPasswordEnvVar) {
        throw "Both -SqlUserName and -SqlPasswordEnvVar are required together for SQL authentication via environment variable."
    }
}

if (-not $WhatIfDeploy) {
    if (-not (Get-Module -ListAvailable -Name SqlServer)) {
        throw @"
The 'SqlServer' PowerShell module is not available on this machine.
Install it with (requires internet access):
    Install-Module -Name SqlServer -Scope CurrentUser
Or re-run with -WhatIfDeploy to see the completeness check and the planned
file order without connecting to anything.
"@
    }
    Import-Module SqlServer -ErrorAction Stop
    $script:InvokeSqlcmdParams = (Get-Command Invoke-Sqlcmd).Parameters.Keys
    $script:SupportsTrustedConnection = $script:InvokeSqlcmdParams -contains 'TrustedConnection'
    $script:SupportsCredentialParam = $script:InvokeSqlcmdParams -contains 'Credential'
}

function Get-SqlCmdConnectionArgs {
    $connArgs = @{
        ServerInstance    = $ServerInstance
        Database          = $Database
        QueryTimeout      = $CommandTimeoutSeconds
        ConnectionTimeout = $ConnectionTimeoutSeconds
        ErrorAction       = 'Stop'
    }
    if ($TrustServerCertificate -and ($script:InvokeSqlcmdParams -contains 'TrustServerCertificate')) {
        $connArgs['TrustServerCertificate'] = $true
    }
    switch ($PSCmdlet.ParameterSetName) {
        'SqlCredential' {
            if ($script:SupportsCredentialParam) {
                $connArgs['Credential'] = $Credential
            }
            else {
                Write-Warning "  Installed Invoke-Sqlcmd has no -Credential parameter (older SqlServer module). Falling back to plain -Username/-Password -- see db/tools/Export-StoredProcedures.ps1's header comment for the same caveat."
                $connArgs['Username'] = $Credential.UserName
                $connArgs['Password'] = $Credential.GetNetworkCredential().Password
            }
        }
        'SqlEnvVar' {
            $plainPassword = [System.Environment]::GetEnvironmentVariable($SqlPasswordEnvVar)
            if ([string]::IsNullOrEmpty($plainPassword)) {
                throw "Environment variable '$SqlPasswordEnvVar' is not set or is empty."
            }
            if ($script:SupportsCredentialParam) {
                $securePassword = ConvertTo-SecureString -String $plainPassword -AsPlainText -Force
                $connArgs['Credential'] = New-Object System.Management.Automation.PSCredential ($SqlUserName, $securePassword)
            }
            else {
                Write-Warning "  Installed Invoke-Sqlcmd has no -Credential parameter (older SqlServer module). Falling back to plain -Username/-Password."
                $connArgs['Username'] = $SqlUserName
                $connArgs['Password'] = $plainPassword
            }
        }
        default {
            if ($script:SupportsTrustedConnection) {
                $connArgs['TrustedConnection'] = $true
            }
        }
    }
    return $connArgs
}

# -----------------------------------------------------------------------
# Enumerate every .sql file under SourceDirectory, recursively (this is
# what picks up both the flat directory and relocated-legacy/), in a
# single stable sort by relative path. manifest.csv / README.md /
# CAPTURE-STATUS.md never match *.sql, so binding requirement #6 (deploy
# *.sql only) holds by construction, not by an extra exclude list.
# -----------------------------------------------------------------------
$sourceRoot = (Resolve-Path -LiteralPath $SourceDirectory).Path
$allFiles = Get-ChildItem -LiteralPath $sourceRoot -Filter '*.sql' -Recurse -File
$allFiles = @($allFiles | Sort-Object { $_.FullName.Substring($sourceRoot.Length).TrimStart('\', '/') } -CaseSensitive:$false)

Write-Section "Discovery"
Write-Host "Source directory : $sourceRoot"
Write-Host ".sql files found : $($allFiles.Count) (recursive)"

# -----------------------------------------------------------------------
# Completeness check (binding requirement #4) -- runs before any
# connection is opened, and does not require one.
# -----------------------------------------------------------------------
Write-Section "Completeness check"

$manifestRows = Import-Csv -LiteralPath $ManifestPath
$captureStatusText = ''
if (Test-Path -LiteralPath $CaptureStatusPath) {
    $captureStatusText = Get-Content -LiteralPath $CaptureStatusPath -Raw
}
else {
    Write-Warning "CAPTURE-STATUS.md not found at '$CaptureStatusPath' -- documented-exception lookups below will all miss, which turns every one of them into a hard gap instead of a warning."
}

# repo root = parent of the directory this script lives in (db/deploy-stored-procedures.ps1 -> db/ -> repo root)
$repoRoot = (Resolve-Path -LiteralPath (Split-Path -Parent $PSScriptRoot)).Path

# Build the set of files actually present, keyed by a repo-relative path
# with forward slashes, so it compares cleanly against manifest.csv's
# scripted_file column (which is always written with forward slashes).
$presentByRelPath = @{}
foreach ($f in $allFiles) {
    $rel = $f.FullName.Substring($repoRoot.Length).TrimStart('\', '/') -replace '\\', '/'
    $presentByRelPath[$rel] = $f
}
# Also index by bare file name, for 'missing'-status rows, whose expected
# location is db/stored-procedures/<procedure_name>.sql (no scripted_file
# value in the manifest -- that column is only ever populated for rows
# M0-01-02/M0-01-03 pointed at a real file).
$presentByFileName = @{}
foreach ($f in $allFiles) {
    $presentByFileName[$f.Name] = $f
}

$hardGaps = New-Object System.Collections.Generic.List[string]
$warnGaps = New-Object System.Collections.Generic.List[string]
$expectedRelPaths = New-Object System.Collections.Generic.HashSet[string]

foreach ($row in $manifestRows) {
    $name = $row.procedure_name
    $status = $row.status
    $scriptedFile = $row.scripted_file

    if ($status -eq 'missing') {
        $expectedFileName = "$name.sql"
        if ($presentByFileName.ContainsKey($expectedFileName)) {
            $rel = ($presentByFileName[$expectedFileName].FullName.Substring($repoRoot.Length).TrimStart('\', '/') -replace '\\', '/')
            [void]$expectedRelPaths.Add($rel)
        }
        elseif ($captureStatusText -and $captureStatusText.Contains($name)) {
            # Same heuristic, and the same caveat, as db/tools/verify-capture.sh's
            # equivalent check: CAPTURE-STATUS.md's per-procedure table lists every
            # one of the 82 originally-'missing' names, not only the 4 genuinely
            # absent ones -- a name appearing there is NOT proof it was recorded as
            # a "not found" exception, only that the string is present somewhere in
            # the file. A name whose file went missing after being genuinely
            # captured would also match here and be silently downgraded to a
            # warning. This is a known, disclosed limitation, not a precise lookup.
            $warnGaps.Add("$name.sql absent, but '$name' appears in CAPTURE-STATUS.md -- treated as a recorded, owned exception, not a hard failure. This is a text-match, not a parsed lookup of that file's per-procedure outcome column -- confirm by hand that the entry actually says 'not found' (or another documented gap), not 'captured' with a file that has since gone missing.")
        }
        else {
            $hardGaps.Add("$name.sql absent (manifest status: missing) and '$name' is not mentioned in CAPTURE-STATUS.md -- undocumented gap.")
        }
    }
    elseif ($scriptedFile) {
        $normalizedScripted = $scriptedFile -replace '\\', '/'
        if ($presentByRelPath.ContainsKey($normalizedScripted)) {
            [void]$expectedRelPaths.Add($normalizedScripted)
        }
        else {
            $hardGaps.Add("'$normalizedScripted' (manifest status: $status, procedure '$name') does not exist on disk.")
        }
    }
    # Rows with a non-'missing' status and an empty scripted_file are not
    # expected to have ever had a file (none currently exist in
    # manifest.csv as of M0-01-03) -- nothing to check.
}

# Files present on disk with no manifest correspondence at all -- not a
# hard failure (the manifest is the worklist authority, but an extra file
# is a different, lesser risk than a missing one), but never deployed
# silently: reported as 'skipped' below and named here.
$unmappedFiles = New-Object System.Collections.Generic.List[string]
foreach ($rel in $presentByRelPath.Keys) {
    if (-not $expectedRelPaths.Contains($rel)) {
        $unmappedFiles.Add($rel)
    }
}

Write-Host "Manifest rows                 : $($manifestRows.Count)"
Write-Host "Expected files present        : $($expectedRelPaths.Count)"
Write-Host "Documented exceptions (warn)  : $($warnGaps.Count)"
Write-Host "Undocumented gaps (hard)      : $($hardGaps.Count)"
Write-Host "Files with no manifest row    : $($unmappedFiles.Count)"

foreach ($w in $warnGaps) { Write-Warning $w }
foreach ($u in $unmappedFiles) { Write-Warning "Not in manifest.csv, will be SKIPPED (not deployed): $u" }

if ($hardGaps.Count -gt 0) {
    Write-Host ""
    Write-Host "The following manifest-referenced procedures have no file and no recorded exception:" -ForegroundColor Red
    foreach ($g in $hardGaps) { Write-Host "  - $g" -ForegroundColor Red }

    if (-not $SkipCompletenessCheck) {
        Write-Host ""
        # Write-Host, not Write-Error: with $ErrorActionPreference = 'Stop' in
        # effect script-wide, Write-Error becomes an immediately-terminating
        # error (verified in this session) -- fine here since nothing else
        # needs to run afterward, but kept as Write-Host for consistency with
        # the deploy-loop failure path below, where it is NOT fine (see there).
        Write-Host "Completeness check FAILED -- $($hardGaps.Count) undocumented gap(s). Refusing to deploy a partial capture (that is exactly the false confidence this script exists to prevent). Re-run with -SkipCompletenessCheck to proceed anyway (loud warning, not a silent override)." -ForegroundColor Red
        exit 1
    }
    else {
        Write-Warning "-SkipCompletenessCheck was passed -- proceeding despite $($hardGaps.Count) undocumented gap(s) listed above. The resulting database will NOT have every procedure the application calls."
    }
}
else {
    Write-Host "Completeness check passed -- every manifest-referenced procedure has a file or a documented exception." -ForegroundColor Green
}

# -----------------------------------------------------------------------
# Deploy -- only files that map to a manifest row are applied. Stable
# order per the header comment ("Ordering").
# -----------------------------------------------------------------------
Write-Section "Deploy"
Write-Host "Target server   : $ServerInstance"
Write-Host "Target database : $Database"
if ($WhatIfDeploy) { Write-Host "(-WhatIfDeploy: no connection will be made)" -ForegroundColor DarkGray }

$toDeploy = @($allFiles | Where-Object {
    $rel = ($_.FullName.Substring($repoRoot.Length).TrimStart('\', '/') -replace '\\', '/')
    $expectedRelPaths.Contains($rel)
})

$applied = 0
$failed = 0
$skipped = $unmappedFiles.Count
$aborted = $false
$abortedFile = $null

foreach ($f in $toDeploy) {
    $rel = ($f.FullName.Substring($repoRoot.Length).TrimStart('\', '/') -replace '\\', '/')

    if ($f.Name -eq 'Sp_Print_PurchaseOrder.sql') {
        Write-Host "  [$rel] note: retained-but-unreferenced (manifest status: unreferenced) -- deploying anyway, see docs/kb/execution/tasks/M0-01-03.md" -ForegroundColor DarkYellow
    }

    if ($WhatIfDeploy) {
        Write-Host "  [WhatIfDeploy] would apply: $rel" -ForegroundColor DarkGray
        $applied++
        continue
    }

    if (-not $PSCmdlet.ShouldProcess("$Database ($ServerInstance)", "Apply $rel")) {
        $skipped++
        continue
    }

    try {
        $sql = [System.IO.File]::ReadAllText($f.FullName)
        $connArgs = Get-SqlCmdConnectionArgs
        Invoke-Sqlcmd @connArgs -Query $sql | Out-Null
        Write-Host "  [$rel] applied" -ForegroundColor Green
        $applied++
    }
    catch {
        # Write-Host, NOT Write-Error: with $ErrorActionPreference = 'Stop' in
        # effect script-wide, Write-Error is an immediately-terminating error
        # -- verified in this session -- which would skip $failed++,
        # $aborted = $true, and (critically) the entire Summary section below.
        # That would silently defeat binding requirement #7 ("prints a
        # summary ... or as much of one as was reached before an abort") the
        # very first time a file actually fails to apply.
        Write-Host ""
        Write-Host "FAILED to apply '$rel': $($_.Exception.Message)" -ForegroundColor Red
        $failed++
        $aborted = $true
        $abortedFile = $rel
        break
    }
}

$notAttempted = $toDeploy.Count - $applied - $failed

# -----------------------------------------------------------------------
# Summary (binding requirement #7) -- target database NAME only, never a
# connection string.
# -----------------------------------------------------------------------
Write-Section "Summary"
Write-Host "Target database   : $Database"
Write-Host "Applied           : $applied"
Write-Host "Skipped           : $skipped  (not in manifest.csv, or -WhatIf declined)"
Write-Host "Failed            : $failed"
if ($aborted) {
    Write-Host "Not attempted     : $notAttempted  (run aborted after '$abortedFile' failed)" -ForegroundColor Yellow
}

if ($aborted) {
    Write-Host ""
    Write-Host "Deployment ABORTED after '$abortedFile' failed to apply. $notAttempted file(s) were never attempted. Fix the named file and re-run -- this script is safe to re-run in full (every file is CREATE OR ALTER)." -ForegroundColor Red
    exit 1
}

if ($failed -gt 0) {
    # Defensive: should be unreachable given the break-on-first-failure
    # logic above, but never report success with a nonzero failure count.
    exit 1
}

Write-Host ""
Write-Host "Deployment complete." -ForegroundColor Green
exit 0
