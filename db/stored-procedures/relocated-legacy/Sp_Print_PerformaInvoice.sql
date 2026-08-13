
Create Or ALTER Procedure [dbo].[Sp_Print_PerformaInvoice]
@InvId int
As

SELECT  Concat(pn.Prefix, ' ',pn.InvNo, ' ',pn.suffix) as InvNo, Convert(Varchar,cast(pn.InvDate as date),103)[InvDate],pns.RefPoNo,convert(varchar,cast(pns.RefPoDate as date),103)[RefPoDate],
c.CustName,c.CustAddr,c.VendorCode,c.GSTNo,c.PANNo,c.ContactNo,c.StateId,i.ItemCode,i.ItemName,i.MeasureUnit,i.HSNCode,
pns.DueDate,pns.Qty,pns.UnitPrice,(pns.Qty*pns.UnitPrice) as ItemAmount,pn.TotalGrossAmount,pn.TotalBasicAmount,
pn.OtherCharges,pn.TotalTaxable,pn.GrandTotal,pn.FreightCharges,cu.currid,pns.slno,cu.CurrSub,cu.Symbol,pn.MainRemark,
pn.CGstRate,pn.TotalCGSTAmount,pn.SGstRate,pn.TotalSGSTAmount,pn.IGstRate,pn.TotalIGSTAmount,pns.LineDiscountPercent,
pn.RoundOff,pn.PayTerms,pn.DeliveryTerms,
  CASE WHEN pn.InsuranceAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(pn.InsurancePercent AS decimal(10,2)),' %') 
    END AS InsuranceSymbol,

    -- Amount column
Format( pn.InsuranceCharges ,'N2') as InsuranceAmount,

	 CASE WHEN pn.TCSAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(pn.TCSPercent AS decimal(10,2)),' %') 
    END AS TcsSybol,

    -- Amount column
FORMAT(pn.TCSAmount, 'N2') AS TcsAmount,

CASE WHEN pn.PackingAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(pn.PackingPercent AS decimal(10,2)),' %') 
    END AS PackSymbol,

    -- Amount column
Format( pn.PackingCharges ,'N2') as PackAmount,

CASE WHEN pn.DiscAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(pn.DiscountPercent AS decimal(10,2)),' %') 
    END AS DiscSymbol,

    -- Amount column
Format( pn.DiscountAmount ,'N2') as DiscountAmount,

   CASE When CI.AltCustId IS NOT NULL 
	     Then CI.AltCustName
	ELSE C.CustName
	END As PrintCustName,
	CASE When CI.AltCustId IS NOT NULL 
	     Then CONCAT(CI.AltCustAddr1,CHAR(10),CI.AltCustAddr2,',',CI.City,',',CI.Pincode)
	ELSE C.CustAddr
	END As PrintCustAddr,
	CASE When CI.ContactNo Is Not Null 
	     Then CI.ContactNo 
	Else C.ContactNo
	End As PrintContactNo,
	CASE When CI.GSTNo Is Not Null 
	     Then CI.GSTNo 
	Else C.GSTNo
	End As PrintGSTNo,
	CASE When CI.PANNo Is Not Null 
	     Then CI.PANNo 
	Else C.PANNo
	End As PrintPANNo,
	CASE When CI.State Is Not Null 
	     Then CI.State 
	Else C.StateId
	End As PrintState

FROM PerformaInv pn join PerformaInvSub pns on pn.InvId=pns.InvId
join Customer c on c.CustId = pn.CustId
join item i on i.ItemId=pns.ItemId
left join Currency cu on cu.CurrId=pn.CurrId
LEFT JOIN CustomerIndirect CI ON CI.CustId=PN.CustId
where pn.InvId=@InvId ORDER BY pns.SlNo
