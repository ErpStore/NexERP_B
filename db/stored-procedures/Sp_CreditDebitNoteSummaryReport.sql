
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[Sp_CreditDebitNoteSummaryReport]
	@ReportType VARCHAR(50),
	@FromDate DATE,
    @ToDate DATE
	
AS
BEGIN

 IF @ReportType = 'Sales'
    BEGIN
	SET NOCOUNT ON;
		SELECT mis.SlNo,c.CustName AS Customer,'Sales' AS Type,mi.InvNo AS InvoiceNo,CAST(mi.InvDate AS DATE) AS InvoiceDate,mi.TotalBasicAmount,
		mi.DiscountPercent,mi.DiscountAmount,mi.TotalGrossAmount,mi.FreightCharges,mi.PackingPercent,mi.PackingCharges,mi.InsurancePercent,
		mi.InsuranceCharges,mi.TotalTaxable,mi.CGstRate,mi.TotalCGSTAmount,mi.SGstRate,mi.TotalSGSTAmount,mi.IGstRate,mi.TotalIGSTAmount,
		mi.TCSPercent,mi.TCSAmount,mi.OtherCharges,mi.GrandTotal,cn.GrandTotal AS CreditAmt,cn.MainRemark
		FROM CreditNote cn
		INNER JOIN CreditNoteSub cns ON cn.CrId = cns.CrId
		INNER JOIN MfgInvSub mis ON mis.InvSubId = cns.RefInvSubId
		INNER JOIN MfgInv mi ON mi.InvId = mis.InvId
		INNER JOIN Customer c ON c.CustId = mi.CustId
		WHERE cn.CreditDate BETWEEN @FromDate AND @ToDate

		UNION ALL
		-----Labour-----

		SELECT lis.SlNo,c.CustName AS Customer,'Labour' AS Type,li.LabInvNo AS InvoiceNo,CAST(li.LabInvDate AS DATE) AS InvoiceDate,li.TotalBasicAmount,
		li.DiscountPercent,li.DiscountAmount,li.TotalGrossAmount,li.FreightCharges,li.PackingPercent,li.PackingCharges,li.InsurancePercent,
		li.InsuranceCharges,li.TotalTaxable,li.CGstRate,li.TotalCGSTAmount,li.SGstRate,li.TotalSGSTAmount,li.IGstRate,li.TotalIGSTAmount,
		li.TCSPercent,li.TCSAmount,li.OtherCharges,li.GrandTotal,cn.GrandTotal AS CreditAmt,cn.MainRemark
		FROM CreditNote cn
		INNER JOIN CreditNoteSub cns ON cn.CrId = cns.CrId
		INNER JOIN LabInvSub lis ON lis.LabInvSubId = cns.RefInvSubId
		INNER JOIN LabInv li ON li.LabInvId = lis.LabInvId
		INNER JOIN Customer c ON c.CustId = li.CustId
		WHERE cn.CreditDate BETWEEN @FromDate AND @ToDate
	END

	ELSE IF @ReportType = 'Purchase'
    BEGIN

			-----Purchase------

			SELECT psis.SlNo,v.VendorName AS Customer,'Purchase' AS Type,psi.InvNo AS InvoiceNo,CAST(psi.InvDate AS DATE) AS InvoiceDate,
    psi.TotalBasicAmount,psi.DiscountPercent,psi.DiscountAmount,psi.TotalGrossAmount,psi.FreightCharges,psi.PackingPercent,
    psi.PackingCharges,psi.InsurancePercent,psi.InsuranceCharges,psi.TotalTaxable,psi.CGstRate,psi.TotalCGSTAmount,psi.SGstRate,
    psi.TotalSGSTAmount,psi.IGstRate,psi.TotalIGSTAmount,psi.TCSPercent,psi.TCSAmount,psi.OtherCharges,psi.GrandTotal,
    dn.GrandTotal AS CreditAmt,dn.MainRemark
	FROM DebitNote dn
	INNER JOIN DebitNoteSub dbs ON dn.DbId = dbs.DbId
	INNER JOIN PurchaseInvoiceSub psis ON psis.InvSubId = dbs.RefPurchInvSubId
	INNER JOIN PurchaseInvoice psi ON psi.InvId = psis.InvId
	INNER JOIN Vendor v ON v.VendorCode = psi.VendorCode
    WHERE dn.DebitDate >= @FromDate
AND dn.DebitDate < DATEADD(DAY,1,@ToDate)
			

			UNION ALL
			-----SubContract------

			SELECT sis.SlNo,v.VendorName AS Customer,'SubContract' AS Type,si.InvNo AS InvoiceNo,CAST(si.InvDate AS DATE) AS InvoiceDate,
			si.TotalBasicAmount,si.DiscountPercent,si.DiscountAmount,si.TotalGrossAmount,si.FreightCharges,si.PackingPercent,si.PackingCharges,
			si.InsurancePercent,si.InsuranceCharges,si.TotalTaxable,si.CGstRate,si.TotalCGSTAmount,si.SGstRate,si.TotalSGSTAmount,si.IGstRate,
			si.TotalIGSTAmount,si.TCSPercent,si.TCSAmount,si.OtherCharges,si.GrandTotal,dn.GrandTotal AS CreditAmt,dn.MainRemark
			FROM DebitNote dn
			INNER JOIN DebitNoteSub dns ON dn.DbId = dns.DbId
			INNER JOIN SubConInvSub sis ON sis.InvSubId = dns.RefSubConInvSubId
			INNER JOIN SubConInv si ON si.InvId = sis.InvId
			INNER JOIN Vendor v ON v.VendorCode = si.VendorCode
			WHERE dn.DebitDate BETWEEN @FromDate AND @ToDate

		END
END
