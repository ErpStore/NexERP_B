/* ============================================================================
   Q-10 -- numbering constraint and duplicate census
   Task M2-B12-02. Generated from KB-100 section 9
   (docs/kb/modules/document-numbering.md).

   READ-ONLY. This script only reads. It runs SELECT statements and reads
   system catalogue views. It changes no row, no schema object and no server
   setting. It is safe to run against a live production tenant.

   HOW TO RUN -- see Q-10-numbering-constraints.md (KB-101) beside this file.
   Short version: SSMS -> Query -> Results To -> Results to Text, then run the
   whole file against ONE tenant database and save the entire text output.
   Repeat per tenant, one output file each. Send the files back unedited.

   The file is split into batches by GO. If one series fails -- a table absent
   from this tenant, or a column named differently here -- only that batch
   stops. Every other batch still runs. Please send the output back WITH any
   error text rather than removing it.

   NOTE ON Suffix: financial-year suffixes are stored WITH A LEADING SLASH
   (for example /2025-26). A filter written as Suffix = '2025-26' matches
   nothing. This script never filters on a suffix literal -- it groups by the
   stored value -- so the stored form is what you will see.
   ============================================================================ */

SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;  -- avoids blocking live users
GO

/* ---- PREAMBLE: the output self-identifies, so no annotation is needed ---- */
SELECT
    'PREAMBLE'              AS Block,
    DB_NAME()               AS DatabaseName,
    @@SERVERNAME            AS ServerName,
    SYSUTCDATETIME()        AS RunAtUtc,
    @@VERSION               AS SqlServerVersion;
GO

/* ============================================================================
   BLOCK 1 -- CONSTRAINT INVENTORY
   Every unique index and unique key constraint in this database, with its full
   key column list, uniqueness, filter and primary-key flag. This is what
   answers Q-10: does the number column carry a unique constraint here?
   ============================================================================ */
SELECT
    'BLOCK1-CONSTRAINTS'                              AS Block,
    t.name                                            AS TableName,
    i.name                                            AS IndexName,
    i.type_desc                                       AS IndexType,
    i.is_unique                                       AS IsUnique,
    i.is_primary_key                                  AS IsPrimaryKey,
    i.is_unique_constraint                            AS IsUniqueConstraint,
    i.has_filter                                      AS HasFilter,
    ISNULL(i.filter_definition, '')                   AS FilterDefinition,
    STUFF((
        SELECT ', ' + c2.name +
               CASE WHEN ic2.is_descending_key = 1 THEN ' DESC' ELSE '' END
        FROM sys.index_columns AS ic2
        JOIN sys.columns AS c2
          ON c2.object_id = ic2.object_id AND c2.column_id = ic2.column_id
        WHERE ic2.object_id = i.object_id
          AND ic2.index_id  = i.index_id
          AND ic2.is_included_column = 0
        ORDER BY ic2.key_ordinal
        FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, '') AS KeyColumns
FROM sys.indexes AS i
JOIN sys.tables  AS t ON t.object_id = i.object_id
WHERE i.is_unique = 1
  AND t.is_ms_shipped = 0
ORDER BY t.name, i.name;
GO

/* Companion to block 1: every column whose name looks like a document-number
   column, indexed or not. This reveals a series whose table is present but
   whose number column is named differently in this tenant. */
SELECT
    'BLOCK1-NUMBERCOLUMNS'  AS Block,
    t.name                  AS TableName,
    c.name                  AS ColumnName,
    ty.name                 AS DataType,
    c.max_length            AS MaxLength,
    c.is_nullable           AS IsNullable
FROM sys.columns AS c
JOIN sys.tables  AS t  ON t.object_id = c.object_id
JOIN sys.types   AS ty ON ty.user_type_id = c.user_type_id
WHERE t.is_ms_shipped = 0
  AND (c.name LIKE '%No' OR c.name LIKE '%Number' OR c.name = 'Suffix')
ORDER BY t.name, c.name;
GO

/* ---------------------------------------------------------------------------
   SERIES 01 -- MfgDc.DcNo /scope:Suffix+CustId
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.MfgDc', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: DcNo + Suffix + CustId */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'MfgDc.DcNo /scope:Suffix+CustId' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [DcNo], [Suffix], [CustId], COUNT(*) AS RowsInGroup
        FROM dbo.[MfgDc]
        WHERE [DcNo] IS NOT NULL
        GROUP BY [DcNo], [Suffix], [CustId]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'MfgDc.DcNo /scope:Suffix+CustId' AS Series,
        CAST([DcNo] AS nvarchar(200)) AS [DcNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        CAST([CustId] AS nvarchar(200)) AS [CustId],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[MfgDc]
    WHERE [DcNo] IS NOT NULL
    GROUP BY [DcNo], [Suffix], [CustId]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: DcNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'MfgDc.DcNo /scope:Suffix+CustId' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [DcNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[MfgDc]
        WHERE [DcNo] IS NOT NULL
        GROUP BY [DcNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'MfgDc.DcNo /scope:Suffix+CustId' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([DcNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([DcNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([DcNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[MfgDc]
    WHERE [DcNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([DcNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'MfgDc.DcNo /scope:Suffix+CustId' AS Series,
           'table dbo.MfgDc is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 02 -- MfgInv.InvNo /scope:Suffix+CustId
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.MfgInv', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: InvNo + Suffix + CustId */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'MfgInv.InvNo /scope:Suffix+CustId' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [InvNo], [Suffix], [CustId], COUNT(*) AS RowsInGroup
        FROM dbo.[MfgInv]
        WHERE [InvNo] IS NOT NULL
        GROUP BY [InvNo], [Suffix], [CustId]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'MfgInv.InvNo /scope:Suffix+CustId' AS Series,
        CAST([InvNo] AS nvarchar(200)) AS [InvNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        CAST([CustId] AS nvarchar(200)) AS [CustId],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[MfgInv]
    WHERE [InvNo] IS NOT NULL
    GROUP BY [InvNo], [Suffix], [CustId]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: InvNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'MfgInv.InvNo /scope:Suffix+CustId' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [InvNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[MfgInv]
        WHERE [InvNo] IS NOT NULL
        GROUP BY [InvNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'MfgInv.InvNo /scope:Suffix+CustId' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([InvNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([InvNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([InvNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[MfgInv]
    WHERE [InvNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([InvNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'MfgInv.InvNo /scope:Suffix+CustId' AS Series,
           'table dbo.MfgInv is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 03 -- ExpInv.ExpInvNo /scope:Suffix+CustId
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ExpInv', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: ExpInvNo + Suffix + CustId */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'ExpInv.ExpInvNo /scope:Suffix+CustId' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [ExpInvNo], [Suffix], [CustId], COUNT(*) AS RowsInGroup
        FROM dbo.[ExpInv]
        WHERE [ExpInvNo] IS NOT NULL
        GROUP BY [ExpInvNo], [Suffix], [CustId]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'ExpInv.ExpInvNo /scope:Suffix+CustId' AS Series,
        CAST([ExpInvNo] AS nvarchar(200)) AS [ExpInvNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        CAST([CustId] AS nvarchar(200)) AS [CustId],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[ExpInv]
    WHERE [ExpInvNo] IS NOT NULL
    GROUP BY [ExpInvNo], [Suffix], [CustId]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: ExpInvNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'ExpInv.ExpInvNo /scope:Suffix+CustId' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [ExpInvNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ExpInv]
        WHERE [ExpInvNo] IS NOT NULL
        GROUP BY [ExpInvNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'ExpInv.ExpInvNo /scope:Suffix+CustId' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([ExpInvNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([ExpInvNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([ExpInvNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[ExpInv]
    WHERE [ExpInvNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([ExpInvNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'ExpInv.ExpInvNo /scope:Suffix+CustId' AS Series,
           'table dbo.ExpInv is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 04 -- LabInv.LabInvNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.LabInv', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: LabInvNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'LabInv.LabInvNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [LabInvNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[LabInv]
        WHERE [LabInvNo] IS NOT NULL
        GROUP BY [LabInvNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'LabInv.LabInvNo /scope:Suffix' AS Series,
        CAST([LabInvNo] AS nvarchar(200)) AS [LabInvNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[LabInv]
    WHERE [LabInvNo] IS NOT NULL
    GROUP BY [LabInvNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: LabInvNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'LabInv.LabInvNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [LabInvNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[LabInv]
        WHERE [LabInvNo] IS NOT NULL
        GROUP BY [LabInvNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'LabInv.LabInvNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([LabInvNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([LabInvNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([LabInvNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[LabInv]
    WHERE [LabInvNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([LabInvNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'LabInv.LabInvNo /scope:Suffix' AS Series,
           'table dbo.LabInv is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 05 -- MfgQuote.QuoteNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.MfgQuote', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: QuoteNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'MfgQuote.QuoteNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [QuoteNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[MfgQuote]
        WHERE [QuoteNo] IS NOT NULL
        GROUP BY [QuoteNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'MfgQuote.QuoteNo /scope:Suffix' AS Series,
        CAST([QuoteNo] AS nvarchar(200)) AS [QuoteNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[MfgQuote]
    WHERE [QuoteNo] IS NOT NULL
    GROUP BY [QuoteNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: QuoteNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'MfgQuote.QuoteNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [QuoteNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[MfgQuote]
        WHERE [QuoteNo] IS NOT NULL
        GROUP BY [QuoteNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'MfgQuote.QuoteNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([QuoteNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([QuoteNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([QuoteNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[MfgQuote]
    WHERE [QuoteNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([QuoteNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'MfgQuote.QuoteNo /scope:Suffix' AS Series,
           'table dbo.MfgQuote is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 06 -- PerformaInv.InvNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.PerformaInv', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: InvNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'PerformaInv.InvNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [InvNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[PerformaInv]
        WHERE [InvNo] IS NOT NULL
        GROUP BY [InvNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'PerformaInv.InvNo /scope:Suffix' AS Series,
        CAST([InvNo] AS nvarchar(200)) AS [InvNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[PerformaInv]
    WHERE [InvNo] IS NOT NULL
    GROUP BY [InvNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: InvNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'PerformaInv.InvNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [InvNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[PerformaInv]
        WHERE [InvNo] IS NOT NULL
        GROUP BY [InvNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'PerformaInv.InvNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([InvNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([InvNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([InvNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[PerformaInv]
    WHERE [InvNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([InvNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'PerformaInv.InvNo /scope:Suffix' AS Series,
           'table dbo.PerformaInv is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 07 -- MfgPo.PONo /scope:Suffix+custId
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.MfgPo', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: PONo + Suffix + custId */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'MfgPo.PONo /scope:Suffix+custId' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [PONo], [Suffix], [custId], COUNT(*) AS RowsInGroup
        FROM dbo.[MfgPo]
        WHERE [PONo] IS NOT NULL
        GROUP BY [PONo], [Suffix], [custId]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'MfgPo.PONo /scope:Suffix+custId' AS Series,
        CAST([PONo] AS nvarchar(200)) AS [PONo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        CAST([custId] AS nvarchar(200)) AS [custId],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[MfgPo]
    WHERE [PONo] IS NOT NULL
    GROUP BY [PONo], [Suffix], [custId]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: PONo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'MfgPo.PONo /scope:Suffix+custId' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [PONo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[MfgPo]
        WHERE [PONo] IS NOT NULL
        GROUP BY [PONo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'MfgPo.PONo /scope:Suffix+custId' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([PONo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([PONo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([PONo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[MfgPo]
    WHERE [PONo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([PONo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'MfgPo.PONo /scope:Suffix+custId' AS Series,
           'table dbo.MfgPo is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 08 -- MfgPo.SaleOrderNo /scope:PoTypeId
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.MfgPo', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: SaleOrderNo + PoTypeId */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'MfgPo.SaleOrderNo /scope:PoTypeId' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [SaleOrderNo], [PoTypeId], COUNT(*) AS RowsInGroup
        FROM dbo.[MfgPo]
        WHERE [SaleOrderNo] IS NOT NULL
        GROUP BY [SaleOrderNo], [PoTypeId]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'MfgPo.SaleOrderNo /scope:PoTypeId' AS Series,
        CAST([SaleOrderNo] AS nvarchar(200)) AS [SaleOrderNo],
        CAST([PoTypeId] AS nvarchar(200)) AS [PoTypeId],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[MfgPo]
    WHERE [SaleOrderNo] IS NOT NULL
    GROUP BY [SaleOrderNo], [PoTypeId]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: SaleOrderNo
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'MfgPo.SaleOrderNo /scope:PoTypeId' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [SaleOrderNo], COUNT(*) AS RowsInGroup
        FROM dbo.[MfgPo]
        WHERE [SaleOrderNo] IS NOT NULL
        GROUP BY [SaleOrderNo]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'MfgPo.SaleOrderNo /scope:PoTypeId' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([SaleOrderNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([SaleOrderNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([SaleOrderNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[MfgPo]
    WHERE [SaleOrderNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([SaleOrderNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'MfgPo.SaleOrderNo /scope:PoTypeId' AS Series,
           'table dbo.MfgPo is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 09 -- MfgPo.OANo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.MfgPo', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: OANo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'MfgPo.OANo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [OANo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[MfgPo]
        WHERE [OANo] IS NOT NULL
        GROUP BY [OANo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'MfgPo.OANo /scope:Suffix' AS Series,
        CAST([OANo] AS nvarchar(200)) AS [OANo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[MfgPo]
    WHERE [OANo] IS NOT NULL
    GROUP BY [OANo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: OANo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'MfgPo.OANo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [OANo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[MfgPo]
        WHERE [OANo] IS NOT NULL
        GROUP BY [OANo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'MfgPo.OANo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([OANo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([OANo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([OANo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[MfgPo]
    WHERE [OANo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([OANo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'MfgPo.OANo /scope:Suffix' AS Series,
           'table dbo.MfgPo is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 10 -- ContractReview.OANo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ContractReview', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: OANo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'ContractReview.OANo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [OANo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ContractReview]
        WHERE [OANo] IS NOT NULL
        GROUP BY [OANo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'ContractReview.OANo /scope:Suffix' AS Series,
        CAST([OANo] AS nvarchar(200)) AS [OANo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[ContractReview]
    WHERE [OANo] IS NOT NULL
    GROUP BY [OANo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: OANo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'ContractReview.OANo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [OANo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ContractReview]
        WHERE [OANo] IS NOT NULL
        GROUP BY [OANo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'ContractReview.OANo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([OANo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([OANo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([OANo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[ContractReview]
    WHERE [OANo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([OANo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'ContractReview.OANo /scope:Suffix' AS Series,
           'table dbo.ContractReview is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 11 -- EnquirySales.EnquiryNo /scope:Suffix+custId
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.EnquirySales', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: EnquiryNo + Suffix + custId */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'EnquirySales.EnquiryNo /scope:Suffix+custId' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [EnquiryNo], [Suffix], [custId], COUNT(*) AS RowsInGroup
        FROM dbo.[EnquirySales]
        WHERE [EnquiryNo] IS NOT NULL
        GROUP BY [EnquiryNo], [Suffix], [custId]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'EnquirySales.EnquiryNo /scope:Suffix+custId' AS Series,
        CAST([EnquiryNo] AS nvarchar(200)) AS [EnquiryNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        CAST([custId] AS nvarchar(200)) AS [custId],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[EnquirySales]
    WHERE [EnquiryNo] IS NOT NULL
    GROUP BY [EnquiryNo], [Suffix], [custId]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: EnquiryNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'EnquirySales.EnquiryNo /scope:Suffix+custId' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [EnquiryNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[EnquirySales]
        WHERE [EnquiryNo] IS NOT NULL
        GROUP BY [EnquiryNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'EnquirySales.EnquiryNo /scope:Suffix+custId' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([EnquiryNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([EnquiryNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([EnquiryNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[EnquirySales]
    WHERE [EnquiryNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([EnquiryNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'EnquirySales.EnquiryNo /scope:Suffix+custId' AS Series,
           'table dbo.EnquirySales is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 12 -- LabourDcOutgoing.DcNo [NonReturnDc = 0] /scope:Suffix+CustId
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.LabourDcOutgoing', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: DcNo + Suffix + CustId */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'LabourDcOutgoing.DcNo [NonReturnDc = 0] /scope:Suffix+CustId' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [DcNo], [Suffix], [CustId], COUNT(*) AS RowsInGroup
        FROM dbo.[LabourDcOutgoing]
        WHERE [DcNo] IS NOT NULL
          AND NonReturnDc = 0
        GROUP BY [DcNo], [Suffix], [CustId]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'LabourDcOutgoing.DcNo [NonReturnDc = 0] /scope:Suffix+CustId' AS Series,
        CAST([DcNo] AS nvarchar(200)) AS [DcNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        CAST([CustId] AS nvarchar(200)) AS [CustId],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[LabourDcOutgoing]
    WHERE [DcNo] IS NOT NULL
      AND NonReturnDc = 0
    GROUP BY [DcNo], [Suffix], [CustId]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: DcNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'LabourDcOutgoing.DcNo [NonReturnDc = 0] /scope:Suffix+CustId' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [DcNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[LabourDcOutgoing]
        WHERE [DcNo] IS NOT NULL
          AND NonReturnDc = 0
        GROUP BY [DcNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'LabourDcOutgoing.DcNo [NonReturnDc = 0] /scope:Suffix+CustId' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([DcNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([DcNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([DcNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[LabourDcOutgoing]
    WHERE [DcNo] IS NOT NULL
      AND NonReturnDc = 0
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([DcNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'LabourDcOutgoing.DcNo [NonReturnDc = 0] /scope:Suffix+CustId' AS Series,
           'table dbo.LabourDcOutgoing is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 13 -- LabourDcOutgoing.DcNo [NonReturnDc = 1 AND [DcNo] LIKE 'NR%'] /scope:none
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.LabourDcOutgoing', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: DcNo */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'LabourDcOutgoing.DcNo [NonReturnDc = 1 AND [DcNo] LIKE ''NR%''] /scope:none' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [DcNo], COUNT(*) AS RowsInGroup
        FROM dbo.[LabourDcOutgoing]
        WHERE [DcNo] IS NOT NULL
          AND NonReturnDc = 1 AND [DcNo] LIKE 'NR%'
        GROUP BY [DcNo]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'LabourDcOutgoing.DcNo [NonReturnDc = 1 AND [DcNo] LIKE ''NR%''] /scope:none' AS Series,
        CAST([DcNo] AS nvarchar(200)) AS [DcNo],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[LabourDcOutgoing]
    WHERE [DcNo] IS NOT NULL
      AND NonReturnDc = 1 AND [DcNo] LIKE 'NR%'
    GROUP BY [DcNo]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: DcNo
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'LabourDcOutgoing.DcNo [NonReturnDc = 1 AND [DcNo] LIKE ''NR%''] /scope:none' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [DcNo], COUNT(*) AS RowsInGroup
        FROM dbo.[LabourDcOutgoing]
        WHERE [DcNo] IS NOT NULL
          AND NonReturnDc = 1 AND [DcNo] LIKE 'NR%'
        GROUP BY [DcNo]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'LabourDcOutgoing.DcNo [NonReturnDc = 1 AND [DcNo] LIKE ''NR%''] /scope:none' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([DcNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([DcNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([DcNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[LabourDcOutgoing]
    WHERE [DcNo] IS NOT NULL
      AND NonReturnDc = 1 AND [DcNo] LIKE 'NR%'
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([DcNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'LabourDcOutgoing.DcNo [NonReturnDc = 1 AND [DcNo] LIKE ''NR%''] /scope:none' AS Series,
           'table dbo.LabourDcOutgoing is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 14 -- LabourGRN.GRNNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.LabourGRN', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: GRNNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'LabourGRN.GRNNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [GRNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[LabourGRN]
        WHERE [GRNNo] IS NOT NULL
        GROUP BY [GRNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'LabourGRN.GRNNo /scope:Suffix' AS Series,
        CAST([GRNNo] AS nvarchar(200)) AS [GRNNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[LabourGRN]
    WHERE [GRNNo] IS NOT NULL
    GROUP BY [GRNNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: GRNNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'LabourGRN.GRNNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [GRNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[LabourGRN]
        WHERE [GRNNo] IS NOT NULL
        GROUP BY [GRNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'LabourGRN.GRNNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([GRNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([GRNNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([GRNNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[LabourGRN]
    WHERE [GRNNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([GRNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'LabourGRN.GRNNo /scope:Suffix' AS Series,
           'table dbo.LabourGRN is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 15 -- LabourSCN.SCNNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.LabourSCN', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: SCNNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'LabourSCN.SCNNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [SCNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[LabourSCN]
        WHERE [SCNNo] IS NOT NULL
        GROUP BY [SCNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'LabourSCN.SCNNo /scope:Suffix' AS Series,
        CAST([SCNNo] AS nvarchar(200)) AS [SCNNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[LabourSCN]
    WHERE [SCNNo] IS NOT NULL
    GROUP BY [SCNNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: SCNNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'LabourSCN.SCNNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [SCNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[LabourSCN]
        WHERE [SCNNo] IS NOT NULL
        GROUP BY [SCNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'LabourSCN.SCNNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([SCNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([SCNNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([SCNNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[LabourSCN]
    WHERE [SCNNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([SCNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'LabourSCN.SCNNo /scope:Suffix' AS Series,
           'table dbo.LabourSCN is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 16 -- SubConDcOut.DcNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.SubConDcOut', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: DcNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'SubConDcOut.DcNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [DcNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[SubConDcOut]
        WHERE [DcNo] IS NOT NULL
        GROUP BY [DcNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'SubConDcOut.DcNo /scope:Suffix' AS Series,
        CAST([DcNo] AS nvarchar(200)) AS [DcNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[SubConDcOut]
    WHERE [DcNo] IS NOT NULL
    GROUP BY [DcNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: DcNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'SubConDcOut.DcNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [DcNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[SubConDcOut]
        WHERE [DcNo] IS NOT NULL
        GROUP BY [DcNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'SubConDcOut.DcNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([DcNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([DcNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([DcNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[SubConDcOut]
    WHERE [DcNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([DcNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'SubConDcOut.DcNo /scope:Suffix' AS Series,
           'table dbo.SubConDcOut is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 17 -- SubConGRN.GRNNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.SubConGRN', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: GRNNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'SubConGRN.GRNNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [GRNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[SubConGRN]
        WHERE [GRNNo] IS NOT NULL
        GROUP BY [GRNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'SubConGRN.GRNNo /scope:Suffix' AS Series,
        CAST([GRNNo] AS nvarchar(200)) AS [GRNNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[SubConGRN]
    WHERE [GRNNo] IS NOT NULL
    GROUP BY [GRNNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: GRNNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'SubConGRN.GRNNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [GRNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[SubConGRN]
        WHERE [GRNNo] IS NOT NULL
        GROUP BY [GRNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'SubConGRN.GRNNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([GRNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([GRNNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([GRNNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[SubConGRN]
    WHERE [GRNNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([GRNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'SubConGRN.GRNNo /scope:Suffix' AS Series,
           'table dbo.SubConGRN is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 18 -- SubConSCN.SCNNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.SubConSCN', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: SCNNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'SubConSCN.SCNNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [SCNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[SubConSCN]
        WHERE [SCNNo] IS NOT NULL
        GROUP BY [SCNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'SubConSCN.SCNNo /scope:Suffix' AS Series,
        CAST([SCNNo] AS nvarchar(200)) AS [SCNNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[SubConSCN]
    WHERE [SCNNo] IS NOT NULL
    GROUP BY [SCNNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: SCNNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'SubConSCN.SCNNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [SCNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[SubConSCN]
        WHERE [SCNNo] IS NOT NULL
        GROUP BY [SCNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'SubConSCN.SCNNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([SCNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([SCNNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([SCNNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[SubConSCN]
    WHERE [SCNNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([SCNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'SubConSCN.SCNNo /scope:Suffix' AS Series,
           'table dbo.SubConSCN is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 19 -- SubConInv.InvNo /scope:Suffix+VendorCode
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.SubConInv', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: InvNo + Suffix + VendorCode */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'SubConInv.InvNo /scope:Suffix+VendorCode' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [InvNo], [Suffix], [VendorCode], COUNT(*) AS RowsInGroup
        FROM dbo.[SubConInv]
        WHERE [InvNo] IS NOT NULL
        GROUP BY [InvNo], [Suffix], [VendorCode]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'SubConInv.InvNo /scope:Suffix+VendorCode' AS Series,
        CAST([InvNo] AS nvarchar(200)) AS [InvNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        CAST([VendorCode] AS nvarchar(200)) AS [VendorCode],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[SubConInv]
    WHERE [InvNo] IS NOT NULL
    GROUP BY [InvNo], [Suffix], [VendorCode]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: InvNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'SubConInv.InvNo /scope:Suffix+VendorCode' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [InvNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[SubConInv]
        WHERE [InvNo] IS NOT NULL
        GROUP BY [InvNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'SubConInv.InvNo /scope:Suffix+VendorCode' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([InvNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([InvNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([InvNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[SubConInv]
    WHERE [InvNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([InvNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'SubConInv.InvNo /scope:Suffix+VendorCode' AS Series,
           'table dbo.SubConInv is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 20 -- PurchPo.PONo /scope:Suffix+vendorcode+RevesionNo
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.PurchPo', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: PONo + Suffix + vendorcode + RevesionNo */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'PurchPo.PONo /scope:Suffix+vendorcode+RevesionNo' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [PONo], [Suffix], [vendorcode], [RevesionNo], COUNT(*) AS RowsInGroup
        FROM dbo.[PurchPo]
        WHERE [PONo] IS NOT NULL
        GROUP BY [PONo], [Suffix], [vendorcode], [RevesionNo]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'PurchPo.PONo /scope:Suffix+vendorcode+RevesionNo' AS Series,
        CAST([PONo] AS nvarchar(200)) AS [PONo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        CAST([vendorcode] AS nvarchar(200)) AS [vendorcode],
        CAST([RevesionNo] AS nvarchar(200)) AS [RevesionNo],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[PurchPo]
    WHERE [PONo] IS NOT NULL
    GROUP BY [PONo], [Suffix], [vendorcode], [RevesionNo]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: PONo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'PurchPo.PONo /scope:Suffix+vendorcode+RevesionNo' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [PONo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[PurchPo]
        WHERE [PONo] IS NOT NULL
        GROUP BY [PONo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'PurchPo.PONo /scope:Suffix+vendorcode+RevesionNo' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([PONo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([PONo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([PONo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[PurchPo]
    WHERE [PONo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([PONo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'PurchPo.PONo /scope:Suffix+vendorcode+RevesionNo' AS Series,
           'table dbo.PurchPo is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 21 -- PurchaseGRN.GRNNo /scope:Suffix+vendorCode
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.PurchaseGRN', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: GRNNo + Suffix + vendorCode */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'PurchaseGRN.GRNNo /scope:Suffix+vendorCode' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [GRNNo], [Suffix], [vendorCode], COUNT(*) AS RowsInGroup
        FROM dbo.[PurchaseGRN]
        WHERE [GRNNo] IS NOT NULL
        GROUP BY [GRNNo], [Suffix], [vendorCode]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'PurchaseGRN.GRNNo /scope:Suffix+vendorCode' AS Series,
        CAST([GRNNo] AS nvarchar(200)) AS [GRNNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        CAST([vendorCode] AS nvarchar(200)) AS [vendorCode],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[PurchaseGRN]
    WHERE [GRNNo] IS NOT NULL
    GROUP BY [GRNNo], [Suffix], [vendorCode]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: GRNNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'PurchaseGRN.GRNNo /scope:Suffix+vendorCode' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [GRNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[PurchaseGRN]
        WHERE [GRNNo] IS NOT NULL
        GROUP BY [GRNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'PurchaseGRN.GRNNo /scope:Suffix+vendorCode' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([GRNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([GRNNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([GRNNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[PurchaseGRN]
    WHERE [GRNNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([GRNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'PurchaseGRN.GRNNo /scope:Suffix+vendorCode' AS Series,
           'table dbo.PurchaseGRN is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 22 -- PurchaseSCN.SCNNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.PurchaseSCN', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: SCNNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'PurchaseSCN.SCNNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [SCNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[PurchaseSCN]
        WHERE [SCNNo] IS NOT NULL
        GROUP BY [SCNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'PurchaseSCN.SCNNo /scope:Suffix' AS Series,
        CAST([SCNNo] AS nvarchar(200)) AS [SCNNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[PurchaseSCN]
    WHERE [SCNNo] IS NOT NULL
    GROUP BY [SCNNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: SCNNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'PurchaseSCN.SCNNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [SCNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[PurchaseSCN]
        WHERE [SCNNo] IS NOT NULL
        GROUP BY [SCNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'PurchaseSCN.SCNNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([SCNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([SCNNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([SCNNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[PurchaseSCN]
    WHERE [SCNNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([SCNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'PurchaseSCN.SCNNo /scope:Suffix' AS Series,
           'table dbo.PurchaseSCN is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 23 -- PurchaseInvoice.InvNo /scope:Suffix+VendorCode
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.PurchaseInvoice', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: InvNo + Suffix + VendorCode */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'PurchaseInvoice.InvNo /scope:Suffix+VendorCode' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [InvNo], [Suffix], [VendorCode], COUNT(*) AS RowsInGroup
        FROM dbo.[PurchaseInvoice]
        WHERE [InvNo] IS NOT NULL
        GROUP BY [InvNo], [Suffix], [VendorCode]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'PurchaseInvoice.InvNo /scope:Suffix+VendorCode' AS Series,
        CAST([InvNo] AS nvarchar(200)) AS [InvNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        CAST([VendorCode] AS nvarchar(200)) AS [VendorCode],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[PurchaseInvoice]
    WHERE [InvNo] IS NOT NULL
    GROUP BY [InvNo], [Suffix], [VendorCode]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: InvNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'PurchaseInvoice.InvNo /scope:Suffix+VendorCode' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [InvNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[PurchaseInvoice]
        WHERE [InvNo] IS NOT NULL
        GROUP BY [InvNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'PurchaseInvoice.InvNo /scope:Suffix+VendorCode' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([InvNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([InvNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([InvNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[PurchaseInvoice]
    WHERE [InvNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([InvNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'PurchaseInvoice.InvNo /scope:Suffix+VendorCode' AS Series,
           'table dbo.PurchaseInvoice is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 24 -- PurchaseQuote.QuoteNo /scope:Suffix+VendorCode
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.PurchaseQuote', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: QuoteNo + Suffix + VendorCode */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'PurchaseQuote.QuoteNo /scope:Suffix+VendorCode' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [QuoteNo], [Suffix], [VendorCode], COUNT(*) AS RowsInGroup
        FROM dbo.[PurchaseQuote]
        WHERE [QuoteNo] IS NOT NULL
        GROUP BY [QuoteNo], [Suffix], [VendorCode]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'PurchaseQuote.QuoteNo /scope:Suffix+VendorCode' AS Series,
        CAST([QuoteNo] AS nvarchar(200)) AS [QuoteNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        CAST([VendorCode] AS nvarchar(200)) AS [VendorCode],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[PurchaseQuote]
    WHERE [QuoteNo] IS NOT NULL
    GROUP BY [QuoteNo], [Suffix], [VendorCode]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: QuoteNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'PurchaseQuote.QuoteNo /scope:Suffix+VendorCode' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [QuoteNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[PurchaseQuote]
        WHERE [QuoteNo] IS NOT NULL
        GROUP BY [QuoteNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'PurchaseQuote.QuoteNo /scope:Suffix+VendorCode' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([QuoteNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([QuoteNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([QuoteNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[PurchaseQuote]
    WHERE [QuoteNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([QuoteNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'PurchaseQuote.QuoteNo /scope:Suffix+VendorCode' AS Series,
           'table dbo.PurchaseQuote is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 25 -- EnquiryPurchase.EnquiryNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.EnquiryPurchase', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: EnquiryNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'EnquiryPurchase.EnquiryNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [EnquiryNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[EnquiryPurchase]
        WHERE [EnquiryNo] IS NOT NULL
        GROUP BY [EnquiryNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'EnquiryPurchase.EnquiryNo /scope:Suffix' AS Series,
        CAST([EnquiryNo] AS nvarchar(200)) AS [EnquiryNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[EnquiryPurchase]
    WHERE [EnquiryNo] IS NOT NULL
    GROUP BY [EnquiryNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: EnquiryNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'EnquiryPurchase.EnquiryNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [EnquiryNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[EnquiryPurchase]
        WHERE [EnquiryNo] IS NOT NULL
        GROUP BY [EnquiryNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'EnquiryPurchase.EnquiryNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([EnquiryNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([EnquiryNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([EnquiryNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[EnquiryPurchase]
    WHERE [EnquiryNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([EnquiryNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'EnquiryPurchase.EnquiryNo /scope:Suffix' AS Series,
           'table dbo.EnquiryPurchase is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 26 -- MaterialReq.MReqNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.MaterialReq', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: MReqNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'MaterialReq.MReqNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [MReqNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[MaterialReq]
        WHERE [MReqNo] IS NOT NULL
        GROUP BY [MReqNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'MaterialReq.MReqNo /scope:Suffix' AS Series,
        CAST([MReqNo] AS nvarchar(200)) AS [MReqNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[MaterialReq]
    WHERE [MReqNo] IS NOT NULL
    GROUP BY [MReqNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: MReqNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'MaterialReq.MReqNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [MReqNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[MaterialReq]
        WHERE [MReqNo] IS NOT NULL
        GROUP BY [MReqNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'MaterialReq.MReqNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([MReqNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([MReqNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([MReqNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[MaterialReq]
    WHERE [MReqNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([MReqNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'MaterialReq.MReqNo /scope:Suffix' AS Series,
           'table dbo.MaterialReq is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 27 -- DebitNote.DebitNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.DebitNote', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: DebitNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'DebitNote.DebitNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [DebitNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[DebitNote]
        WHERE [DebitNo] IS NOT NULL
        GROUP BY [DebitNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'DebitNote.DebitNo /scope:Suffix' AS Series,
        CAST([DebitNo] AS nvarchar(200)) AS [DebitNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[DebitNote]
    WHERE [DebitNo] IS NOT NULL
    GROUP BY [DebitNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: DebitNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'DebitNote.DebitNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [DebitNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[DebitNote]
        WHERE [DebitNo] IS NOT NULL
        GROUP BY [DebitNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'DebitNote.DebitNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([DebitNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([DebitNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([DebitNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[DebitNote]
    WHERE [DebitNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([DebitNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'DebitNote.DebitNo /scope:Suffix' AS Series,
           'table dbo.DebitNote is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 28 -- CreditNote.CreditNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.CreditNote', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: CreditNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'CreditNote.CreditNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [CreditNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[CreditNote]
        WHERE [CreditNo] IS NOT NULL
        GROUP BY [CreditNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'CreditNote.CreditNo /scope:Suffix' AS Series,
        CAST([CreditNo] AS nvarchar(200)) AS [CreditNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[CreditNote]
    WHERE [CreditNo] IS NOT NULL
    GROUP BY [CreditNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: CreditNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'CreditNote.CreditNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [CreditNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[CreditNote]
        WHERE [CreditNo] IS NOT NULL
        GROUP BY [CreditNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'CreditNote.CreditNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([CreditNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([CreditNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([CreditNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[CreditNote]
    WHERE [CreditNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([CreditNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'CreditNote.CreditNo /scope:Suffix' AS Series,
           'table dbo.CreditNote is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 29 -- RouteCard.RCNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.RouteCard', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: RCNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'RouteCard.RCNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [RCNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[RouteCard]
        WHERE [RCNo] IS NOT NULL
        GROUP BY [RCNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'RouteCard.RCNo /scope:Suffix' AS Series,
        CAST([RCNo] AS nvarchar(200)) AS [RCNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[RouteCard]
    WHERE [RCNo] IS NOT NULL
    GROUP BY [RCNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: RCNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'RouteCard.RCNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [RCNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[RouteCard]
        WHERE [RCNo] IS NOT NULL
        GROUP BY [RCNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'RouteCard.RCNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([RCNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([RCNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([RCNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[RouteCard]
    WHERE [RCNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([RCNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'RouteCard.RCNo /scope:Suffix' AS Series,
           'table dbo.RouteCard is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 30 -- RouteCardRelease.RcReleaseNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.RouteCardRelease', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: RcReleaseNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'RouteCardRelease.RcReleaseNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [RcReleaseNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[RouteCardRelease]
        WHERE [RcReleaseNo] IS NOT NULL
        GROUP BY [RcReleaseNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'RouteCardRelease.RcReleaseNo /scope:Suffix' AS Series,
        CAST([RcReleaseNo] AS nvarchar(200)) AS [RcReleaseNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[RouteCardRelease]
    WHERE [RcReleaseNo] IS NOT NULL
    GROUP BY [RcReleaseNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: RcReleaseNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'RouteCardRelease.RcReleaseNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [RcReleaseNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[RouteCardRelease]
        WHERE [RcReleaseNo] IS NOT NULL
        GROUP BY [RcReleaseNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'RouteCardRelease.RcReleaseNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([RcReleaseNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([RcReleaseNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([RcReleaseNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[RouteCardRelease]
    WHERE [RcReleaseNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([RcReleaseNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'RouteCardRelease.RcReleaseNo /scope:Suffix' AS Series,
           'table dbo.RouteCardRelease is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 31 -- JobOrder.JobNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.JobOrder', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: JobNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'JobOrder.JobNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [JobNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[JobOrder]
        WHERE [JobNo] IS NOT NULL
        GROUP BY [JobNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'JobOrder.JobNo /scope:Suffix' AS Series,
        CAST([JobNo] AS nvarchar(200)) AS [JobNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[JobOrder]
    WHERE [JobNo] IS NOT NULL
    GROUP BY [JobNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: JobNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'JobOrder.JobNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [JobNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[JobOrder]
        WHERE [JobNo] IS NOT NULL
        GROUP BY [JobNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'JobOrder.JobNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([JobNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([JobNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([JobNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[JobOrder]
    WHERE [JobNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([JobNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'JobOrder.JobNo /scope:Suffix' AS Series,
           'table dbo.JobOrder is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 32 -- Estimate.EstiamateNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.Estimate', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: EstiamateNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'Estimate.EstiamateNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [EstiamateNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[Estimate]
        WHERE [EstiamateNo] IS NOT NULL
        GROUP BY [EstiamateNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'Estimate.EstiamateNo /scope:Suffix' AS Series,
        CAST([EstiamateNo] AS nvarchar(200)) AS [EstiamateNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[Estimate]
    WHERE [EstiamateNo] IS NOT NULL
    GROUP BY [EstiamateNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: EstiamateNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'Estimate.EstiamateNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [EstiamateNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[Estimate]
        WHERE [EstiamateNo] IS NOT NULL
        GROUP BY [EstiamateNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'Estimate.EstiamateNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([EstiamateNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([EstiamateNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([EstiamateNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[Estimate]
    WHERE [EstiamateNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([EstiamateNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'Estimate.EstiamateNo /scope:Suffix' AS Series,
           'table dbo.Estimate is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 33 -- ProductionLog.LogNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ProductionLog', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: LogNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'ProductionLog.LogNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [LogNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionLog]
        WHERE [LogNo] IS NOT NULL
        GROUP BY [LogNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'ProductionLog.LogNo /scope:Suffix' AS Series,
        CAST([LogNo] AS nvarchar(200)) AS [LogNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[ProductionLog]
    WHERE [LogNo] IS NOT NULL
    GROUP BY [LogNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: LogNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'ProductionLog.LogNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [LogNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionLog]
        WHERE [LogNo] IS NOT NULL
        GROUP BY [LogNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'ProductionLog.LogNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([LogNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([LogNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([LogNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[ProductionLog]
    WHERE [LogNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([LogNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'ProductionLog.LogNo /scope:Suffix' AS Series,
           'table dbo.ProductionLog is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 34 -- ProductionIssueAssy.IssueNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ProductionIssueAssy', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: IssueNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'ProductionIssueAssy.IssueNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [IssueNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionIssueAssy]
        WHERE [IssueNo] IS NOT NULL
        GROUP BY [IssueNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'ProductionIssueAssy.IssueNo /scope:Suffix' AS Series,
        CAST([IssueNo] AS nvarchar(200)) AS [IssueNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[ProductionIssueAssy]
    WHERE [IssueNo] IS NOT NULL
    GROUP BY [IssueNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: IssueNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'ProductionIssueAssy.IssueNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [IssueNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionIssueAssy]
        WHERE [IssueNo] IS NOT NULL
        GROUP BY [IssueNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'ProductionIssueAssy.IssueNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([IssueNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([IssueNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([IssueNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[ProductionIssueAssy]
    WHERE [IssueNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([IssueNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'ProductionIssueAssy.IssueNo /scope:Suffix' AS Series,
           'table dbo.ProductionIssueAssy is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 35 -- ProductionIssueAssy.IssueNo /scope:DepartmentCode+MonthCode+Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ProductionIssueAssy', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: IssueNo + DepartmentCode + MonthCode + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'ProductionIssueAssy.IssueNo /scope:DepartmentCode+MonthCode+Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [IssueNo], [DepartmentCode], [MonthCode], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionIssueAssy]
        WHERE [IssueNo] IS NOT NULL
        GROUP BY [IssueNo], [DepartmentCode], [MonthCode], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'ProductionIssueAssy.IssueNo /scope:DepartmentCode+MonthCode+Suffix' AS Series,
        CAST([IssueNo] AS nvarchar(200)) AS [IssueNo],
        CAST([DepartmentCode] AS nvarchar(200)) AS [DepartmentCode],
        CAST([MonthCode] AS nvarchar(200)) AS [MonthCode],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[ProductionIssueAssy]
    WHERE [IssueNo] IS NOT NULL
    GROUP BY [IssueNo], [DepartmentCode], [MonthCode], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: IssueNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'ProductionIssueAssy.IssueNo /scope:DepartmentCode+MonthCode+Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [IssueNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionIssueAssy]
        WHERE [IssueNo] IS NOT NULL
        GROUP BY [IssueNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'ProductionIssueAssy.IssueNo /scope:DepartmentCode+MonthCode+Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([IssueNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([IssueNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([IssueNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[ProductionIssueAssy]
    WHERE [IssueNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([IssueNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'ProductionIssueAssy.IssueNo /scope:DepartmentCode+MonthCode+Suffix' AS Series,
           'table dbo.ProductionIssueAssy is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 36 -- ProductionReturnAssy.ReturnNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ProductionReturnAssy', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: ReturnNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'ProductionReturnAssy.ReturnNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [ReturnNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionReturnAssy]
        WHERE [ReturnNo] IS NOT NULL
        GROUP BY [ReturnNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'ProductionReturnAssy.ReturnNo /scope:Suffix' AS Series,
        CAST([ReturnNo] AS nvarchar(200)) AS [ReturnNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[ProductionReturnAssy]
    WHERE [ReturnNo] IS NOT NULL
    GROUP BY [ReturnNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: ReturnNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'ProductionReturnAssy.ReturnNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [ReturnNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionReturnAssy]
        WHERE [ReturnNo] IS NOT NULL
        GROUP BY [ReturnNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'ProductionReturnAssy.ReturnNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([ReturnNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([ReturnNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([ReturnNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[ProductionReturnAssy]
    WHERE [ReturnNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([ReturnNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'ProductionReturnAssy.ReturnNo /scope:Suffix' AS Series,
           'table dbo.ProductionReturnAssy is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 37 -- ProductionSCNAssy.SCNNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ProductionSCNAssy', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: SCNNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'ProductionSCNAssy.SCNNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [SCNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionSCNAssy]
        WHERE [SCNNo] IS NOT NULL
        GROUP BY [SCNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'ProductionSCNAssy.SCNNo /scope:Suffix' AS Series,
        CAST([SCNNo] AS nvarchar(200)) AS [SCNNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[ProductionSCNAssy]
    WHERE [SCNNo] IS NOT NULL
    GROUP BY [SCNNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: SCNNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'ProductionSCNAssy.SCNNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [SCNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionSCNAssy]
        WHERE [SCNNo] IS NOT NULL
        GROUP BY [SCNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'ProductionSCNAssy.SCNNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([SCNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([SCNNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([SCNNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[ProductionSCNAssy]
    WHERE [SCNNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([SCNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'ProductionSCNAssy.SCNNo /scope:Suffix' AS Series,
           'table dbo.ProductionSCNAssy is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 38 -- ProductionIssueComp.IssueNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ProductionIssueComp', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: IssueNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'ProductionIssueComp.IssueNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [IssueNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionIssueComp]
        WHERE [IssueNo] IS NOT NULL
        GROUP BY [IssueNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'ProductionIssueComp.IssueNo /scope:Suffix' AS Series,
        CAST([IssueNo] AS nvarchar(200)) AS [IssueNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[ProductionIssueComp]
    WHERE [IssueNo] IS NOT NULL
    GROUP BY [IssueNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: IssueNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'ProductionIssueComp.IssueNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [IssueNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionIssueComp]
        WHERE [IssueNo] IS NOT NULL
        GROUP BY [IssueNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'ProductionIssueComp.IssueNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([IssueNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([IssueNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([IssueNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[ProductionIssueComp]
    WHERE [IssueNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([IssueNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'ProductionIssueComp.IssueNo /scope:Suffix' AS Series,
           'table dbo.ProductionIssueComp is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 39 -- ProductionReturnComp.ReturnNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ProductionReturnComp', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: ReturnNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'ProductionReturnComp.ReturnNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [ReturnNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionReturnComp]
        WHERE [ReturnNo] IS NOT NULL
        GROUP BY [ReturnNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'ProductionReturnComp.ReturnNo /scope:Suffix' AS Series,
        CAST([ReturnNo] AS nvarchar(200)) AS [ReturnNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[ProductionReturnComp]
    WHERE [ReturnNo] IS NOT NULL
    GROUP BY [ReturnNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: ReturnNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'ProductionReturnComp.ReturnNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [ReturnNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionReturnComp]
        WHERE [ReturnNo] IS NOT NULL
        GROUP BY [ReturnNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'ProductionReturnComp.ReturnNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([ReturnNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([ReturnNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([ReturnNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[ProductionReturnComp]
    WHERE [ReturnNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([ReturnNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'ProductionReturnComp.ReturnNo /scope:Suffix' AS Series,
           'table dbo.ProductionReturnComp is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 40 -- ProductionSCNComp.SCNNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ProductionSCNComp', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: SCNNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'ProductionSCNComp.SCNNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [SCNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionSCNComp]
        WHERE [SCNNo] IS NOT NULL
        GROUP BY [SCNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'ProductionSCNComp.SCNNo /scope:Suffix' AS Series,
        CAST([SCNNo] AS nvarchar(200)) AS [SCNNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[ProductionSCNComp]
    WHERE [SCNNo] IS NOT NULL
    GROUP BY [SCNNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: SCNNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'ProductionSCNComp.SCNNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [SCNNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ProductionSCNComp]
        WHERE [SCNNo] IS NOT NULL
        GROUP BY [SCNNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'ProductionSCNComp.SCNNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([SCNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([SCNNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([SCNNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[ProductionSCNComp]
    WHERE [SCNNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([SCNNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'ProductionSCNComp.SCNNo /scope:Suffix' AS Series,
           'table dbo.ProductionSCNComp is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 41 -- MaterialIssNote.IssueNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.MaterialIssNote', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: IssueNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'MaterialIssNote.IssueNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [IssueNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[MaterialIssNote]
        WHERE [IssueNo] IS NOT NULL
        GROUP BY [IssueNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'MaterialIssNote.IssueNo /scope:Suffix' AS Series,
        CAST([IssueNo] AS nvarchar(200)) AS [IssueNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[MaterialIssNote]
    WHERE [IssueNo] IS NOT NULL
    GROUP BY [IssueNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: IssueNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'MaterialIssNote.IssueNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [IssueNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[MaterialIssNote]
        WHERE [IssueNo] IS NOT NULL
        GROUP BY [IssueNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'MaterialIssNote.IssueNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([IssueNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([IssueNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([IssueNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[MaterialIssNote]
    WHERE [IssueNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([IssueNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'MaterialIssNote.IssueNo /scope:Suffix' AS Series,
           'table dbo.MaterialIssNote is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 42 -- SCNGen.SCNGenNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.SCNGen', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: SCNGenNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'SCNGen.SCNGenNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [SCNGenNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[SCNGen]
        WHERE [SCNGenNo] IS NOT NULL
        GROUP BY [SCNGenNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'SCNGen.SCNGenNo /scope:Suffix' AS Series,
        CAST([SCNGenNo] AS nvarchar(200)) AS [SCNGenNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[SCNGen]
    WHERE [SCNGenNo] IS NOT NULL
    GROUP BY [SCNGenNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: SCNGenNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'SCNGen.SCNGenNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [SCNGenNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[SCNGen]
        WHERE [SCNGenNo] IS NOT NULL
        GROUP BY [SCNGenNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'SCNGen.SCNGenNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([SCNGenNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([SCNGenNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([SCNGenNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[SCNGen]
    WHERE [SCNGenNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([SCNGenNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'SCNGen.SCNGenNo /scope:Suffix' AS Series,
           'table dbo.SCNGen is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 43 -- StockIssueRequest.IssueNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.StockIssueRequest', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: IssueNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'StockIssueRequest.IssueNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [IssueNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[StockIssueRequest]
        WHERE [IssueNo] IS NOT NULL
        GROUP BY [IssueNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'StockIssueRequest.IssueNo /scope:Suffix' AS Series,
        CAST([IssueNo] AS nvarchar(200)) AS [IssueNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[StockIssueRequest]
    WHERE [IssueNo] IS NOT NULL
    GROUP BY [IssueNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: IssueNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'StockIssueRequest.IssueNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [IssueNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[StockIssueRequest]
        WHERE [IssueNo] IS NOT NULL
        GROUP BY [IssueNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'StockIssueRequest.IssueNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([IssueNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([IssueNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([IssueNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[StockIssueRequest]
    WHERE [IssueNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([IssueNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'StockIssueRequest.IssueNo /scope:Suffix' AS Series,
           'table dbo.StockIssueRequest is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 44 -- StoreInterTrans.ISTNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.StoreInterTrans', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: ISTNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'StoreInterTrans.ISTNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [ISTNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[StoreInterTrans]
        WHERE [ISTNo] IS NOT NULL
        GROUP BY [ISTNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'StoreInterTrans.ISTNo /scope:Suffix' AS Series,
        CAST([ISTNo] AS nvarchar(200)) AS [ISTNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[StoreInterTrans]
    WHERE [ISTNo] IS NOT NULL
    GROUP BY [ISTNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: ISTNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'StoreInterTrans.ISTNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [ISTNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[StoreInterTrans]
        WHERE [ISTNo] IS NOT NULL
        GROUP BY [ISTNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'StoreInterTrans.ISTNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([ISTNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([ISTNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([ISTNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[StoreInterTrans]
    WHERE [ISTNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([ISTNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'StoreInterTrans.ISTNo /scope:Suffix' AS Series,
           'table dbo.StoreInterTrans is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 45 -- ToolCribIssue.TCIssueNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ToolCribIssue', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: TCIssueNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'ToolCribIssue.TCIssueNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [TCIssueNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ToolCribIssue]
        WHERE [TCIssueNo] IS NOT NULL
        GROUP BY [TCIssueNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'ToolCribIssue.TCIssueNo /scope:Suffix' AS Series,
        CAST([TCIssueNo] AS nvarchar(200)) AS [TCIssueNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[ToolCribIssue]
    WHERE [TCIssueNo] IS NOT NULL
    GROUP BY [TCIssueNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: TCIssueNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'ToolCribIssue.TCIssueNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [TCIssueNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ToolCribIssue]
        WHERE [TCIssueNo] IS NOT NULL
        GROUP BY [TCIssueNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'ToolCribIssue.TCIssueNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([TCIssueNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([TCIssueNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([TCIssueNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[ToolCribIssue]
    WHERE [TCIssueNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([TCIssueNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'ToolCribIssue.TCIssueNo /scope:Suffix' AS Series,
           'table dbo.ToolCribIssue is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 46 -- ToolCribReturns.TCReturnNo /scope:Suffix
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.ToolCribReturns', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: TCReturnNo + Suffix */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'ToolCribReturns.TCReturnNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [TCReturnNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ToolCribReturns]
        WHERE [TCReturnNo] IS NOT NULL
        GROUP BY [TCReturnNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'ToolCribReturns.TCReturnNo /scope:Suffix' AS Series,
        CAST([TCReturnNo] AS nvarchar(200)) AS [TCReturnNo],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[ToolCribReturns]
    WHERE [TCReturnNo] IS NOT NULL
    GROUP BY [TCReturnNo], [Suffix]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: TCReturnNo + Suffix
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'ToolCribReturns.TCReturnNo /scope:Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [TCReturnNo], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[ToolCribReturns]
        WHERE [TCReturnNo] IS NOT NULL
        GROUP BY [TCReturnNo], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'ToolCribReturns.TCReturnNo /scope:Suffix' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([TCReturnNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([TCReturnNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([TCReturnNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[ToolCribReturns]
    WHERE [TCReturnNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([TCReturnNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'ToolCribReturns.TCReturnNo /scope:Suffix' AS Series,
           'table dbo.ToolCribReturns is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 47 -- StaffLoan.LoanNo /scope:none
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.StaffLoan', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: LoanNo */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'StaffLoan.LoanNo /scope:none' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [LoanNo], COUNT(*) AS RowsInGroup
        FROM dbo.[StaffLoan]
        WHERE [LoanNo] IS NOT NULL
        GROUP BY [LoanNo]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'StaffLoan.LoanNo /scope:none' AS Series,
        CAST([LoanNo] AS nvarchar(200)) AS [LoanNo],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[StaffLoan]
    WHERE [LoanNo] IS NOT NULL
    GROUP BY [LoanNo]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: LoanNo
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'StaffLoan.LoanNo /scope:none' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [LoanNo], COUNT(*) AS RowsInGroup
        FROM dbo.[StaffLoan]
        WHERE [LoanNo] IS NOT NULL
        GROUP BY [LoanNo]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'StaffLoan.LoanNo /scope:none' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([LoanNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([LoanNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([LoanNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[StaffLoan]
    WHERE [LoanNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([LoanNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'StaffLoan.LoanNo /scope:none' AS Series,
           'table dbo.StaffLoan is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 48 -- Payments.PaymentNo /scope:none
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: PaymentNo */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'Payments.PaymentNo /scope:none' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [PaymentNo], COUNT(*) AS RowsInGroup
        FROM dbo.[Payments]
        WHERE [PaymentNo] IS NOT NULL
        GROUP BY [PaymentNo]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'Payments.PaymentNo /scope:none' AS Series,
        CAST([PaymentNo] AS nvarchar(200)) AS [PaymentNo],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[Payments]
    WHERE [PaymentNo] IS NOT NULL
    GROUP BY [PaymentNo]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: PaymentNo
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'Payments.PaymentNo /scope:none' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [PaymentNo], COUNT(*) AS RowsInGroup
        FROM dbo.[Payments]
        WHERE [PaymentNo] IS NOT NULL
        GROUP BY [PaymentNo]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'Payments.PaymentNo /scope:none' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([PaymentNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([PaymentNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([PaymentNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[Payments]
    WHERE [PaymentNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([PaymentNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'Payments.PaymentNo /scope:none' AS Series,
           'table dbo.Payments is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 49 -- Receipts.PaymentNo /scope:none
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.Receipts', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 -- duplicate census, APPLICATION scoping: PaymentNo */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'Receipts.PaymentNo /scope:none' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [PaymentNo], COUNT(*) AS RowsInGroup
        FROM dbo.[Receipts]
        WHERE [PaymentNo] IS NOT NULL
        GROUP BY [PaymentNo]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 2 sample -- up to 20 offending groups, largest first */
    SELECT TOP (20)
        'BLOCK2-SAMPLE'               AS Block,
        'Receipts.PaymentNo /scope:none' AS Series,
        CAST([PaymentNo] AS nvarchar(200)) AS [PaymentNo],
        COUNT(*)                      AS RowsInGroup
    FROM dbo.[Receipts]
    WHERE [PaymentNo] IS NOT NULL
    GROUP BY [PaymentNo]
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;

    /* BLOCK 3 -- duplicate census, UNQUALIFIED scoping: PaymentNo
       The gap between block 2 and block 3 is the number of groups a naive
       unique index would reject but the application accepts today.
       This block is required. It is NOT redundant with block 2. */
    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'Receipts.PaymentNo /scope:none' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [PaymentNo], COUNT(*) AS RowsInGroup
        FROM dbo.[Receipts]
        WHERE [PaymentNo] IS NOT NULL
        GROUP BY [PaymentNo]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- format-shape census. Digit runs collapse to a single '#',
       so historical shape variation in the stored number becomes visible.
       M2-B12-03 must preserve the user-visible format exactly, and cannot
       do that if a series turns out to hold several historical shapes. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'Receipts.PaymentNo /scope:none' AS Series,
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([PaymentNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#') AS NumberShape,
        COUNT(*)                      AS RowCountForShape,
        MIN(CAST([PaymentNo] AS nvarchar(200))) AS ExampleLowest,
        MAX(CAST([PaymentNo] AS nvarchar(200))) AS ExampleHighest
    FROM dbo.[Receipts]
    WHERE [PaymentNo] IS NOT NULL
    GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CAST([PaymentNo] AS nvarchar(200)),'0','#'),'1','#'),'2','#'),'3','#'),'4','#'),'5','#'),'6','#'),'7','#'),'8','#'),'9','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#'),'##','#')
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'Receipts.PaymentNo /scope:none' AS Series,
           'table dbo.Receipts is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 50 -- DcRunningNumbers.LastNumber /key:DcType+Suffix   (ALLOCATION TABLE)
   KB-100 section 9 records NO unique index on (DcType, Suffix) in the
   EF model. If duplicate key rows exist, the allocator's FirstOrDefaultAsync
   silently picks one of them. That is what block 2 measures here.
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.DcRunningNumbers', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 and BLOCK 3 coincide for an allocation table: its logical key IS
       the unqualified key. Both are reported so the census stays uniform. */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'DcRunningNumbers.LastNumber /key:DcType+Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [DcType], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[DcRunningNumbers]
        GROUP BY [DcType], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'DcRunningNumbers.LastNumber /key:DcType+Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount
    FROM (
        SELECT [DcType], [Suffix]
        FROM dbo.[DcRunningNumbers]
        GROUP BY [DcType], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- the stored allocator state itself, per key. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'DcRunningNumbers.LastNumber /key:DcType+Suffix' AS Series,
        CAST([DcType] AS nvarchar(200)) AS [DcType],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        MIN([LastNumber])           AS MinLastNumber,
        MAX([LastNumber])           AS MaxLastNumber,
        COUNT(*)                      AS RowsForKey
    FROM dbo.[DcRunningNumbers]
    GROUP BY [DcType], [Suffix]
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'DcRunningNumbers.LastNumber /key:DcType+Suffix' AS Series,
           'table dbo.DcRunningNumbers is not present in this database' AS Note;
GO

/* ---------------------------------------------------------------------------
   SERIES 51 -- InvoiceAutoRunningNumbers.LastNumber /key:InvoiceType+Suffix   (ALLOCATION TABLE)
   KB-100 section 9 records NO unique index on (InvoiceType, Suffix) in the
   EF model. If duplicate key rows exist, the allocator's FirstOrDefaultAsync
   silently picks one of them. That is what block 2 measures here.
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.InvoiceAutoRunningNumbers', 'U') IS NOT NULL
BEGIN
    /* BLOCK 2 and BLOCK 3 coincide for an allocation table: its logical key IS
       the unqualified key. Both are reported so the census stays uniform. */
    SELECT
        'BLOCK2-APPSCOPE'             AS Block,
        'InvoiceAutoRunningNumbers.LastNumber /key:InvoiceType+Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount,
        ISNULL(SUM(g.RowsInGroup), 0) AS RowsInDuplicateGroups,
        ISNULL(MAX(g.RowsInGroup), 0) AS LargestGroup
    FROM (
        SELECT [InvoiceType], [Suffix], COUNT(*) AS RowsInGroup
        FROM dbo.[InvoiceAutoRunningNumbers]
        GROUP BY [InvoiceType], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    SELECT
        'BLOCK3-UNQUALIFIED'          AS Block,
        'InvoiceAutoRunningNumbers.LastNumber /key:InvoiceType+Suffix' AS Series,
        COUNT(*)                      AS DuplicateGroupCount
    FROM (
        SELECT [InvoiceType], [Suffix]
        FROM dbo.[InvoiceAutoRunningNumbers]
        GROUP BY [InvoiceType], [Suffix]
        HAVING COUNT(*) > 1
    ) AS g;

    /* BLOCK 4 -- the stored allocator state itself, per key. */
    SELECT
        'BLOCK4-SHAPE'                AS Block,
        'InvoiceAutoRunningNumbers.LastNumber /key:InvoiceType+Suffix' AS Series,
        CAST([InvoiceType] AS nvarchar(200)) AS [InvoiceType],
        CAST([Suffix] AS nvarchar(200)) AS [Suffix],
        MIN([LastNumber])           AS MinLastNumber,
        MAX([LastNumber])           AS MaxLastNumber,
        COUNT(*)                      AS RowsForKey
    FROM dbo.[InvoiceAutoRunningNumbers]
    GROUP BY [InvoiceType], [Suffix]
    ORDER BY COUNT(*) DESC;
END
ELSE
    SELECT 'SERIES-ABSENT' AS Block, 'InvoiceAutoRunningNumbers.LastNumber /key:InvoiceType+Suffix' AS Series,
           'table dbo.InvoiceAutoRunningNumbers is not present in this database' AS Note;
GO

/* ============================================================================
   END OF SCRIPT -- 51 series covered.
   Please save the ENTIRE text output, including any error lines, and send it
   back unedited, one file per tenant database.
   ============================================================================ */
