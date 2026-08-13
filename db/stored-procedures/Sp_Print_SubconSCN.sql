CREATE OR ALTER   Procedure [dbo].[Sp_Print_SubconSCN]
@SCNID int
As
select v.VendorName,v.VendorAddress,v.GSTNo,v.PANNo,dss.SlNo,i.ItemCode,i.ItemName,i.MeasureUnit,dss.UnitPrice,
dss.AccQty,dss.RejQty,ds.SCNNo,ds.SCNDate,ds.MainRemark
from DcSubConSCN ds join DcSubConSCNSub dss on ds.SCNId=dss.SCNId
join Vendor v on v.VendorCode = ds.VendorCode
join item i on i.ItemId=dss.ItemId
where ds.SCNId=@SCNID ORDER BY dss.SlNo
