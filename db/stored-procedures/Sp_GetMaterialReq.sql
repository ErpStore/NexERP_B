CREATE OR ALTER   PROCEDURE [dbo].[Sp_GetMaterialReq]
(
    @Status NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY s.MReqSubId) AS SlNo,

        ISNULL(c.CustName, '') AS Customer,

        CAST(ISNULL(p.MReqNo, 0) AS VARCHAR(20)) AS MReqNo,
        p.MReqDate,
        ISNULL(p.MainRemark, '') AS MainRemark,

        CASE
            WHEN ISNULL(p.PRTally, 0) = 1 THEN 'Completed'
            WHEN ISNULL(p.MReqCancel, 0) = 1 THEN 'Cancelled'
            WHEN ISNULL(p.ShortClose, 0) = 1 THEN 'Short Closed'
            ELSE 'Pending'
        END AS MReqStatus,

        ISNULL(i.ItemCode, '') AS ItemCode,
        ISNULL(i.ItemName, '') AS ItemName,
        ISNULL(i.Specification, '') AS ItemSpecification,
        ISNULL(i.MeasureUnit, '') AS MeasureUnit,
        ISNULL(i.HSNCode, '') AS HSNCode,

        ISNULL(s.PurchQty, 0) AS PurchQty,
        ISNULL(s.BalQty, 0) AS BalQty,
        ISNULL(s.UnitPrice, 0) AS UnitPrice,
        ISNULL(s.Remark, '') AS Remark,

        CAST(ISNULL(mp.PONo, 0) AS VARCHAR(20)) AS PONo,
 mp.PODate, 

        ISNULL(ms.Qty, 0) AS Qty

    FROM MaterialReq p

    INNER JOIN MaterialReqSub s
        ON p.MReqId = s.MReqId

    INNER JOIN Item i
        ON s.ItemId = i.ItemId

    LEFT JOIN Customer c
        ON s.CustId = c.CustId

    LEFT JOIN MfgPoSub ms
        ON s.RefPoSubId = ms.PoSubId

    LEFT JOIN MfgPo mp
        ON ms.PoId = mp.PoId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
        OR (@Status = 'Completed'
            AND ISNULL(p.PRTally, 0) = 1)
        OR (@Status = 'Pending'
            AND ISNULL(p.PRTally, 0) = 0
            AND ISNULL(p.MReqCancel, 0) = 0
            AND ISNULL(p.ShortClose, 0) = 0)
        OR (@Status = 'Cancelled'
            AND ISNULL(p.MReqCancel, 0) = 1)
        OR (@Status = 'Short Closed'
            AND ISNULL(p.ShortClose, 0) = 1)
    )

    ORDER BY s.MReqSubId;
END
