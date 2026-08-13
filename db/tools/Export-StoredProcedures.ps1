<#
================================================================================
 db/tools/Export-StoredProcedures.ps1
================================================================================

 STATUS: run against a real database, twice, with real bugs found and fixed.

 It was authored by an AI session with no database credentials and no
 network path to a SQL Server instance, by design (task M0-01-02 / R-01,
 R-02 -- see docs/kb/risks/technical-debt-register.md) -- so it could not
 be tested by that session. It has since actually been executed by the repo
 owner:
   - 2026-08-13, first rehearsal: against NexGenErpDb (DESKTOP-FIIBE97\
     SQLEXPRESS), which at the time had zero Sp_* procedures deployed. Found
     three parameter-shape/certificate bugs against a modern (22.x) SqlServer
     module -- fixed (see git history).
   - 2026-08-13, real capture: against the same NexGenErpDb after the repo
     owner manually deployed a procedure set originally scripted from
     IQSMARTDEMO_DB_2025-26 into it. 75 of 82 `missing` procedures captured
     cleanly; 3 initially failed ("unrecognized leading statement") because
     their deployed definitions carry a leading `-- ====...` header comment
     that the leading-statement regex did not tolerate -- fixed here, same
     day. 4 were genuinely not found (real gap, not a tool defect).
   - See db/stored-procedures/CAPTURE-STATUS.md for the full record and
     provenance caveats (NexGenErpDb is the query source; IQSMARTDEMO_DB_2025-26
     is the DDL's actual origin -- these are not the same tenant).
 This has NOT been rehearsed against any OTHER tenant database, and no
 session has verified this script's behaviour against a genuinely
 unmodified, connection-only-once production tenant. Treat every new
 target as unverified until you have:
   1. Read db/RUNBOOK-capture-stored-procedures.md in full.
   2. Run with -DryRun first and inspected the summary.
   3. Spot-checked a handful of output files by hand -- diff one against
      what you see in SSMS's "Script Procedure as CREATE" for the same
      object.

 Cmdlet / module availability has NOT been verified in this environment
 either:
   - This script uses Invoke-Sqlcmd, which requires the "SqlServer"
     PowerShell module (the older "SQLPS" module is deprecated and may
     behave differently). It is NOT bundled with Windows PowerShell or
     PowerShell 7 by default. Check with:
         Get-Module -ListAvailable -Name SqlServer
     before assuming it is present on the machine you run this from.
   - If it is not installed and cannot be installed (no internet access,
     no permission to install modules), do not try to work around it here
     -- use db/tools/list-deployed-procedures.sql directly in SSMS or Azure
     Data Studio instead. See the runbook's Preconditions section.
   - -Credential support in Invoke-Sqlcmd (SQL-authentication path) requires
     a reasonably recent SqlServer module version (21.1.18068+). Older
     versions only accept -Username/-Password as plain arguments, which is
     a weaker guarantee (visible to anything that can read the process's
     command line for the duration of the call). This script prefers
     Windows Authentication for exactly this reason -- see the -Credential
     parameter below and the runbook's secrets section.

 Secrets policy (binding, see docs/kb/risks/technical-debt-register.md
 R-01/R-02 and db/RUNBOOK-capture-stored-procedures.md):
   - No connection string, server name, database name, username or password
     is hardcoded anywhere in this file. All of it comes from parameters or
     environment variables supplied by the caller at run time.
   - This script never writes a connection string, credential, or
     environment-variable value to any file, log, or transcript. It writes
     only captured procedure DDL (which this task forbids from containing
     any of those things -- see the forbidden-pattern check below) and
     progress/status text to the console.
   - Do not redirect this script's console output to a file that gets
     committed. It should not contain secrets, but treat that as a second
     line of defence, not the first.

 What this script does:
   1. Reads db/stored-procedures/manifest.csv and takes every row whose
      status is exactly "missing" as its worklist -- it does not enumerate
      the database's own contents, so a procedure the application does not
      call is never captured (README.md, "What belongs here").
   2. For each name, looks it up in the target database's sys.procedures
      (joined to sys.schemas so a non-dbo match is visible, since the
      application always calls dbo.{procedureName} --
      V.SMART.Shared/BusinessLayer/BusinessService/ReportService/
      TrackReportService/ReportExecutor.cs:25,27,35-39,105-107).
   3. Captures OBJECT_DEFINITION() for the single unambiguous match, and
      cross-checks the client-received length against a server-computed
      LEN() in the same round trip -- if they disagree, the transfer was
      truncated (a known Invoke-Sqlcmd/console-width gotcha for long
      nvarchar(max) results) and the script refuses to write that file
      rather than risk writing a silently truncated procedure body (see
      "Fail loudly" requirement in db/stored-procedures/README.md and
      docs/kb/execution/tasks/M0-01-02.md).
   4. Applies the ONE permitted transformation -- converts the leading
      CREATE PROCEDURE / ALTER PROCEDURE keyword pair to
      CREATE OR ALTER PROCEDURE -- and normalizes line endings to LF. It
      does not reformat, re-indent, "fix", or otherwise touch the body.
   5. Writes db/stored-procedures/<ProcedureName>.sql, UTF-8 without a BOM,
      LF line endings, overwriting cleanly so the script is safely
      re-runnable / resumable.
   6. Reports, per procedure: captured / not found / permission denied (or
      encrypted) / multiple objects matched / failed (truncation suspected)
      / query error / skipped.

 What this script deliberately does NOT do:
   - It does not invent, reconstruct, or guess a procedure body under any
     circumstance. If it cannot get a clean, complete definition from the
     server, it writes nothing for that procedure and reports the reason.
   - It does not touch anything under "Existing Store Procedures/
     StoredProcedures/" (the 13 already-scripted files) -- that is out of
     scope for this task (M0-01-03 relocates them).
   - It does not modify db/stored-procedures/manifest.csv or README.md.

================================================================================
#>

#Requires -Version 5.1
[CmdletBinding(SupportsShouldProcess = $true, DefaultParameterSetName = 'TrustedConnection')]
param(
    # --- Target ---------------------------------------------------------
    [Parameter(Mandatory = $true)]
    [string]$ServerInstance,

    [Parameter(Mandatory = $true)]
    [string]$Database,

    # --- Authentication (choose exactly one style) -----------------------
    # Default / preferred: Windows Authentication. No parameter needed --
    # this is what happens if neither -Credential nor
    # -SqlUserName/-SqlPasswordEnvVar is supplied.
    [Parameter(ParameterSetName = 'SqlCredential')]
    [System.Management.Automation.PSCredential]$Credential,

    # Alternative SQL-auth path: username on the command line (not a
    # secret), password read from an environment variable at run time
    # (never a script parameter, never echoed, never written anywhere).
    [Parameter(ParameterSetName = 'SqlEnvVar')]
    [string]$SqlUserName,

    [Parameter(ParameterSetName = 'SqlEnvVar')]
    [string]$SqlPasswordEnvVar,

    # Modern Invoke-Sqlcmd (SqlServer module 22.x+, Microsoft.Data.SqlClient-
    # based) encrypts connections and validates the server certificate by
    # default. Most on-prem / local SQL Server instances -- including every
    # tenant connection string this application already uses, e.g.
    # ApplicationDbContextFactory.cs -- run with a self-signed certificate,
    # so a strict validation fails with an SSL trust error. This switch is
    # the capture tool's equivalent of that connection string's
    # TrustServerCertificate=True. It does not weaken authentication (you
    # still need real credentials) -- only certificate-chain validation.
    # Prefer NOT passing this against a server with a properly issued
    # certificate.
    [switch]$TrustServerCertificate,

    # --- Worklist / output ------------------------------------------------
    [string]$ManifestPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'stored-procedures\manifest.csv'),
    [string]$OutputDirectory = (Join-Path (Split-Path -Parent $PSScriptRoot) 'stored-procedures'),

    [int]$CommandTimeoutSeconds = 300,
    [int]$ConnectionTimeoutSeconds = 30,

    # Query and report without writing any .sql files. Use this for the
    # first rehearsal pass against a non-production copy.
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

# -----------------------------------------------------------------------
# Preconditions
# -----------------------------------------------------------------------
if (-not (Get-Module -ListAvailable -Name SqlServer)) {
    throw @"
The 'SqlServer' PowerShell module is not available on this machine.
Install it with (requires internet access):
    Install-Module -Name SqlServer -Scope CurrentUser
If that is not possible in your environment, do not try to route around
this script -- use db/tools/list-deployed-procedures.sql directly in SSMS
or Azure Data Studio instead. See db/RUNBOOK-capture-stored-procedures.md,
Preconditions.
"@
}
Import-Module SqlServer -ErrorAction Stop

# Different major versions of the SqlServer module expose different
# Invoke-Sqlcmd parameter shapes (see header notes above) -- e.g. the
# pre-21.x API requires an explicit -TrustedConnection switch for Windows
# Authentication, while 22.x+ (built on Microsoft.Data.SqlClient) dropped
# that switch and simply defaults to Windows/Integrated auth whenever
# neither -Credential nor -Username/-Password is supplied. Detect what's
# actually available at run time instead of assuming one shape -- an
# assumption that does not hold is exactly how this script would otherwise
# fail loudly with "parameter cannot be found" against a real database,
# which is what happened during this task's own rehearsal.
$script:InvokeSqlcmdParams = (Get-Command Invoke-Sqlcmd).Parameters.Keys
$script:SupportsTrustedConnection = $script:InvokeSqlcmdParams -contains 'TrustedConnection'
$script:SupportsCredentialParam = $script:InvokeSqlcmdParams -contains 'Credential'

if (-not (Test-Path -LiteralPath $ManifestPath)) {
    throw "Manifest not found at '$ManifestPath'. Run M0-01-01 first, or pass -ManifestPath explicitly."
}

if ($PSCmdlet.ParameterSetName -eq 'SqlEnvVar') {
    if (-not $SqlUserName -or -not $SqlPasswordEnvVar) {
        throw "Both -SqlUserName and -SqlPasswordEnvVar are required together for SQL authentication via environment variable."
    }
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

# -----------------------------------------------------------------------
# Build the connection argument set once. Never logged, never written to
# a file -- held only in memory for the lifetime of this script.
# -----------------------------------------------------------------------
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
                # Older Invoke-Sqlcmd (pre-21.1.18068) has no -Credential
                # parameter at all -- fall back to plain -Username/-Password,
                # which the header comment already documents as a weaker
                # guarantee (visible on the process command line for the
                # duration of the call). Not a silent choice: warn every time.
                Write-Warning "  Installed Invoke-Sqlcmd has no -Credential parameter (older SqlServer module). Falling back to plain -Username/-Password -- see this script's header comment."
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
                Write-Warning "  Installed Invoke-Sqlcmd has no -Credential parameter (older SqlServer module). Falling back to plain -Username/-Password -- see this script's header comment."
                $connArgs['Username'] = $SqlUserName
                $connArgs['Password'] = $plainPassword
            }
            # $plainPassword is not explicitly zeroed -- PowerShell strings are
            # immutable and .NET does not guarantee secure erasure of plain
            # System.String contents. This is a known, accepted limitation;
            # prefer Windows Authentication wherever the environment allows it.
        }
        default {
            # Windows Authentication. Pre-21.x Invoke-Sqlcmd needs the
            # explicit -TrustedConnection switch; 22.x+ (Microsoft.Data.
            # SqlClient-based) has no such parameter and simply defaults to
            # Windows/Integrated auth when no credential is supplied at all --
            # setting it there would fail with "parameter cannot be found",
            # exactly as this script did on its first real run.
            if ($script:SupportsTrustedConnection) {
                $connArgs['TrustedConnection'] = $true
            }
        }
    }
    return $connArgs
}

# -----------------------------------------------------------------------
# Read the worklist -- status == missing, exactly as M0-01-01 recorded it.
# -----------------------------------------------------------------------
$manifest = Import-Csv -LiteralPath $ManifestPath
$worklist = @($manifest | Where-Object { $_.status -eq 'missing' })

Write-Host "Worklist: $($worklist.Count) procedure(s) with status 'missing' in '$ManifestPath'." -ForegroundColor Cyan
if ($worklist.Count -eq 0) {
    Write-Warning "Nothing to capture -- manifest has no 'missing' rows. Exiting."
    return
}

$results = New-Object System.Collections.Generic.List[object]

foreach ($row in $worklist) {
    $procName = $row.procedure_name
    Write-Host ""
    Write-Host "==> $procName" -ForegroundColor Cyan

    # Defence in depth: procedure names come only from manifest.csv, which
    # this task must not edit, and M0-01-01's extraction is scoped to
    # Sp_[A-Za-z0-9_]+ -- but refuse to interpolate anything that doesn't
    # look like a bare SQL identifier into a query string, regardless.
    if ($procName -notmatch '^[A-Za-z0-9_]+$') {
        Write-Warning "  Skipping -- '$procName' is not a bare identifier; refusing to interpolate it into SQL."
        $results.Add([pscustomobject]@{ ProcedureName = $procName; Status = 'skipped (unsafe name)' })
        continue
    }

    $connArgs = Get-SqlCmdConnectionArgs

    # --- Step 1: find it, and detect ambiguity / wrong schema -----------
    $lookupQuery = "SELECT s.name AS SchemaName, p.name AS ProcName FROM sys.procedures p JOIN sys.schemas s ON p.schema_id = s.schema_id WHERE p.name = N'$procName';"
    try {
        $matches = @(Invoke-Sqlcmd @connArgs -Query $lookupQuery)
    }
    catch {
        Write-Warning "  Lookup query failed: $($_.Exception.Message)"
        $results.Add([pscustomobject]@{ ProcedureName = $procName; Status = 'query error (lookup)' })
        continue
    }

    if ($matches.Count -eq 0) {
        Write-Host "  not found" -ForegroundColor Yellow
        $results.Add([pscustomobject]@{ ProcedureName = $procName; Status = 'not found' })
        continue
    }
    if ($matches.Count -gt 1) {
        $schemas = ($matches | ForEach-Object { $_.SchemaName }) -join ', '
        Write-Warning "  Exists under multiple schemas: $schemas -- ambiguous. Skipping; record this as a finding, do not guess which one the application means."
        $results.Add([pscustomobject]@{ ProcedureName = $procName; Status = "multiple objects matched ($schemas)" })
        continue
    }

    $schemaName = $matches[0].SchemaName
    if ($schemaName -ne 'dbo') {
        Write-Warning "  Exists only under schema '$schemaName', not 'dbo'. The application always executes dbo.$procName (ReportExecutor.cs) -- this is very likely a live defect, not a false positive. Capturing anyway; record it as a finding in CAPTURE-STATUS.md."
    }

    # --- Step 2: capture the definition, with a server-side length check
    #     to catch client-side truncation before it becomes a file. -------
    $qualified = "[$schemaName].[$procName]"
    $defQuery = "SELECT OBJECT_DEFINITION(OBJECT_ID(N'$qualified')) AS Def, LEN(OBJECT_DEFINITION(OBJECT_ID(N'$qualified'))) AS ExpectedLen;"
    try {
        # -MaxCharLength: Invoke-Sqlcmd truncates character results to a
        # small default (historically 4000/2033-ish, version-dependent)
        # unless told otherwise -- a real, known gotcha for nvarchar(max)
        # results like OBJECT_DEFINITION on anything but a trivial
        # procedure. The ExpectedLen cross-check below is the actual
        # safety net; this is a first line of defence.
        $defResult = @(Invoke-Sqlcmd @connArgs -Query $defQuery -MaxCharLength 200000000)[0]
    }
    catch {
        Write-Warning "  Definition query failed: $($_.Exception.Message)"
        $results.Add([pscustomobject]@{ ProcedureName = $procName; Status = 'query error (definition)' })
        continue
    }

    $definition = $defResult.Def
    $expectedLen = $defResult.ExpectedLen

    if ([string]::IsNullOrEmpty($definition)) {
        Write-Warning "  OBJECT_DEFINITION returned NULL/empty. Either the caller lacks VIEW DEFINITION on this object, or it is WITH ENCRYPTION. Treating as permission denied -- this is not something this script can resolve; escalate to whoever granted the DBA's login its rights."
        $results.Add([pscustomobject]@{ ProcedureName = $procName; Status = 'permission denied (or encrypted)' })
        continue
    }

    if ($null -ne $expectedLen -and $definition.Length -ne [int]$expectedLen) {
        Write-Warning "  TRUNCATION SUSPECTED -- server reports LEN=$expectedLen but $($definition.Length) characters were received. Refusing to write a possibly-truncated file. Re-run with a larger -MaxCharLength or capture this one manually via db/tools/list-deployed-procedures.sql / SSMS 'Script as CREATE'."
        $results.Add([pscustomobject]@{ ProcedureName = $procName; Status = "FAILED - truncation suspected (expected $expectedLen chars, got $($definition.Length))" })
        continue
    }

    # --- Step 3: the ONE permitted transformation ------------------------
    # Tolerate -- but do not strip -- a leading SQL comment block before the
    # CREATE/ALTER PROCEDURE keyword. Some procedures in this tenant are
    # deployed with a header comment (e.g. a "-- ====...." divider) directly
    # above the CREATE statement, in the same batch, so OBJECT_DEFINITION()
    # legitimately returns that comment as part of the module text.
    # db/tools/verify-capture.sh's own spec ("first *non-comment* statement
    # is CREATE OR ALTER PROCEDURE") already allows this; this regex
    # previously did not, and silently refused every procedure shaped that
    # way. Found during M0-01-02's real capture run, 2026-08-13
    # (Sp_CreditDebitNoteSummaryReport, Sp_GetHSNSummaryReport,
    # Sp_TDSReport -- see db/stored-procedures/CAPTURE-STATUS.md). The
    # comment text itself is never altered or removed -- only the
    # CREATE/ALTER keyword pair that follows it is ever rewritten, and only
    # the first match in the definition is touched.
    $commentOrSpace = '(?:\s|--[^\r\n]*(?:\r?\n|$)|/\*[\s\S]*?\*/)*'
    $createOrAlterPattern = "^$commentOrSpace(CREATE\s+OR\s+ALTER|CREATE|ALTER)\s+PROCEDURE"
    if ($definition -notmatch "(?i)$createOrAlterPattern") {
        Write-Warning "  Did not find a recognizable leading CREATE/ALTER PROCEDURE statement (even allowing a leading comment block). Refusing to guess -- writing nothing. Inspect this one by hand."
        $results.Add([pscustomobject]@{ ProcedureName = $procName; Status = 'FAILED - unrecognized leading statement' })
        continue
    }
    $leadingStatementRegex = New-Object System.Text.RegularExpressions.Regex(
        "^($commentOrSpace)(CREATE\s+OR\s+ALTER|CREATE|ALTER)(\s+PROCEDURE)",
        [System.Text.RegularExpressions.RegexOptions]'IgnoreCase'
    )
    $converted = $leadingStatementRegex.Replace($definition, '$1CREATE OR ALTER$3', 1)

    # Line-ending normalization only (README convention #3 -- encoding, not
    # a body edit). CR/CRLF -> LF.
    $normalized = $converted -replace "`r`n", "`n" -replace "`r", "`n"

    $outPath = Join-Path $OutputDirectory "$procName.sql"

    if ($DryRun) {
        Write-Host "  [DryRun] would write $($normalized.Length) chars -> $outPath" -ForegroundColor DarkGray
        $results.Add([pscustomobject]@{ ProcedureName = $procName; Status = 'dry-run (not written)' })
        continue
    }

    if ($PSCmdlet.ShouldProcess($outPath, 'Write captured procedure DDL')) {
        $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
        [System.IO.File]::WriteAllText($outPath, $normalized, $utf8NoBom)
        Write-Host "  captured -> $outPath ($($normalized.Length) chars)" -ForegroundColor Green
        $results.Add([pscustomobject]@{ ProcedureName = $procName; Status = 'captured' })
    }
}

# -----------------------------------------------------------------------
# Summary -- paste this into CAPTURE-STATUS.md / the task ticket.
# -----------------------------------------------------------------------
Write-Host ""
Write-Host "==================== Summary ====================" -ForegroundColor Cyan
$results | Format-Table -AutoSize | Out-Host
$results | Group-Object Status | Sort-Object Count -Descending | Format-Table Name, Count -AutoSize | Out-Host

$problemStatuses = $results | Where-Object { $_.Status -like 'FAILED*' -or $_.Status -like 'query error*' -or $_.Status -eq 'not found' -or $_.Status -like 'permission denied*' -or $_.Status -like 'multiple objects matched*' }
if ($problemStatuses.Count -gt 0) {
    Write-Warning "$($problemStatuses.Count) of $($worklist.Count) procedure(s) were not captured cleanly. Record each one in db/stored-procedures/CAPTURE-STATUS.md with a named owner -- do not leave it unrecorded."
}

Write-Host ""
Write-Host "Next step: bash db/tools/verify-capture.sh (run from the repository root)." -ForegroundColor Cyan
