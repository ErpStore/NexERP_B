Create or ALTER PROCEDURE [dbo].[Sp_Print_PurchaseOrder]
    @PoId INT
AS
BEGIN

    SELECT 
        v.VendorName,v.VendorAddress,v.VendorCode,v.GSTNo,v.PANNo,v.ContactNo,v.CustomerCode,concat(P.Prefix, ' ',p.pono,  ' ' ,p.Suffix) as PoNo,
		Convert(varchar,cast(p.PODate as date),103)[PODate],concat(pq.QuoteNo, ' ', pq.Suffix) as QuoteNo,Convert(Varchar,Cast(pq.QuoteDate as date),103)[QuoteDate],
		concat(mq.MReqNo, ' ',mq.Suffix) as MReqNo,convert(varchar,cast(mq.MreqDate as date),103)[MreqDate],ps.SlNo,
		i.ItemCode,i.ItemName,i.HSNCode,ps.Qty,i.MeasureUnit,ps.DueDate,ps.UnitPrice,(ps.Qty*ps.UnitPrice) as ItemAmount,
		ps.LineBasicAmount,ps.LineDiscountPercent,tac.Details
		
	FROM purchpo p 
    INNER JOIN PurchPoSub ps ON p.PoId = ps.PoId
    INNER JOIN vendor v ON v.VendorCode = p.VendorCode
    INNER JOIN item i ON i.ItemId = ps.ItemId
    INNER JOIN Currency C ON C.CurrId = p.CurrId
    LEFT JOIN TermsAndConditions Tc ON p.TermsId = Tc.Id
	left join PurchaseQuoteSub PQS on PQS.QuoteSubId = ps.RefQuoteSubId
	left join PurchaseQuote pq on pq.QuoteId = pqs.QuoteId
	left join MaterialReqSub mqs on mqs.MreqSubId = ps.RefMReqSubId
	left join MaterialReq mq on mq.MreqId = mqs.MreqId
	left join TermsAndConditions tac on tc.Id = p.TermsId

    WHERE p.PoId = @PoId

    ORDER BY ps.SlNo

END