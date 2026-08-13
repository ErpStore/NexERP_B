<#
.SYNOPSIS
    Captures the DDL of every "missing" stored procedure from a live tenant
    database into db/stored-procedures/, one idempotent .sql file per
    procedure.

.DESCRIPTION
    ############################################################################
    # THIS SCRIPT IS UNVERIFIED. It has never been executed against a
    # database in the environment that authored it (that environment has no
    # database credentials, no network path to a SQL Server, and no
    # PowerShell runtime with the SqlServer module installed to even
    # syntax-check it end-to-end).
    #
    # Before using this against any tenant that matters, the operator MUST:
    #   1. Rehearse it against a NON-PRODUCTION copy of a tenant database
    #      first, and inspect the output files before trusting them.
    #   2. Confirm the SqlServer PowerShell module (which provides
    #      Invoke-Sqlcmd) is installed and importable: `Get-Module -ListAvailable SqlServer`.
    #      If it is not installed: `Install-Module -Name SqlServer -Scope CurrentUser`.
    #      This script has not verified that cmdlet's exact behavior across
    #      SqlServer module versions — different versions have shipped
    #      different default behaviors for NULL handling and output typing.
    #   3. Read db/RUNBOOK-capture-stored-procedures.md in full before running
    #      this against anything that matters.
    ############################################################################

    Reads db/stored-procedures/manifest.csv (produced by task M0-01-01) and,
    for every row whose status is "missing", queries the target SQL Server
    for that procedure's OBJECT_DEFINITION and writes it to
    db/stored-procedures/<ProcedureName>.sql as
    "CREATE OR ALTER PROCEDURE ...", UTF-8 without BOM, LF line endings.

    It does NOT alter the captured body in any way beyond that one
    substitution (the leading CREATE/ALTER keyword pair). It does NOT
    reformat, re-indent, add SET options, or fix anything it finds odd.
    Any oddity is the operator's finding to record in
    db/stored-procedures/CAPTURE-STATUS.md, not this script's problem to fix.

    Connection details are NEVER hardcoded here and NEVER written to any
    file by this script. Pass them as parameters or via environment
    variables at invocation time. This follows the same rule task M0-03
    applies to the application's own configuration — this tooling does not
    invent a separate mechanism, and does not read any appsettings*.json.

.PARAMETER ServerInstance
    The SQL Server instance to connect to, e.g. "tenant-sql-01" or
    "tenant-sql-01\INSTANCENAME,1433". Required. Pass this at invocation
    time; do not bake it into a saved copy of this script.

.PARAMETER Database
    The target tenant database name. Required, same rule as ServerInstance.

.PARAMETER Credential
    Optional PSCredential for SQL authentication. If omitted, the script
    uses integrated (Windows) authentication via -TrustedConnection, which
    is the preferred mode. Never pass a plaintext password on the command
    line — build the PSCredential interactively with Get-Credential, or via
    an environment-variable-backed SecureString the caller constructs
    themselves. This script never echoes, logs, or persists it.

.PARAMETER ManifestPath
    Path to manifest.csv. Defaults to db/stored-procedures/manifest.csv
    relative to the repository root.

.PARAMETER OutputDir
    Directory to write captured .sql files into. Defaults to
    db/stored-procedures relative to the repository root.

.EXAMPLE
    # Windows Authentication (preferred)
    .\Export-StoredProcedures.ps1 -ServerInstance "tenant-sql-01" -Database "Tenant_Acme"

.EXAMPLE
    # SQL Authentication — build the credential interactively, never inline
    $cred = Get-Credential
    .\Export-StoredProcedures.ps1 -ServerInstance "tenant-sql-01" -Database "Tenant_Acme" -Credential $cred

.NOTES
    Requires the SqlServer PowerShell module (Invoke-Sqlcmd). Availability
    NOT verified in the authoring environment — see the UNVERIFIED banner
    above. If Invoke-Sqlcmd is unavailable and cannot be installed, the
    read-only query in db/tools/list-deployed-procedures.sql can be pasted
    into SSMS manually and its OBJECT_DEFINITION column output saved by
    hand per procedure, following the same file-naming and CREATE OR ALTER
    substitution rule.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ServerInstance,

    [Parameter(Mandatory = $true)]
    [string]$Database,

    [Parameter(Mandatory = $false)]
    [System.Management.Automation.PSCredential]$Credential,

    [Parameter(Mandatory = $false)]
    [string]$ManifestPath = (Join-Path $PSScriptRoot "..\stored-procedures\manifest.csv"),

    [Parameter(Mandatory = $false)]
    [string]$OutputDir = (Join-Path $PSScriptRoot "..\stored-procedures")
)

$ErrorActionPreference = "Stop"

# --- 0. Preconditions -------------------------------------------------------

if (-not (Get-Module -ListAvailable -Name SqlServer)) {
    Write-Error @"
The SqlServer PowerShell module is not installed (Invoke-Sqlcmd unavailable).
Install it with:
    Install-Module -Name SqlServer -Scope CurrentUser
or use db/tools/list-deployed-procedures.sql manually in SSMS instead.
This script has not verified its own behavior against any SqlServer module
version — read the UNVERIFIED banner in this file's header before proceeding.
"@
    exit 1
}
Import-Module SqlServer -ErrorAction Stop

if (-not (Test-Path $ManifestPath)) {
    Write-Error "Manifest not found at '$ManifestPath'. M0-01-01 must complete first."
    exit 1
}

if (-not (Test-Path $OutputDir)) {
    Write-Error "Output directory '$OutputDir' does not exist. Expected it to already exist from M0-01-01."
    exit 1
}

# --- 1. Read the worklist: manifest rows with status == missing ------------
# Deliberately does NOT enumerate every procedure in the database — only
# what the application actually references, per M0-01-01's classification.
# case_mismatch is intentionally excluded: that row's *correct* spelling is
# already scripted (Existing Store Procedures/StoredProcedures/); this
# script's job is the `missing` set only. Re-run M0-01-01's reconciliation,
# not this script, if the case_mismatch resolution needs to change.

$manifestRows = Import-Csv -Path $ManifestPath
$worklist = $manifestRows | Where-Object { $_.status -eq "missing" } | Select-Object -ExpandProperty procedure_name -Unique

if ($worklist.Count -eq 0) {
    Write-Warning "No rows with status=missing found in the manifest. Nothing to capture."
    exit 0
}

Write-Host "Worklist: $($worklist.Count) procedures to capture from [$Database] on [$ServerInstance]."
Write-Host ""

# --- 2. Build the connection arguments (never persisted, never logged) -----

$sqlcmdArgs = @{
    ServerInstance = $ServerInstance
    Database       = $Database
    QueryTimeout   = 300   # matches the application's own ReportExecutor.cs command timeout
}
if ($Credential) {
    $sqlcmdArgs["Username"] = $Credential.UserName
    $sqlcmdArgs["Password"] = $Credential.GetNetworkCredential().Password
} else {
    $sqlcmdArgs["TrustedConnection"] = $true
}

# --- 3. Capture each procedure ----------------------------------------------

$results = @()

foreach ($procName in $worklist) {

    # Ask for the definition and object metadata in one round trip.
    # sys.sql_modules.definition is the canonical source — the exact T-SQL
    # SSMS's "Script as CREATE" would show, unmodified.
    $query = @"
SELECT
    o.name                         AS ProcName,
    s.name                         AS SchemaName,
    m.definition                   AS Definition,
    (SELECT COUNT(*) FROM sys.objects o2
      JOIN sys.schemas s2 ON o2.schema_id = s2.schema_id
      WHERE o2.name = @procName AND o2.type = 'P')  AS MatchCount
FROM sys.objects o
JOIN sys.schemas s      ON o.schema_id = s.schema_id
JOIN sys.sql_modules m  ON m.object_id = o.object_id
WHERE o.name = @procName AND o.type = 'P';
"@
    # Invoke-Sqlcmd does not support named parameters uniformly across
    # module versions in all execution modes; substitute the literal name
    # ourselves, escaping any embedded single quote the SQL-string way
    # (' -> ''). Sp_* identifiers never legitimately contain a quote, but
    # escape defensively rather than assume.
    $escapedProcName = $procName -replace "'", "''"
    $procNameLiteral = "N'" + $escapedProcName + "'"
    $safeQuery = $query.Replace('@procName', $procNameLiteral)

    try {
        $row = Invoke-Sqlcmd @sqlcmdArgs -Query $safeQuery -ErrorAction Stop
    } catch {
        Write-Host "  [ERROR]            $procName — query failed: $($_.Exception.Message)"
        $results += [PSCustomObject]@{ Procedure = $procName; Outcome = "query_error" }
        continue
    }

    if (-not $row -or $row.Count -eq 0) {
        Write-Host "  [NOT FOUND]        $procName — no object with this name and type P in [$Database]"
        $results += [PSCustomObject]@{ Procedure = $procName; Outcome = "not_found" }
        continue
    }

    $matchCount = ($row | Select-Object -First 1).MatchCount
    if ($matchCount -gt 1) {
        Write-Host "  [MULTIPLE MATCH]   $procName — $matchCount objects matched; not captured, needs human resolution"
        $results += [PSCustomObject]@{ Procedure = $procName; Outcome = "multiple_matched" }
        continue
    }

    $schemaName = ($row | Select-Object -First 1).SchemaName
    if ($schemaName -ne "dbo") {
        Write-Host "  [NON-DBO SCHEMA]   $procName — found under schema '$schemaName', not 'dbo'. Captured, but flag in CAPTURE-STATUS.md: the application calls EXEC dbo.<name> unconditionally (ReportExecutor.cs:27)."
    }

    $definition = ($row | Select-Object -First 1).Definition
    if ([string]::IsNullOrWhiteSpace($definition)) {
        # Fail loudly rather than write an empty/truncated file.
        Write-Host "  [ERROR]            $procName — definition returned empty/null. NOT writing a file. Check VIEW DEFINITION permission and whether the procedure is WITH ENCRYPTION (OBJECT_DEFINITION returns NULL for encrypted modules — this script cannot capture those; escalate to a human)."
        $results += [PSCustomObject]@{ Procedure = $procName; Outcome = "empty_definition" }
        continue
    }

    # --- The ONLY permitted transformation: normalise the leading
    #     CREATE PROCEDURE / ALTER PROCEDURE / CREATE OR ALTER PROCEDURE
    #     into CREATE OR ALTER PROCEDURE, for idempotent redeployment.
    #     Nothing else about the body is touched.
    $normalized = [regex]::Replace(
        $definition,
        '^\s*CREATE\s+(OR\s+ALTER\s+)?PROC(EDURE)?\b',
        'CREATE OR ALTER PROCEDURE',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    )

    if ($normalized -eq $definition -and $definition -notmatch '(?i)^\s*CREATE\s+OR\s+ALTER\s+PROC') {
        # The regex didn't match at all — the definition doesn't start with
        # a recognizable CREATE/ALTER PROCEDURE. Fail loudly; do not guess.
        Write-Host "  [ERROR]            $procName — definition does not start with a recognizable CREATE/ALTER PROCEDURE statement. NOT writing a file; needs human review."
        $results += [PSCustomObject]@{ Procedure = $procName; Outcome = "unrecognized_header" }
        continue
    }

    # Normalize line endings to LF and strip any BOM, per the README
    # convention. Trailing whitespace/newline handling is deliberately
    # minimal — this is a transcription, not a formatter.
    $lfBody = $normalized -replace "`r`n", "`n" -replace "`r", "`n"
    $lfBody = $lfBody.TrimStart([char]0xFEFF)  # strip a UTF-8 BOM if present in the source

    $outPath = Join-Path $OutputDir "$procName.sql"
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($outPath, $lfBody, $utf8NoBom)

    Write-Host "  [CAPTURED]         $procName -> $outPath"
    $results += [PSCustomObject]@{ Procedure = $procName; Outcome = "captured" }
}

# --- 4. Summary ---------------------------------------------------------

Write-Host ""
Write-Host "=== Summary ==="
$results | Group-Object Outcome | ForEach-Object { Write-Host ("  {0,-20} {1}" -f $_.Name, $_.Count) }
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Fill in db/stored-procedures/CAPTURE-STATUS.md with the tenant identifier, capture date and operator."
Write-Host "  2. Run: bash db/tools/verify-capture.sh"
Write-Host "  3. Paste that script's output into the task ticket."
Write-Host ""
Write-Host "REMINDER: this connection's credentials were never written to any file by this script. Do not paste a connection string, Tenants-table row, or password into a ticket, chat, log, or commit."
