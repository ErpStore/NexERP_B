-- db/tools/list-deployed-procedures.sql
--
-- Read-only. Lists every stored procedure whose name starts with "Sp_" in
-- the current database, with a definition fingerprint. Paste into SSMS
-- (or run via sqlcmd/Invoke-Sqlcmd) against ONE tenant database at a time —
-- multi-tenancy here is database-per-tenant (KB-012), so this query only
-- ever sees one tenant's procedures per execution.
--
-- Two uses:
--   1. M0-01-02 (this task): the DBA runs this first, before capture, to
--      see what actually exists server-side and cross-check it against
--      db/stored-procedures/manifest.csv's "missing" worklist —
--      catching name drift or a procedure that turns out to already exist
--      under a different name/schema before running Export-StoredProcedures.ps1.
--   2. M0-02 (cross-tenant drift, Q-14): this is deliberately reused, not
--      re-derived — run it against a second tenant and diff the
--      Definition_SHA2_256 column per procedure name to detect drift.
--
-- Contains no USE statement (run it against whichever database your SSMS
-- connection/Invoke-Sqlcmd -Database parameter is already pointed at) and
-- no credential of any kind.

SELECT
    s.name                                            AS SchemaName,
    o.name                                            AS ProcedureName,
    o.create_date                                     AS CreateDate,
    o.modify_date                                     AS ModifyDate,
    LEN(m.definition)                                 AS DefinitionLength,
    CONVERT(varchar(64), HASHBYTES('SHA2_256', m.definition), 2)
                                                       AS Definition_SHA2_256,
    m.is_schema_bound                                 AS IsSchemaBound,
    CASE WHEN m.definition IS NULL THEN 1 ELSE 0 END  AS DefinitionUnavailable
    -- DefinitionUnavailable = 1 typically means the module is
    -- WITH ENCRYPTION — OBJECT_DEFINITION()/sys.sql_modules.definition
    -- both return NULL for encrypted procedures. This tool cannot capture
    -- those; if any row shows 1 here, escalate to a human rather than
    -- treating it as "not found".
FROM sys.objects AS o
JOIN sys.schemas AS s     ON o.schema_id = s.schema_id
LEFT JOIN sys.sql_modules AS m ON m.object_id = o.object_id
WHERE o.type = 'P'
  AND o.name LIKE 'Sp[_]%'
ORDER BY s.name, o.name;
