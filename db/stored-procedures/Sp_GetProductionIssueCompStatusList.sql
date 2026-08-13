
CREATE OR ALTER   PROCEDURE [dbo].[Sp_GetProductionIssueCompStatusList]
(
    @Status NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY pic.IssueId DESC, pics.IssueSubId DESC) AS SlNo,

        ISNULL(pic.IssueNo,'') + ISNULL(pic.Suffix,'') AS IssueNo,
        ISNULL(CONVERT(VARCHAR(20), pic.IssueDate, 103),'') AS IssueDate,

        ISNULL(pic.IssueToWhom,'') AS IssueTo,
        ISNULL(c.CustName,'') AS Customer,
        ISNULL(s.StoreName,'') AS StoreName,

        ISNULL(pic.MainRemark,'') AS MainRemark,

        CASE
            WHEN ISNULL(pic.IssueTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END AS IssueStatus,

        ISNULL(i.ItemCode,'') AS ItemCode,
        ISNULL(i.ItemName,'') AS ItemName,
        ISNULL(i.Specification,'') AS ItemSpecification,
        ISNULL(i.MeasureUnit,'') AS MeasureUnit,
        ISNULL(i.HSNCode,'') AS HSNCode,

        ISNULL(pics.Qty,0) AS Qty,
        ISNULL(pics.BalQty,0) AS BalQty,
        ISNULL(pics.UnitPrice,0) AS UnitPrice,

        ISNULL(pics.ProcessCost,0) AS ProcessCost,


        ISNULL(pics.Remark,'') AS ItemRemarks,

        ISNULL(pic.CreatedBy,'') AS CreatedBy,
        ISNULL(CONVERT(VARCHAR(20), pic.CreatedDate, 103),'') AS CreatedDate,
        ISNULL(pic.ModifiedBy,'') AS ModifiedBy,
        ISNULL(CONVERT(VARCHAR(20), pic.ModifiedDate, 103),'') AS ModifiedDate

    FROM ProductionIssueComp pic

    INNER JOIN ProductionIssueCompSub pics
        ON pic.IssueId = pics.IssueId

    -- IMPORTANT: Only OUT items
    AND pics.TransType = 'Out'

    LEFT JOIN Item i
        ON pics.ItemId = i.ItemId

    LEFT JOIN Stores s
        ON pic.StoreIssId = s.StoreId

    LEFT JOIN Customer c
        ON pic.CustId = c.CustId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
        OR
        CASE
            WHEN ISNULL(pic.IssueTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END = @Status
    )

    ORDER BY pic.IssueId DESC, pics.IssueSubId DESC;

END
