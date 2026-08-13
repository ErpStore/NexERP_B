CREATE OR ALTER   PROCEDURE [dbo].[Sp_StockLedger]
(
    @ItemId INT,
	@StoreId INT,
    @FromDate DATETIME,
    @ToDate DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;
    ---------------------------------------------------------
    -- 1. OPENING BALANCE
    ---------------------------------------------------------
    DECLARE @OpeningBalance DECIMAL(18,2) = 0;

    SELECT @OpeningBalance =
        CAST(ISNULL(SUM(ReceivedQty),0) - ISNULL(SUM(IssueQty),0) AS DECIMAL(18,2))
    FROM
    (
        SELECT SUM(sa.AddQty) AS ReceivedQty, 0 AS IssueQty
        FROM StockAdd sa
        WHERE sa.ItemId = @ItemId
          AND sa.StoreId = @StoreId
          AND sa.AddDate < @FromDate

        UNION ALL

        SELECT 0, SUM(si.IssueQty)
        FROM StockIssue si
        WHERE si.ItemId = @ItemId
          AND si.StoreId = @StoreId
          AND si.IssueDate < @FromDate
    ) A;

    ---------------------------------------------------------
    -- 2. STORE LEDGER DATA INTO TEMP TABLE
    ---------------------------------------------------------
    IF OBJECT_ID('tempdb..#LedgerData') IS NOT NULL
        DROP TABLE #LedgerData;

    SELECT *
    INTO #LedgerData
    FROM
    (
        -- RECEIVED
        SELECT 
            sa.AddDate AS [Date],
            i.ItemCode,
            sa.RefNo AS DocNo,
            sa.AddQty AS ReceivedQty,
            0 AS IssueQty,
            sa.UnitPrice,
            sa.AddQty * sa.UnitPrice AS Amount,
            s.ScreenName AS Screen
        FROM StockAdd sa
        INNER JOIN Item i ON i.ItemId = sa.ItemId
        LEFT JOIN Screens s ON sa.Screencode = s.ScreenCode
        WHERE sa.ItemId = @ItemId
          AND sa.StoreId = @StoreId
          --AND sa.AddDate BETWEEN @FromDate AND @ToDate
		  AND sa.AddDate >= @FromDate
		  AND sa.AddDate < DATEADD(DAY, 1, @ToDate)

        UNION ALL

        -- ISSUED
        SELECT 
            si.IssueDate AS [Date],
            i.ItemCode,
            si.RefNo AS DocNo,
            0 AS ReceivedQty,
            si.IssueQty,
            si.UnitPrice,
            si.IssueQty * si.UnitPrice as Amount,
            s.ScreenName as Screen
        FROM StockIssue si
        INNER JOIN Item i ON i.ItemId = si.ItemId
        LEFT JOIN Screens s ON s.ScreenCode = si.ScreenCode
        WHERE si.ItemId = @ItemId
          AND si.StoreId = @StoreId
          --AND si.IssueDate BETWEEN @FromDate AND @ToDate
		  AND si.IssueDate >= @FromDate
		  AND si.IssueDate < DATEADD(DAY, 1, @ToDate)
    ) AS LedgerData;

    ---------------------------------------------------------
    -- 3. FINAL OUTPUT WITH RUNNING STOCK
    ---------------------------------------------------------
    SELECT 
        ROW_NUMBER() OVER (ORDER BY [Date], DocNo) AS SlNo,
        [Date],
        ItemCode,
        CAST(DocNo AS VARCHAR) + '/' + CAST(CASE 
											WHEN MONTH([Date]) >= 4 
											THEN YEAR([Date])
											ELSE YEAR([Date]) - 1
											END AS VARCHAR) + '-' +
											RIGHT(CAST(CASE WHEN MONTH([Date]) >= 4 
															THEN YEAR([Date]) + 1
															ELSE YEAR([Date])
															END AS VARCHAR),2) AS DocNo,
        CAST(ISNULL(ReceivedQty,0) AS DECIMAL(18,2)) AS ReceivedQty,
        CAST(ISNULL(IssueQty,0) AS DECIMAL(18,2)) AS IssueQty,
        CAST(ISNULL(UnitPrice,0) AS DECIMAL(18,2)) AS UnitPrice,
        CAST(ISNULL(Amount,0) AS DECIMAL(18,2)) AS Amount,
		Screen,
        CAST(ISNULL(@OpeningBalance,0) + 
        SUM(ISNULL(ReceivedQty,0) - ISNULL(IssueQty,0)) OVER 
        (ORDER BY [Date], DocNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS DECIMAL(18,2))
        AS StockQty,

        CAST(ISNULL(@OpeningBalance,0) AS DECIMAL(18,2)) AS OpeningBalance
		FROM #LedgerData
		ORDER BY [Date], DocNo;

    ---------------------------------------------------------
    -- 4. CLOSING BALANCE
    ---------------------------------------------------------
    --SELECT 
    --    @OpeningBalance +
    --    ISNULL(SUM(ReceivedQty - IssueQty),0) AS ClosingBalance
    --FROM #LedgerData;

END
