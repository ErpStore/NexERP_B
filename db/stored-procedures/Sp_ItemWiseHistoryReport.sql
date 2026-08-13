CREATE OR ALTER          PROCEDURE [dbo].[Sp_ItemWiseHistoryReport]
(
    @ItemId int,
    @FromDate DATE,
    @ToDate DATE
)
AS
BEGIN

    SET NOCOUNT ON;

    ;WITH ItemReport AS
    (

        ---------------------------------------------------
        -- PURCHASE REQUISITION
        ---------------------------------------------------
        SELECT
            CAST(m.MreqDate AS DATE) AS [Date],

            'Purchase Requisition' AS Heading,

            CONCAT(m.MreqNo, ' ', m.Suffix) [DocNo],

            c.CustName AS FromToWhom,

            i.ItemCode AS ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(s.PurchQty AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(s.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM MaterialReqSub s
        INNER JOIN MaterialReq m
            ON s.MreqId = m.MreqId
        LEFT JOIN Customer c
            ON c.CustId = s.CostId
        INNER JOIN Item i
            ON i.ItemId = s.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(m.MreqDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- PURCHASE PO
        ---------------------------------------------------
        SELECT
            CAST(m.PODate AS DATE),

            'Purchase PO',

            CONCAT(m.PONo, ' ', m.Suffix) [DocNo],

            c.VendorName,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)),

            CAST(s.Qty AS DECIMAL(18,2)),

            CAST(0 AS DECIMAL(18,2)),

            CAST(0 AS DECIMAL(18,2)),

            CAST(s.UnitPrice AS DECIMAL(18,2))

        FROM PurchPOSub s
        INNER JOIN PurchPO m
            ON s.PoId = m.PoId
        INNER JOIN Vendor c
            ON c.VendorCode = m.VendorCode
        INNER JOIN Item i
            ON i.ItemId = s.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(m.PODate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- SALES PO
        ---------------------------------------------------
        SELECT
            CAST(m.PODate AS DATE),

            'Sales PO',

            CONCAT(m.PONo, ' ', m.Suffix) [DocNo],

            c.CustName,

            i.ItemCode,

            CAST(s.Qty AS DECIMAL(18,2)),

            CAST(0 AS DECIMAL(18,2)),

            CAST(0 AS DECIMAL(18,2)),

            CAST(0 AS DECIMAL(18,2)),

            CAST(s.UnitPrice AS DECIMAL(18,2))

        FROM MfgPOSub s
        INNER JOIN MfgPO m
            ON s.PoId = m.PoId
        INNER JOIN Customer c
            ON c.CustId = m.CustId
        INNER JOIN Item i
            ON i.ItemId = s.ItemId

        WHERE i.ItemId = @ItemId
      
        AND CAST(m.PODate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- PURCHASE GRN
        ---------------------------------------------------
        SELECT
            CAST(purch.GRNDate AS DATE),

            'Purchase-GRN',

            CONCAT(purch.GRNNo, ' ', purch.Suffix) [DocNo],

            v.VendorName,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)),

            CAST(purchsub.Qty AS DECIMAL(18,2)),

            CAST(0 AS DECIMAL(18,2)),

            CAST(0 AS DECIMAL(18,2)),

            CAST(purchsub.UnitPrice AS DECIMAL(18,2))

        FROM PurchaseGRN purch
        INNER JOIN PurchaseGRNSub purchsub
            ON purch.GRNId = purchsub.GRNId
        INNER JOIN Item i
            ON i.ItemId = purchsub.ItemId
        INNER JOIN Vendor v
            ON v.VendorCode = purch.VendorCode

        WHERE i.ItemId = @ItemId
        AND CAST(purch.GRNDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- PURCHASE SCN
        ---------------------------------------------------
        SELECT
            CAST(purch.SCNDate AS DATE),

            'Purchase-SCN',

            CONCAT(purch.SCNNo, ' ', purch.Suffix) [DocNo],

            v.VendorName,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)),

            CAST(purchsub.AcceptQty AS DECIMAL(18,2)),

            CAST(purchsub.RejectQty AS DECIMAL(18,2)),

            CAST(purchsub.ReworkQty AS DECIMAL(18,2)),

            CAST(purchsub.UnitPrice AS DECIMAL(18,2))

        FROM PurchaseSCN purch
        INNER JOIN PurchaseSCNSub purchsub
            ON purch.SCNId = purchsub.SCNId
        INNER JOIN Item i
            ON i.ItemId = purchsub.ItemId
        INNER JOIN Vendor v
            ON v.VendorCode = purch.VendorCode

        WHERE i.ItemId = @ItemId
        AND CAST(purch.SCNDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- PURCHASE INVOICE
        ---------------------------------------------------
        SELECT
            CAST(purch.InvDate AS DATE),

            'Purchase-Invoice',

            CONCAT(purch.InvNo, ' ', purch.Suffix) [DocNo],

            v.VendorName,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)),

            CAST(purchsub.Qty AS DECIMAL(18,2)),

            CAST(purchsub.RejectQty AS DECIMAL(18,2)),

            CAST(0 AS DECIMAL(18,2)),

            CAST(purchsub.UnitPrice AS DECIMAL(18,2))

        FROM PurchaseInvoice purch
        INNER JOIN PurchaseInvoiceSub purchsub
            ON purch.InvId = purchsub.InvId
        INNER JOIN Item i
            ON i.ItemId = purchsub.ItemId
        INNER JOIN Vendor v
            ON v.VendorCode = purch.VendorCode

        WHERE i.ItemId = @ItemId
        AND CAST(purch.InvDate AS DATE)
            BETWEEN @FromDate AND @ToDate


			        UNION ALL

        ---------------------------------------------------
        -- SALES DC
        ---------------------------------------------------
        SELECT
            CAST(dc.DcDate AS DATE),

            'Sales-DCOut' AS Heading,

            CONCAT(dc.DCNo, ' ', dc.Suffix) AS DocNo,

            c.CustName AS FromToWhom,

            i.ItemCode,

            CAST(dcsub.Qty AS DECIMAL(18,2)) AS QtyOut,

            CAST(0 AS DECIMAL(18,2)) AS QtyIn,

            CAST(dcsub.RejectQty AS DECIMAL(18,2)) AS RejQty,

            CAST(dcsub.ReworkQty AS DECIMAL(18,2)) AS ReworkQty,

            CAST(dcsub.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM MfgDc dc
        INNER JOIN MfgDcSub dcsub
            ON dc.DcId = dcsub.DcId
        INNER JOIN Item i
            ON i.ItemId = dcsub.ItemId
        INNER JOIN Customer c
            ON c.CustId = dc.CustId

        WHERE i.ItemId = @ItemId
        AND CAST(dc.DcDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- SALES INVOICE
        ---------------------------------------------------
        SELECT
            CAST(m.InvDate AS DATE),

            'Sales Invoice' AS Heading,

            CONCAT(m.InvNo, ' ', m.Suffix) AS DocNo,

            c.CustName AS FromToWhom,

            i.ItemCode,

            CAST(s.Qty AS DECIMAL(18,2)) AS QtyOut,

            CAST(0 AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(s.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM MfgInvSub s
        INNER JOIN MfgInv m
            ON s.InvId = m.InvId
        INNER JOIN Customer c
            ON c.CustId = m.CustId
        INNER JOIN Item i
            ON i.ItemId = s.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(m.InvDate AS DATE)
            BETWEEN @FromDate AND @ToDate

			        UNION ALL

        ---------------------------------------------------
        -- LABOUR GRN
        ---------------------------------------------------
        SELECT
            CAST(DI.GRNDate AS DATE),

            'Labour-GRN' AS Heading,

            CONCAT(DI.GRNNo, ' ', DI.Suffix) AS DocNo,

            c.CustName AS FromToWhom,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(DISub.Qty AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(0 AS DECIMAL(18,2)) AS Rate

        FROM LabourGRN DI
        INNER JOIN LabourGRNSub DISub
            ON DI.GRNId = DISub.GRNId
        INNER JOIN Item i
            ON i.ItemId = DISub.ItemId
        INNER JOIN Customer c
            ON c.CustId = DI.CustId

        WHERE i.ItemId = @ItemId
        AND CAST(DI.GRNDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- LABOUR SCN
        ---------------------------------------------------
        SELECT
            CAST(DI.SCNDate AS DATE),

            'Labour-SCN' AS Heading,

            concat(di.SCNId, '',di.Suffix) as DocNo,

            c.CustName AS FromToWhom,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(DISub.AcceptQty AS DECIMAL(18,2)) AS QtyIn,

            CAST(DISub.RejectQty AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(0 AS DECIMAL(18,2)) AS Rate

        FROM LabourSCN DI
        INNER JOIN LabourSCNSub DISub
            ON DI.SCNId = DISub.SCNId
        INNER JOIN Item i
            ON i.ItemId = DISub.ItemId
        INNER JOIN Customer c
            ON c.CustId = DI.CustId

        WHERE i.ItemId = @ItemId
        AND CAST(DI.SCNDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- LABOUR DC OUT
        ---------------------------------------------------
        SELECT
            CAST(DcOUT.DcDate AS DATE),

            'Labour-DCOut' AS Heading,

            CONCAT(DcOUT.DcNo, ' ', DcOUT.Suffix) AS DocNo,

            c.CustName AS FromToWhom,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(DISub.Qty AS DECIMAL(18,2)) AS QtyIn,

            CAST(DISub.RejectQty AS DECIMAL(18,2)) AS RejQty,

            CAST(DISub.ReworkQty AS DECIMAL(18,2)) AS ReworkQty,

            CAST(DISub.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM LabourDcOutgoing DcOUT
        INNER JOIN LabourDcOutgoingSub DISub
            ON DcOUT.DcId = DISub.DcId
        INNER JOIN Item i
            ON i.ItemId = DISub.ItemId
        INNER JOIN Customer c
            ON c.CustId = DcOUT.CustId

        WHERE i.ItemId = @ItemId
        AND CAST(DcOUT.DcDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- LABOUR INVOICE
        ---------------------------------------------------
        SELECT
            CAST(DI.LabInvDate AS DATE),

            'Labour-Invoice' AS Heading,

            CONCAT(DI.LabInvNo, ' ', DI.Suffix) AS DocNo,

            c.CustName AS FromToWhom,

            i.ItemCode,

            CAST(DISub.Qty AS DECIMAL(18,2)) AS QtyOut,

            CAST(0 AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(0 AS DECIMAL(18,2)) AS Rate

        FROM LabInv DI
        INNER JOIN LabInvSub DISub
            ON DI.LabInvId = DISub.LabInvId
        INNER JOIN Item i
            ON i.ItemId = DISub.ItemId
        INNER JOIN Customer c
            ON c.CustId = DI.CustId

        WHERE i.ItemId = @ItemId
        AND CAST(DI.LabInvDate AS DATE)
            BETWEEN @FromDate AND @ToDate


		UNION ALL

        ---------------------------------------------------
        -- SUBCON DC OUT
        ---------------------------------------------------
        SELECT
            CAST(DcSubOUT.DcDate AS DATE),

            'SubCon-DCOut' AS Heading,

            CONCAT(DcSubOUT.DcNo, ' ', DcSubOUT.Suffix) AS DocNo,

            v.VendorName AS FromToWhom,

            i.ItemCode,

            CAST(DCsub.Qty AS DECIMAL(18,2)) AS QtyOut,

            CAST(0 AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(0 AS DECIMAL(18,2)) AS Rate

        FROM SubConDcOut DcSubOUT
        INNER JOIN SubConDcOutSub DCsub
            ON DcSubOUT.DcId = DCsub.DcId
        INNER JOIN Item i
            ON i.ItemId = DCsub.ItemId
        INNER JOIN Vendor v
            ON v.VendorCode = DcSubOUT.VendorCode

        WHERE i.ItemId = @ItemId
        AND CAST(DcSubOUT.DcDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- SUBCON GRN
        ---------------------------------------------------
        SELECT
            CAST(DcSubIn.GRNDate AS DATE),

            'SubCon-GRN' AS Heading,

            concat(DcSubIn.GRNNo, ' ',DcSubIn.Suffix ) AS DocNo,

            v.VendorName AS FromToWhom,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(DCsub.Qty AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(DCsub.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM SubConGRN DcSubIn
        INNER JOIN SubConGRNSub DCsub
            ON DcSubIn.GRNId = DCsub.GRNId
        INNER JOIN Item i
            ON i.ItemId = DCsub.ItemId
        INNER JOIN Vendor v
            ON v.VendorCode = DcSubIn.VendorCode

        WHERE i.ItemId = @ItemId
        AND CAST(DcSubIn.GRNDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- SUBCON SCN
        ---------------------------------------------------
        SELECT
            CAST(DcSubIn.SCNDate AS DATE),

            'SubCon-SCN' AS Heading,

            concat(DcSubIn.SCNNo, ' ',DcSubIn.Suffix) AS DocNo,

            v.VendorName AS FromToWhom,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(DCsub.AccQty AS DECIMAL(18,2)) AS QtyIn,

            CAST(DCsub.RejQty AS DECIMAL(18,2)) AS RejQty,

            CAST(DCsub.RewQty AS DECIMAL(18,2)) AS ReworkQty,

            CAST(DCsub.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM SubConSCN DcSubIn
        INNER JOIN SubConSCNSub DCsub
            ON DcSubIn.SCNId = DCsub.SCNId
        INNER JOIN Item i
            ON i.ItemId = DCsub.ItemId
        INNER JOIN Vendor v
            ON v.VendorCode = DcSubIn.VendorCode

        WHERE i.ItemId = @ItemId
        AND CAST(DcSubIn.SCNDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- SUBCON INVOICE
        ---------------------------------------------------
        SELECT
            CAST(DcSubIn.InvDate AS DATE),

            'SubCon-Invoice' AS Heading,

            CONCAT(DcSubIn.InvNo, ' ', DcSubIn.Suffix) AS DocNo,

            v.VendorName AS FromToWhom,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(DCsub.Qty AS DECIMAL(18,2)) AS QtyIn,

            CAST(DCsub.RejectQty AS DECIMAL(18,2)) AS RejQty,

            CAST(DCsub.ReworkQty AS DECIMAL(18,2)) AS ReworkQty,

            CAST(DCsub.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM SubConInv DcSubIn
        INNER JOIN SubConInvSub DCsub
            ON DcSubIn.InvId = DCsub.InvId
        INNER JOIN Item i
            ON i.ItemId = DCsub.ItemId
        INNER JOIN Vendor v
            ON v.VendorCode = DcSubIn.VendorCode

        WHERE i.ItemId = @ItemId
        AND CAST(DcSubIn.InvDate AS DATE)
            BETWEEN @FromDate AND @ToDate

			        UNION ALL

        ---------------------------------------------------
        -- GENERAL SCN
        ---------------------------------------------------
        SELECT
            CAST(sgen.SCNGenDate AS DATE),

            'Stock Transfer Note' AS Heading,

            concat(sgen.SCNGenNo, ' ',sgen.Suffix) AS DocNo,

            sgen.CreatedBy AS FromToWhom,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(scnsub.Qty AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(scnsub.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM SCNGen sgen
        INNER JOIN SCNGenSub scnsub
            ON sgen.SCNGenId = scnsub.SCNGenId
        INNER JOIN Item i
            ON i.ItemId = scnsub.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(sgen.SCNGenDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- MATERIAL ISSUE NOTE
        ---------------------------------------------------
        SELECT
            CAST(Iss.IssueDate AS DATE),

            'Mat. Iss. Note' AS Heading,

            concat(Iss.IssueNo, ' ',Iss.Suffix) AS DocNo,

            Iss.ToWhom AS FromToWhom,

            i.ItemCode,

            CAST(Isssub.UtlQty AS DECIMAL(18,2)) AS QtyOut,

            CAST(0 AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(Isssub.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM MaterialIssNote Iss
        INNER JOIN MaterialIssNoteSub Isssub
            ON Iss.MINId = Isssub.MINId
        INNER JOIN Item i
            ON i.ItemId = Isssub.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(Iss.IssueDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- TOOL CRIB ISSUE
        ---------------------------------------------------
        SELECT
            CAST(TLC.TCIssueDate AS DATE),

            'Tool Crib Issue' AS Heading,

            concat(TLC.TCIssueNo, ' ',tlc.Suffix) AS DocNo,

            TLC.IssuedToWhom AS FromToWhom,

            i.ItemCode,

            CAST(tlcs.QtyOut AS DECIMAL(18,2)) AS QtyOut,

            CAST(0 AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(tlcs.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM ToolCribIssue tlc

        INNER JOIN ToolCribIssueSub tlcs
           ON tlc.TCIssueId = tlcs.TCIssueId
        INNER JOIN Item i
            ON i.ItemId = tlcs.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(TLC.TCIssueDate AS DATE)
            BETWEEN @FromDate AND @ToDate


	UNION ALL

        ---------------------------------------------------
        -- TOOL CRIB RETURN
        ---------------------------------------------------
        SELECT
            CAST(TLC.TCReturnDate AS DATE),

            'Tool Crib Return' AS Heading,

            concat(TLC.TCReturnNo, ' ',tlc.Suffix) AS DocNo,

            TLC.FromWhom AS FromToWhom,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(tlcs.AccpQty AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(0 AS DECIMAL(18,2)) AS Rate

        FROM ToolCribReturns tlc

        INNER JOIN ToolCribReturnSubs tlcs
           ON tlc.TCReturnId = tlcs.TCReturnId
        INNER JOIN Item i
            ON i.ItemId = tlcs.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(TLC.TCReturnDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- STORE INTER TRANSFER
        ---------------------------------------------------
        SELECT
            CAST(SIT.ISTdate AS DATE),

            'Store Trans. Note(Gen)' AS Heading,

            concat(SIT.ISTno, ' ',SIT.Suffix) AS DocNo,

            SIT.CreatedBy AS FromToWhom,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(SITsub.Qty AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(SITsub.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM StoreInterTrans SIT
        INNER JOIN StoreInterTransSub SITSub
            ON SIT.ISTId = SITSub.ISTId
        INNER JOIN Item i
            ON i.ItemId = SITSub.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(SIT.ISTdate AS DATE)
            BETWEEN @FromDate AND @ToDate

		        UNION ALL

        ---------------------------------------------------
        -- PRODUCTION ISS COMP
        ---------------------------------------------------
        SELECT
            CAST(PIss.IssueDate AS DATE),

            'Production Iss Comp' AS Heading,

            concat(PIss.IssueNo, ' ',PIss.Suffix) AS DocNo,

            PIss.IssueToWhom AS FromToWhom,

            i.ItemCode,

            CAST(PIssSub.Qty AS DECIMAL(18,2)) AS QtyOut,

            CAST(0 AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(PIssSub.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM ProductionIssueComp PIss
        INNER JOIN ProductionIssueCompSub PIssSub
            ON PIss.IssueId = PIssSub.IssueId
        INNER JOIN Item i
            ON i.ItemId = PIssSub.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(PIss.IssueDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- PRODUCTION RETURN COMP
        ---------------------------------------------------
        SELECT
            CAST(PWR.ReturnDate AS DATE),

            'Production Ret Comp' AS Heading,

            concat(PWR.ReturnNo, ' ',PWR.Suffix) AS DocNo,

            PWR.ReturnBy AS FromToWhom,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(PWRSub.Qty AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(PWRSub.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM ProductionReturnComp PWR
        INNER JOIN ProductionReturnCompSub PWRSub
            ON PWR.ReturnId = PWRSub.ReturnId
        INNER JOIN Item i
            ON i.ItemId = PWRSub.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(PWR.ReturnDate AS DATE)
            BETWEEN @FromDate AND @ToDate

        UNION ALL

        ---------------------------------------------------
        -- PRODUCTION SCN COMP
        ---------------------------------------------------
        SELECT
            CAST(PWR.SCNDate AS DATE),

            'Production SCN Comp' AS Heading,

            concat(PWR.SCNNo, ' ',PWR.Suffix) AS DocNo,

            PWR.CreatedBy AS FromToWhom,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(PWRSub.AccQty AS DECIMAL(18,2)) AS QtyIn,

            CAST(PWRSub.RejQty AS DECIMAL(18,2)) AS RejQty,

            CAST(PWRSub.RewQty AS DECIMAL(18,2)) AS ReworkQty,

            CAST(0 AS DECIMAL(18,2)) AS Rate

        FROM ProductionSCNComp PWR
        INNER JOIN ProductionSCNCompSub PWRSub
            ON PWR.SCNId = PWRSub.SCNId
        INNER JOIN Item i
            ON i.ItemId = PWRSub.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(PWR.SCNDate AS DATE)
            BETWEEN @FromDate AND @ToDate

						UNION ALL
	---------------------------------------------------
				--ProductionIssueAssy
	---------------------------------------------------
	SELECT
            CAST(pa.IssueDate AS DATE),

            'Production Issue Assembly' AS Heading,

            concat(pa.IssueNo, ' ',pa.Suffix) AS DocNo,

            pa.CreatedBy AS FromToWhom,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(pas.IssueQty AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(pas.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM ProductionIssueAssy PA
        INNER JOIN ProductionIssueAssySub PAS
            ON pa.IssueId = pas.IssueId
        INNER JOIN Item i
            ON i.ItemId = pas.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(pa.IssueDate AS DATE)
            BETWEEN @FromDate AND @ToDate

				UNION ALL
	---------------------------------------------------
				--ProductionReturnAssy
	---------------------------------------------------
	SELECT
            CAST(pa.ReturnDate AS DATE),

            'Production GRN Assmbely' AS Heading,

            concat(pa.ReturnNo, ' ',pa.Suffix) AS DocNo,

            pa.CreatedBy AS FromToWhom,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(pas.QtyReturned AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(pas.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM ProductionReturnAssy PA
        INNER JOIN ProductionReturnAssysub PAS
            ON pa.ReturnId = pas.ReturnId
        INNER JOIN Item i
            ON i.ItemId = pas.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(pa.ReturnDate AS DATE)
            BETWEEN @FromDate AND @ToDate

							UNION ALL
	---------------------------------------------------
				--ProductionSCNAssy
	---------------------------------------------------
	SELECT
            CAST(pa.SCNDate AS DATE),

            'Production SCN Assembly' AS Heading,

            concat(pa.SCNNo, ' ',pa.Suffix) AS DocNo,

            pa.CreatedBy AS FromToWhom,

            i.ItemCode,

            CAST(0 AS DECIMAL(18,2)) AS QtyOut,

            CAST(pas.AccQty AS DECIMAL(18,2)) AS QtyIn,

            CAST(pas.RejQty AS DECIMAL(18,2)) AS RejQty,

            CAST(pas.RewQty AS DECIMAL(18,2)) AS ReworkQty,

            CAST(pas.UnitPrice AS DECIMAL(18,2)) AS Rate

        FROM ProductionSCNAssy PA
        INNER JOIN ProductionSCNAssySub PAS
            ON pa.SCNId = pas.SCNId
        INNER JOIN Item i
            ON i.ItemId = pas.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(pa.SCNDate AS DATE)
            BETWEEN @FromDate AND @ToDate


								UNION ALL
	---------------------------------------------------
				--JobOrder
	---------------------------------------------------
	SELECT
            CAST(jo.JobDate AS DATE),

            'Joborder' AS Heading,

            concat(jo.JobNo, ' ',jo.Suffix) AS DocNo,

            jo.CreatedBy AS FromToWhom,

            i.ItemCode,

            CAST(jos.ReqQty AS DECIMAL(18,2)) AS QtyOut,

            CAST(jo.POQty AS DECIMAL(18,2)) AS QtyIn,

            CAST(0 AS DECIMAL(18,2)) AS RejQty,

            CAST(0 AS DECIMAL(18,2)) AS ReworkQty,

            CAST(0 AS DECIMAL(18,2)) AS Rate

        FROM JobOrder jo
        INNER JOIN JobOrderSub jos
            ON jo.JobId = jos.JobId
        INNER JOIN Item i
            ON i.ItemId = jos.ItemId

        WHERE i.ItemId = @ItemId
        AND CAST(jo.JobDate AS DATE)
            BETWEEN @FromDate AND @ToDate

    )

    ---------------------------------------------------
    -- FINAL RESULT
    ---------------------------------------------------
    SELECT

        ROW_NUMBER() OVER (ORDER BY [Date]) AS SlNo,

        [Date],

        Heading,

        DocNo,

        FromToWhom,

        ItemCode,

        QtyOut,

        QtyIn,

        RejQty,

        ReworkQty,

        Rate

    FROM ItemReport

    ORDER BY [Date]

END



