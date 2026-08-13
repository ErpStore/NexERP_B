
CREATE OR ALTER   PROCEDURE [dbo].[Sp_GetManufacturingInvoiceStatusList]
(
    @Status NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY mi.InvId DESC, mis.InvSubId DESC) AS SlNo,
		ISNULL(c.CustName, '') AS Customer,
        ISNULL(mi.InvNo + mi.Suffix, '') AS InvoiceNo,
        ISNULL(CONVERT(VARCHAR(20), mi.InvDate, 103), '') AS InvoiceDate,

        ISNULL(d.DcNo + d.Suffix, '') AS RefDCNo,
	    ISNULL(CONVERT(VARCHAR(20), d.DcDate, 103), '') AS RefDCDate,

		 ISNULL(p.PONo, '') AS RefPoNo,
	     ISNULL(CONVERT(VARCHAR(20), p.PODate, 103), '') AS RefPoDate,
		 ISNULL(ps.[LineNo], 0) AS [LineNo],
        ISNULL(mi.MainRemark, '') AS MainRemarks,

        CASE
            WHEN ISNULL(mi.IsCancel,0) = 1 THEN 'Cancelled'
            WHEN ISNULL(mi.InvTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END AS InvoiceStatus,

        ISNULL(i.ItemCode,'') AS ItemCode,
        ISNULL(i.ItemName,'') AS ItemName,
        ISNULL(i.Specification,'') AS ItemSpecification,
        ISNULL(i.MeasureUnit,'') AS MeasureUnit,
        ISNULL(i.HSNCode,'') AS HSNCode,

        ISNULL(mis.Qty,0) AS Qty,
        ISNULL(mis.BalQty,0) AS BalQty,
        ISNULL(mis.UnitPrice,0) AS UnitPrice,

        ISNULL(mis.LineGross,0) AS LineGross,
        ISNULL(mis.LineDiscountAmount,0) AS LineDiscountAmount,
        ISNULL(mis.LineBasicAmount,0) AS LineBasicAmount,

        ISNULL(mis.LineCGSTRate,0) AS LineCGSTRate,
        ISNULL(mis.LineSGSTRate,0) AS LineSGSTRate,
        ISNULL(mis.LineIGSTRate,0) AS LineIGSTRate,

        ISNULL(mis.Remarks,'') AS ItemRemarks,

        CAST(ISNULL(mis.ItemCancel,0) AS BIT) AS ItemCancel,

        ISNULL(mi.GrandTotal,0) AS GrandTotal,
        ISNULL(mi.Balance,0) AS Balance,

        ISNULL(mi.CreatedBy,'') AS CreatedBy,
        ISNULL(CONVERT(VARCHAR(20), mi.CreatedDate, 103), '') AS CreatedDate,
        ISNULL(mi.ModifiedBy,'') AS ModifiedBy,
        ISNULL(CONVERT(VARCHAR(20), mi.ModifiedDate, 103), '') AS ModifiedDate

    FROM MfgInv mi

    INNER JOIN MfgInvSub mis
        ON mi.InvId = mis.InvId

    LEFT JOIN Customer c
        ON mi.CustId = c.CustId

    LEFT JOIN Item i
        ON mis.ItemId = i.ItemId

    LEFT JOIN MfgDcSub ds
		on ds.DcSubId=mis.RefDcSubId
	LEFT JOIN MfgDc  d
		on d.DcId=ds.DcId

    LEFT JOIN MfgPoSub ps
		ON ps.PoSubId =
		   CASE
				WHEN mis.RefPoSubId IS NOT NULL
					 THEN mis.RefPoSubId
				ELSE ds.RefPoSubId
		   END

	LEFT JOIN MfgPo  p
		on p.PoId=ps.PoId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
        OR
        CASE
            WHEN ISNULL(mi.IsCancel,0) = 1 THEN 'Cancelled'
            WHEN ISNULL(mi.InvTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END = @Status
    )

    ORDER BY mi.InvId DESC, mis.InvSubId DESC;

END
