
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[Sp_TDSReport]
	@ReportType VARCHAR(50),
	@FromDate DATE,
    @ToDate DATE
	
AS
BEGIN

 IF @ReportType = 'Sales'
    BEGIN
	SET NOCOUNT ON;
		SELECT
			mis.SlNo,c.CustName AS Customer,'Sales' AS Type,mi.InvNo AS InvoiceNo,CAST(mi.InvDate AS DATE) AS InvoiceDate,
			mi.GrandTotal AS InvoiceValue,mi.TotalBasicAmount,cn.CreditNo AS CreditDebitNoteNo,TRY_CONVERT(DATE, cn.CreditDate) AS CreditDebitNoteDate,
			cn.TotalBasicAmount AS CreditDebitNoteAmt,c.TDSDeductper AS TDSPercent,ROUND(mi.TDSAmount, 2) AS TDSAmt,ROUND(mi.GrandTotal - ISNULL(cn.TotalBasicAmount, 0), 2) AS TotalAmount,
			ROUND(mi.Balance, 2) AS InvoiceBalance,'' AS Remarks,c.CustId,c.PANNo AS PANNo
		FROM MfgInv mi
		INNER JOIN MfgInvSub mis ON mi.InvId = mis.InvId
		INNER JOIN Customer c ON c.CustId = mi.CustId
		LEFT JOIN CreditNote cn ON cn.CustId = mi.CustId
		WHERE ISNULL(mi.TDSAmount,0) > 0 AND mi.InvDate BETWEEN @FromDate AND @ToDate

		UNION ALL
		-----Labour-----
		SELECT
			lis.SlNo,c.CustName AS Customer,'Labour' AS Type ,li.LabInvNo AS InvoiceNo,CAST(li.LabInvDate AS DATE) AS InvoiceDate,
			li.GrandTotal AS InvoiceValue,li.TotalBasicAmount,cn.CreditNo AS CreditDebitNoteNo,TRY_CONVERT(DATE, cn.CreditDate) AS CreditDebitNoteDate,
			cn.TotalBasicAmount AS CreditDebitNoteAmt,c.TDSDeductper AS TDSPercent,ROUND(li.TDSAmount, 2) AS TDSAmt,ROUND(li.GrandTotal - ISNULL(cn.TotalBasicAmount, 0), 2) AS TotalAmount,
			ROUND(li.Balance, 2) AS InvoiceBalance,'' AS Remarks,c.CustId,c.PANNo AS PANNo
		FROM LabInv li
		INNER JOIN LabInvSub lis ON li.LabInvId = lis.LabInvId
		INNER JOIN Customer c ON c.CustId = li.CustId
		LEFT JOIN CreditNote cn ON cn.CustId = li.CustId
		WHERE ISNULL(li.TDSAmount,0) > 0 AND li.LabInvDate BETWEEN @FromDate AND @ToDate

	END

	ELSE IF @ReportType = 'Purchase'
    BEGIN

			-----Purchase------
			SELECT 
				psis.SlNo,v.VendorName AS Customer,'Purchase' AS Type,psi.InvNo AS InvoiceNo,CAST(psi.InvDate AS DATE) AS InvoiceDate,
				psi.GrandTotal AS InvoiceValue,psi.TotalBasicAmount,dn.DebitNo AS CreditDebitNoteNo,TRY_CONVERT(DATE, dn.DebitDate) AS CreditDebitNoteDate,
				dn.TotalBasicAmount AS CreditDebitNoteAmt,v.TDSDeductper AS TDSPercent,ROUND(psi.TDSAmount, 2) AS TDSAmt,ROUND(psi.GrandTotal - ISNULL(dn.TotalBasicAmount, 0), 2) AS TotalAmount,
				ROUND(psi.Balance, 2) AS InvoiceBalance,'' AS Remarks,v.VendorCode,v.PANNo AS PANNo
			FROM PurchaseInvoice psi
			    inner join PurchaseInvoiceSub psis on psi.InvId=psis.InvId 
				inner join Vendor v on v.VendorCode=psi.VendorCode
				left join DebitNoteSub dbs on dbs.RefPurchInvSubId=psis.InvSubId
				left join DebitNote dn on dn.DbId=dbs.DbId and dn.VendorCode=psi.VendorCode
			--INNER JOIN PurchaseInvoiceSub psis ON psi.InvId = psis.InvId
			--INNER JOIN Vendor v ON v.VendorCode = psi.VendorCode
			--LEFT JOIN DebitNote dn ON dn.VendorCode = psi.VendorCode
			WHERE ISNULL(psi.TDSAmount,0) > 0 AND psi.InvDate BETWEEN @FromDate AND @ToDate

			UNION ALL
			-----SubContract------
			SELECT 
				sis.SlNo,v.VendorName AS Customer,'SubContract' AS Type,si.InvNo AS InvoiceNo,CAST(si.InvDate AS DATE) AS InvoiceDate,
				si.GrandTotal AS InvoiceValue,si.TotalBasicAmount,dn.DebitNo AS CreditDebitNoteNo,TRY_CONVERT(DATE, dn.DebitDate) AS CreditDebitNoteDate,
				dn.TotalBasicAmount AS CreditDebitNoteAmt,v.TDSDeductper AS TDSPercent,ROUND(si.TDSAmount, 2) AS TDSAmt,ROUND(si.GrandTotal - ISNULL(dn.TotalBasicAmount, 0), 2) AS TotalAmount,
				ROUND(si.Balance, 2) AS InvoiceBalance,'' AS Remarks,v.VendorCode,v.PANNo AS PANNo
			FROM SubConInv si
			--INNER JOIN SubConInvSub sis ON si.InvId = sis.InvId
			--INNER JOIN Vendor v ON v.VendorCode = si.VendorCode
			--LEFT JOIN DebitNote dn ON dn.VendorCode = si.VendorCode
			inner join SubConInvSub sis ON si.InvId = sis.InvId 
				inner join Vendor v on v.VendorCode=si.VendorCode
				left join DebitNoteSub dns on dns.RefPurchInvSubId=sis.InvSubId
				left join DebitNote dn on dn.DbId=dns.DbId and dn.VendorCode=si.VendorCode
			WHERE ISNULL(si.TDSAmount,0) > 0 AND si.InvDate BETWEEN @FromDate AND @ToDate

		END
END
