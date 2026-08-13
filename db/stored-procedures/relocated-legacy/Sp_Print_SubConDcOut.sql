Create Or ALTER Procedure [dbo].[Sp_Print_SubConDcOut]
@DcId int
As
select dc.VehicleNo,dc.EwayBillNumber,v.VendorName,v.VendorAddress,v.VendorCode,v.StateCode,v.GSTNo,v.PANNo,v.ContactNo,
dcs.SlNo,i.ItemCode,i.ItemName,i.MeasureUnit,i.HSNCode,dcs.Qty,dcs.UnitPrice,(dcs.Qty*dcs.UnitPrice) as ItemAmount,
dcs.Remark,Concat(dc.Prefix, ' ',dc.DcNo, ' ',dc.suffix) as DcNo,Convert(Varchar,cast(dc.DcDate as date),103)[DcDate],pp.PONo,Convert(Varchar,Cast(pp.PODate as date),103)[PODate],dc.MainRemark,
CASE 
 WHEN dc.ModeOfTransportId = '1'
        Then 'ByRoad'
 WHEN dc.ModeOfTransportId = '2'
        Then 'ByRailway'
 WHEN dc.ModeOfTransportId = '3'
        Then 'ByAirway'
 WHEN dc.ModeOfTransportId = '4'
        Then 'BySeaway'
 ELSE ''
END AS TransMode

from SubConDcOut dc join SubConDcOutSub dcs on dc.DcId = dcs.DcId
Left Join Vendor v on v.VendorCode = dc.VendorCode
Left join item i on i.ItemId=dcs.ItemId
left join PurchPoSub pps on pps.PoSubId = dcs.RefPoSubId
left join PurchPo pp on pp.PoId = pps.PoId
where dc.DcId=@DcId 