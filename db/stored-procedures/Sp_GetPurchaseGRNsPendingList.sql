CREATE OR ALTER  PROCEDURE [dbo].[Sp_GetPurchaseGRNsPendingList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY pgs.GRNSubId) AS SlNo,

        -- Header Details
        ISNULL(c.VendorName, '') AS VendorSupplier,
        ISNULL(pg.GRNNo, '')+ISNULL(pg.Suffix,'') AS GRNNo,
        ISNULL(pg.GRNDate, '1900-01-01') AS GRNDate,

        CASE
            WHEN ISNULL(pg.IsWithoutPoGRN, 0) = 1 THEN 'Without PO'
            ELSE 'With PO'
        END AS Type,

        ISNULL(pg.MainRemarks, '') AS Remarks,

        CASE
            WHEN ISNULL(pg.GRNTally, 0) = 1 THEN 'Completed'
            WHEN ISNULL(pg.GRNCancel, 0) = 1 THEN 'Cancelled'
            ELSE 'Pending'
        END AS GRNStatus,

        -- Item Details
        ISNULL(i.ItemCode, '') AS ItemCode,
        ISNULL(i.ItemName, '') AS ItemName,
        ISNULL(i.Specification, '') AS ItemSpecification,
        ISNULL(i.MeasureUnit, '') AS MeasureUnit,
        ISNULL(i.HSNCode, '') AS HSNCode,

        ISNULL(pgs.Qty, 0) AS Qty,
        ISNULL(pgs.BalQty, 0) AS BalQty,
        ISNULL(pgs.UnitPrice, 0) AS UnitPrice,
        ISNULL(pgs.Remark, '') AS ItemRemarks,

        -- PO Reference
        ISNULL(p.PONo+p.Suffix, '') AS RefPoNo,
        ISNULL(CONVERT(VARCHAR(10), p.PODate, 103), '') AS RefPoDt,
		 -- DC Reference
        ISNULL(pg.RefDcNo, '') AS RefDcNo,
        ISNULL(CONVERT(VARCHAR(10), pg.RefDcDate, 103), '') AS RefDcDt,
        -- Inv Reference
   -- Inv Reference
CASE
    WHEN ISNULL(pg.RefInvNo,'') <> ''
        THEN pg.RefInvNo
    WHEN ISNULL(pii.InvNo,'') <> ''
        THEN pii.InvNo
    ELSE ''
END AS RefInvNo,

CASE
    WHEN ISNULL(pg.RefInvNo,'') <> ''
        THEN ISNULL(CONVERT(VARCHAR(10), pg.RefInvDate, 103), '')
    WHEN ISNULL(pii.InvNo,'') <> ''
        THEN ISNULL(CONVERT(VARCHAR(10), pii.InvDate, 103), '')
    ELSE ''
END AS RefInvDt

    FROM PurchaseGRN pg

        INNER JOIN PurchaseGRNSub pgs
            ON pg.GRNId = pgs.GRNId

        INNER JOIN Item i
            ON pgs.ItemId = i.ItemId

        INNER JOIN Vendor c
            ON pg.VendorCode = c.VendorCode

        LEFT JOIN PurchPoSub pps
            ON pgs.RefPoSubId = pps.PoSubId

        LEFT JOIN PurchPo p
            ON pps.PoId = p.PoId

         --inner join PurchaseSCNSub scns on scns.RefGRNSubId=pgs.GRNSubId 

        LEFT JOIN PurchaseInvoiceSub pis
            ON pis.refPoSubId = pgs.RefPoSubId

        LEFT JOIN PurchaseInvoice pii
            ON pii.InvId = pis.InvId


    WHERE
    (
        @Status IS NULL
        OR @Status = ''
		OR @Status = 'All'
        OR
        (
            @Status = 'Completed'
            AND ISNULL(pg.GRNTally, 0) = 1
        )
        OR
        (
            @Status = 'Cancelled'
            AND ISNULL(pg.GRNCancel, 0) = 1
        )
        OR
        (
            @Status = 'Pending'
            AND ISNULL(pg.GRNTally, 0) = 0
            AND ISNULL(pg.GRNCancel, 0) = 0
        )
    )

    ORDER BY pg.GRNNo, pgs.GRNSubId;

END
