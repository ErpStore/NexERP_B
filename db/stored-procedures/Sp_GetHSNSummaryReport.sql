-- =============================================

-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[Sp_GetHSNSummaryReport]
	@ReportType VARCHAR(50),
	@FromDate Date,
	@ToDate Date
AS
BEGIN

	If @ReportType = 'Sales'
	BEGIN
	SET NOCOUNT ON;
		Select mis.SlNo,c.CustName AS Customer,'Sales' AS Type,mi.InvNo AS InvoiceNo,CAST(mi.InvDate AS date) AS InvoiceDate,
		i.ItemCode AS PartNo,i.ItemName AS PartName,i.HSNCode,i.MeasureUnit AS UOM,mis.Qty
		from MfgInv mi
		Inner Join MfgInvSub mis ON mis.InvId = mi.InvId
		Inner Join Customer c ON c.CustId = mi.CustId
		Inner Join Item i ON i.ItemId = mis.ItemId
		WHERE mi.InvDate BETWEEN @FromDate AND @ToDate

	 UNION ALL
		Select lis.SlNo,c.CustName AS Customer,'Labour' AS Type,li.LabInvNo AS InvoiceNo,CAST(li.LabInvDate AS date) AS InvoiceDate,
		i.ItemCode AS PartNo,i.ItemName AS PartName,i.HSNCode,i.MeasureUnit AS UOM,lis.Qty
		from LabInv li
		Inner Join LabInvSub lis ON lis.LabInvId=li.LabInvId
		Inner Join Customer c ON c.CustId = li.CustId
		Inner Join Item i ON i.ItemId = lis.ItemId
		WHERE li.LabInvDate BETWEEN @FromDate AND @ToDate
	END

	ELSE If @ReportType = 'Purchase'
	BEGIN
		Select psis.SlNo,v.VendorName AS Customer,'Purchase' AS Type,psi.InvNo AS InvoiceNo,CAST(psi.InvDate AS date) AS InvoiceDate,
		i.ItemCode AS PartNo,i.ItemName AS PartName,i.HSNCode,i.MeasureUnit AS UOM,psis.Qty
		from PurchaseInvoice psi
		Inner Join PurchaseInvoiceSub psis ON psis.InvId = psi.InvId
		Inner Join Vendor v ON v.VendorCode = psi.VendorCode
		Inner Join item i ON i.ItemId = psis.ItemId
		WHERE psi.InvDate BETWEEN @FromDate AND @ToDate

	UNION ALL

		Select sis.SlNo,v.VendorName AS Vendor,'SubContract' AS Type,si.InvNo AS InvoiceNo,CAST(si.InvDate AS date) AS InvoiceDate,
		i.ItemCode AS PartNo,i.ItemName AS PartName,i.HSNCode,i.MeasureUnit AS  UOM,sis.Qty
		from SubConInv si
		Inner Join SubConInvSub sis ON sis.InvId = si.InvId
		Inner Join Vendor v ON v.VendorCode = si.VendorCode
		Inner Join Item i ON i.ItemId = sis.ItemId
		WHERE si.InvDate BETWEEN @FromDate AND @ToDate
	END
END
