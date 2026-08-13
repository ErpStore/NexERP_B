
CREATE OR ALTER   PROCEDURE [dbo].[Sp_GetProductionIssueAssyStatusList]
(
    @Status NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY pia.IssueId DESC, pis.IssueSubId DESC) AS SlNo,

        ISNULL(pia.IssueNo, '') + ISNULL(pia.Suffix, '') AS IssueNo,
        ISNULL(CONVERT(VARCHAR(20), pia.IssueDate, 103), '') AS IssueDate,

        ISNULL(pia.IssueToWhom, '') AS IssueToWhom,
        ISNULL(s.StoreName, '') AS StoreName,

        ISNULL(pia.DepartmentCode, '') AS DepartmentCode,
        ISNULL(pia.MonthCode, '') AS MonthCode,

        ISNULL(pia.MainRemark, '') AS MainRemark,

        CASE
            WHEN ISNULL(pia.IssueTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END AS IssueStatus,

        ISNULL(i.ItemCode,'') AS ItemCode,
        ISNULL(i.ItemName,'') AS ItemName,
        ISNULL(i.Specification,'') AS ItemSpecification,
        ISNULL(i.MeasureUnit,'') AS MeasureUnit,
        ISNULL(i.HSNCode,'') AS HSNCode,

        ISNULL(pis.IssueQty,0) AS IssueQty,
        ISNULL(pis.BalQty,0) AS BalQty,
        ISNULL(pis.UnitPrice,0) AS UnitPrice,

        ISNULL(pis.BatchNo,'') AS BatchNo,
        ISNULL(pis.Remark,'') AS ItemRemark,

        ISNULL(j.JobNo + j.Suffix,'') AS RefJobNo,
		ISNULL(CONVERT(VARCHAR(20), j.JobDate, 103), '') AS RefJobDate,

        ISNULL(pia.CreatedBy,'') AS CreatedBy,
        ISNULL(CONVERT(VARCHAR(20), pia.CreatedDate, 103), '') AS CreatedDate,
        ISNULL(pia.ModifiedBy,'') AS ModifiedBy,
        ISNULL(CONVERT(VARCHAR(20), pia.ModifiedDate, 103), '') AS ModifiedDate

    FROM ProductionIssueAssy pia

    INNER JOIN ProductionIssueAssySub pis
        ON pia.IssueId = pis.IssueId

    LEFT JOIN Item i
        ON pis.ItemId = i.ItemId

    LEFT JOIN Stores s
        ON pia.StoreIssId = s.StoreId

    LEFT JOIN JobOrder j
        ON pis.RefJobId = j.JobId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
        OR
        CASE
            WHEN ISNULL(pia.IssueTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END = @Status
    )

    ORDER BY pia.IssueId DESC, pis.IssueSubId DESC;

END
