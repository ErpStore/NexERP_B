CREATE OR ALTER          PROCEDURE [dbo].[Sp_Labour_Track]
    @CustId INT = NULL,
    @DocType VARCHAR(50) = NULL,
    @DocNo VARCHAR(50) = NULL,
    @ItemId INT = NULL,
    @FromDate DATE = NULL,      -- ✅ ADDED
    @ToDate DATE = NULL         -- ✅ ADDED
AS
BEGIN
    SET NOCOUNT ON;

    ------------------------------------------------------------
    -- 1️⃣ PO BASED FLOW
    ------------------------------------------------------------
    SELECT DISTINCT
        c.CustId,
        c.CustName,

        item.ItemId,
        item.ItemCode,
        item.ItemName,
        item.CategoryCode,

        mp.PoId,
        mp.PONo,
        mp.PODate,
        mps.PoSubId,
        mps.Qty AS PoQty,
        mps.BalQty AS PoBalQty,
        CASE 
            WHEN mp.PoTally = 1 AND mp.PoCancl = 0 AND mp.ShortClose = 0 THEN 'Completed'
            WHEN mp.PoCancl = 1 THEN 'Cancelled'
            WHEN mp.ShortClose = 1 THEN 'Short Closed'
            ELSE 'Pending'
        END AS POStatus,

        lg.GRNId,
        lg.GRNNo,
        lg.RefDcNo,
        lg.GRNDate,
        lgs.GRNSubId,
        lgs.Qty AS GrnQty,
        lgs.BalQty AS GrnBqty,
        lgs.TransType,
        CASE 
            WHEN lg.DcTally = 1 AND lg.DcCancel = 0 AND lg.ShortClose = 0 THEN 'Completed'
            WHEN lg.DcCancel = 1 THEN 'Cancelled'
            WHEN lg.ShortClose = 1 THEN 'Short Closed'
            ELSE 'Pending'
        END AS GRNStatus,

        ls.SCNId,
        ls.SCNNo,
        ls.SCNDate,
        lss.SCNSubId,
        lss.AcceptQty AS ScnQty,
        lss.BalQty AS ScnBqty,
        CASE 
            WHEN ls.SCNTally = 1 AND ls.SCNCancel = 0 AND ls.ShortClose = 0 THEN 'Completed'
            WHEN ls.SCNCancel = 1 THEN 'Cancelled'
            WHEN ls.ShortClose = 1 THEN 'Short Closed'
            ELSE 'Pending'
        END AS SCNStatus,

        ldc.DcId,
        ldc.DcNo,
        ldc.DcDate,
        COALESCE(ldcs.DcSubId, ldcsp.DcSubId) AS DcSubId,
        COALESCE(ldcs.Qty, ldcsp.Qty) AS DcQty,
        COALESCE(ldcs.BalQty, ldcsp.BalQty) AS DCBQty,
        CASE 
            WHEN ldc.DcTally = 1 AND ldc.DcCancel = 0 AND ldc.ShortClose = 0 THEN 'Completed'
            WHEN ldc.DcCancel = 1 THEN 'Cancelled'
            WHEN ldc.ShortClose = 1 THEN 'Short Closed'
            ELSE 'Pending'
        END AS DCStatus,

        li.LabInvId AS InvId,
        li.LabInvNo AS InvNo,
        li.LabInvDate AS InvDate,
        linv.LabInvSubId,
        linv.Qty AS InvQty,
        linv.BalQty AS InvBQty,
        li.GrandTotal,
        CASE 
            WHEN li.LabInvTally = 1 AND li.InvCancel = 0 AND li.ShortClose = 0 THEN 'Completed'
            WHEN li.InvCancel = 1 THEN 'Cancelled'
            WHEN li.ShortClose = 1 THEN 'Short Closed'
            ELSE 'Pending'
        END AS InvStatus

    FROM MfgPo mp
    INNER JOIN MfgPoSub mps ON mp.PoId = mps.PoId
    LEFT JOIN LabourGRNSub lgs ON lgs.RefPoSubId = mps.PoSubId
    LEFT JOIN LabourGRN lg ON lg.GRNId = lgs.GRNId
    LEFT JOIN LabourSCNSub lss ON lss.RefGRNSubId = lgs.GRNSubId
    LEFT JOIN LabourSCN ls ON ls.SCNId = lss.SCNId
       LEFT JOIN LabourDcOutgoingSub ldcs 
    ON ldcs.RefGRNSubId = lgs.GRNSubId

LEFT JOIN LabourDcOutgoingSub ldcsp 
    ON ldcsp.RefPoSubId = lgs.RefPoSubId   -- 🔥 FIXED (not lgs.RefPoSubId)

LEFT JOIN LabourDcOutgoing ldc 
    ON ldc.DcId = COALESCE(ldcs.DcId, ldcsp.DcId)
   
    LEFT JOIN LabInvSub linv ON linv.RefDcSubId = ldcs.DcSubId
    LEFT JOIN LabInv li ON li.LabInvId = linv.LabInvId
    LEFT JOIN Customer c ON c.CustId = mp.CustId
    LEFT JOIN Item item ON item.ItemId = mps.ItemId

    WHERE 
        (@CustId IS NULL OR c.CustId = @CustId)
        AND (@ItemId IS NULL OR item.ItemId = @ItemId)
        AND mp.MfgORLab = 0

        -- ✅ DATE FILTER ADDED
        AND (@FromDate IS NULL OR li.LabInvDate >= @FromDate)
        AND (@ToDate IS NULL OR li.LabInvDate <= @ToDate)

    UNION ALL

    ------------------------------------------------------------
    -- 2️⃣ GRN WITHOUT PO
    ------------------------------------------------------------
    SELECT DISTINCT
        c.CustId,
        c.CustName,

        item.ItemId,
        item.ItemCode,
        item.ItemName,
        item.CategoryCode,

        NULL, NULL, NULL, NULL, NULL, NULL, NULL,

        lg.GRNId,
        lg.GRNNo,
        lg.RefDcNo,
        lg.GRNDate,
        lgs.GRNSubId,
        lgs.Qty,
        lgs.BalQty,
        lgs.TransType,
        CASE 
            WHEN lg.DcTally = 1 AND lg.DcCancel = 0 AND lg.ShortClose = 0 THEN 'Completed'
            WHEN lg.DcCancel = 1 THEN 'Cancelled'
            WHEN lg.ShortClose = 1 THEN 'Short Closed'
            ELSE 'Pending'
        END,

        ls.SCNId,
        ls.SCNNo,
        ls.SCNDate,
        lss.SCNSubId,
        lss.AcceptQty,
        lss.BalQty,
        CASE 
            WHEN ls.SCNTally = 1 AND ls.SCNCancel = 0 AND ls.ShortClose = 0 THEN 'Completed'
            WHEN ls.SCNCancel = 1 THEN 'Cancelled'
            WHEN ls.ShortClose = 1 THEN 'Short Closed'
            ELSE 'Pending'
        END,

        ldc.DcId,
        ldc.DcNo,
        ldc.DcDate,
      COALESCE(ldcs.DcSubId, ldcsp.DcSubId) AS DcSubId,
      COALESCE(ldcs.Qty, ldcsp.Qty) AS DcQty,
      COALESCE(ldcs.BalQty, ldcsp.BalQty) AS DCBQty,
        CASE 
            WHEN ldc.DcTally = 1 AND ldc.DcCancel = 0 AND ldc.ShortClose = 0 THEN 'Completed'
            WHEN ldc.DcCancel = 1 THEN 'Cancelled'
            WHEN ldc.ShortClose = 1 THEN 'Short Closed'
            ELSE 'Pending'
        END,

        li.LabInvId,
        li.LabInvNo,
        li.LabInvDate,
        linv.LabInvSubId,
        linv.Qty,
        linv.BalQty,
        li.GrandTotal,
        CASE 
            WHEN li.LabInvTally = 1 AND li.InvCancel = 0 AND li.ShortClose = 0 THEN 'Completed'
            WHEN li.InvCancel = 1 THEN 'Cancelled'
            WHEN li.ShortClose = 1 THEN 'Short Closed'
            ELSE 'Pending'
        END

    FROM LabourGRN lg
    INNER JOIN LabourGRNSub lgs ON lg.GRNId = lgs.GRNId
    LEFT JOIN LabourSCNSub lss ON lss.RefGRNSubId = lgs.GRNSubId
    LEFT JOIN LabourSCN ls ON ls.SCNId = lss.SCNId
     LEFT JOIN LabourDcOutgoingSub ldcs 
    ON ldcs.RefGRNSubId = lgs.GRNSubId

LEFT JOIN LabourDcOutgoingSub ldcsp 
    ON ldcsp.RefPoSubId = lgs.RefPoSubId   -- 🔥 FIXED (not lgs.RefPoSubId)

LEFT JOIN LabourDcOutgoing ldc 
    ON ldc.DcId = COALESCE(ldcs.DcId, ldcsp.DcId)

	left join LabourDcOutgoing ldo on ldo.DcId =ldcsp.DcId
    LEFT JOIN LabInvSub linv ON linv.RefDcSubId = ldcs.DcSubId
    LEFT JOIN LabInv li ON li.LabInvId = linv.LabInvId
    LEFT JOIN Customer c ON c.CustId = lg.CustId
    LEFT JOIN Item item ON item.ItemId = lgs.ItemId

    WHERE 
        (@CustId IS NULL OR c.CustId = @CustId)
        AND (@ItemId IS NULL OR item.ItemId = @ItemId)
        AND lgs.RefPoSubId IS NULL

        -- ✅ DATE FILTER ADDED
        AND (@FromDate IS NULL OR li.LabInvDate >= @FromDate)
        AND (@ToDate IS NULL OR li.LabInvDate <= @ToDate)

END
