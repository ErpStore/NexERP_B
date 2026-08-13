
CREATE OR ALTER     PROCEDURE [dbo].[Sp_GetLabourSCNStatusList]
(
    @Status NVARCHAR(50)=NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER
        (
            ORDER BY s.SCNId DESC, ss.SCNSubId DESC
        ) AS SlNo,

        ISNULL(c.CustName,'') AS Customer,

        ISNULL(CAST(s.SCNNo AS VARCHAR(20)) + s.Suffix,'') AS SCNNo,
        ISNULL(CONVERT(VARCHAR(20),s.SCNDate,103),'') AS SCNDate,
		
		ISNULL(CAST(g.GRNNo AS VARCHAR(20)) + s.Suffix,'') AS RefGRNNo,
        ISNULL(CONVERT(VARCHAR(20),g.GRNDate,103),'') AS RefGRNDate,


        ISNULL(st.StoreName,'') AS AddToStore,
		ISNULL(Isu.StoreName,'') AS IssueFromStore,

        ISNULL(s.MainRemarks,'') AS MainRemarks,

        CASE
            WHEN ISNULL(s.SCNTally,0)=1 THEN 'Completed'
            WHEN ISNULL(s.SCNCancel,0)=1 THEN 'Cancelled'
            WHEN ISNULL(s.ShortClose,0)=1 THEN 'Short Closed'
            ELSE 'Pending'
        END AS SCNStatus,

        ISNULL(i.ItemCode,'') AS ItemCode,
        ISNULL(i.ItemName,'') AS ItemName,
        ISNULL(i.Specification,'') AS ItemSpecification,
        ISNULL(i.MeasureUnit,'') AS MeasureUnit,
        ISNULL(i.HSNCode,'') AS HSNCode,

        ISNULL(ss.AcceptQty,0) AS AcceptQty,
        ISNULL(ss.BalQty,0) AS BalQty,
        ISNULL(ss.RejectQty,0) AS RejectQty,
        ISNULL(ss.ReworkQty,0) AS ReworkQty,
        ISNULL(ss.SCNBalQty,0) AS SCNBalQty,

        ISNULL(ss.UnitPrice,0) AS UnitPrice,

        ISNULL(ss.Remark,'') AS ItemRemarks,

        CAST(ISNULL(ss.ItemCancel,0) AS BIT) AS ItemCancel,

        ISNULL(s.CreatedBy,'') AS CreatedBy,
		ISNULL(CONVERT(VARCHAR(20), s.CreatedDate, 103), '') AS CreatedDate,
		ISNULL(s.ModifiedBy,'') AS ModifiedBy,
		ISNULL(CONVERT(VARCHAR(20), s.ModifiedDate, 103), '') AS ModifiedDate

    FROM LabourSCN s

    INNER JOIN LabourSCNSub ss
        ON s.SCNId = ss.SCNId

    INNER JOIN Customer c
        ON s.CustId = c.CustId

    INNER JOIN Stores st
        ON s.AddStoreId = st.StoreId

    INNER JOIN Stores Isu
        ON s.StoreIssId = Isu.StoreId

	INNER JOIN LabourGRNSub gs
	on gs.GRNSubId=ss.RefGRNSubId

	INNER JOIN LabourGRN g
	on g.GRNId=gs.GRNId

    LEFT JOIN Item i
        ON ss.ItemId = i.ItemId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
        OR
        CASE
            WHEN ISNULL(s.SCNTally,0)=1 THEN 'Completed'
            WHEN ISNULL(s.SCNCancel,0)=1 THEN 'Cancelled'
            WHEN ISNULL(s.ShortClose,0)=1 THEN 'Short Closed'
            ELSE 'Pending'
        END = @Status
    )

    ORDER BY s.SCNId DESC, ss.SCNSubId DESC;

END



