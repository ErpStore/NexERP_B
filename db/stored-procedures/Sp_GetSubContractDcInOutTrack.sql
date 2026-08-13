CREATE OR ALTER        PROCEDURE [dbo].[Sp_GetSubContractDcInOutTrack]
(
    @FromDate DATETIME = NULL,
    @ToDate   DATETIME = NULL,
    @SupplyId   INT = 0,
    @ItemId   INT = 0
)
AS
BEGIN

    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY dcout.DcId DESC, dcouts.DcSubId DESC) AS SlNo,

        -------------------------------------------------
        -- CUSTOMER
        -------------------------------------------------
        ISNULL(c.VendorName, '') AS Supplier,

        -------------------------------------------------
        -- GRN DETAILS
        -------------------------------------------------
        ISNULL(CAST(dcout.DcNo AS VARCHAR(50)), '') 
            + ISNULL(dcout.Suffix, '') AS DcNo,

        ISNULL(CONVERT(VARCHAR(10), dcout.DcDate, 103), '') AS DcDate,
		ISNULL(dcouts.TransType, '') AS TransType,
        -------------------------------------------------
        -- ITEM DETAILS
        -------------------------------------------------

        ISNULL(i.ItemCode, '') AS ItemCodeOut,
        ISNULL(i.ItemName, '') AS ItemName,
        ISNULL(i.MeasureUnit, '') AS MeasureUnit,
        ISNULL(i.HSNCode, '') AS HSNCode,

        -------------------------------------------------
        -- INWARD DETAILS
        -------------------------------------------------
        ISNULL(dcouts.Qty, 0) AS QtyOut,
        ISNULL(dcouts.BalQty, 0) AS BalQty,

        -------------------------------------------------
        -- SCN DETAILS
        -------------------------------------------------
        ISNULL(CAST(sc.SCNNo AS VARCHAR(50)), '') 
            + ISNULL(sc.Suffix, '') AS SCNNo,

        ISNULL(CONVERT(VARCHAR(10), sc.SCNDate, 103), '') AS SCNDate,

        ISNULL(scs.AccQty, 0) AS WOPQty,
        ISNULL(scs.RejQty, 0) AS RejQty,
        ISNULL(scs.RewQty, 0) AS RewQty,

        -------------------------------------------------
        -- PO DETAILS
        -------------------------------------------------
        ISNULL(po.PONo, '') AS RefPoNo,

        ISNULL(CONVERT(VARCHAR(10), po.PODate, 103), '') AS RefPoDate,

        -------------------------------------------------
        -- DC OUT DETAILS
        -------------------------------------------------
        ISNULL(CAST(grn.GRNNo AS VARCHAR(50)), '') 
            + ISNULL(grn.Suffix, '') AS GRNNo,

        ISNULL(CONVERT(VARCHAR(10), grn.GRNDate, 103), '') AS GRNDate,

		ISNULL(CAST(grn.PartyDC AS VARCHAR(50)), '') 
            + ISNULL(grn.Suffix, '') AS RefDcNo,

        ISNULL(CONVERT(VARCHAR(10), grn.PartyDCDate, 103), '') AS RefDcDate,

        ISNULL(dcouts.Qty, 0) AS QtyIn,

        -------------------------------------------------
        -- INVOICE DETAILS
        -------------------------------------------------
        ISNULL(CAST(inv.InvNo AS VARCHAR(50)), '') 
            + ISNULL(inv.Suffix, '') AS InvoiceNo,

        ISNULL(CONVERT(VARCHAR(10), inv.InvDate, 103), '') AS InvoiceDate,

        ISNULL(invs.Qty, 0) AS InvQty

    FROM SubConDcOut dcout

    INNER JOIN SubConDcOutSub dcouts
        ON dcout.DcId = dcouts.DcId

    INNER JOIN Vendor c
        ON dcout.VendorCode = c.VendorCode and c.IsitSubCon=1

    INNER JOIN Item i
        ON dcouts.ItemId = i.ItemId 

    -------------------------------------------------
    -- SCN
    -------------------------------------------------
    LEFT JOIN SubConGRNSub grns
        ON grns.RefDcSubId = dcouts.DcSubId

    LEFT JOIN SubConGRN grn
        ON grns.GRNId = grn.GRNId

    -------------------------------------------------
    -- PO
    -------------------------------------------------
    LEFT JOIN PurchPoSub pos
        ON dcouts.RefPoSubId = pos.PoSubId

    LEFT JOIN PurchPo po
        ON pos.PoId = po.PoId

    -------------------------------------------------
    -- DC OUT
    -------------------------------------------------
    LEFT JOIN SubConSCNSub scs
        ON scs.RefGRNSubId = grns.GRNSubId

    LEFT JOIN SubConSCN sc
        ON scs.SCNId = sc.SCNId

    -------------------------------------------------
    -- INVOICE
    -------------------------------------------------
    LEFT JOIN SubConInvSub invs
        ON invs.RefSCNSubId = scs.SCNSubId

    LEFT JOIN SubConInv inv
        ON invs.InvId = inv.InvId

    WHERE

        -------------------------------------------------
        -- ONLY OUT ITEMS
        -------------------------------------------------
       

        -------------------------------------------------
        -- DATE FILTER
        -------------------------------------------------
        
        (
            @FromDate IS NULL
            OR CAST(dcout.DcDate AS DATE) >= CAST(@FromDate AS DATE)
        )

        AND
        (
            @ToDate IS NULL
            OR CAST(dcout.DcDate AS DATE) <= CAST(@ToDate AS DATE)
        )

        -------------------------------------------------
        -- CUSTOMER FILTER
        -------------------------------------------------
        AND
        (
            @SupplyId = 0
            OR dcout.VendorCode = @SupplyId
        )

        -------------------------------------------------
        -- ITEM FILTER
        -------------------------------------------------
        AND
        (
            @ItemId = 0
            OR dcouts.ItemId = @ItemId and  dcouts.TransType = 'In'
        )

    ORDER BY dcout.DcId DESC,
             dcouts.DcSubId DESC;

END
