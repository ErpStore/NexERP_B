CREATE OR ALTER    PROCEDURE [dbo].[Sp_VendorPRRating]
(
	@fromDate DATETIME,
    @toDate DATETIME,
    @vendorcode INT
    --@VendorCode INT = NULL,
    --@FromDate DATE = NULL,
    --@ToDate DATE = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

SELECT 
	
        ROW_NUMBER() OVER
        (
            ORDER BY i.ItemCode
        ) AS SlNo,

    v.VendorName AS VendorName,

    CONCAT(m.MReqNo, m.Suffix) AS PRNo,

    CONVERT(VARCHAR(10), m.MReqDate, 103) AS PRDate,

    CAST(ms.PurchQty AS DECIMAL(18,2)) AS PRQty,

    CAST(ms.ItemCancel AS BIT) AS PRItemCancl,

    m.CreatedBy AS PRCreatedBy,

    CONVERT(VARCHAR(10), m.CreatedDate, 103) AS PRCreatedDate,

    CONCAT(po.PONo, po.Suffix) AS PONo,

    CONVERT(VARCHAR(10), po.PODate, 103) AS PODate,

    CAST(pos.Qty AS DECIMAL(18,2)) AS POQty,

    CAST(pos.ItemCancel AS BIT) AS ItemCancel,

    po.CreatedBy AS POCreatedBy,

    CONVERT(VARCHAR(10), po.CreatedDate, 103) AS POCreatedDate,

    i.ItemCode AS ItemCode,

    i.ItemName AS ItemName,

    DATEDIFF(DAY, m.MReqDate, po.PODate) AS DelayDays,

    CASE
        WHEN DATEDIFF(DAY, m.MReqDate, po.PODate) <= 1 
            THEN 'Excellent'

        WHEN DATEDIFF(DAY, m.MReqDate, po.PODate) <= 3 
            THEN 'Good'

        WHEN DATEDIFF(DAY, m.MReqDate, po.PODate) <= 7 
            THEN 'Average'

        ELSE 'Poor'
    END AS Rating

FROM PurchPo po

INNER JOIN PurchPoSub pos
    ON po.PoId = pos.PoId

INNER JOIN MaterialReqSub ms
    ON ms.MReqSubId = pos.RefMReqSubId and ms.ItemId=pos.ItemId

INNER JOIN MaterialReq m
    ON m.MReqId = ms.MReqId

INNER JOIN Vendor v
    ON v.VendorCode = po.VendorCode

INNER JOIN Item i
    ON i.ItemId = ms.ItemId and i.itemid=pos.ItemId

WHERE
(
    @VendorCode = 0
    OR po.VendorCode = @VendorCode
)
AND
(
    @FromDate IS NULL
    OR CAST(po.PODate AS DATE) >= @FromDate
)
AND
(
    @ToDate IS NULL
    OR CAST(po.PODate AS DATE) <= @ToDate
)
End

