CREATE OR ALTER   Procedure [dbo].[Sp_Print_PurchaseGRN]
@GRNId int
As

SELECT PO.PONo,CONVERT(varchar,CAST(PO.PODate as date),103)[PODate],PR.RefDcNo,v.VendorName,v.VendorAddress,v.GSTNo,
v.PANNo,PR.GRNNo,CONVERT(varchar,CAST(PR.GRNDate as date),103)[GRNDate],PR.RefDcNo,convert(varchar,cast(PR.RefDcDate as date),103)[RefDcDate],
PR.RefInvNo,Convert(varchar,cast(PR.RefInvDate as date),103)[RefInvDate],
PRS.SlNo,i.ItemCode,i.ItemName,i.MeasureUnit,prs.DNQty,(prs.Qty*prs.UnitPrice) as ItemAmount,prs.ExtraQty,
prs.Remark,prs.Qty,prs.UnitPrice,vc.ContactPerson,v.ContactNo,s.StoreName,pr.MainRemarks
FROM PurchaseGRN PR INNER JOIN PurchaseGRNSub PRS ON PRS.GRNId = PR.GRNId 
join Vendor v on v.VendorCode=PR.VendorCode
join item i on i.ItemId=PRS.ItemId
LEFT JOIN PurchPoSub POS ON POS.POSubId = PRS.RefPOSubId
left join VendorContact vc on vc.VendorCode=v.VendorCode
left join Stores s on s.StoreId=pr.AddStoreId
LEFT JOIN PurchPo PO ON PO.PoId = POS.PoId
WHERE PR.GRNId=@GRNId 
