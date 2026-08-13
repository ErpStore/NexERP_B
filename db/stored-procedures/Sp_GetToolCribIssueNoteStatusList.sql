CREATE OR ALTER  PROCEDURE [dbo].[Sp_GetToolCribIssueNoteStatusList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY tcs.TCIssueSubId) AS SlNo,

        -- Header Details
        ISNULL(tc.TCIssueNo, '')+ISNULL(tc.Suffix,'') AS TCIssueNo,
        ISNULL(tc.TCIssueDate, '1900-01-01') AS TCIssueDate,
        ISNULL(tc.MainRemarks, '') AS Remarks,

        CASE
            WHEN ISNULL(tc.TCIssueTally, 0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END AS TCIssueStatus,

        -- Item Details
        ISNULL(i.ItemCode, '') AS ItemCode,
        ISNULL(i.ItemName, '') AS ItemName,
        ISNULL(i.Specification, '') AS ItemSpecification,
        ISNULL(i.MeasureUnit, '') AS MeasureUnit,
        ISNULL(i.HSNCode, '') AS HSNCode,
        ISNULL(tcs.QtyOut, 0) AS QtyOut,
        ISNULL(tcs.BalQty, 0) AS BalQty,
        ISNULL(tcs.UnitPrice, 0) AS UnitPrice,
        ISNULL(tcs.Remark, '') AS ItemRemarks

    FROM ToolCribIssue tc

        INNER JOIN ToolCribIssueSub tcs
            ON tc.TCIssueId = tcs.TCIssueId

        INNER JOIN Item i
            ON tcs.ItemId = i.ItemId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
		OR @Status = 'All'
        OR
        (
            @Status = 'Completed'
            AND ISNULL(tc.TCIssueTally, 0) = 1
        )
        OR
        (
            @Status = 'Pending'
            AND ISNULL(tc.TCIssueTally, 0) = 0
        )
    )

    ORDER BY tc.TCIssueNo, tcs.TCIssueSubId;

END
