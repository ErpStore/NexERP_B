Create Or ALTER Procedure [dbo].[Sp_Print_MfgQuote]
@QuoteId int
As

SELECT Cs.CustName,Cs.CustAddr,Cs.VendorCode,Cs.GSTNo,Cs.PANNo,Cs.ContactNo,CONCAT(qt.Prefix, ' ', qt.QuoteNo, ' ', qt.Suffix) as QuoteNo,
Convert(varchar,cast(qt.QuoteDate as date),103)[QuoteDate],C.Symbol,C.CurrName,C.CurrSub,es.EnquiryNo,Convert(varchar,cast(es.EnquiryDate as date),103)[EnquiryDate],
qt.PayTerms,qts.SlNo,It.MeasureUnit AS [UOM],It.ItemCode,It.ItemName,It.HSNCode,qts.Qty,qts.UnitPrice,qts.LineDiscountPercent,
qts.LineBasicAmount,qt.TotalGrossAmount,qt.FreightCharges,qt.TotalTaxable,qt.CGstRate,qt.TotalCGSTAmount,qt.SGstRate,
qt.TotalSGSTAmount,qt.IGstRate,qt.TotalIGSTAmount,qt.OtherCharges,qt.RoundOff,qt.GrandTotal,tc.Details,

 CASE WHEN qt.InsuranceAmtOrPer = 1 THEN C.Symbol
    ELSE CONCAT(CAST(qt.InsurancePercent AS decimal(10,2)),' %') 
    END AS InsuranceSymbol,

    -- Amount column
Format( qt.InsuranceCharges ,'N2') as InsuranceAmount,

	 CASE WHEN qt.TCSAmtOrPer = 1 THEN c.Symbol
    ELSE CONCAT(CAST(qt.TCSPercent AS decimal(10,2)),' %') 
    END AS TcsSybol,

    -- Amount column
FORMAT(qt.TCSAmount, 'N2') AS TcsAmount,

CASE WHEN qt.PackingAmtOrPer = 1 THEN c.Symbol
    ELSE CONCAT(CAST(qt.PackingPercent AS decimal(10,2)),' %') 
    END AS PackSymbol,

    -- Amount column
Format( qt.PackingCharges ,'N2') as PackAmount,

CASE WHEN qt.DiscAmtOrPer = 1 THEN c.Symbol
    ELSE CONCAT(CAST(qt.DiscountPercent AS decimal(10,2)),' %') 
    END AS DiscSymbol,

    -- Amount column
Format( qt.DiscountAmount ,'N2') as DiscountAmount

FROM MfgQuote Qt
INNER JOIN MfgQuoteSub Qts ON Qt.QuoteId = Qts.QuoteId
INNER JOIN Item It ON Qts.ItemId = It.ItemId
INNER JOIN Customer Cs ON Qt.CustId = Cs.CustId
INNER JOIN Currency C ON C.CurrId = Qt.CurrId
LEFT JOIN TermsAndConditions Tc ON Qt.TermsId = Tc.Id
left join EnquirySalesSub ESS on ess.EnquirySubId = Qts.RefEnqSubId
left join EnquirySales es on es.EnquiryId = ess.EnquiryId
WHERE Qt.QuoteId = @QuoteId