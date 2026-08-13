
CREATE OR ALTER   PROCEDURE [dbo].[Sp_GetPurchaseInvoiceStatusList]
(
    @Status NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY pi.InvId DESC, pis.InvSubId DESC) AS SlNo,

        ISNULL(pi.InvNo,'') + ISNULL(pi.Suffix,'') AS InvoiceNo,
        ISNULL(CONVERT(VARCHAR(20), pi.InvDate, 103),'') AS InvoiceDate,

        ISNULL(v.VendorName,'') AS Vendor,

        ISNULL(pi.MainRemark,'') AS MainRemark,

        CASE
            WHEN ISNULL(pi.InvCancel,0) = 1 THEN 'Cancelled'
            WHEN ISNULL(pi.ShortClose,0) = 1 THEN 'Short Closed'
            WHEN ISNULL(pi.InvTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END AS InvoiceStatus,

		      -- SCN Reference
        ISNULL(scn.SCNNo,'') + ISNULL(scn.Suffix,'') AS RefSCNNo,
        ISNULL(CONVERT(VARCHAR(20), scn.SCNDate,103),'') AS RefSCNDate,

        -- PO Reference
        ISNULL(p.PONo,'') AS RefPoNo,
        ISNULL(CONVERT(VARCHAR(20), p.PODate,103),'') AS RefPoDate,

  

        ISNULL(i.ItemCode,'') AS ItemCode,
        ISNULL(i.ItemName,'') AS ItemName,
        ISNULL(i.Specification,'') AS ItemSpecification,
        ISNULL(i.MeasureUnit,'') AS MeasureUnit,
        ISNULL(i.HSNCode,'') AS HSNCode,

        ISNULL(pis.Qty,0) AS Qty,
        ISNULL(pis.BalQty,0) AS BalQty,
        ISNULL(pis.RejectQty,0) AS RejectQty,
        ISNULL(pis.ReworkQty,0) AS ReworkQty,

        ISNULL(pis.UnitPrice,0) AS UnitPrice,

        ISNULL(pis.Remarks,'') AS ItemRemarks,

        CAST(ISNULL(pis.ItemCancel,0) AS BIT) AS ItemCancel,

        ISNULL(pi.CreatedBy,'') AS CreatedBy,
        ISNULL(CONVERT(VARCHAR(20), pi.CreatedDate,103),'') AS CreatedDate,
        ISNULL(pi.ModifiedBy,'') AS ModifiedBy,
        ISNULL(CONVERT(VARCHAR(20), pi.ModifiedDate,103),'') AS ModifiedDate

    FROM PurchaseInvoice pi

    INNER JOIN PurchaseInvoiceSub pis
        ON pi.InvId = pis.InvId

    LEFT JOIN Item i
        ON pis.ItemId = i.ItemId

    LEFT JOIN Vendor v
        ON pi.VendorCode = v.VendorCode

    -- PO Join
    LEFT JOIN MfgPoSub ps
        ON ps.PoSubId = pis.RefPoSubId

    LEFT JOIN MfgPo p
        ON p.PoId = ps.PoId

    -- SCN Join
    LEFT JOIN ProductionSCNCompSub scns
        ON scns.SCNSubId = pis.RefSCNSubId

    LEFT JOIN ProductionSCNComp scn
        ON scn.SCNId = scns.SCNId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
        OR
        CASE
            WHEN ISNULL(pi.InvCancel,0) = 1 THEN 'Cancelled'
            WHEN ISNULL(pi.ShortClose,0) = 1 THEN 'Short Closed'
            WHEN ISNULL(pi.InvTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END = @Status
    )

    ORDER BY pi.InvId DESC, pis.InvSubId DESC;

END
