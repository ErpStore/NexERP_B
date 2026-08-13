
CREATE OR ALTER   PROCEDURE [dbo].[Sp_GetLabourInvStatusList]
(
    @Status NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
       CAST(
			ROW_NUMBER() OVER
			(
				ORDER BY li.LabInvId DESC, lis.LabInvSubId DESC
			)
		AS BIGINT) AS SlNo,

        ISNULL(li.LabInvNo + li.Suffix, '') AS InvoiceNo,
        ISNULL(CONVERT(VARCHAR(20), li.LabInvDate, 103), '') AS InvoiceDate,
        ISNULL(CONVERT(VARCHAR(20), li.DueDate, 103), '') AS DueDate,

        ISNULL(ldo.DcNo + ldo.Suffix, '') AS RefDCNo,
        ISNULL(CONVERT(VARCHAR(20), ldo.DcDate, 103), '') AS RefDCDate,

        ISNULL(c.CustName, '') AS Customer,
        ISNULL(li.MainRemark, '') AS MainRemarks,

        CASE
            WHEN ISNULL(li.InvCancel,0) = 1 THEN 'Cancelled'
            WHEN ISNULL(li.ShortClose,0) = 1 THEN 'Short Closed'
            WHEN ISNULL(li.LabInvTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END AS InvoiceStatus,

		ISNULL(CONVERT(VARCHAR(20), m.PODate, 103), '') AS RefPoDate,
        ISNULL(m.PONo,'')+m.Suffix AS RefPoNo,
	   CAST(ISNULL(ms.[LineNo],0) AS BIGINT) AS [LineNo],
        ISNULL(i.ItemCode,'') AS ItemCode,
        ISNULL(i.ItemName,'') AS ItemName,
        ISNULL(i.Specification,'') AS ItemSpecification,
        ISNULL(i.MeasureUnit,'') AS MeasureUnit,
        ISNULL(i.HSNCode,'') AS HSNCode,

        ISNULL(lis.Qty,0) AS Qty,
        ISNULL(lis.BalQty,0) AS BalQty,
        ISNULL(lis.UnitPrice,0) AS UnitPrice,

        ISNULL(lis.LineGross,0) AS LineGross,
        ISNULL(lis.LineDiscountAmount,0) AS LineDiscountAmount,
        ISNULL(lis.LineBasicAmount,0) AS LineBasicAmount,

        ISNULL(lis.LineCGSTRate,0) AS LineCGSTRate,
        ISNULL(lis.LineSGSTRate,0) AS LineSGSTRate,
        ISNULL(lis.LineIGSTRate,0) AS LineIGSTRate,

        ISNULL(lis.Remarks,'') AS ItemRemarks,

        CAST(ISNULL(lis.ItemCancel,0) AS BIT) AS ItemCancel,

        ISNULL(li.GrandTotal,0) AS GrandTotal,
        ISNULL(li.Balance,0) AS Balance,

        ISNULL(li.CreatedBy,'') AS CreatedBy,
        ISNULL(CONVERT(VARCHAR(20), li.CreatedDate, 103), '') AS CreatedDate,
        ISNULL(li.ModifiedBy,'') AS ModifiedBy,
        ISNULL(CONVERT(VARCHAR(20), li.ModifiedDate, 103), '') AS ModifiedDate

    FROM LabInv li

    INNER JOIN Customer c
        ON li.CustId = c.CustId

    INNER JOIN LabInvSub lis
        ON li.LabInvId = lis.LabInvId

    INNER JOIN Item i
        ON lis.ItemId = i.ItemId

    LEFT JOIN LabourDcOutgoingSub ldos
        ON ldos.DcSubId = lis.RefDcSubId

    LEFT JOIN LabourDcOutgoing ldo
        ON ldo.DcId = ldos.DcId
	LEFT JOIN MfgPoSub ms on
	     ms.PoSubId=ldos.RefPoSubId
		 LEFT JOIN MfgPo m on 
		 m.PoId=ms.PoId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
        OR
        CASE
            WHEN ISNULL(li.InvCancel,0) = 1 THEN 'Cancelled'
            WHEN ISNULL(li.ShortClose,0) = 1 THEN 'Short Closed'
            WHEN ISNULL(li.LabInvTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END = @Status
    )

    ORDER BY li.LabInvId DESC, lis.LabInvSubId DESC;

END



