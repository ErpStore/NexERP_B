CREATE OR ALTER    Procedure [dbo].[Sp_Print_PurchasePo]
@PoId int
As

select pp.PONo,CONVERT(varchar,CAST(pp.PODate as date),103)[PODate],pq.QuoteNo,CONVERT(varchar,CAST(pq.QuoteDate as date),103)[QuoteDate],
v.VendorName,v.VendorAddress,v.GSTNo,v.PANNo,v.ContactNo,vc.ContactPerson,vi.AltVendorName,concat(vi.AltVendorAddr1,' ',vi.AltVendorAddr2) as Address,
vi.GSTNo,vi.PinCode,vi.PANNo,vi.ContactNo as VContactNo,vi.ContactPerson as VCOntactPerson,pps.SlNo,i.ItemCode,i.ItemName,i.MeasureUnit,
i.HSNCode,FORMAT(pps.DueDate, 'dd/MM/yy') AS DueDate,pps.Qty,pps.UnitPrice,pps.LineDiscountPercent,pps.LineDiscountAmount,
pp.TotalGrossAmount,pp.TotalBasicAmount,pp.TotalTaxable,pp.FreightCharges,pp.PackingCharges,cu.CurrSub,cu.Symbol,
pp.TotalTaxable,PPS.LineBasicAmount,pp.PayTerms,pp.DeliveryTerms,pp.MainRemark,pp.OtherCharges,pp.RoundOff,
pp.GrandTotal,pp.CGstRate,pp.TotalCGSTAmount,pp.SGstRate,pp.TotalSGSTAmount,pp.IGstRate,pp.TotalIGSTAmount,
TC.Details,mr.MReqNo,CONVERT(varchar,CAST(mr.MreqDate as date),103)[MreqDate],
  
  CASE WHEN pp.InsuranceAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(pp.InsurancePercent AS decimal(10,2)),' %') 
    END AS InsuranceSymbol,

    -- Amount column
Format( pp.InsuranceCharges ,'N2') as InsuranceAmount,

	 CASE WHEN pp.TCSAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(pp.TCSPercent AS decimal(10,2)),' %') 
    END AS TcsSybol,

    -- Amount column
FORMAT(pp.TCSAmount, 'N2') AS TcsAmount,

CASE WHEN pp.PackingAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(pp.PackingPercent AS decimal(10,2)),' %') 
    END AS PackSymbol,

    -- Amount column
Format( pp.PackingCharges ,'N2') as PackAmount,

CASE WHEN pp.DiscAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(pp.DiscountPercent AS decimal(10,2)),' %') 
    END AS DiscSymbol,

    -- Amount column
Format( pp.DiscountAmount ,'N2') as DiscountAmount

from PurchPo pp join PurchPoSub pps on pp.PoId = pps.PoId
JOIN Vendor v ON v.VendorCode = pp.VendorCode
JOIN Item i ON i.ItemId = pps.ItemId
LEFT JOIN TermsAndConditions TC ON TC.Id = PP.TermsId
left join VendorInDirect vi on vi.VendorCode=pp.VendorCode
left join Currency cu on cu.CurrId=pp.CurrId
left join MaterialReqSub mrs on mrs.MreqSubId=pps.RefMReqSubId
left join MaterialReq mr on mr.MreqId=mrs.MreqId
left join VendorContact vc on vc.VendorCode=v.VendorCode
left join PurchaseQuote pq on pq.QuoteId = (select top 1 QuoteId from PurchaseQuote where CurrId = pp.CurrId order by QuoteDate desc)
where pp.PoId = @PoId
