CREATE OR ALTER   Procedure [dbo].[Sp_Print_ProdAssyGRN]
@ReturnId int
As

select j.JobNo,pg.ReturnNo,pg.ReturnDate,s.StoreName,pgs.SlNo,i.ItemCode,i.ItemName,i.MeasureUnit,
pgs.UnitPrice,pgs.Remarks,pgs.QtyReturned,pgs.BalQty,(pgs.UnitPrice*pgs.QtyReturned) as ItemAmount,pg.ReturnBy
from ProductionReturnAssy pg join ProductionReturnAssySub pgs on pg.ReturnId=pgs.ReturnId
LEFT JOIN JobOrder j ON j.JobId = pgs.RefJobOrderId
left join MfgPoSub mps on mps.PoSubId=j.RefPoSubId
left join MfgPo mp on mp.PoId = mps.PoId
left join Stores s on s.StoreId = pg.AddStoreId
left join Item i on i.ItemId = pgs.ItemId
where pg.ReturnId=@ReturnId
