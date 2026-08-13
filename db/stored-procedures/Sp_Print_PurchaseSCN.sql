CREATE OR ALTER   Procedure [dbo].[Sp_Print_PurchaseSCN]
@SCNID int
As

SELECT  v.VendorName,v.VendorAddress,v.GSTNo,v.PANNo,psub.SlNo,i.ItemCode,i.ItemName,i.MeasureUnit,pgs.DNQty,
pgs.Qty,pg.RefDcNo,pg.RefDcDate,pg.RefInvNo,pg.RefInvDate,pg.GRNNo,pg.GRNDate,p.SCNNo,p.SCNDate,psub.AcceptQty,
psub.RejectQty,psub.UnitPrice,(psub.AcceptQty * psub.UnitPrice) AS ItemAmount,sa.StoreName,
si.StoreName as StoreIssName,vc.ContactPerson,v.ContactNo,p.MainRemarks,psub.Remark,i.HSNCode
FROM PurchaseSCN p JOIN PurchaseSCNSub psub ON p.SCNId = psub.SCNId
JOIN Vendor v ON v.VendorCode = p.VendorCode
JOIN Item i ON i.ItemId = psub.ItemId
LEFT JOIN Stores sa ON sa.StoreId=p.AddStoreId
LEFT JOIN Stores si ON si.StoreId=p.StoreIssId
LEFT JOIN PurchaseGRN pg  ON pg.GRNNo = p.SCNNo   
left join VendorContact vc on vc.VendorCode=p.VendorCode
LEFT JOIN PurchaseGRNSub pgs ON pgs.GRNId = pg.GRNId AND pgs.ItemId = psub.ItemId
WHERE p.SCNId = @SCNID
