CREATE OR ALTER   Procedure [dbo].[Sp_Print_CREDITNOTE]
@CrId int
As

select Cns.SlNo,i.itemname,i.ItemCode,c.CustName,Cn.CreatedDate,Cn.TotalGrossAmount,Cn.InvType,Cn.TotalBasicAmount,
Cn.CGstRate,Cn.SGstRate,Cn.TotalCGSTAmount,Cns.Qty,Cns.RejQty,Cns.CrDrQty,Cns.CrDrUnitPrice,Cn.CrId,
  CONCAT(Cn.Prefix,Cn.CreditNo,Cn.Suffix)[CreditNo],Cns.LineDiscountAmount,Cns.LineBasicAmount,
  Cns.UnitPrice,Cns.RewQty,Cns.LineGross,Cn.FreightCharges,Cn.TotalTaxable,
  Cn.IGstRate,Cn.OtherCharges,Cn.RoundOff,Cn.GrandTotal,c.CustAddr,cu.Symbol,Cn.TotalSGSTAmount,
  Cn.TotalIGSTAmount,Cn.ACKNO,
  CASE WHEN Cn.ACKNODate is null Then '' 
  ELSE CONVERT(varchar,cast(Cn.ACKNODate as date),103) 
  END AS ACKNODate,
  Cn.IRNNo,
  CASE WHEN Cn.InsuranceAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(Cn.InsurancePercent AS decimal(10,2)),' %') 
    END AS InsuranceSymbol,

    -- Amount column
Format( Cn.InsuranceCharges ,'N2') as InsuranceAmount,

	 CASE WHEN Cn.TCSAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(Cn.TCSPercent AS decimal(10,2)),' %') 
    END AS TcsSybol,

    -- Amount column
FORMAT(Cn.TCSAmount, 'N2') AS TcsAmount,

CASE WHEN Cn.PackingAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(Cn.PackingPercent AS decimal(10,2)),' %') 
    END AS PackSymbol,

    -- Amount column
Format( Cn.PackingCharges ,'N2') as PackAmount,

CASE WHEN Cn.DiscAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(Cn.DiscountPercent AS decimal(10,2)),' %') 
    END AS DiscSymbol,

    -- Amount column
Format( Cn.DiscountAmount ,'N2') as DiscountAmount

from CreditNote Cn
join CreditNoteSub Cns on Cn.CrId=Cns.CrId
left join Customer c on c.CustId=Cn.CustId
left join CustomerIndirect ci on ci.CustId=Cn.CustId
left join item i on i.ItemId=Cns.ItemId
left join Currency cu on cu.CurrId=Cn.CurrId
where Cn.CrId=@CrId
