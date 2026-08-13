CREATE OR ALTER      PROCEDURE [dbo].[Sp_GetTaxDetailsReport]

    @CustId INT = NULL,
    @VendorCode INT = NULL,
    @Topic VARCHAR(50),
	@FromDate DATE,
    @ToDate DATE
	AS
BEGIN

    SET NOCOUNT ON;

    /* =========================================
       SALES + LABOUR
    ========================================= */

    IF @Topic = 'Sales'
    BEGIN

        SELECT
            'Sales' AS InvType,
            mis.SlNo,
			mi.Invid As InvId,
            mi.InvNo+mi.Suffix as InvNo,
            CAST(mi.InvDate AS DATE) AS InvDate,
            c.CustName,
            c.GSTNo,
            mi.ACKNO,
            mi.IRNNo,
			'' as Process,
            mp.PONo,
            CAST(mp.PODate AS DATE) AS PODate,
			ISNULL(mps.[LineNo],0) as [LineNo],
            md.DCNO+md.Suffix AS RefDCNo,
            CAST(md.DcDate AS DATE) AS RefDcDate,
            i.ItemCode,
            i.ItemName,
            i.Specification,
            i.MeasureUnit,
            i.HSNCode,
            mis.Qty,

            mi.CGstRate,
            mi.SGstRate,
            mi.IGstRate,

            mi.TotalCGSTAmount,
            mi.TotalSGSTAmount,
            mi.TotalIGSTAmount,

            mi.TotalBasicAmount,
            mi.GrandTotal,

            mi.DiscountPercent,
            mi.DiscountAmount,

            mis.UnitPrice,

            mis.LineCGSTRate,
            mis.LineSGSTRate,
            mis.LineIGSTRate,

            mis.LineDiscountPercent,
            mis.LineDiscountAmount,
			mis.LineBasicAmount,
		 --  CASE WHEN i.IsLabourItem = 1 THEN i.LabourTimeMinutes END AS [Labour_Time],
			--CASE WHEN i.IsLabourItem = 1 THEN (i.LabourTimeMinutes*mis.Qty) END AS [Labour_Time_Spent] 
			li.AssemblyPrice,
			li.LabourPrice,
			li.LabourTimeMinutes,
			null as LabourGrandTotal
			
			

        FROM MfgInv mi

        LEFT JOIN MfgInvSub mis ON mis.InvId = mi.InvId

        INNER JOIN Item i ON i.ItemId = mis.ItemId

        INNER JOIN Customer c ON c.CustId = mi.CustId

     
        LEFT JOIN MfgDcSub mds ON mds.DcSubId = mis.RefDcSubId

        LEFT JOIN MfgDc md ON md.DcId = mds.DcId
		  LEFT JOIN MfgPoSub mps
		ON mps.PoSubId = COALESCE(mis.RefPoSubId, mds.RefPoSubId)


        LEFT JOIN MfgPo mp ON mp.PoId = mps.PoId


		 OUTER APPLY
		(
			SELECT TOP (1)
				i.AssemblyPrice,
				i.LabourPrice,
				i.LabourTimeMinutes
			FROM AssmblyDefLabour al
			INNER JOIN Item i
				ON i.ItemId = al.ItemId
			   AND i.IsLabourItem = 1
			WHERE al.AssmblyId = mps.ItemId
			ORDER BY al.ItemId
		) li


        WHERE (@CustId IS NULL OR @CustId = 0 OR mi.CustId = @CustId)
		AND mi.InvDate >= @FromDate
        AND mi.InvDate < DATEADD(DAY, 1, @ToDate)


        UNION ALL

        SELECT
            'LABOUR' AS InvType,
            lis.SlNo,
			li.LabInvId As InvId,
            li.LabInvNo+li.Suffix AS InvNo,
            CAST(li.LabInvDate AS DATE) AS InvDate,
            c.CustName,
            c.GSTNo,
            li.ACKNO,
            li.IRNNo,
			p.ProcessName as Process,
            mp.PONo AS PONo,
            CAST(mp.PODate AS DATE) AS PODate,
			ISNULL(mfg.[LineNo],0) as [LineNo],
            ld.DcNo AS RefDCNo,
            CAST(ld.DcDate AS DATE) AS RefDcDate,
            i.ItemCode,
            i.ItemName,
            i.Specification,
            i.MeasureUnit,
            i.HSNCode,
            lis.Qty,

            li.CGstRate,
            li.SGstRate,
            li.IGstRate,

            li.TotalCGSTAmount,
            li.TotalSGSTAmount,
            li.TotalIGSTAmount,

            li.TotalBasicAmount,
            li.GrandTotal,

            li.DiscountPercent,
            li.DiscountAmount,

            lis.UnitPrice,

            lis.LineCGSTRate,
            lis.LineSGSTRate,
            lis.LineIGSTRate,

            lis.LineDiscountPercent,
            lis.LineDiscountAmount,
			lis.LineBasicAmount,
			--CASE WHEN i.IsLabourItem = 1 THEN i.LabourTimeMinutes END AS [Labour_Time],
			--CASE WHEN i.IsLabourItem = 1 THEN (i.LabourTimeMinutes*lis.Qty) END AS [Labour_Time_Spent]
			i.LabourPrice,
			i.AssemblyPrice,
			i.LabourTimeMinutes,
			null as LabourGrandTotal
			


        FROM LabInv li

        LEFT JOIN LabInvSub lis ON lis.LabInvId = li.LabInvId

        INNER JOIN Item i ON i.ItemId = lis.ItemId

        INNER JOIN Customer c ON c.CustId = li.CustId

        LEFT JOIN LabourGRNSub lgs ON lgs.GRNSubId = lis.RefDcSubId

        LEFT JOIN LabourGRN lg ON lg.GRNId = lgs.GRNId

        LEFT JOIN LabourDcOutgoingSub lds ON lds.DcSubId = lis.RefDcSubId

        LEFT JOIN LabourDcOutgoing ld ON ld.DcId = lds.DcId

      LEFT JOIN MfgPoSub mfg ON mfg.PoSubId = COALESCE(lds.RefPoSubId, lgs.RefPoSubId)
    LEFT JOIN MfgPo mp ON mp.PoId = mfg.PoId


		LEFT JOIN Process p on p.ProcessId=li.ProcessId

        WHERE (@CustId IS NULL OR @CustId = 0 OR li.CustId = @CustId)
		AND li.LabInvDate >= @FromDate
        AND li.LabInvDate < DATEADD(DAY, 1, @ToDate)

    END

    /* =========================================
       EXPORT
    ========================================= */

    ELSE IF @Topic = 'Exports'
    BEGIN

        SELECT
            'EXPORT' AS InvType,
            eis.SlNo,
			ei.ExpInvId As InvId,
            ei.ExpInvNo+ei.Suffix AS InvNo,
            CAST(ei.ExpInvDate AS DATE) AS InvDate,
            c.CustName,
            c.GSTNo,
            ei.ACKNO,
            ei.IRNNo,
			'' as Process,
            mp.PONo,
            CAST(mp.PODate AS DATE) AS PODate,
			ISNULL(mps.[LineNo],0) as [LineNo],
            md.DCNO+md.Suffix AS RefDCNo,
            CAST(md.DcDate AS DATE) AS RefDcDate,
            i.ItemCode,
            i.ItemName,
            i.Specification,
            i.MeasureUnit,
            i.HSNCode,
            eis.Qty,

            ei.CGstRate,
            ei.SGstRate,
            ei.IGstRate,

            ei.TotalCGSTAmount,
            ei.TotalSGSTAmount,
            ei.TotalIGSTAmount,

            ei.TotalBasicAmount,
            ei.GrandTotal,

            ei.DiscountPercent,
            ei.DiscountAmount,

            eis.UnitPrice,

            eis.LineCGSTRate,
            eis.LineSGSTRate,
            eis.LineIGSTRate,

            eis.LineDiscountPercent,
            eis.LineDiscountAmount,
			eis.LineBasicAmount,
		i.AssemblyPrice,
			i.LabourPrice,
			i.LabourTimeMinutes,
			null as LabourGrandTotal

        FROM ExpInv ei

        LEFT JOIN ExpInvSub eis ON eis.ExpInvId = ei.ExpInvId

        INNER JOIN Item i ON i.ItemId = eis.ItemId

        INNER JOIN Customer c ON c.CustId = ei.CustId

        LEFT JOIN MfgPoSub mps ON mps.PoSubId = eis.RefPoSubId

        LEFT JOIN MfgPo mp ON mp.PoId = mps.PoId

        LEFT JOIN MfgDcSub mds ON mds.DcSubId = eis.RefDcSubId

        LEFT JOIN MfgDc md ON md.DcId = mds.DcId

        WHERE (@CustId IS NULL OR @CustId = 0 OR ei.CustId = @CustId)
		AND ei.ExpInvDate >= @FromDate
        AND ei.ExpInvDate < DATEADD(DAY, 1, @ToDate)


    END

    /* =========================================
       PURCHASE + SUBCONTRACT
    ========================================= */

    ELSE IF @Topic = 'Purchase'
    BEGIN

        SELECT
            'PURCHASE' AS InvType,
            psis.SlNo,
			psi.InvId As InvId,
            psi.InvNo+psi.Suffix as InvNo,
            CAST(psi.InvDate AS DATE) AS InvDate,
            v.VendorName AS CustName,
            v.GSTNo,
            NULL AS ACKNO,
            NULL AS IRNNo,
			'' as Process,
            mp.PONo,
            CAST(mp.PODate AS DATE) AS PODate,
			0 as [LineNo],
            pg.GRNNo+pg.Suffix AS RefDCNo,
            CAST(pg.GRNDate AS DATE) AS RefDcDate,
            i.ItemCode,
            i.ItemName,
            i.Specification,
            i.MeasureUnit,
            i.HSNCode,
            psis.Qty,

            psi.CGstRate,
            psi.SGstRate,
            psi.IGstRate,

            psi.TotalCGSTAmount,
            psi.TotalSGSTAmount,
            psi.TotalIGSTAmount,

            psi.TotalBasicAmount,
            psi.GrandTotal,

            psi.DiscountPercent,
            psi.DiscountAmount,

            psis.UnitPrice,

            psis.LineCGSTRate,
            psis.LineSGSTRate,
            psis.LineIGSTRate,

            psis.LineDiscountPercent,
            psis.LineDiscountAmount,
			psis.LineBasicAmount,
			i.AssemblyPrice,
			i.LabourPrice,
			i.LabourTimeMinutes,
			null as LabourGrandTotal
        FROM PurchaseInvoice psi

        LEFT JOIN PurchaseInvoiceSub psis ON psis.InvId = psi.InvId

        INNER JOIN Item i ON i.ItemId = psis.ItemId

        INNER JOIN Vendor v ON v.VendorCode = psi.VendorCode

        LEFT JOIN PurchPoSub mps ON mps.PoSubId = psis.RefPoSubId

        LEFT JOIN PurchPo mp ON mp.PoId = mps.PoId

        LEFT JOIN PurchaseGRNSub pgs ON pgs.GRNSubId = psis.RefPoSubId

        LEFT JOIN PurchaseGRN pg ON pg.GRNId = pgs.GRNId

        WHERE (@VendorCode IS NULL OR @VendorCode = 0 OR psi.VendorCode = @VendorCode)
		AND psi.InvDate >= @FromDate
        AND psi.InvDate < DATEADD(DAY, 1, @ToDate)


        UNION ALL

        SELECT
            'SUBCONTRACT' AS InvType,
            scis.SlNo,
			sci.InvId As InvId,
            sci.InvNo+sci.Suffix as InvNo,
            CAST(sci.InvDate AS DATE) AS InvDate,
            v.VendorName AS CustName,
            v.GSTNo,
            NULL AS ACKNO,
            NULL AS IRNNo,
			'' as Process,
            po.PONo,
            CAST(po.PODate AS DATE) AS PODate,
			0 as [LineNo],
            sg.GRNNo+sg.Suffix AS RefDCNo,
            CAST(sg.GRNDate AS DATE) AS RefDcDate,
            i.ItemCode,
            i.ItemName,
            i.Specification,
            i.MeasureUnit,
            i.HSNCode,
            scis.Qty,

            sci.CGstRate,
            sci.SGstRate,
            sci.IGstRate,

            sci.TotalCGSTAmount,
            sci.TotalSGSTAmount,
            sci.TotalIGSTAmount,

            sci.TotalBasicAmount,
            sci.GrandTotal,

            sci.DiscountPercent,
            sci.DiscountAmount,

            scis.UnitPrice,

            scis.LineCGSTRate,
            scis.LineSGSTRate,
            scis.LineIGSTRate,

            scis.LineDiscountPercent,
            scis.LineDiscountAmount,
			scis.LineBasicAmount,
			i.AssemblyPrice,
			i.LabourPrice,
			i.LabourTimeMinutes,
			null as LabourGrandTotal


        FROM SubConInv sci

        LEFT JOIN SubConInvSub scis ON scis.InvId = sci.InvId

        INNER JOIN Item i ON i.ItemId = scis.ItemId

        INNER JOIN Vendor v ON v.VendorCode = sci.VendorCode

        LEFT JOIN PurchPoSub pos ON pos.PoSubId = scis.RefSCNSubId

        LEFT JOIN PurchPo po ON po.PoId = pos.PoId

        LEFT JOIN SubConGRNSub sgs ON sgs.GRNSubId = scis.RefSCNSubId

        LEFT JOIN SubConGRN sg ON sg.GRNId = sgs.GRNId

        WHERE (@VendorCode IS NULL OR @VendorCode = 0 OR sci.VendorCode = @VendorCode)
		AND sci.InvDate >= @FromDate
        AND sci.InvDate < DATEADD(DAY, 1, @ToDate)

    END

END

--AS
--BEGIN

--    SET NOCOUNT ON;

--    /* =========================================
--       SALES + LABOUR
--    ========================================= */

--    IF @Topic = 'Sales'
--    BEGIN

--        SELECT
--            'Sales' AS InvType,
--            mis.SlNo,
--			mi.Invid As InvId,
--            mi.prefix + mi.InvNo+mi.Suffix as InvNo,
--            CAST(mi.InvDate AS DATE) AS InvDate,
--            c.CustName,
--            c.GSTNo,
--            mi.ACKNO,
--            mi.IRNNo,
--			'' as Process,
--            mp.PONo,
--            CAST(mp.PODate AS DATE) AS PODate,
--			ISNULL(mps.[LineNo],0) as [LineNo],
--            md.DCNO+md.Suffix AS RefDCNo,
--            CAST(md.DcDate AS DATE) AS RefDcDate,
			
--            i.ItemCode,
--            i.ItemName,
--            i.Specification,
--            i.MeasureUnit,
--            i.HSNCode,
--            mis.Qty,

--            mi.CGstRate,
--            mi.SGstRate,
--            mi.IGstRate,

--            mi.TotalCGSTAmount,
--            mi.TotalSGSTAmount,
--            mi.TotalIGSTAmount,

--            mi.TotalBasicAmount,
--            mi.GrandTotal,

--            mi.DiscountPercent,
--            mi.DiscountAmount,

--            mis.UnitPrice,

--            mis.LineCGSTRate,
--            mis.LineSGSTRate,
--            mis.LineIGSTRate,

--            mis.LineDiscountPercent,
--            mis.LineDiscountAmount,
--			mis.LineBasicAmount
			

--        FROM MfgInv mi

--		LEFT JOIN MfgInvSub mis
--			ON mis.InvId = mi.InvId

--		INNER JOIN Item i
--			ON i.ItemId = mis.ItemId

--		INNER JOIN Customer c
--			ON c.CustId = mi.CustId

--		LEFT JOIN MfgDcSub mds
--			ON mds.DcSubId = mis.RefDcSubId

--		LEFT JOIN MfgDc md
--			ON md.DcId = mds.DcId

--		LEFT JOIN MfgPoSub mps
--			ON mps.PoSubId = COALESCE(mis.RefPoSubId, mds.RefPoSubId)

--		LEFT JOIN MfgPo mp
--			ON mp.PoId = mps.PoId

--        WHERE (@CustId IS NULL OR @CustId = 0 OR mi.CustId = @CustId)
--		AND mi.InvDate >= @FromDate
--        AND mi.InvDate < DATEADD(DAY, 1, @ToDate) AND mi.IsCancel=0 AND mi.PoShortClose=0



--        UNION ALL

--        SELECT
--            'LABOUR' AS InvType,
--            lis.SlNo,
--			li.LabInvId As InvId,
--            li.prefix+li.LabInvNo+li.Suffix AS InvNo,
--            CAST(li.LabInvDate AS DATE) AS InvDate,
--            c.CustName,
--            c.GSTNo,
--            li.ACKNO,
--            li.IRNNo,
--			p.ProcessName as Process,
--            mp.PONo AS PONo,
--            CAST(mp.PODate AS DATE) AS PODate,
--			ISNULL(mfg.[LineNo],0) as [LineNo],
--            ld.DcNo AS RefDCNo,
--            CAST(ld.DcDate AS DATE) AS RefDcDate,
--            i.ItemCode,
--            i.ItemName,
--            i.Specification,
--            i.MeasureUnit,
--            i.HSNCode,
--            lis.Qty,

--            li.CGstRate,
--            li.SGstRate,
--            li.IGstRate,

--            li.TotalCGSTAmount,
--            li.TotalSGSTAmount,
--            li.TotalIGSTAmount,

--            li.TotalBasicAmount,
--            li.GrandTotal,

--            li.DiscountPercent,
--            li.DiscountAmount,

--            lis.UnitPrice,

--            lis.LineCGSTRate,
--            lis.LineSGSTRate,
--            lis.LineIGSTRate,

--            lis.LineDiscountPercent,
--            lis.LineDiscountAmount,
--			lis.LineBasicAmount
			


--        FROM LabInv li

--        LEFT JOIN LabInvSub lis ON lis.LabInvId = li.LabInvId

--        INNER JOIN Item i ON i.ItemId = lis.ItemId

--        INNER JOIN Customer c ON c.CustId = li.CustId

--        LEFT JOIN LabourGRNSub lgs ON lgs.GRNSubId = lis.RefDcSubId

--        LEFT JOIN LabourGRN lg ON lg.GRNId = lgs.GRNId

--        LEFT JOIN LabourDcOutgoingSub lds ON lds.DcSubId = lis.RefDcSubId

--        LEFT JOIN LabourDcOutgoing ld ON ld.DcId = lds.DcId

--        --LEFT JOIN MfgPoSub mfg ON mfg.PoSubId = lds.RefPoSubId
--		LEFT JOIN MfgPoSub mfg ON mfg.PoSubId = COALESCE(lds.RefPoSubId, lgs.RefPoSubId)

--		LEFT JOIN MfgPo mp ON mp.PoId = mfg.PoId

--		LEFT JOIN Process p on p.ProcessId=li.ProcessId

--        WHERE (@CustId IS NULL OR @CustId = 0 OR li.CustId = @CustId)
--		AND li.LabInvDate >= @FromDate 
--        AND li.LabInvDate < DATEADD(DAY, 1, @ToDate) AND li.InvCancel=0 AND li.ShortClose=0

--    END

--    /* =========================================
--       EXPORT
--    ========================================= */

--    ELSE IF @Topic = 'Exports'
--    BEGIN

--        SELECT
--            'EXPORT' AS InvType,
--            eis.SlNo,
--			ei.ExpInvId As InvId,
--            ei.ExpInvNo+ei.Suffix AS InvNo,
--            CAST(ei.ExpInvDate AS DATE) AS InvDate,
--            c.CustName,
--            c.GSTNo,
--            ei.ACKNO,
--            ei.IRNNo,
--			'' as Process,
--            mp.PONo,
--            CAST(mp.PODate AS DATE) AS PODate,
--			ISNULL(mps.[LineNo],0) as [LineNo],
--            md.DCNO+md.Suffix AS RefDCNo,
--            CAST(md.DcDate AS DATE) AS RefDcDate,
--            i.ItemCode,
--            i.ItemName,
--            i.Specification,
--            i.MeasureUnit,
--            i.HSNCode,
--            eis.Qty,

--            ei.CGstRate,
--            ei.SGstRate,
--            ei.IGstRate,

--            ei.TotalCGSTAmount,
--            ei.TotalSGSTAmount,
--            ei.TotalIGSTAmount,

--            ei.TotalBasicAmount,
--            ei.GrandTotal,

--            ei.DiscountPercent,
--            ei.DiscountAmount,

--            eis.UnitPrice,

--            eis.LineCGSTRate,
--            eis.LineSGSTRate,
--            eis.LineIGSTRate,

--            eis.LineDiscountPercent,
--            eis.LineDiscountAmount,
--			eis.LineBasicAmount

--        FROM ExpInv ei

--        LEFT JOIN ExpInvSub eis ON eis.ExpInvId = ei.ExpInvId

--        INNER JOIN Item i ON i.ItemId = eis.ItemId

--        INNER JOIN Customer c ON c.CustId = ei.CustId

		
--        LEFT JOIN MfgDcSub mds ON mds.DcSubId = eis.RefDcSubId 

--        LEFT JOIN MfgDc md ON md.DcId = mds.DcId

		
--        LEFT JOIN MfgPoSub mps ON mps.PoSubId = COALESCE(mds.RefPoSubId, eis.RefPoSubId)

--        LEFT JOIN MfgPo mp ON mp.PoId = mps.PoId

--        WHERE (@CustId IS NULL OR @CustId = 0 OR ei.CustId = @CustId)
--		AND ei.ExpInvDate >= @FromDate
--        AND ei.ExpInvDate < DATEADD(DAY, 1, @ToDate)

--    END

--    /* =========================================
--       PURCHASE + SUBCONTRACT
--    ========================================= */

--    ELSE IF @Topic = 'Purchase'
--    BEGIN

--        SELECT
--            'PURCHASE' AS InvType,
--            psis.SlNo,
--			psi.InvId As InvId,
--            psi.InvNo+psi.Suffix as InvNo,
--            CAST(psi.InvDate AS DATE) AS InvDate,
--            v.VendorName AS CustName,
--            v.GSTNo,
--            NULL AS ACKNO,
--            NULL AS IRNNo,
--			'' as Process,
--            mp.PONo,
--            CAST(mp.PODate AS DATE) AS PODate,
--			0 as [LineNo],
--            pg.GRNNo+pg.Suffix AS RefDCNo,
--            CAST(pg.GRNDate AS DATE) AS RefDcDate,
--            i.ItemCode,
--            i.ItemName,
--            i.Specification,
--            i.MeasureUnit,
--            i.HSNCode,
--            psis.Qty,

--            psi.CGstRate,
--            psi.SGstRate,
--            psi.IGstRate,

--            psi.TotalCGSTAmount,
--            psi.TotalSGSTAmount,
--            psi.TotalIGSTAmount,

--            psi.TotalBasicAmount,
--            psi.GrandTotal,

--            psi.DiscountPercent,
--            psi.DiscountAmount,

--            psis.UnitPrice,

--            psis.LineCGSTRate,
--            psis.LineSGSTRate,
--            psis.LineIGSTRate,

--            psis.LineDiscountPercent,
--            psis.LineDiscountAmount,
--			psis.LineBasicAmount

--        FROM PurchaseInvoice psi

--        LEFT JOIN PurchaseInvoiceSub psis ON psis.InvId = psi.InvId

--        INNER JOIN Item i ON i.ItemId = psis.ItemId

--        INNER JOIN Vendor v ON v.VendorCode = psi.VendorCode

--        LEFT JOIN PurchPoSub mps ON mps.PoSubId = psis.RefPoSubId

--        LEFT JOIN PurchPo mp ON mp.PoId = mps.PoId

--        LEFT JOIN PurchaseGRNSub pgs ON pgs.GRNSubId = psis.RefPoSubId

--        LEFT JOIN PurchaseGRN pg ON pg.GRNId = pgs.GRNId

--        WHERE (@VendorCode IS NULL OR @VendorCode = 0 OR psi.VendorCode = @VendorCode)
--		AND psi.InvDate >= @FromDate
--        AND psi.InvDate < DATEADD(DAY, 1, @ToDate) AND psi.InvCancel=0 AND psi.ShortClose=0


--        UNION ALL

--        SELECT
--            'SUBCONTRACT' AS InvType,
--            scis.SlNo,
--			sci.InvId As InvId,
--            sci.InvNo+sci.Suffix as InvNo,
--            CAST(sci.InvDate AS DATE) AS InvDate,
--            v.VendorName AS CustName,
--            v.GSTNo,
--            NULL AS ACKNO,
--            NULL AS IRNNo,
--			'' as Process,
--            po.PONo,
--            CAST(po.PODate AS DATE) AS PODate,
--			0 as [LineNo],
--            sg.GRNNo+sg.Suffix AS RefDCNo,
--            CAST(sg.GRNDate AS DATE) AS RefDcDate,
--            i.ItemCode,
--            i.ItemName,
--            i.Specification,
--            i.MeasureUnit,
--            i.HSNCode,
--            scis.Qty,

--            sci.CGstRate,
--            sci.SGstRate,
--            sci.IGstRate,

--            sci.TotalCGSTAmount,
--            sci.TotalSGSTAmount,
--            sci.TotalIGSTAmount,

--            sci.TotalBasicAmount,
--            sci.GrandTotal,

--            sci.DiscountPercent,
--            sci.DiscountAmount,

--            scis.UnitPrice,

--            scis.LineCGSTRate,
--            scis.LineSGSTRate,
--            scis.LineIGSTRate,

--            scis.LineDiscountPercent,
--            scis.LineDiscountAmount,
--			scis.LineBasicAmount

--        FROM SubConInv sci

--        LEFT JOIN SubConInvSub scis ON scis.InvId = sci.InvId

--        INNER JOIN Item i ON i.ItemId = scis.ItemId

--        INNER JOIN Vendor v ON v.VendorCode = sci.VendorCode

--        LEFT JOIN PurchPoSub pos ON pos.PoSubId = scis.RefSCNSubId

--        LEFT JOIN PurchPo po ON po.PoId = pos.PoId

--        LEFT JOIN SubConGRNSub sgs ON sgs.GRNSubId = scis.RefSCNSubId

--        LEFT JOIN SubConGRN sg ON sg.GRNId = sgs.GRNId

--        WHERE (@VendorCode IS NULL OR @VendorCode = 0 OR sci.VendorCode = @VendorCode)
--		AND sci.InvDate >= @FromDate
--        AND sci.InvDate < DATEADD(DAY, 1, @ToDate) AND sci.InvCancel=0

--    END

--END

