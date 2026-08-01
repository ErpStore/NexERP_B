CREATE OR ALTER Procedure Sp_Print_LabourGRN
@GRNID int
As

select c.CustName,c.CustAddr,c.GSTNo,c.PANNo,lg.GRNNo,convert(varchar,cast(lg.GRNDate as date),103)[GRNDate],
lg.RefDcNo,convert(varchar,cast(lg.RefDcDate as date),103)[RefDcDate],mp.PONo,convert(varchar,cast(mp.PODate as date),103)[PODate],
lgs.SlNo,i.ItemCode,i.ItemName,i.MeasureUnit,i.HSNCode,lgs.Qty,lgs.UnitPrice,(lgs.Qty * lgs.UnitPrice) as ItemAmount,LGS.RowRemarks,
LG.Remarks,lgs.dcqty,lgs.ItemSpecification
from LabourGRN lg join LabourGRNSub lgs on lg.GRNId=lgs.GRNId
join Customer c on c.CustId=lg.CustId
left join LabourDcOutgoing ld on ld.DcNo=lg.RefDcNo
left join MfgPoSub mps ON mps.PoSubId=lgs.RefPoSubId
left join MfgPo mp on mp.PoId=mps.PoId
left join Item i on i.ItemId=lgs.ItemId
where lg.GRNId=@GRNID and lgs.TransType='In'