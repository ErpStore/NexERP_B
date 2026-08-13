CREATE OR ALTER PROCEDURE [dbo].[Sp_RouteCardAnalysisDetails]
    @RCId INT
	
AS
BEGIN
 select ROW_NUMBER() OVER (ORDER BY rc.RCId) As SlNo,rc.RCId,rcs.SeqNo,p.ProcessName,
CASE WHEN rcs.VendorId>0 THEN 'SUBCONTRACT'
	 WHEN rcs.MachineId>0 THEN 'PRODUCTION LOG'
	 ELSE 'PRODUCTION-COMP' END [TransType],
rcs.TotalQty,rcs.IssuedQty,rcs.AccQty,rcs.RejQty,rcs.RewQty,
CASE WHEN rcs.VendorId>0 THEN CONCAT(sd.DcNo,sd.Suffix)
	 WHEN rcs.MachineId>0 THEN CONCAT(pl.LogNo,pl.Suffix)
	 ELSE CONCAT(pic.IssueNo,pic.Suffix) END [IssueNo],
CASE WHEN rcs.VendorId>0 THEN CONCAT(sg.GRNNo,sg.Suffix)
	 WHEN rcs.MachineId>0 THEN CONCAT(pl.LogNo,pl.Suffix)
	 ELSE CONCAT(prc.ReturnNo,prc.Suffix) END [GRNNo],
CASE WHEN rcs.VendorId>0 THEN CONCAT(ss.SCNNo,ss.Suffix)
	 WHEN rcs.MachineId>0 THEN CONCAT(pl.LogNo,pl.Suffix)
	 ELSE CONCAT(psc.SCNNo,psc.Suffix) END [SCNNo],CAST(rcs.ProcessStatus AS INT)[ProcessStatus]	 
	 from RouteCard rc 
inner join Routecardsub rcs 
on rc.RCId=rcs.RCId

left join ProductionLog pl
on pl.RCId=rc.RCId and rcs.processid=pl.ProcessId

left join SubConDcOutSub sds
on sds.RcSubId=rcs.RCSubId and rcs.processid=sds.ProcessId and TransType='Out'
left join SubConDcOut sd on sd.DcId=sds.Dcid
left join SubConGRNSub sgs on sgs.RefRcSubId=sds.RcSubId
left join SubConGRN sg on sg.GRNId=sgs.GRNId
left join SubConSCNSub sss on sss.Rcsubid=sgs.RefRcSubId
left join SubConSCN ss on ss.SCNId=sss.SCNId

left join ProductionIssueCompSub pics
on pics.RcSubId=rcs.RCSubId and rcs.processid=pics.ProcessId and pics.TransType='Out'
left join ProductionIssueComp pic on pic.IssueId=pics.Issueid
left join ProductionReturnCompSub prcs on prcs.RefRcSubId=pics.RcSubId
left join ProductionReturnComp prc on prc.ReturnId=prcs.ReturnId
left join ProductionSCNCompSub pscs on pscs.Rcsubid=prcs.RefRcSubId
left join ProductionSCNComp psc on psc.SCNId=pscs.SCNId

left join Process p on p.ProcessId=rcs.ProcessId
where rc.RCId=@RCId

END
