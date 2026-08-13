
CREATE OR ALTER PROCEDURE [dbo].[Sp_Print_DebitNote]
@Dbid int
AS
BEGIN

select i.ItemCode, CONCAT(dn.Prefix,dn.DebitNo,dn.Suffix) AS [DebitNo],V.VendorName,v.VendorAddress,dn.DebitDate,dn.CreatedBy,
dn.CreatedDate,dn.ACKNO,
CASE WHEN dn.ACKNODate is null then '' 
Else CONVERT(varchar,cast(dn.ACKNoDate as date),103) END As ACKNoDate,dn.IRNNo,dn.GrandTotal,dns.LineBasicAmount,dns.LineCGSTRate,dns.LineDiscountPercent,
dns.LineDiscountAmount,dns.LineSGSTRate,dns.Qty,dns.SlNo,dns.UnitPrice,dns.RejQty,dns.RewQty,dns.CrDrQty,dns.CrDrUnitPrice,dn.totalGrossAmount,
dn.CgstRate,dn.sgstRate,dn.igstrate,dn.totalcgstAmount,dn.freightCharges,dn.OTHERcHARGES,dn.ROUNDOFF,dn.TotalSGSTAmount,
dn.TotalIGSTAmount,dn.TotalTaxable,cu.Symbol,

 CASE WHEN dn.InsuranceAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(dn.InsurancePercent AS decimal(10,2)),' %') 
    END AS InsuranceSymbol,

    -- Amount column
Format(dn.InsuranceCharges ,'N2') as InsuranceAmount,

	 CASE WHEN dn.TCSAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(dn.TCSPercent AS decimal(10,2)),' %') 
    END AS TcsSybol,

    -- Amount column
FORMAT(dn.TCSAmount, 'N2') AS TcsAmount,

CASE WHEN dn.PackingAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(dn.PackingPercent AS decimal(10,2)),' %') 
    END AS PackSymbol,

    -- Amount column
Format(dn.PackingCharges ,'N2') as PackAmount,

CASE WHEN dn.DiscAmtOrPer = 1 THEN cu.Symbol
    ELSE CONCAT(CAST(dn.DiscountPercent AS decimal(10,2)),' %') 
    END AS DiscSymbol,

    -- Amount column
Format(dn.DiscountAmount ,'N2') as DiscountAmount

from DebitNote dn left join DebitNoteSub dns on dn.DbId=dns.DbId
left join item i on i.ItemId=dns.ItemId
LEFT JOIN Vendor V ON V.VendorCode=dn.VendorCode
left join Currency cu on cu.CurrId=dn.CurrId
where dns.DbId=@Dbid
END
