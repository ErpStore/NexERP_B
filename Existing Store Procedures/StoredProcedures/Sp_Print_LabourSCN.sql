CREATE OR ALTER Procedure Sp_Print_LabourSCN
@SCNID int
As

select c.CustName,c.CustAddr,c.GSTNo,c.PANNo,ls.SCNNo,convert(varchar,cast(ls.SCNDate as date),103)[SCNDate],
lg.GRNNo,convert(varchar,cast(lg.GRNDate as date),103)[GRNDate],lg.RefDcNo,convert(varchar,cast(lg.RefDcDate as date),103)[RefDcDate],
sa.StoreName as Addstore,si.StoreName as Issstore,ls.MainRemarks,lss.SlNo,i.ItemCode,i.ItemName,i.MeasureUnit,i.HSNCode,
lss.AcceptQty,lss.RejectQty,lss.ReworkQty,lss.UnitPrice,(lss.AcceptQty * lss.UnitPrice) as ItemAmount,lss.Remark,
cp.ContactPersonName,cp.PhoneNo
from LabourSCN ls join LabourSCNSub lss on ls.SCNId=lss.SCNId
join Customer c on c.CustId=ls.CustId
left join LabourGRNSub lgs on lgs.GRNSubId=lss.RefGRNSubId
left join LabourGRN lg on lg.GRNId=lgs.GRNId
left join Stores sa on sa.StoreId=ls.AddStoreId
left join Stores si on si.StoreId=ls.StoreIssId
left join Item i on i.ItemId=lss.ItemId
left join ContactPerson cp on cp.CustId=ls.CustId
where ls.SCNId=@SCNId
