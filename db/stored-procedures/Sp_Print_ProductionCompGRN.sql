CREATE OR ALTER   Procedure [dbo].[Sp_Print_ProductionCompGRN]
@ReturnId int
As

select rc.RCNo,CONCAT(pr.ReturnNo, ' ', pr.Suffix) AS ReturnNo,Convert(Varchar,Cast(pr.ReturnDate as date),103)ReturnDate,
prs.SlNo,i.ItemCode,i.ItemName,i.MeasureUnit,prs.UnitPrice,prs.Remarks,prs.qty,prs.BalQty,
CAST(ROUND(ISNULL(prs.UnitPrice,0) * ISNULL(prs.Qty,0), 2) AS DECIMAL(18,2)) AS ItemAmount,pr.ReturnBy,s.StoreName,c.CustName,mp.PONo,pr.MainRemark,cc.CostCenterName
from ProductionReturnComp pr join ProductionReturnCompSub prs on pr.ReturnId=prs.ReturnId
LEFT JOIN RouteCardSub rcs ON rcs.RCSubId = prs.RefRcSubId
left join RouteCard rc on rc.RCId=rcs.RCId
left join Stores s on s.StoreId = pr.AddStoreId
left join Item i on i.ItemId = prs.ItemId
left join Customer c on c.CustId=pr.CustId
left join MfgPoSub mps on mps.PoSubId=prs.RefPoSubId
left join MfgPo mp on mp.PoId=mps.PoId
left join CostCenter cc on cc.CustId=pr.CustId
where pr.ReturnId=@ReturnId
