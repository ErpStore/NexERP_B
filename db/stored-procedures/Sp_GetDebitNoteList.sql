CREATE OR ALTER  PROCEDURE [dbo].[Sp_GetDebitNoteList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY dns.DbSubId) AS SlNo,

        -- Header Details
        ISNULL(v.VendorName, '') AS VendorSupplier,
        ISNULL(dn.DebitNo, '')+ISNULL(dn.Suffix,'') AS DebitNo,
        ISNULL(dn.DebitDate, '1900-01-01') AS DebitDate,

		CASE
            WHEN ISNULL(dn.Rejection, 0) = 1 THEN 'Rejection'
            ELSE 'Rate Differenence'
        END AS Type,
        ISNULL(dn.MainRemark, '') AS Remarks,

		 CASE
            WHEN ISNULL(dn.Balance, 0) <= 0 THEN 'Completed'
            ELSE 'Pending'
        END AS CreditNoteStatus,
        -- Item Details
        ISNULL(i.ItemCode, '') AS ItemCode,
        ISNULL(i.ItemName, '') AS ItemName,
        ISNULL(i.Specification, '') AS ItemSpecification,
        ISNULL(i.MeasureUnit, '') AS MeasureUnit,
        ISNULL(i.HSNCode, '') AS HSNCode,

        ISNULL(dns.Qty, 0) AS Qty,
        ISNULL(dns.RejQty, 0) AS RejectQty,
        ISNULL(dns.RewQty, 0) AS ReworkQty,
        ISNULL(dns.CrDrQty, '') AS CDQty,
		ISNULL(dns.CrDrUnitPrice, '') AS CrDrUnitPrice,
		ISNULL(dns.Remarks, '') AS ItemRemarks,
        -- PO Reference
        ISNULL(p.PONo, '') AS RefPoNo,
        ISNULL(CONVERT(VARCHAR(10), p.PODate, 103), '') AS RefPoDt,
        --PurchInv Reference
        ISNULL(pii.InvNo, '') AS RefInvNo,
        ISNULL(CONVERT(VARCHAR(10), pii.InvDate, 103), '') AS RefInvDt,
		--SubConInv Reference
        ISNULL(si.InvNo, '') AS RefSubConInvNo,
        ISNULL(CONVERT(VARCHAR(10), si.InvDate, 103), '') AS RefSubConInvDt
    FROM DebitNote dn

        INNER JOIN DebitNoteSub dns
            ON dns.DbId = dns.DbId

        INNER JOIN Item i
            ON dns.ItemId = i.ItemId

        INNER JOIN Vendor v
            ON dn.VendorCode = v.VendorCode

        LEFT JOIN PurchPoSub pps
            ON dns.RefPoSubId = pps.PoId

        LEFT JOIN PurchPo p
            ON pps.PoId = p.PoId

        LEFT JOIN PurchaseInvoiceSub pis
            ON dns.RefPurchInvSubId = pis.InvId

        LEFT JOIN PurchaseInvoice pii
            ON pii.InvId = pis.InvId

		LEFT JOIN SubConInvSub sis
            ON dns.RefSubConInvSubId = sis.InvId

        LEFT JOIN SubConInv si
            ON pii.InvId = pis.InvId

	WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'

        OR (
            @Status = 'Completed'
            AND ISNULL(dn.Balance, 0) <= 0
        )

        OR (
            @Status = 'Pending'
            AND ISNULL(dn.Balance, 0) > 0
        )
    )
    ORDER BY dn.DebitNo, dns.DbSubId;

END

