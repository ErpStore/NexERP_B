CREATE OR ALTER       PROCEDURE [dbo].[Sp_GetLabourDcInOutTrack]
(
    @FromDate DATETIME = NULL,
    @ToDate   DATETIME = NULL,
    @CustId   INT = 0,
    @ItemId   INT = 0
)
AS
BEGIN

    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY g.GRNId DESC, gs.GRNSubId DESC) AS SlNo,

        -------------------------------------------------
        -- CUSTOMER
        -------------------------------------------------
        ISNULL(c.CustName, '') AS Customer,

        -------------------------------------------------
        -- GRN DETAILS
        -------------------------------------------------
        ISNULL(CAST(g.GRNNo AS VARCHAR(50)), '') 
            + ISNULL(g.Suffix, '') AS GRNNo,

        ISNULL(CONVERT(VARCHAR(10), g.GRNDate, 103), '') AS GRNDate,

        -------------------------------------------------
        -- REF DC IN
        -------------------------------------------------
        ISNULL(CAST(g.RefDcNo AS VARCHAR(50)), '') 
            + ISNULL(g.Suffix, '') AS RefDcNo,

        ISNULL(CONVERT(VARCHAR(10), g.RefDcDate, 103), '') AS RefDcDate,
		ISNULL(g.BatchNo, '') AS BatchNo,
		ISNULL(gs.TransType, '') AS TransType,
        -------------------------------------------------
        -- ITEM DETAILS TransType
        -------------------------------------------------
        ISNULL(i.ItemCode, '') AS ItemCode_In,
        ISNULL(i.ItemName, '') AS ItemName,
        ISNULL(i.MeasureUnit, '') AS MeasureUnit,
        ISNULL(i.HSNCode, '') AS HSNCode,

        -------------------------------------------------
        -- INWARD DETAILS
        -------------------------------------------------
        ISNULL(gs.Qty, 0) AS ReceivedQty,
        ISNULL(gs.BalQty, 0) AS BalQty,

        -------------------------------------------------
        -- SCN DETAILS
        -------------------------------------------------
        ISNULL(CAST(scn.SCNNo AS VARCHAR(50)), '') 
            + ISNULL(scn.Suffix, '') AS SCNNo,

        ISNULL(CONVERT(VARCHAR(10), scn.SCNDate, 103), '') AS SCNDate,

        ISNULL(scns.BalQty, 0) AS WOPQty,
        ISNULL(scns.RejectQty, 0) AS RejQty,
        ISNULL(scns.ReworkQty, 0) AS RewQty,

        -------------------------------------------------
        -- PO DETAILS
        -------------------------------------------------
        ISNULL(po.PONo, '') AS PONo,

        ISNULL(CONVERT(VARCHAR(10), po.PODate, 103), '') AS PODate,
		ISNULL(pos.[LineNo],0) as [LineNo],
        -------------------------------------------------
        -- DC OUT DETAILS
        -------------------------------------------------
        ISNULL(CAST(dcout.DCNo AS VARCHAR(50)), '') 
            + ISNULL(dcout.Suffix, '') AS DcNoOut,

        ISNULL(CONVERT(VARCHAR(10), dcout.DCDate, 103), '') AS DcDate,

        ISNULL(dcouts.Qty, 0) AS Qty_Out,

		  -- ITEM DETAILS
        -------------------------------------------------


        -------------------------------------------------
        -- INVOICE DETAILS
        -------------------------------------------------
        ISNULL(CAST(inv.LabInvNo AS VARCHAR(50)), '') 
            + ISNULL(inv.Suffix, '') AS InvoiceNo,

        ISNULL(CONVERT(VARCHAR(10), inv.LabInvDate, 103), '') AS InvDate,

        ISNULL(invs.Qty, 0) AS Qty,p.ProcessName as Process,

		DATEDIFF(DAY, g.RefDcDate, inv.LabInvDate) AS Duedays

    FROM LabourGRN g

    INNER JOIN LabourGRNSub gs
        ON g.GRNId = gs.GRNId

    INNER JOIN Customer c
        ON g.CustId = c.CustId

    INNER JOIN Item i
        ON gs.ItemId = i.ItemId

    -------------------------------------------------
    -- SCN
    -------------------------------------------------
    LEFT JOIN LabourSCNSub scns
        ON scns.RefGRNSubId = gs.GRNSubId

    LEFT JOIN LabourSCN scn
        ON scns.SCNId = scn.SCNId

    -------------------------------------------------
    -- PO
    -------------------------------------------------
    LEFT JOIN MfgPoSub pos
        ON gs.RefPoSubId = pos.PoSubId

    LEFT JOIN MfgPo po
        ON pos.PoId = po.PoId

    -------------------------------------------------
    -- DC OUT
    -------------------------------------------------
    LEFT JOIN LabourDcOutgoingSub dcouts
        ON dcouts.RefGRNSubId = gs.GRNSubId

   AND EXISTS
   (
       SELECT 1
       FROM LabourDcOutgoing d
       WHERE d.DcId = dcouts.DcId
         AND ISNULL(d.DcCancel,0) = 0
   )

    LEFT JOIN LabourDcOutgoing dcout
        ON dcouts.DcId = dcout.DcId



    -------------------------------------------------
    -- INVOICE
    -------------------------------------------------
    LEFT JOIN LabInvSub invs
        ON invs.RefDcSubId = dcouts.DcSubId

    LEFT JOIN LabInv inv
        ON invs.LabInvId = inv.LabInvId

	LEFT JOIN Process p on p.ProcessId=inv.ProcessId

    WHERE

        -------------------------------------------------
        -- ONLY OUT ITEMS
        -------------------------------------------------
        

        -------------------------------------------------
        -- DATE FILTER
        -------------------------------------------------
        
        (
            @FromDate IS NULL
            OR CAST(g.GRNDate AS DATE) >= CAST(@FromDate AS DATE)
        )

        AND
        (
            @ToDate IS NULL
            OR CAST(g.GRNDate AS DATE) <= CAST(@ToDate AS DATE)
        )

        -------------------------------------------------
        -- CUSTOMER FILTER
        -------------------------------------------------
        AND
        (
            @CustId = 0
            OR g.CustId = @CustId
        )

        -------------------------------------------------
        -- ITEM FILTER
        -------------------------------------------------
        AND
        (
            @ItemId = 0
            OR gs.ItemId = @ItemId and gs.TransType = 'Out' AND g.DcCancel=0 
        )

    ORDER BY g.GRNId DESC,
             gs.GRNSubId DESC;
END
