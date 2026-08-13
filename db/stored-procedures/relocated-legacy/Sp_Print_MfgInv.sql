Create Or ALTER Procedure [dbo].[Sp_Print_MfgInv]
@InvId int
As

select Concat(mi.Prefix, ' ',mi.InvNo, ' ',mi.Suffix) as InvNo,convert(varchar,cast(mi.InvDate as date),103)[InvDate],md.DcNo,Convert(varchar,cast(md.DcDate as date),103)[DcDate],
si.StoreName as issuestore,md.VehicleNo,md.TransFrom,md.TransTo,mi.MainRemark,c.CustName,c.CustAddr,c.VendorCode,
c.StateId,c.GSTNo,c.PANNo,c.ContactNo,ci.AltCustName,concat(ci.AltCustAddr1,' ',ci.AltCustAddr2) as ConsigneeAddress,
ci.State as CongState,ci.GSTNo as CongGst,ci.PANNo as CongPan,ci.ContactNo as CongContactNo,mis.SlNo,i.ItemCode,
i.ItemName,i.MeasureUnit,i.HSNCode,mis.Qty,mis.UnitPrice,mis.LineDiscountPercent,mis.LineBasicAmount,cu.CurrName,
cu.Symbol,mi.FreightCharges,mi.OtherCharges,mi.RoundOff,mi.TotalGrossAmount,mi.TotalBasicAmount,mi.TotalTaxable,
mi.GrandTotal,mi.CGstRate,mi.TotalCGSTAmount,mi.SGstRate,mi.TotalSGSTAmount,mi.IGstRate,mi.TotalIGSTAmount,
Concat(mi.LcNo, ' ',mi.LcDate) as LcLd,mi.ACKNO,mi.ACKNODate,mi.IRNNo,mi.SignedInvoiceQrCode,mi.EWayNo,mp.PONo,
convert(varchar,cast(mp.PODate as date),103)[PODate],c.StateName,ci.ContactPerson,
CASE 
 WHEN md.ModeofTrasnport = '1'
        Then 'ByRoad'
 WHEN md.ModeofTrasnport = '2'
        Then 'ByRailway'
 WHEN md.ModeofTrasnport = '3'
        Then 'ByAirway'
 WHEN md.ModeofTrasnport = '4'
        Then 'BySeaway'
 ELSE ''
END AS TransMode,
  CASE WHEN mi.InsuranceAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(mi.InsurancePercent AS decimal(10,2)),' %') 
    END AS InsuranceSymbol,

    -- Amount column
Format( mi.InsuranceCharges ,'N2') as InsuranceAmount,

	 CASE WHEN mi.TCSAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(mi.TCSPercent AS decimal(10,2)),' %') 
    END AS TcsSybol,

    -- Amount column
FORMAT(mi.TCSAmount, 'N2') AS TcsAmount,

CASE WHEN mi.PackingAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(mi.PackingPercent AS decimal(10,2)),' %') 
    END AS PackSymbol,

    -- Amount column
Format( mi.PackingCharges ,'N2') as PackAmount,

CASE WHEN mi.DiscAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(mi.DiscountPercent AS decimal(10,2)),' %') 
    END AS DiscSymbol,

    -- Amount column
Format( mi.DiscountAmount ,'N2') as DiscountAmount

from MfgInv mi join MfgInvSub mis on mi.InvId=mis.InvId
left join MfgDcSub mds on mds.DcSubId=mis.RefDcSubId
left join MfgDc md on md.DcId=mds.DcId
left join Stores si on si.StoreId=md.IssueStoreId
left join Customer c on c.CustId=mi.CustId
left join CustomerIndirect ci on ci.CustId=mi.CustId
left join Item i on i.ItemId=mis.ItemId
left join Currency cu on cu.CurrId=mi.CurrId
left join MfgPoSub mps on mps.PoSubId=mis.RefPoSubId
left join MfgPo mp on mp.PoId=mps.PoId
where mi.InvId=@InvId