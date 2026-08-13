
CREATE OR ALTER   PROCEDURE [dbo].[Sp_GetProductionReturnAssyStatusList]
(
    @Status NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY pra.ReturnId DESC, pras.ReturnSubId DESC) AS SlNo,

        ISNULL(pra.ReturnNo, '') + ISNULL(pra.Suffix, '') AS ReturnNo,
        ISNULL(CONVERT(VARCHAR(20), pra.ReturnDate, 103), '') AS ReturnDate,

        ISNULL(pra.ReturnBy, '') AS ReturnBy,
        ISNULL(s.StoreName, '') AS StoreName,

        ISNULL(pra.MainRemark, '') AS MainRemark,

        CASE
            WHEN ISNULL(pra.ReturnTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END AS ReturnStatus,

        ISNULL(i.ItemCode,'') AS ItemCode,
        ISNULL(i.ItemName,'') AS ItemName,
        ISNULL(i.Specification,'') AS ItemSpecification,
        ISNULL(i.MeasureUnit,'') AS MeasureUnit,
        ISNULL(i.HSNCode,'') AS HSNCode,

        ISNULL(pras.QtyReturned,0) AS QtyReturned,
        ISNULL(pras.BalQty,0) AS BalQty,
        ISNULL(pras.UnitPrice,0) AS UnitPrice,

		 ISNULL(p.IssueNo + p.Suffix,'') AS RefIssueNo,
		ISNULL(CONVERT(VARCHAR(20), p.IssueDate, 103), '') AS RefIssueDate,

        ISNULL(j.JobNo + j.Suffix,'') AS RefJobNo,
		ISNULL(CONVERT(VARCHAR(20), j.JobDate, 103), '') AS RefJobDate,

        ISNULL(pras.Remarks,'') AS ItemRemarks,

        ISNULL(pra.CreatedBy,'') AS CreatedBy,
        ISNULL(CONVERT(VARCHAR(20), pra.CreatedDate, 103), '') AS CreatedDate,
        ISNULL(pra.ModifiedBy,'') AS ModifiedBy,
        ISNULL(CONVERT(VARCHAR(20), pra.ModifiedDate, 103), '') AS ModifiedDate

    FROM ProductionReturnAssy pra

    INNER JOIN ProductionReturnAssySub pras
        ON pra.ReturnId = pras.ReturnId

    LEFT JOIN Item i
        ON pras.ItemId = i.ItemId

    LEFT JOIN Stores s
        ON pra.AddStoreId = s.StoreId

    LEFT JOIN JobOrder j
        ON pras.RefJobOrderId = j.JobId

    LEFT JOIN ProductionIssueAssySub ps
        ON ps.IssueSubId = pras.RefIssueSubId
    LEFT JOIN ProductionIssueAssy p
        ON p.IssueId = ps.IssueId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
        OR
        CASE
            WHEN ISNULL(pra.ReturnTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END = @Status
    )

    ORDER BY pra.ReturnId DESC, pras.ReturnSubId DESC;

END
