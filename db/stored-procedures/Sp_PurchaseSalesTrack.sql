CREATE OR ALTER   PROCEDURE [dbo].[Sp_PurchaseSalesTrack]
(
    @FromDate Datetime,
    @ToDate   Datetime
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Query 1 : GRNs with PO
   SELECT 0  AS SlNo,
        V.VendorName,
		
        CONCAT(P.PONo,' ',P.Suffix) AS PONo,CAST (P.PODate AS DATE) AS PODate ,
       
        CONCAT(PGRN.GRNNo,' ',PGRN.Suffix) AS GRNNo,CAST(PGRN.GRNDate AS DATE) AS GRNDate,
		I.ItemCode AS ItemCode,
        I.ItemName AS ItemName,
		I.MeasureUnit[UOM],I.HSNCode,
		 PS.Qty AS POQty,
        PSCNS.AcceptQty [AccQty],
		 PSCNS.RejectQty [RejQty],
		CAST(Ps.UnitPrice AS DECIMAL(18,3)) AS Unit_Price,
		 CAST(ps.LineDiscountAmount AS DECIMAL(18,3)) AS Purch_Discount, 
        SA.BalQty,
        C.CustName,
        CONCAT(MPO.PONo,' ',MPO.Suffix) AS MfgPONo, CAST(MPO.PODate AS DATE)  AS MfgpoDate,
        CONCAT(M.DcNo,' ',M.Suffix) AS DcNo, CAST(M.DcDate AS DATE) AS DcDate, MDCS.Qty AS DCQty,
        CONCAT(MI.InvNo,' ',MI.Suffix) AS InvNo,CAST(MI.InvDate AS DATE) AS InvDate,
        MISub.Qty,
		CAST(MISub.UnitPrice AS DECIMAL(18,3)) as Sales_price,
		CAST(MISub.LineDiscountAmount AS DECIMAL(18,3)) [Discount],
	 -- Purchase Amount
   CAST((
        ISNULL(MISub.Qty, 0) * ISNULL(PS.UnitPrice, 0)
    ) - ISNULL((PS.LineDiscountAmount/PSCNS.AcceptQty)*MISub.Qty, 0) AS DECIMAL(18,3)) AS PurchaseAmount,

    -- Sales Amount
    CAST((
        ISNULL(MISub.Qty, 0) * ISNULL(MISub.UnitPrice, 0)
    ) - ISNULL(MISub.LineDiscountAmount, 0) AS DECIMAL(18,3)) AS SalesAmount,

    -- Profit
    CAST((
        (
            ISNULL(MISub.Qty, 0) * ISNULL(MISub.UnitPrice, 0)
        ) - ISNULL(MISub.LineDiscountAmount, 0)
    )
    -
    (
        (
            ISNULL(MISub.Qty, 0) * ISNULL(PS.UnitPrice, 0)
        ) - ISNULL((PS.LineDiscountAmount/PSCNS.AcceptQty)*MISub.Qty, 0)
    ) AS DECIMAL(18,3)) AS Profit,

        I.ItemId,
        C.CustId,
        V.VendorCode
    FROM PurchPO P
    LEFT JOIN PurchPOSub PS ON P.POId = PS.POId
    LEFT JOIN PurchaseGRNSub PGRNS ON PS.PoSubId = PGRNS.RefPoSubId
    LEFT JOIN PurchaseGRN PGRN ON PGRN.GRNId = PGRNS.GRNId
    LEFT JOIN PurchaseSCNSub PSCNS ON PSCNS.RefGRNSubId = PGRNS.GRNSubId
    LEFT JOIN PurchaseSCN PSCN ON PSCN.SCNId = PSCNS.SCNId
    LEFT JOIN Item I ON I.ItemId = PS.ItemId
    LEFT JOIN Vendor V ON V.VendorCode = P.VendorCode
    LEFT JOIN StockAdd SA
        ON SA.SubItemRefID = PSCNS.SCNSubId
       AND SA.ItemId = PSCNS.ItemId
       AND SA.ScreenCode = 78
    LEFT JOIN StockIssueTrack SIT ON SIT.AddId = SA.AddId
    LEFT JOIN StockIssue SI ON SIT.IssueId = SI.IssueId
    LEFT JOIN MfgDcSub MDCS
        ON SI.SubItemRefID = MDCS.DcSubId
       AND SI.ScreenCode = 42
    LEFT JOIN MfgDc M ON MDCS.DcId = M.DcId
    LEFT JOIN MfgInvSub MISub
        ON (MISub.InvSubId = SI.SubItemRefID AND SI.ScreenCode = 80)
        OR MISub.RefDcSubId = MDCS.DcSubId
    LEFT JOIN MfgInv MI ON MI.InvId = MISub.InvId
    LEFT JOIN MfgPoSub MPOSUB
        ON MPOSUB.PoSubId = MDCS.RefPoSubId
        OR MPOSUB.PoSubId = MISub.RefPoSubId
    LEFT JOIN MfgPo MPO ON MPO.PoId = MPOSUB.PoId
    LEFT JOIN Customer C
        ON MPO.CustId = C.CustId
        OR M.CustId = C.CustId
    WHERE  CAST(P.PODate AS DATE)
    BETWEEN CAST(@FromDate AS DATE)
        AND CAST(@ToDate AS DATE) and ((misub.ItemId is not null and misub.ItemId>0) or (mdcs.ItemId is not null and mdcs.ItemId>0))

    GROUP BY
        P.PONo, PS.ItemId, PGRN.GRNNo,
        MPO.PONo, M.DcNo, MI.InvNo,
        I.ItemCode, V.VendorName, P.Suffix, I.ItemName,
        PGRN.Suffix, PS.Qty, 
        MDCS.Qty,
        MISub.Qty,
         MPO.Suffix,
        M.Suffix, MI.Suffix,
        C.CustName, I.ItemId,
        C.CustId, V.VendorCode,P.PODate ,PGRN.GRNDate,  MPO.PODate, M.DcDate,MI.InvDate,
		 Ps.UnitPrice,
		 ps.LineDiscountAmount,
		 PSCNs.AcceptQty,PSCNS.RejectQty,MISub.LineDiscountAmount,I.MeasureUnit,I.HSNCode,SA.BalQty,MISub.UnitPrice

    UNION ALL

    -- Query 2 : GRNs without PO
     SELECT
       0  AS SlNo, V.VendorName,
        '' AS PONo,CAST(NULL AS DATE) AS PODate ,
        CONCAT(PGRN.GRNNo,' ',PGRN.Suffix) AS GRNNo,CAST(PGRN.GRNDate AS DATE) AS GRNDate,
         I.ItemCode AS ItemCode,
        I.ItemName AS ItemName,
		I.MeasureUnit[UOM],I.HSNCode,
		 0 AS POQty,
		PSCNS.AcceptQty [AccQty],
	    PSCNS.RejectQty [RejQty],
		CAST(PGRNS.UnitPrice AS DECIMAL(18,3)) AS Unit_Price,
	    0 AS [Purch_Discount], 
        SA.BalQty,
        C.CustName,
        CONCAT(MPO.PONo,' ',MPO.Suffix) AS MfgPONo, CAST(MPO.PODate AS DATE)  AS MfgpoDate,
        CONCAT(M.DcNo,' ',M.Suffix) AS DcNo, CAST(M.DcDate AS DATE) AS DcDate, MDCS.Qty AS DCQty,
        CONCAT(MI.InvNo,' ',MI.Suffix) AS InvNo,CAST(MI.InvDate AS DATE) AS InvDate, MISub.Qty,
		CAST(MISub.UnitPrice AS DECIMAL(18,3)) AS Sales_price,
		CAST(MISub.LineDiscountAmount AS DECIMAL(18,3)) [Discount],
	 -- Purchase Amount
    CAST((
        ISNULL(MISub.Qty, 0) * ISNULL(PGRNS.UnitPrice, 0)
    ) - ISNULL(0, 0) AS DECIMAL(18,3)) AS PurchaseAmount,

    -- Sales Amount
    CAST((
        ISNULL(MISub.Qty, 0) * ISNULL(MISub.UnitPrice, 0)
    ) - ISNULL(MISub.LineDiscountAmount, 0) AS DECIMAL(18,3)) AS SalesAmount,

    -- Profit
   CAST((
        (
            ISNULL(MISub.Qty, 0) * ISNULL(MISub.UnitPrice, 0)
        ) - ISNULL(MISub.LineDiscountAmount, 0)
    )
    -
    (
        (
            ISNULL(MISub.Qty, 0) * ISNULL(PGRNS.UnitPrice, 0)
        ) - ISNULL(0, 0)
    ) AS DECIMAL(18,3)) AS Profit,

        I.ItemId,
        C.CustId,
        V.VendorCode
    FROM PurchaseGRNSub PGRNS
    LEFT JOIN PurchaseGRN PGRN ON PGRN.GRNId = PGRNS.GRNId
    LEFT JOIN PurchaseSCNSub PSCNS ON PSCNS.RefGRNSubId = PGRNS.GRNSubId
    LEFT JOIN PurchaseSCN PSCN ON PSCN.SCNId = PSCNS.SCNId
    LEFT JOIN Item I ON I.ItemId = PGRNS.ItemId
    LEFT JOIN Vendor V ON V.VendorCode = PGRN.VendorCode
    LEFT JOIN StockAdd SA
        ON SA.SubItemRefID = PSCNS.SCNSubId
       AND SA.ItemId = PSCNS.ItemId
       AND SA.ScreenCode = 78
    LEFT JOIN StockIssueTrack SIT ON SIT.AddId = SA.AddId
    LEFT JOIN StockIssue SI ON SIT.IssueId = SI.IssueId
    LEFT JOIN MfgDcSub MDCS
        ON SI.SubItemRefID = MDCS.DcSubId
       AND SI.ScreenCode = 42
    LEFT JOIN MfgDc M ON MDCS.DcId = M.DcId
    LEFT JOIN MfgInvSub MISub
        ON (MISub.InvSubId = SI.SubItemRefID AND SI.ScreenCode = 80)
        OR MISub.RefDcSubId = MDCS.DcSubId
    LEFT JOIN MfgInv MI ON MI.InvId = MISub.InvId
    LEFT JOIN MfgPoSub MPOSUB
        ON MPOSUB.PoSubId = MDCS.RefPoSubId
        OR MPOSUB.PoSubId = MISub.RefPoSubId
    LEFT JOIN MfgPo MPO ON MPO.PoId = MPOSUB.PoId
    LEFT JOIN Customer C
        ON MPO.CustId = C.CustId
        OR M.CustId = C.CustId
    WHERE PGRNS.RefPoSubId IS NULL
      --AND PGRN.GRNDate BETWEEN  @FromDate AND @ToDate
	  AND CAST(PGRN.GRNDate AS DATE)
    BETWEEN CAST(@FromDate AS DATE)
        AND CAST(@ToDate AS DATE) and ((misub.ItemId is not null and misub.ItemId>0) or (mdcs.ItemId is not null and mdcs.ItemId>0))

    GROUP BY
        PGRN.GRNNo, 
        MPO.PONo, M.DcNo,
        MI.InvNo, PGRNS.ItemId,
        I.ItemCode, V.VendorName,
        I.ItemName, PGRN.Suffix,
        MDCS.Qty,
        MISub.Qty,
        PSCN.Suffix, MPO.Suffix,
        M.Suffix, MI.Suffix,
        C.CustName, I.ItemId,
        C.CustId, V.VendorCode  ,PGRN.GRNDate,  MPO.PODate, M.DcDate,MI.InvDate,
		 PSCNs.AcceptQty,PSCNS.RejectQty,MISub.LineDiscountAmount,I.MeasureUnit,I.HSNCode,SA.BalQty,MISub.UnitPrice,PGRNS.UnitPrice

    ORDER BY VendorName, ItemCode;
END
