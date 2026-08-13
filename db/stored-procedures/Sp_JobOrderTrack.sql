 CREATE OR ALTER    Procedure [dbo].[Sp_JobOrderTrack]
     @Type nvarchar(20),   -- 1 = MfgPo, 2 = Other
	 @Fromdate datetime,
	 @Todate datetime
As
IF (@Type = 'Manufacturing')
BEGIN
    SELECT 
	0  AS SlNo,
	    C.CustName AS CustName,
        CONCAT(mf.PoNo, mf.Suffix) AS PoNo,
        mf.PODate [PODate],mfs.duedate AS Duedate,mfs.[LineNo] As LineId ,

        case when s.DepartmentCode is not null then
		 CONCAT(s.DepartmentCode,'/' ,j.JobNo, j.Suffix) else
		 CONCAT(s.DepartmentCode ,j.JobNo, j.Suffix) end AS JobNo,
        j.JobDate,

        j.JobOrderQty AS JobQty,
        j.JobBalQty AS JobBalQty,

        ii.ItemCode AS Assembly,
        i.ItemCode AS Item,

        CONCAT(PA.IssueId, PA.Suffix) AS AssemblyIssNo,PA.IssueDate As AssemblyIssDate,
        PAS.IssueQty AS IssQty,
        PAS.BalQty AS IssBalQty,

        CONCAT(PRA.ReturnNo, PRA.Suffix) AS GRNNo,PRA.ReturnDate As GRNDate,iii.ItemCode as ReturnItem,
        PRAS.QtyReturned AS ReturnQty,
        PRAS.BalQty AS ReturnBalQty,


        CONCAT(PSA.SCNNo, PSA.Suffix) AS AssSCNNo,PSA.SCNDate AS AssSCNDate,
        PSAS.AccQty AS AccQty,
        PSAS.RewQty AS ReworkQty,
        PSAS.RejQty AS RejQty,
        PSAS.BalQty AS SCNBalQty,

        CONCAT(mfg.DcNo, mfg.Suffix) AS DcNo,
		mfg.Dcdate As DcDate,
        CONCAT(mfgi.InvNo, mfgi.Suffix) AS InvNo,
         mfgi.InvDate As InvDate,
        NULL AS LabourGrnNo,
        NULL AS LabourScnNo,
        NULL AS DcOutgoingNo,
        NULL AS LabInvNo,
		NULL As LabDCDate,
		NULL As LabInvDate,
		DATEDIFF(DAY,j.JobDate, PRA.ReturnDate) AS DelayDays
		--,CAST(NULL AS DATETIME) AS Fromdate,CAST(NULL AS DATETIME) AS Todate,NULL AS BOMType
    FROM MfgPo mf
    LEFT JOIN MfgPoSub mfs ON mf.PoId = mfs.PoId
    LEFT JOIN JobOrder j ON j.RefPoId = mf.PoId and j.refposubid=mfs.posubid
    LEFT JOIN JobOrderSub js ON j.JobId = js.JobId
	
   LEFT JOIN ProductionIssueAssysub PAS ON j.JobId = PAS.RefJobId and js.JobSubId=RefJobOrderSubId
    LEFT JOIN ProductionIssueAssy PA ON PA.IssueId = PAS.IssueId
	LEFT JOIN Item i ON PAS.ItemId = i.ItemId
   -- LEFT JOIN ProductionReturnAssySub PRAS ON PRAS.RefJobOrderId = PAS.RefJobId 
   LEFT JOIN ProductionReturnAssySub PRAS ON PRAS.RefJobOrderId = j.JobId
	left join item iii on PRAS.itemid=iii.ItemId
    LEFT JOIN ProductionReturnAssy PRA ON PRA.ReturnId = PRAS.ReturnId
    LEFT JOIN ProductionSCNAssySub PSAS ON PSAS.RefReturnSubId = PRAS.ReturnSubId
    LEFT JOIN ProductionSCNAssy PSA ON PSA.SCNId = PSAS.SCNId

    LEFT JOIN Item ii ON j.AssyId = ii.ItemId
    LEFT JOIN MfgDcSub mfgdcsub ON mfgdcsub.ItemId = mfs.ItemId  and mf.PONo=mfgdcsub.RefPoNo  and mfs.posubid=mfgdcsub.refposubid
    LEFT JOIN MfgDc mfg ON mfg.DcId = mfgdcsub.DcId  and mf.CustId=mfg.CustId
    LEFT JOIN MfgInvSub mfgis ON mfgis.ItemId = mfgdcsub.ItemId and mfgis.refdcsubid=mfgdcsub.dcsubid
    LEFT JOIN MfgInv mfgi ON mfgi.InvId = mfgis.InvId
	left join Customer C on mf.CustId=c.CustId
	left Join staff s on s.staffid=j.staffid

	 where mf.MfgORLab=1 and j.JobDate between @Fromdate and @Todate
    GROUP BY 
        mf.PoNo, mf.Suffix, mf.PODate,
        j.JobNo, j.Suffix, j.JobDate,
        js.ReqQty, js.BalQty,
        ii.ItemCode, i.ItemCode,
        PA.IssueId, PA.Suffix,
        PAS.IssueQty, PAS.BalQty,
        PRA.ReturnNo, PRA.Suffix,
        PRAS.QtyReturned, PRAS.BalQty,
        PSA.SCNNo, PSA.Suffix,
        PSAS.AccQty, PSAS.RewQty, PSAS.RejQty, PSAS.BalQty,
        mfg.DcNo, mfg.Suffix,mfg.Dcdate,mfs.[LineNo],PA.IssueDate,PRA.ReturnDate,
        mfgi.InvNo, mfgi.Suffix,iii.ItemCode,j.JobBalQty,j.JobOrderQty,C.CustName,s.DepartmentCode,mfs.duedate, mfgi.InvDate,PRA.ReturnDate,PSA.SCNDate
END

 ELSE IF (@Type = 'Labour')
BEGIN
    SELECT 
	0 AS SlNo,
	 C.CustName AS CustName,
        CONCAT(mf.PoNo, mf.Suffix) AS PoNo,
        mf.PODate ,mfs.duedate AS Duedate,mfs.[LineNo] As LineId ,

        case when s.DepartmentCode is not null then
		 CONCAT(s.DepartmentCode,'/' ,j.JobNo, j.Suffix) else
		 CONCAT(s.DepartmentCode ,j.JobNo, j.Suffix) end AS JobNo,
        j.JobDate,

        j.JobOrderQty AS JobQty,
        j.JobBalQty AS JobBalQty,

        ii.ItemCode AS Assembly,
        i.ItemCode AS Item,

        CONCAT(PA.IssueId, PA.Suffix) AS AssemblyIssNo,PA.IssueDate As AssemblyIssDate,
        PAS.IssueQty AS IssQty,
        PAS.BalQty AS IssBalQty,

        CONCAT(PRA.ReturnNo, PRA.Suffix) AS GRNNo,PRA.ReturnDate As GRNDate,iii.ItemCode as ReturnItem,
        PRAS.QtyReturned AS ReturnQty,
        PRAS.BalQty AS ReturnBalQty,

        CONCAT(PSA.SCNNo, PSA.Suffix) AS AssSCNNo,PSA.SCNDate AS AssSCNDate,
        PSAS.AccQty AS AccQty,
        PSAS.RewQty AS ReworkQty,
        PSAS.RejQty AS RejQty,
        PSAS.BalQty AS SCNBalQty,

        NULL AS DcNo,
		 NULL AS DcDate,
        NULL AS InvNo,NULL AS InvDate,

        CONCAT(la.GRNNo, la.Suffix) AS LabourGrnNo,
         CONCAT(lascn.SCNNo,lascn.Suffix) AS LabourScnNo,
        CONCAT(ldcout.DcNo, ldcout.Suffix) AS DcOutgoingNo,
		ldcout.Dcdate As LabDCDate,
        CONCAT(li.LabInvNo, li.Suffix) AS LabInvNo,
		li.LabInvDate As LabInvDate,
		DATEDIFF(DAY, j.JobDate, PRA.ReturnDate) AS DelayDays
		--,CAST(NULL AS DATETIME) AS Fromdate,CAST(NULL AS DATETIME) AS Todate, NULL AS BOMType
    FROM MfgPo mf
    LEFT JOIN MfgPoSub mfs ON mf.PoId = mfs.PoId 
    LEFT JOIN JobOrder j ON j.RefPoId = mf.PoId and j.refposubid=mfs.posubid
    LEFT JOIN JobOrderSub js ON j.JobId = js.JobId
    LEFT JOIN ProductionIssueAssysub PAS ON j.JobId = PAS.RefJobId and js.JobSubId=RefJobOrderSubId
    LEFT JOIN ProductionIssueAssy PA ON PA.IssueId = PAS.IssueId
    LEFT JOIN ProductionReturnAssySub PRAS ON PRAS.RefJobOrderId = j.JobId
	left join item iii on PRAS.itemid=iii.ItemId
    LEFT JOIN ProductionReturnAssy PRA ON PRA.ReturnId = PRAS.ReturnId
    LEFT JOIN ProductionSCNAssySub PSAS ON PSAS.RefReturnSubId = PRAS.ReturnSubId
    LEFT JOIN ProductionSCNAssy PSA ON PSA.SCNId = PSAS.SCNId
    LEFT JOIN Item i ON PAS.ItemId = i.ItemId
    LEFT JOIN Item ii ON j.AssyId = ii.ItemId
	left join LabourGRNSub ls on mfs.PoSubId =ls.RefPoSubId 
   left join LabourGRNSub lss on ls.GRNId =lss.GRNId and lss.GRNId=ls.GRNId  and lss.TransType<>'out'
    LEFT JOIN LabourGRN la ON la.GRNId = lss.GRNId   -- FIXED
    LEFT JOIN LabourSCNSub lscnsub ON lscnsub.ItemId = lss.ItemId and lscnsub.RefGRNSubId=lss.GRNSubId 
    LEFT JOIN LabourSCN lascn ON lascn.SCNId = lscnsub.SCNId  
    LEFT JOIN LabourDcOutgoingSub ldcouts ON  mfs.PoSubId=ldcouts.RefPoSubId 
    LEFT JOIN LabourDcOutgoing ldcout ON ldcout.DcId = ldcouts.DcId
    LEFT JOIN LabInvSub lisub ON lisub.ItemId = ldcouts.ItemId and lisub.RefDcSubId = ldcouts.DcSubId
    LEFT JOIN LabInv li ON lisub.LabInvId = li.LabInvId
	left join Customer C on mf.CustId=c.CustId
	left Join staff s on s.staffid=j.staffid
	where mf.MfgORLab=0 and j.JobDate between @Fromdate and @Todate
    GROUP BY 
        mf.PoNo, mf.Suffix, mf.PODate,
        j.JobNo, j.Suffix, j.JobDate,
        js.ReqQty, js.BalQty,
        ii.ItemCode, i.ItemCode,
        PA.IssueId, PA.Suffix,
        PAS.IssueQty, PAS.BalQty,
        PRA.ReturnNo, PRA.Suffix,
        PRAS.QtyReturned, PRAS.BalQty,
        PSA.SCNNo, PSA.Suffix,
        PSAS.AccQty, PSAS.RewQty, PSAS.RejQty, PSAS.BalQty,
        la.GRNNo, la.Suffix,ldcout.DcDate,mfs.[LineNo],PA.IssueDate,PRA.ReturnDate,
        iii.ItemCode,lascn.SCNNo,ldcout.DcNo, ldcout.Suffix,li.LabInvNo, li.Suffix,j.JobBalQty,j.JobOrderQty,lascn.Suffix,C.CustName ,s.DepartmentCode,mfs.duedate, li.labInvDate,PRA.ReturnDate,PSA.SCNDate
END
 ELSE IF (@Type = 'Manual')
BEGIN
     selecT 0  AS SlNo,
	  NULL AS CustName,
        CONCAT(MPO.PoNo, MPO.Suffix) AS PoNo,
      MPO.PODate [PODate],

      case when s.DepartmentCode is not null then
		 CONCAT(s.DepartmentCode,'/' ,j.JobNo, j.Suffix) else
		 CONCAT(s.DepartmentCode ,j.JobNo, j.Suffix) end AS JobNo,
        j.JobDate,

        j.JobOrderQty AS JobQty,
        j.JobBalQty AS JobBalQty,

        ii.ItemCode AS Assembly,
        i.ItemCode AS Item,

        CONCAT(PA.IssueId, PA.Suffix) AS AssemblyIssNo,PA.IssueDate As AssemblyIssDate,
        PAS.IssueQty AS IssQty,
        PAS.BalQty AS IssBalQty,

        CONCAT(PRA.ReturnNo, PRA.Suffix) AS GRNNo,PRA.ReturnDate As GRNDate,iii.ItemCode as ReturnItem,
        PRAS.QtyReturned AS ReturnQty,
        PRAS.BalQty AS ReturnBalQty,


        CONCAT(PSA.SCNNo, PSA.Suffix) AS AssSCNNo,PSA.SCNDate AS AssSCNDate,
        PSAS.AccQty AS AccQty,
        PSAS.RewQty AS ReworkQty,
        PSAS.RejQty AS RejQty,
        PSAS.BalQty AS SCNBalQty,

        CONCAT(M.DcNo, M.Suffix) AS DcNo,
		M.Dcdate As DcDate,
        CONCAT(MI.InvNo, MI.Suffix) AS InvNo,
        MI.InvDate As InvDate,
        NULL AS LabourGrnNo,
        NULL AS LabourScnNo,
        NULL AS DcOutgoingNo,
        NULL AS LabInvNo,
		NULL As LabDCDate,
		NULL As LabInvDate,
		DATEDIFF(DAY,j.JobDate, PRA.ReturnDate) AS DelayDays
		--,CAST(NULL AS DATETIME) AS Fromdate,CAST(NULL AS DATETIME) AS Todate,NULL AS BOMType
		from Joborder j
   left join JobOrderSub js   ON j.JobId = js.JobId
    LEFT JOIN ProductionIssueAssysub PAS ON j.JobId = PAS.RefJobId and js.JobSubId=RefJobOrderSubId
    LEFT JOIN ProductionIssueAssy PA ON PA.IssueId = PAS.IssueId
    LEFT JOIN ProductionReturnAssySub PRAS ON PRAS.RefJobOrderId = j.JobId
	left join item iii on PRAS.itemid=iii.ItemId
    LEFT JOIN ProductionReturnAssy PRA ON PRA.ReturnId = PRAS.ReturnId
    LEFT JOIN ProductionSCNAssySub PSAS ON PSAS.RefReturnSubId = PRAS.ReturnSubId
    LEFT JOIN ProductionSCNAssy PSA ON PSA.SCNId = PSAS.SCNId
    LEFT JOIN Item i ON PAS.ItemId = i.ItemId
    LEFT JOIN Item ii ON j.AssyId = ii.ItemId 
	LEFT JOIN StockAdd SA
        ON SA.SubItemRefID = PSAS.SCNSubId
       AND SA.ItemId = PSAS.ItemId
       AND SA.ScreenCode = 56
    LEFT JOIN StockIssueTrack SIT ON SIT.AddId = SA.AddId
    LEFT JOIN StockIssue SI ON SIT.IssueId = SI.IssueId
    LEFT JOIN MfgDcSub MDCS 
	ON SI.SubItemRefID = MDCS.DcSubId
       AND SI.ScreenCode = 42
	   LEFT JOIN MfgDc M ON MDCS.DcId = M.DcId
	   LEFT JOIN MfgInvSub MISub
        ON (MISub.InvSubId = SI.SubItemRefID AND SI.ScreenCode = 80)
        OR MISub.RefDcSubId = MDCS.DcSubId
    LEFT JOIN MfgInv MI ON MI.InvId = MISub.InvId
    LEFT JOIN MfgPoSub MPOSUB
        ON MPOSUB.PoSubId = MDCS.RefPoSubId
        OR MPOSUB.PoSubId = MISub.RefPoSubId
    LEFT JOIN MfgPo MPO ON MPO.PoId = MPOSUB.PoId
	left Join staff s on s.staffid=j.staffid
	where j.IsManual=1 and j.JobDate between @Fromdate and @Todate
    GROUP BY 
        MPO.PoNo, MPO.Suffix, MPO.PODate,
        j.JobNo, j.Suffix, j.JobDate,
        js.ReqQty, js.BalQty,
        ii.ItemCode, i.ItemCode,
        PA.IssueId, PA.Suffix,
        PAS.IssueQty, PAS.BalQty,
        PRA.ReturnNo, PRA.Suffix,
        PRAS.QtyReturned, PRAS.BalQty,
        PSA.SCNNo, PSA.Suffix,
        PSAS.AccQty, PSAS.RewQty, PSAS.RejQty, PSAS.BalQty,
        iii.ItemCode,j.JobBalQty,j.JobOrderQty,
		M.DcNo, M.Suffix,
        MI.InvNo, MI.Suffix,iii.ItemCode,s.DepartmentCode,PA.IssueDate,PRA.ReturnDate ,PSA.SCNDate,M.Dcdate,MI.InvDate
END

