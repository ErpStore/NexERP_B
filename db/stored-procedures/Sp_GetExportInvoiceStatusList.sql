
CREATE OR ALTER   PROCEDURE [dbo].[Sp_GetExportInvoiceStatusList]
(
    @Status NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY ei.ExpInvId DESC, eis.ExpInvSubId DESC) AS SlNo,

        ISNULL(ei.ExpInvNo + ei.Suffix, '') AS ExpInvoiceNo,
        ISNULL(CONVERT(VARCHAR(20), ei.ExpInvDate, 103), '') AS ExpInvoiceDate,

        ISNULL(c.CustName, '') AS Customer,

		 ISNULL(d.DcNo + d.Suffix, '') AS RefDCNo,
	    ISNULL(CONVERT(VARCHAR(20), d.DcDate, 103), '') AS RefDCDate,

		 ISNULL(p.PONo, '') AS RefPoNo,
	     ISNULL(CONVERT(VARCHAR(20), p.PODate, 103), '') AS RefPoDate,

        ISNULL(ei.MainRemark, '') AS MainRemarks,

        CASE
            WHEN ISNULL(ei.IsCancel,0) = 1 THEN 'Cancelled'
            WHEN ISNULL(ei.InvTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END AS InvoiceStatus,

        ISNULL(i.ItemCode,'') AS ItemCode,
        ISNULL(i.ItemName,'') AS ItemName,
        ISNULL(i.Specification,'') AS ItemSpecification,
        ISNULL(i.MeasureUnit,'') AS MeasureUnit,
        ISNULL(i.HSNCode,'') AS HSNCode,

        ISNULL(eis.Qty,0) AS Qty,
        ISNULL(eis.BalQty,0) AS BalQty,
        ISNULL(eis.UnitPrice,0) AS UnitPrice,

        ISNULL(eis.LineGross,0) AS LineGross,
        ISNULL(eis.LineDiscountAmount,0) AS LineDiscountAmount,
        ISNULL(eis.LineBasicAmount,0) AS LineBasicAmount,

        ISNULL(eis.LineCGSTRate,0) AS LineCGSTRate,
        ISNULL(eis.LineSGSTRate,0) AS LineSGSTRate,
        ISNULL(eis.LineIGSTRate,0) AS LineIGSTRate,


        ISNULL(eis.Remarks,'') AS ItemRemarks,

        CAST(ISNULL(eis.ItemCancel,0) AS BIT) AS ItemCancel,

        ISNULL(ei.GrandTotal,0) AS GrandTotal,
        ISNULL(ei.Balance,0) AS Balance,

        ISNULL(ei.CreatedBy,'') AS CreatedBy,
        ISNULL(CONVERT(VARCHAR(20), ei.CreatedDate, 103), '') AS CreatedDate,
        ISNULL(ei.ModifiedBy,'') AS ModifiedBy,
        ISNULL(CONVERT(VARCHAR(20), ei.ModifiedDate, 103), '') AS ModifiedDate

    FROM ExpInv ei

    INNER JOIN ExpInvSub eis
        ON ei.ExpInvId = eis.ExpInvId

    LEFT JOIN Customer c
        ON ei.CustId = c.CustId

    LEFT JOIN Item i
        ON eis.ItemId = i.ItemId

    LEFT JOIN MfgDcSub ds
		on ds.DcSubId=eis.RefDcSubId
	LEFT JOIN MfgDc  d
		on d.DcId=ds.DcId

	LEFT JOIN MfgPoSub ps
		on ps.PoSubId=eis.RefPoSubId
	LEFT JOIN MfgPo  p
		on p.PoId=ps.PoId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
        OR
        CASE
            WHEN ISNULL(ei.IsCancel,0) = 1 THEN 'Cancelled'
            WHEN ISNULL(ei.InvTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END = @Status
    )

    ORDER BY ei.ExpInvId DESC, eis.ExpInvSubId DESC;

END
