CREATE OR ALTER  PROCEDURE [dbo].[Sp_GetPurchaseSCNsPendingList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY pss.SCNSubId) AS SlNo,

        -- Header Details
        ISNULL(c.VendorName, '') AS VendorSupplier,
        ISNULL(ps.SCNNo, '')+ISNULL(ps.Suffix,'') AS SCNNo,
        ISNULL(ps.SCNDate, '1900-01-01') AS SCNDate,
        ISNULL(ps.MainRemarks, '') AS Remarks,

        CASE
            WHEN ISNULL(ps.SCNTally, 0) = 1 THEN 'Completed'
            WHEN ISNULL(ps.SCNCancel, 0) = 1 THEN 'Cancelled'
            ELSE 'Pending'
        END AS SCNStatus,

        -- Item Details
        ISNULL(i.ItemCode, '') AS ItemCode,
        ISNULL(i.ItemName, '') AS ItemName,
        ISNULL(i.Specification, '') AS ItemSpecification,
        ISNULL(i.MeasureUnit, '') AS MeasureUnit,
        ISNULL(i.HSNCode, '') AS HSNCode,

        ISNULL(pss.AcceptQty, 0) AS Qty,
        ISNULL(pss.BalQty, 0) AS BalQty,
		ISNULL(pss.RejectQty, 0) AS RejectQty,
		ISNULL(pss.RejectBalQty, 0) AS RejectBalQty,
		ISNULL(pss.ReworkQty, 0) AS RewQty,
        ISNULL(pss.UnitPrice, 0) AS UnitPrice,
        ISNULL(pss.Remark, '') AS ItemRemarks,

        -- PO Reference
        ISNULL(p.PONo, '') AS RefPoNo,
        ISNULL(CONVERT(VARCHAR(10), p.PODate, 103), '') AS RefPoDt,
        -- GRN Reference
        ISNULL(pg.GRNNo, '') AS RefGRNNo,
        ISNULL(CONVERT(VARCHAR(10), pg.GRNDate, 103), '') AS RefGRNDt

    FROM PurchaseSCN ps

        INNER JOIN PurchaseSCNSub pss
            ON ps.SCNId = pss.SCNId

        INNER JOIN Item i
            ON pss.ItemId = i.ItemId

        INNER JOIN Vendor c
            ON ps.VendorCode = c.VendorCode

        LEFT JOIN PurchPoSub pps
            ON pss.RefPoSubId = pps.PoId

        LEFT JOIN PurchPo p
            ON pps.PoId = p.PoId

		LEFT JOIN PurchaseGRNSub pgs
            ON pss.RefGRNSubId = pgs.GRNId

        LEFT JOIN PurchaseGRN pg
            ON pgs.GRNId = pg.GRNId


    WHERE
    (
        @Status IS NULL
        OR @Status = ''
		OR @Status = 'All'
        OR
        (
            @Status = 'Completed'
            AND ISNULL(ps.SCNTally, 0) = 1
        )
        OR
        (
            @Status = 'Cancelled'
            AND ISNULL(ps.SCNCancel, 0) = 1
        )
        OR
        (
            @Status = 'Pending'
            AND ISNULL(ps.SCNTally, 0) = 0
            AND ISNULL(ps.SCNTally, 0) = 0
        )
    )

    ORDER BY ps.SCNNo, pss.SCNSubId;

END
