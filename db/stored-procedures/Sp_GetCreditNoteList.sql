CREATE OR ALTER  PROCEDURE [dbo].[Sp_GetCreditNoteList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY cns.CrSubId) AS SlNo,

        -- Header Details
        ISNULL(c.CustName, '') AS Customer,
        ISNULL(cn.CreditNo, '')+ISNULL(cn.Suffix,'') AS CreditNo,
        ISNULL(cn.CreditDate, '1900-01-01') AS CreditDate,

		CASE
            WHEN ISNULL(cn.Rejection, 0) = 1 THEN 'Rejection'
            ELSE 'Rate Differenence'
        END AS Type,
        ISNULL(cn.MainRemark, '') AS Remarks,

		 CASE
            WHEN ISNULL(cn.Balance, 0) <= 0 THEN 'Completed'
            ELSE 'Pending'
        END AS CreditNoteStatus,

        -- Item Details
        ISNULL(i.ItemCode, '') AS ItemCode,
        ISNULL(i.ItemName, '') AS ItemName,
        ISNULL(i.Specification, '') AS ItemSpecification,
        ISNULL(i.MeasureUnit, '') AS MeasureUnit,
        ISNULL(i.HSNCode, '') AS HSNCode,

        ISNULL(cns.Qty, 0) AS Qty,
        ISNULL(cns.RejQty, 0) AS RejectQty,
        ISNULL(cns.RewQty, 0) AS ReworkQty,
        ISNULL(cns.CrDrQty, '') AS CDQty,
		ISNULL(cns.CrDrUnitPrice, '') AS CrDrUnitPrice,
		ISNULL(cns.Remarks, '') AS ItemRemarks,

        -- PO Reference
        ISNULL(p.PONo, '') AS RefPoNo,
        ISNULL(CONVERT(VARCHAR(10), p.PODate, 103), '') AS RefPoDt,
        --Mfg Inv Reference
        ISNULL(mi.InvNo, '') AS RefInvNo,
        ISNULL(CONVERT(VARCHAR(10), mi.InvDate, 103), '') AS RefInvDt,
		--Lab Inv Reference
        ISNULL(li.LabInvNo, '') AS RefLabInvNo,
        ISNULL(CONVERT(VARCHAR(10), li.LabInvDate, 103), '') AS RefLabInvDt

    FROM CreditNote cn

        INNER JOIN CreditNoteSub cns
            ON cn.CrId = cns.CrId

        INNER JOIN Item i
            ON cns.ItemId = i.ItemId

        INNER JOIN Customer c
            ON cn.CustId = c.CustId

        LEFT JOIN MfgPoSub mps
            ON cns.RefPoSubId = mps.PoId

        LEFT JOIN MfgPo p
            ON mps.PoId = p.PoId

        LEFT JOIN MfgInvSub mis
            ON cns.RefInvSubId = mis.InvId

        LEFT JOIN MfgInv mi
            ON mi.InvId = mis.InvId

		LEFT JOIN LabInvSub lis
            ON cns.LabInvSubId = lis.LabInvId

        LEFT JOIN LabInv li
            ON li.LabInvId = lis.LabInvId

	WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'

        OR (
            @Status = 'Completed'
            AND ISNULL(cn.Balance, 0) <= 0
        )

        OR (
            @Status = 'Pending'
            AND ISNULL(cn.Balance, 0) > 0
        )
    )

    ORDER BY cn.CreditNo, cns.CrSubId;

END

