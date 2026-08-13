
CREATE OR ALTER     PROCEDURE [dbo].[Sp_GetLabourDcOutgoingStatusList]
(
    @Status NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER
        (
            ORDER BY dco.DcId DESC, dcos.DcSubId DESC
        ) AS SlNo,

        ISNULL(dco.DcNo + dco.Suffix, '') AS DCNo,
        ISNULL(CONVERT(VARCHAR(20), dco.DcDate, 103), '') AS DCDate,

        ISNULL(g.GRNNo + g.Suffix, '') AS RefGRNNo,
        ISNULL(CONVERT(VARCHAR(20), g.GRNDate, 103), '') AS RefGRNDate,

        ISNULL(p.PONo, '') AS RefPoNo,
        ISNULL(CONVERT(VARCHAR(20), p.PODate, 103), '') AS RefPoDate,

        ISNULL(c.CustName, '') AS Customer,
        ISNULL(s.StoreName, '') AS IssueFromStore,

        ISNULL(dco.TransFrom, '') AS TransferFrom,
        ISNULL(dco.TransTo, '') AS TransferTo,

        ISNULL(dco.VehicleNo, '') AS VehicleNo,

       ISNULL(
			CASE dco.TransportMode
				WHEN 0 THEN '--Select SupplyType--'
				WHEN 1 THEN 'Supply'
				WHEN 4 THEN 'JobWork'
				WHEN 5 THEN 'ForOwnUse'
				WHEN 8 THEN 'Others'
				ELSE ''
			END, ''
		) AS TransportMode,

        ISNULL(dco.Remarks, '') AS MainRemarks,

        CASE
            WHEN ISNULL(dco.DcCancel,0) = 1 THEN 'Cancelled'
            WHEN ISNULL(dco.ShortClose,0) = 1 THEN 'Short Closed'
            WHEN ISNULL(dco.DcTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END AS DCStatus,

        ISNULL(i.ItemCode,'') AS ItemCode,
        ISNULL(i.ItemName,'') AS ItemName,
        ISNULL(i.Specification,'') AS ItemSpecification,
        ISNULL(i.MeasureUnit,'') AS MeasureUnit,
        ISNULL(i.HSNCode,'') AS HSNCode,

        ISNULL(dcos.Qty,0) AS Qty,
        ISNULL(dcos.BalQty,0) AS BalQty,
        ISNULL(dcos.RejectQty,0) AS RejectQty,
        ISNULL(dcos.ReworkQty,0) AS ReworkQty,
        ISNULL(dcos.UnitPrice,0) AS UnitPrice,

        ISNULL(dcos.DcRowRem,'') AS ItemRemarks,

        CAST(ISNULL(dcos.ItemCancel,0) AS BIT) AS ItemCancel,

        ISNULL(dco.CreatedBy,'') AS CreatedBy,
        ISNULL(CONVERT(VARCHAR(20), dco.CreatedDate, 103), '') AS CreatedDate,
        ISNULL(dco.ModifiedBy,'') AS ModifiedBy,
        ISNULL(CONVERT(VARCHAR(20), dco.ModifiedDate, 103), '') AS ModifiedDate

    FROM LabourDcOutgoing dco

    INNER JOIN Customer c
        ON dco.CustId = c.CustId

    INNER JOIN LabourDcOutgoingSub dcos
        ON dco.DcId = dcos.DcId
       AND dcos.TransType = 'Out'

    INNER JOIN Item i
        ON dcos.ItemId = i.ItemId

    INNER JOIN Stores s
        ON dco.StoreIssId = s.StoreId

    LEFT JOIN LabourGRNSub gs
        ON gs.GRNSubId = dcos.RefGRNSubId

    LEFT JOIN LabourGRN g
        ON g.GRNId = gs.GRNId

    LEFT JOIN MfgPoSub ps
        ON ps.PoSubId = dcos.RefPoSubId

    LEFT JOIN MfgPo p
        ON p.PoId = ps.PoId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
        OR
        CASE
            WHEN ISNULL(dco.DcCancel,0) = 1 THEN 'Cancelled'
            WHEN ISNULL(dco.ShortClose,0) = 1 THEN 'Short Closed'
            WHEN ISNULL(dco.DcTally,0) = 1 THEN 'Completed'
            ELSE 'Pending'
        END = @Status
    )

    ORDER BY dco.DcId DESC, dcos.DcSubId DESC;

END



