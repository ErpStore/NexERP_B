
CREATE OR ALTER   PROCEDURE [dbo].[Sp_GetProductionSCNAssyStatusList]
(
    @Status NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY pscn.SCNId DESC, pscns.SCNSubId DESC) AS SlNo,

        ISNULL(pscn.SCNNo, '') + ISNULL(pscn.Suffix, '') AS SCNNo,
        ISNULL(CONVERT(VARCHAR(20), pscn.SCNDate, 103), '') AS SCNDate,

        ISNULL(sAdd.StoreName, '') AS AddStore,
        ISNULL(sIss.StoreName, '') AS IssueStore,

        ISNULL(pscn.MainRemark, '') AS MainRemark,

        -- No explicit status column → default pattern
        'Completed' AS SCNStatus,

        ISNULL(i.ItemCode,'') AS ItemCode,
        ISNULL(i.ItemName,'') AS ItemName,
        ISNULL(i.Specification,'') AS ItemSpecification,
        ISNULL(i.MeasureUnit,'') AS MeasureUnit,
        ISNULL(i.HSNCode,'') AS HSNCode,

        ISNULL(pscns.AccQty,0) AS AcceptQty,
        ISNULL(pscns.RejQty,0) AS RejectQty,
        ISNULL(pscns.RejReason,'') AS RejectReason,

        ISNULL(pscns.RewQty,0) AS ReworkQty,
        ISNULL(pscns.RewReason,'') AS ReworkReason,

        ISNULL(pscns.BalQty,0) AS BalQty,
        ISNULL(pscns.UnitPrice,0) AS UnitPrice,

        ISNULL(pscns.Remark,'') AS ItemRemarks,

        ISNULL(pscn.CreatedBy,'') AS CreatedBy,
        ISNULL(CONVERT(VARCHAR(20), pscn.CreatedDate, 103), '') AS CreatedDate,
        ISNULL(pscn.ModifiedBy,'') AS ModifiedBy,
        ISNULL(CONVERT(VARCHAR(20), pscn.ModifiedDate, 103), '') AS ModifiedDate

    FROM ProductionSCNAssy pscn

    INNER JOIN ProductionSCNAssySub pscns
        ON pscn.SCNId = pscns.SCNId

    LEFT JOIN Item i
        ON pscns.ItemId = i.ItemId

    LEFT JOIN Stores sAdd
        ON pscn.AddStoreId = sAdd.StoreId

    LEFT JOIN Stores sIss
        ON pscn.IssueStoreId = sIss.StoreId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
    )

    ORDER BY pscn.SCNId DESC, pscns.SCNSubId DESC;

END
