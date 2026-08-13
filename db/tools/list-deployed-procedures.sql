-- db/tools/list-deployed-procedures.sql
--
-- Read-only. Lists every stored procedure whose name starts with "Sp_" that
-- is deployed in the database the caller is currently connected to.
--
-- No USE statement -- point SSMS / Azure Data Studio / sqlcmd at the target
-- tenant database yourself before running this (see
-- db/RUNBOOK-capture-stored-procedures.md). No credential, connection
-- string, host name, or tenant name appears anywhere in this file.
--
-- Used by:
--   - M0-01-02 (this task), as the manual SSMS/Azure Data Studio fallback
--     for db/tools/Export-StoredProcedures.ps1 when the SqlServer
--     PowerShell module / Invoke-Sqlcmd is not available on the DBA's
--     machine. Paste the results (schema + name + counts + hash columns
--     only -- never a row from the Tenants table) into the task ticket if
--     asked to show what was found.
--   - M0-02, as the per-tenant stored-procedure fingerprint query for
--     detecting drift across tenant databases (Q-14,
--     docs/kb/open-questions.md). Reuse this query there rather than
--     writing a second, subtly different one.
--
-- Requires VIEW DEFINITION (or db_owner) on the target database to see
-- non-NULL DefinitionLength / DefinitionChecksum / DefinitionSha256Hex --
-- see the DefinitionNote column below for what a NULL means here.

SELECT
    s.name                                                        AS SchemaName,
    p.name                                                        AS ProcedureName,
    p.create_date                                                 AS CreateDate,
    p.modify_date                                                 AS ModifyDate,
    LEN(OBJECT_DEFINITION(p.object_id))                           AS DefinitionLength,
    -- Definitions are normalized (CR and TAB stripped) before hashing so
    -- that line-ending or indentation differences alone don't register as
    -- drift between environments -- only real content differences should.
    -- CHECKSUM is cheap but has known collisions; keep it only as a quick
    -- eyeball value. HASHBYTES(SHA2_256) is the column any drift decision
    -- (Q-14 / M0-02) should actually compare.
    CHECKSUM(
        REPLACE(REPLACE(OBJECT_DEFINITION(p.object_id), CHAR(13), ''), CHAR(9), '')
    )                                                              AS DefinitionChecksum,
    CONVERT(
        VARCHAR(64),
        HASHBYTES(
            'SHA2_256',
            REPLACE(REPLACE(OBJECT_DEFINITION(p.object_id), CHAR(13), ''), CHAR(9), '')
        ),
        2
    )                                                              AS DefinitionSha256Hex,
    CASE
        WHEN OBJECT_DEFINITION(p.object_id) IS NULL
            THEN 'NULL definition -- caller lacks VIEW DEFINITION on this object, or it is WITH ENCRYPTION'
        WHEN s.name <> 'dbo'
            THEN 'non-dbo schema -- the application always calls dbo.<name> (ReportExecutor.cs); this object is likely unreachable as-is'
        ELSE NULL
    END                                                            AS Note
FROM sys.procedures p
JOIN sys.schemas s ON p.schema_id = s.schema_id
WHERE p.name LIKE N'Sp[_]%'   -- '_' is a LIKE wildcard; [_] escapes it to a literal underscore
ORDER BY s.name, p.name;
