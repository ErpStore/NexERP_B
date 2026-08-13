
CREATE OR ALTER   PROCEDURE [dbo].[Sp_GetProductionReturnCompStatusList]
(
    @Status NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY prc.ReturnId DESC, prcs.ReturnSubId DESC) AS SlNo,

        ISNULL(prc.ReturnNo,'') + ISNULL(prc.Suffix,'') AS ReturnNo,
        ISNULL(CONVERT(VARCHAR(20), prc.ReturnDate, 103),'') AS ReturnDate,

        ISNULL(prc.ReturnBy,'') AS ReturnBy,
        ISNULL(c.CustName,'') AS Customer,
        ISNULL(s.StoreName,'') AS StoreName,

        ISNULL(prc.MainRemark,'') AS MainRemark,

        CASE
            WHEN ISNULL(prc.ReturnTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END AS ReturnStatus,

        ISNULL(i.ItemCode,'') AS ItemCode,
        ISNULL(i.ItemName,'') AS ItemName,
        ISNULL(i.Specification,'') AS ItemSpecification,
        ISNULL(i.MeasureUnit,'') AS MeasureUnit,
        ISNULL(i.HSNCode,'') AS HSNCode,

        ISNULL(prcs.Qty,0) AS Qty,
        ISNULL(prcs.BalQty,0) AS BalQty,
        ISNULL(prcs.UnitPrice,0) AS UnitPrice,

        ISNULL(ct.CategoryName,'') AS CategoryName,

		ISNULL(p.IssueNo,'') + ISNULL(p.Suffix,'') AS RefIssueNo,
        ISNULL(CONVERT(VARCHAR(20), p.IssueDate, 103),'') AS RefIssueDate,

	    ISNULL(m.PONo,'') + ISNULL(m.Suffix,'') AS RefPoNo,
        ISNULL(CONVERT(VARCHAR(20), m.PODate, 103),'') AS RefPoDate,

		ISNULL(r.RCNo,'') + ISNULL(r.Suffix,'') AS RCNo,
        ISNULL(CONVERT(VARCHAR(20), r.RCDate, 103),'') AS RCDate,

        ISNULL(prcs.Remarks,'') AS ItemRemarks,


        ISNULL(prc.CreatedBy,'') AS CreatedBy,
        ISNULL(CONVERT(VARCHAR(20), prc.CreatedDate, 103),'') AS CreatedDate,
        ISNULL(prc.ModifiedBy,'') AS ModifiedBy,
        ISNULL(CONVERT(VARCHAR(20), prc.ModifiedDate, 103),'') AS ModifiedDate

    FROM ProductionReturnComp prc

    INNER JOIN ProductionReturnCompSub prcs
        ON prc.ReturnId = prcs.ReturnId
        AND prcs.TransType = 'In'   -- ✅ IMPORTANT

    LEFT JOIN Item i
        ON prcs.ItemId = i.ItemId

    LEFT JOIN Stores s
        ON prc.AddStoreId = s.StoreId

    LEFT JOIN Customer c
        ON prc.CustId = c.CustId

	LEFT JOIN Category ct
		on ct.CategoryCode=prcs.CategoryCode

	LEFT JOIN ProductionIssueCompSub ps
		on ps.IssueSubId=prcs.RefIssueSubId

	LEFT JOIN ProductionIssueComp p
		on p.IssueId=ps.IssueId

	LEFT JOIN MfgPoSub ms
		on ms.PoSubId=prcs.RefPoSubId

	LEFT JOIN MfgPo m
		on m.PoId=ms.PoId

	LEFT JOIN RouteCardSub rs
		on rs.RCSubId=prcs.RefRcSubId

	LEFT JOIN RouteCard r
		on r.RCId=rs.RCId


    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
        OR
        CASE
            WHEN ISNULL(prc.ReturnTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END = @Status
    )

    ORDER BY prc.ReturnId DESC, prcs.ReturnSubId DESC;

END
