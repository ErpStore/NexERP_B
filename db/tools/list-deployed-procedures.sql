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
--
-- THIS FILE CONTAINS TWO QUERIES.
--   Query A (below)  -- the human-readable listing written by M0-01-02.
--                       Read it on screen. Do NOT export it as the drift
--                       fingerprint: its Note column contains commas and
--                       would break the CSV.
--   Query B (bottom) -- FINGERPRINT_QUERY_VERSION 2 (M0-02). The one and
--                       only query to run and export per tenant for the
--                       drift comparison (db/RUNBOOK-tenant-drift-check.md).
--                       It emits hash_raw and hash_normalised, which
--                       Query A does not.
--
-- VERSION NOTE (M0-02, 2026-08-17): Query A's single DefinitionSha256Hex
-- column is computed over a definition with only CR and TAB stripped. It is
-- therefore neither hash_raw (verbatim) nor hash_normalised (whitespace runs
-- collapsed and case folded), and MUST NOT be used as either in a drift
-- comparison. Any tenant fingerprinted before FINGERPRINT_QUERY_VERSION 2
-- existed must be re-collected with Query B -- mixing query versions
-- silently invalidates the comparison.

-- =====================================================================
-- Query A -- human-readable listing (M0-01-02). Not the drift export.
-- =====================================================================
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


-- =====================================================================
-- Query B -- PER-TENANT DRIFT FINGERPRINT  (M0-02, Q-14)
-- FINGERPRINT_QUERY_VERSION 2
--
-- Run THIS query, unchanged, against every tenant database being compared,
-- and export its result set to db/drift/<tenant-label>.csv. A query that
-- differs between tenants produces a comparison of the query, not of the
-- procedures.
--
-- Column names below are the CSV header expected by
-- db/tools/compare-tenant-fingerprints.sh, exactly, in this order:
--   schema_name,procedure_name,create_date,modify_date,definition_length,hash_raw,hash_normalised
-- The comparison script FAILS LOUDLY on any other header, because a
-- different header means a different query version.
--
-- Read-only. No USE statement, no credential, no host name, no tenant name
-- appears in this file -- connect to the target tenant database yourself
-- before running it (see db/RUNBOOK-tenant-drift-check.md).
--
-- The two hashes are the whole design:
--   hash_raw         SHA2_256 over OBJECT_DEFINITION(object_id) VERBATIM.
--   hash_normalised  SHA2_256 over the definition with line endings
--                    normalised, tabs and newlines turned into spaces, runs
--                    of whitespace collapsed to one space, outer whitespace
--                    trimmed, and the whole thing case folded.
-- hash_raw differing while hash_normalised matches => the drift is COSMETIC.
-- Both differing => the drift is POTENTIALLY BEHAVIOURAL and is escalated.
--
-- Honest limits of hash_normalised (state these wherever it is used):
--   * Whitespace collapsing cannot distinguish a semantic difference inside
--     a string literal (e.g. 'A  B' vs 'A B') from mere formatting, so a
--     hash_normalised match is INFERRED equivalence, not proof.
--   * Case folding means a difference confined to a quoted literal's case
--     is also invisible.
--   * HASHBYTES accepts inputs larger than 8000 bytes only on SQL Server
--     2016 (13.x) and later. On an older instance both hash columns come
--     back NULL for long procedures -- that is a collection failure, not
--     "no drift". Do not hand back a CSV with NULL hashes.
--   * LEN() ignores trailing whitespace, so definition_length is a coarse
--     signal only; the hashes are the comparison.
--   * CHAR(1) and CHAR(2) are used as collapse sentinels below. A procedure
--     body containing either byte literally would normalise incorrectly;
--     that is vanishingly unlikely in T-SQL source and is recorded here
--     rather than defended against.
-- =====================================================================
SELECT
    s.name                                                        AS schema_name,
    p.name                                                        AS procedure_name,
    CONVERT(VARCHAR(23), p.create_date, 126)                      AS create_date,
    CONVERT(VARCHAR(23), p.modify_date, 126)                      AS modify_date,
    LEN(OBJECT_DEFINITION(p.object_id))                           AS definition_length,
    CONVERT(
        VARCHAR(64),
        HASHBYTES('SHA2_256', OBJECT_DEFINITION(p.object_id)),
        2
    )                                                              AS hash_raw,
    CONVERT(
        VARCHAR(64),
        HASHBYTES(
            'SHA2_256',
            LOWER(
                LTRIM(RTRIM(
                    -- 3. collapse runs of spaces to exactly one space
                    REPLACE(REPLACE(REPLACE(
                        -- 2. every line ending / tab becomes a space
                        REPLACE(REPLACE(REPLACE(
                            OBJECT_DEFINITION(p.object_id),
                        CHAR(13), CHAR(10)), CHAR(10), N' '), CHAR(9), N' '),
                    N' ', CHAR(1) + CHAR(2)), CHAR(2) + CHAR(1), N''), CHAR(1) + CHAR(2), N' ')
                ))
            )
        ),
        2
    )                                                              AS hash_normalised
FROM sys.procedures p
JOIN sys.schemas s ON p.schema_id = s.schema_id
WHERE p.name LIKE N'Sp[_]%'   -- '_' is a LIKE wildcard; [_] escapes it to a literal underscore
ORDER BY s.name, p.name;
