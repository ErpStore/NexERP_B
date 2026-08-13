CREATE OR ALTER    PROCEDURE [dbo].[Sp_Print_PurchaseInvoice]
@InvId INT
AS
BEGIN

SELECT 
    Cs.VendorName,Cs.VendorAddress,Cs.VendorCode,Cs.GSTNo,Cs.PANNo, Cs.ContactNo, CONCAT(Qt.InvNo, '', Qt.Suffix) AS [Invs No],
    It.MeasureUnit AS [UOM],CONCAT(It.ItemCode, ' ,', It.ItemName) AS [Desc], It.HSNCode,
    CONCAT(pg.SCNNo, '', pg.Suffix, ' Dt:', CONVERT(VARCHAR(10), pg.SCNDate, 105)) AS SCNNo, C.CurrName,C.CurrSub,C.Symbol,
    CASE 
        WHEN (Qts.LineCGSTRate + Qts.LineSGSTRate) > 0 
            THEN (Qts.LineCGSTRate + Qts.LineSGSTRate) 
        ELSE Qts.LineIGSTRate 
    END AS GST,
    /* Display Discount Type (Disc % or Disc ₹) */
    CASE 
        WHEN Qts.LineDiscountPercent > 0 THEN 'Disc %'
        WHEN Qts.LineDiscountAmount > 0 THEN 'Disc ₹'
        WHEN Qt.DiscAmtOrPer > 0 AND Qt.IsItemWiseDiscAmtOrPerReq = 0 
             THEN CASE WHEN Qt.DiscAmtOrPer = 1 THEN 'Disc %' ELSE 'Disc ₹' END
        ELSE 'Disc %'
    END AS DispDisc,

    /* Discount Value */
    CASE 
        WHEN Qts.LineDiscountPercent > 0 THEN Qts.LineDiscountPercent
        WHEN Qts.LineDiscountAmount > 0 THEN Qts.LineDiscountAmount
        ELSE Qt.DiscountAmount  -- main discount if item wise not given
    END AS DiscValue,

    /* Discount Display Format (%) or Symbol */
    CASE 
		WHEN Qts.LineDiscountPercent > 0 
			THEN CONCAT(FORMAT(Qts.LineDiscountPercent, 'N2', 'en-IN'), '%')

		WHEN Qts.LineDiscountAmount > 0 
			THEN C.Symbol

		WHEN Qt.IsItemWiseDiscAmtOrPerReq = 0 
			 AND Qt.DiscAmtOrPer = 0 
			THEN CONCAT(FORMAT(Qt.DiscountPercent, 'N2', 'en-IN'), '%')

		ELSE C.Symbol
	END AS Disc,

    /* Packing */
    CASE 
        WHEN Qt.PackingAmtOrPer = 0 AND Qt.PackingPercent > 0 
            THEN CONCAT(FORMAT(CAST(Qt.PackingPercent AS DECIMAL(10,1)), 'N1', 'EN-IN'), '%')
        ELSE C.Symbol 
    END AS pk,

    /* Insurance */
    CASE 
        WHEN Qt.InsuranceAmtOrPer = 0 AND Qt.InsurancePercent > 0 
            THEN CONCAT(FORMAT(CAST(Qt.InsurancePercent AS DECIMAL(10,1)), 'N1', 'EN-IN'), '%')
        ELSE C.Symbol 
    END AS Ins,

    /* TCS */
    CASE 
        WHEN Qt.TCSAmtOrPer = 0 AND Qt.TCSPercent > 0 
            THEN CONCAT(FORMAT(CAST(Qt.TCSPercent AS DECIMAL(10,1)), 'N1', 'EN-IN'), '%')
        ELSE C.Symbol 
    END AS tcs,
	CASE
	   WHEN Qt.IsAcptRejRewQtyRequired=1 then (Qts.qty+Qts.RejectQty+Qts.ReworkQty) else Qts.Qty end as reqQty,

    Qt.*,
    Qts.*

FROM PurchaseInvoice Qt
INNER JOIN PurchaseInvoiceSub Qts ON Qt.InvId = Qts.InvId
INNER JOIN Item It ON Qts.ItemId = It.ItemId
INNER JOIN Vendor Cs ON Qt.VendorCode = Cs.VendorCode
INNER JOIN PurchaseSCNSub Ps ON Ps.SCNSubId = Qts.RefSCNSubId
INNER JOIN PurchaseSCN Pg ON Pg.SCNId = Ps.SCNId
INNER JOIN Currency C ON C.CurrId = Qt.CurrId
WHERE Qt.InvId = @InvId
ORDER BY Qts.SlNo;

END
