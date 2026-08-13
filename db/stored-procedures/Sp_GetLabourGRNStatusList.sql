
CREATE OR ALTER     PROCEDURE [dbo].[Sp_GetLabourGRNStatusList]
(
    @Status NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER
        (
            ORDER BY g.GRNId DESC, gsout.GRNSubId DESC
        ) AS SlNo,

        ISNULL(g.GRNNo + g.Suffix, '') AS GRNNo,
        ISNULL(CONVERT(VARCHAR(20), g.GRNDate, 103), '') AS GRNDate,

        ISNULL(g.RefDcNo, '') AS RefDcNo,
        ISNULL(CONVERT(VARCHAR(20), g.RefDcDate, 103), '') AS RefDcDate,

        ISNULL(c.CustName, '') AS Customer,
        ISNULL(st.StoreName, '') AS StoreName,
        ISNULL(g.Remarks, '') AS Remarks,

        CASE
            WHEN ISNULL(g.DcTally,0)=1 THEN 'Completed'
            WHEN ISNULL(g.DcCancel,0)=1 THEN 'Cancelled'
            WHEN ISNULL(g.ShortClose,0)=1 THEN 'Short Closed'
            ELSE 'Pending'
        END AS GRNStatus,

        -- OUT
        ISNULL(iout.ItemCode,'') AS ItemCode_Out,
        ISNULL(iout.ItemName,'') AS ItemName_Out,
        ISNULL(iout.Specification,'') AS ItemSpecification_Out,
        ISNULL(iout.MeasureUnit,'') AS MeasureUnit_out,
        ISNULL(iout.HSNCode,'') AS HSNCode_Out,
        ISNULL(gsout.Qty,0) AS Qty_Out,
        ISNULL(gsout.BalQty,0) AS BalQty_Out,
        ISNULL(gsout.UnitPrice,0) AS UnitPrice_out,

        -- IN
        ISNULL(iin.ItemCode,'') AS ItemCode_In,
        ISNULL(iin.ItemName,'') AS ItemName_In,
        ISNULL(iin.Specification,'') AS ItemSpecification_In,
        ISNULL(iin.MeasureUnit,'') AS MeasureUnit_In,
        ISNULL(iin.HSNCode,'') AS HSNCode_In,
        ISNULL(gsin.Qty,0) AS ReceivedQty,
        ISNULL(gsin.BalQty,0) AS ReceivedBalQty,
        ISNULL(gsin.UnitPrice,0) AS UnitPrice,

        -- Extra
        ISNULL(gsout.RowRemarks,'') AS ItemRemarks,
        ISNULL(p.PONo + p.Suffix,'') AS RefPoNo,
        ISNULL(CONVERT(VARCHAR(20), p.PODate, 103), '') AS RefPoDt,
        ISNULL(ps.[LineNo],0) AS [LineNo],
        CAST(ISNULL(gsout.ItemCancel,0) AS BIT) AS ItemCancel,
		ISNULL(g.CreatedBy,'') AS CreatedBy,
		ISNULL(CONVERT(VARCHAR(20), g.CreatedDate, 103), '') AS CreatedDate,
		ISNULL(g.ModifiedBy,'') AS ModifiedBy,
		ISNULL(CONVERT(VARCHAR(20), g.ModifiedDate, 103), '') AS ModifiedDate

    FROM LabourGRN g
    INNER JOIN Customer c ON g.CustId = c.CustId
    INNER JOIN Stores st ON g.AddStoreId = st.StoreId

    LEFT JOIN LabourGRNSub gsout
        ON g.GRNId = gsout.GRNId
       AND gsout.TransType = 'Out'

    LEFT JOIN LabourGRNSub gsin
        ON g.GRNId = gsin.GRNId
       AND gsin.TransType = 'In'
       AND gsin.GRNSubId = gsout.GRNSubId + 1

    LEFT JOIN Item iout ON gsout.ItemId = iout.ItemId
    LEFT JOIN Item iin ON gsin.ItemId = iin.ItemId

    LEFT JOIN MfgPoSub ps ON ps.PoSubId = gsout.RefPoSubId
    LEFT JOIN MfgPo p ON p.PoId = ps.PoId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
        OR
        CASE
            WHEN ISNULL(g.DcTally,0)=1 THEN 'Completed'
            WHEN ISNULL(g.DcCancel,0)=1 THEN 'Cancelled'
            WHEN ISNULL(g.ShortClose,0)=1 THEN 'Short Closed'
            ELSE 'Pending'
        END = @Status
    )

    ORDER BY g.GRNId DESC, gsout.GRNSubId DESC;

END



