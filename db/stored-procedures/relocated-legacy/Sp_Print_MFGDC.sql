Create Or ALTER Procedure [dbo].[Sp_Print_MFGDC]
@DCID int
As

select dcs.slno,i.ItemCode,i.ItemName,i.MeasureUnit,i.HSNCode,dcs.Qty,dcs.UnitPrice,c.CustName,c.CustAddr,c.VendorCode,
c.GSTNo,c.PANNo,c.ContactNo,c.StateId,Concat(dc.Prefix, ' ',dc.DcNo, ' ',dc.Suffix) as DcNo,dc.DcDate,dc.VehicleNo,dcs.RefPoNo,dcs.RefPoDate,dc.EwayBillNo,
(dcs.Qty*dcs.UnitPrice) as ItemAmount,dcs.Remark,dc.MainRemarks,ci.AltCustName,CONCAT(ci.AltCustAddr1, ' ', ci.AltCustAddr2) AS ConsigneeAddress,
ci.GSTNo as congst,ci.ContactNo,ci.State,ci.PANNo as congpan,c.StateName,
CASE    
 WHEN dc.ModeofTrasnport = '1'
        Then 'ByRoad'
 WHEN dc.ModeofTrasnport = '2'
        Then 'ByRailway'
 WHEN dc.ModeofTrasnport = '3'
        Then 'ByAirway'
 WHEN dc.ModeofTrasnport = '4'
        Then 'BySeaway'
 ELSE ''
END AS TransMode
from MfgDc dc join MfgDcSub dcs on dc.DcId=dcs.DcId
join item i on i.ItemId=dcs.ItemId
join Customer c on c.CustId = dc.CustId
left join CustomerIndirect ci on ci.CustId=dc.CustId
where dc.DcId=@DCID
ORDER BY SlNo