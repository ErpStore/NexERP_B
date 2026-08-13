CREATE OR ALTER  PROCEDURE [dbo].[Sp_GetToolCribReturnNoteStatusList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY trs.TCReturnSubId) AS SlNo,

        -- Header Details
        ISNULL(tr.TCReturnNo, '')+ISNULL(tr.Suffix,'') AS TCReturnNo,
        ISNULL(tr.TCReturnDate, '1900-01-01') AS TCReturnDate,
        ISNULL(tr.MainRemarks, '') AS Remarks,

        CASE
            WHEN ISNULL(ti.TCIssueTally, 0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END AS TCReturnStatus,

        -- Item Details
        ISNULL(i.ItemCode, '') AS ItemCode,
        ISNULL(i.ItemName, '') AS ItemName,
        ISNULL(i.Specification, '') AS ItemSpecification,
        ISNULL(i.MeasureUnit, '') AS MeasureUnit,
        ISNULL(i.HSNCode, '') AS HSNCode,
        ISNULL(trs.AccpQty, 0) AS QtyOut,
        ISNULL(trs.RejQty, 0) AS RejectQty,
		ISNULL(trs.RejRemark, 0) AS RejectRemark,
        ISNULL(trs.RewQty, 0) AS ReworkQty,
		ISNULL(trs.RewRemark, 0) AS ReworkRemark,
        ISNULL(trs.Remark, '') AS ItemRemarks

    FROM ToolCribReturns tr

        INNER JOIN ToolCribReturnSubs trs
            ON tr.TCReturnId = trs.TCReturnId

        LEFT JOIN ToolCribIssueSub tis
            ON trs.RefTCIssueSubId = tis.TCIssueId
        LEFT JOIN ToolCribIssue ti
            ON tis.TCIssueId = ti.TCIssueId

        INNER JOIN Item i
            ON trs.ItemId = i.ItemId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
		OR @Status = 'All'
        OR
        (
            @Status = 'Completed'
            AND ISNULL(ti.TCIssueTally, 0) = 1
        )
        OR
        (
            @Status = 'Pending'
            AND ISNULL(ti.TCIssueTally, 0) = 0
        )
    )

    ORDER BY tr.TCReturnNo, trs.TCReturnSubId;

END
