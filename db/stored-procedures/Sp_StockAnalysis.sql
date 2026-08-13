CREATE OR ALTER         PROCEDURE [dbo].[Sp_StockAnalysis]
    @StoreId INT,
	@CategoryCode INT,
	@Sdate DATETIME,
    @EndDate DATETIME
AS
BEGIN



--WITH OpeningStock AS (
--    SELECT 
--        sa.ItemId,
--        SUM(sa.AddQty) 
--        - ISNULL(SUM(si.IssueQty), 0) AS OpeningQty
--    FROM StockAdd sa
--    LEFT JOIN StockIssue si 
--        ON si.ItemId = sa.ItemId 
--        AND si.StoreId = sa.StoreId
--        AND si.IssueDate < @SDate
--    WHERE sa.StoreId = @StoreId 
--      AND sa.AddDate < @SDate
--    GROUP BY sa.ItemId
--),  //19052026
WITH OpeningStock AS
(
    SELECT t.ItemId,CAST(SUM(t.ReceivedQty) - SUM(t.IssueQty) AS DECIMAL(18,2)) AS OpeningQty
    FROM
    (
        SELECT 
            sa.ItemId,
            SUM(sa.AddQty) AS ReceivedQty,
            0 AS IssueQty
        FROM StockAdd sa
        WHERE sa.StoreId = @StoreId
          AND sa.AddDate < @SDate
        GROUP BY sa.ItemId

        UNION ALL

        SELECT 
            si.ItemId,
            0,
            SUM(si.IssueQty)
        FROM StockIssue si
        WHERE si.StoreId = @StoreId
          AND si.IssueDate < @SDate
        GROUP BY si.ItemId
    ) t
    GROUP BY t.ItemId
),

ReceivedStock AS (
    SELECT 
        ItemId, 
        SUM(sa.AddQty) AS RecQty,
        --SUM(sa.AddQty * ROUND(sa.UnitPrice,2)) / NULLIF(SUM(sa.AddQty), 0) AS UnitPrice
		CAST(SUM(sa.AddQty * sa.UnitPrice) / NULLIF(SUM(sa.AddQty), 0) AS DECIMAL(18,2)) AS UnitPrice
    FROM StockAdd sa
    WHERE sa.StoreId = @StoreId 
      --AND sa.AddDate BETWEEN @SDate AND @EndDate
	  AND sa.AddDate >= @Sdate
		  AND sa.AddDate < DATEADD(DAY, 1, @EndDate)
    GROUP BY ItemId
),

IssuedStock AS (
    SELECT 
        ItemId, 
        SUM(si.IssueQty) AS IssQty
    FROM StockIssue si
    WHERE si.StoreId = @StoreId 
      --AND si.IssueDate BETWEEN @Sdate AND @EndDate
	  AND si.IssueDate >= @Sdate
		  AND si.IssueDate < DATEADD(DAY, 1, @EndDate)
    GROUP BY ItemId
)

SELECT  ROW_NUMBER() OVER (ORDER BY ItemCode) AS SlNo, 
    i.ItemId,
    i.ItemCode,
    i.ItemName,
    i.MeasureUnit,
    c.CategoryName,
    CAST(ISNULL(i.Rate,0) AS DECIMAL(18,2)) AS UnitRate,

    CAST(ISNULL(o.OpeningQty, 0) AS DECIMAL(18,2)) AS OpeningStockQty,
    CAST(ISNULL(r.RecQty, 0) AS DECIMAL(18,2)) AS ReceivedQty,
    CAST(ISNULL(s.IssQty, 0) AS DECIMAL(18,2)) AS IssuedQty,

    -- Closing Qty
    CAST((ISNULL(o.OpeningQty, 0) + ISNULL(r.RecQty, 0) - ISNULL(s.IssQty, 0)) AS DECIMAL(18,2)) AS ClosingStockQty,

    ISNULL(r.UnitPrice, 0) AS UnitPrice,

    -- Values
    --ISNULL(o.OpeningQty, 0) * i.Rate AS OpeningStockValue,
    --ISNULL(r.RecQty, 0) * ISNULL(r.UnitPrice, 0) AS ReceivedQtyValue,
    --ISNULL(s.IssQty, 0) * i.Rate AS IssuedQtyValue,
    --(ISNULL(o.OpeningQty, 0) 
    -- + ISNULL(r.RecQty, 0) 
    -- - ISNULL(s.IssQty, 0)) * i.Rate AS ClosingStockValue

	CAST(ISNULL(o.OpeningQty, 0) * i.Rate AS DECIMAL(18,2)) AS OpeningStockValue,
	CAST(ISNULL(r.RecQty, 0) * ISNULL(r.UnitPrice, 0) AS DECIMAL(18,2)) AS ReceivedQtyValue,
	CAST(ISNULL(s.IssQty, 0) * ISNULL(i.Rate, 0) AS DECIMAL(18,2)) AS IssuedQtyValue,
	CAST((ISNULL(o.OpeningQty, 0) + ISNULL(r.RecQty, 0) - ISNULL(s.IssQty, 0)) * i.Rate AS DECIMAL(18,2)) AS ClosingStockValue

FROM Item i
INNER JOIN Category c 
    ON c.CategoryCode = i.CategoryCode
LEFT JOIN OpeningStock o 
    ON o.ItemId = i.ItemId
LEFT JOIN ReceivedStock r 
    ON r.ItemId = i.ItemId
LEFT JOIN IssuedStock s 
    ON s.ItemId = i.ItemId

WHERE i.CategoryCode = @CategoryCode

ORDER BY i.ItemCode,c.CategoryName;

END
