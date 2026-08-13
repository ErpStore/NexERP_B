CREATE OR ALTER PROCEDURE [dbo].[Sp_RouteCardAnalysisSummary]
(
 --   @CustId INT NULL,
	--@RCId INT NULL,
	--@Status INT = -1,
    @FromDate DATETIME,
    @ToDate DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;
    
SELECT 
    distinct(rc.RCId),
	--CAST(ROW_NUMBER() OVER (ORDER BY rc.RCNo) AS INT) As SlNo,
	CAST(DENSE_RANK() OVER (ORDER BY rc.RCId) AS INT) As SlNo,
	rc.CustId,
	CASE 
	WHEN mp.MfgOrLab=1 THEN 'Mfg'
	WHEN mp.MfgOrLab=0 THEN 'Lab'
	ELSE 'N/A' END AS MfgOrLab,
	--c.CustName As Customer,
	CASE 
    WHEN rc.IsManual = 1 THEN 'Manual'
    ELSE ISNULL(c.CustName,'N/A')
	END AS Customer,
	CONCAT(mp.PONo,mp.Suffix) as PONo,
	Convert(varchar,cast(mp.PODate as date),103) AS PODate,
    CONCAT(rc.RCNo,rc.Suffix) as RCNo,
	rc.Suffix,
	Convert(varchar,cast(rc.RCDate as date),103) AS RCDate,
    rc.IsManual,

    ISNULL(i2.ItemCode, '-') AS Assembly,
    i.ItemCode AS Component,
	rc.RcQty as RCQty,
	i3.ItemCode AS RM,
	rc.RMReqQty as RMQty,
    CAST(rc.RcStatus AS INT) AS RcStatus,
 ------------------------------------------------
    -- DC Details
    ------------------------------------------------
    CASE 
        WHEN mp.MfgOrLab = 1 
            THEN CONCAT(md.DCNo, md.Suffix)

        WHEN mp.MfgOrLab = 0 
            THEN CONCAT(ld.DCNo, ld.Suffix)

		ELSE CONCAT(md.DCNo, md.Suffix)
    END AS DCNo,


    CASE 
        WHEN mp.MfgOrLab = 1 
            THEN CONVERT(varchar, CAST(md.DcDate AS date), 103)

        WHEN mp.MfgOrLab = 0 
            THEN CONVERT(varchar, CAST(ld.DcDate AS date), 103)

		ELSE CONVERT(varchar, CAST(md.DcDate AS date), 103)
    END AS DCDate,


    ------------------------------------------------
    -- Invoice Details
    ------------------------------------------------
    CASE 
        WHEN mp.MfgOrLab = 1 
            THEN CONCAT(mi.InvNo, mi.Suffix)

        WHEN mp.MfgOrLab = 0 
            THEN CONCAT(li.LabInvNo, li.Suffix)

		ELSE CONCAT(mi.InvNo, mi.Suffix)

    END AS InvNo,


    CASE 
        WHEN mp.MfgOrLab = 1 
            THEN CONVERT(varchar, CAST(mi.InvDate AS date), 103)

        WHEN mp.MfgOrLab = 0 
            THEN CONVERT(varchar, CAST(li.LabInvDate AS date), 103)

		ELSE CONVERT(varchar, CAST(mi.InvDate AS date), 103)
    END AS InvDate

FROM RouteCard rc

INNER JOIN RouteCardSub rcs 
    ON rc.RCId = rcs.RCId

LEFT JOIN Customer c 
    ON c.CustId = rc.CustId

LEFT JOIN Item i 
    ON i.ItemId = rc.CompItemId

LEFT JOIN Item i2 
    ON i2.ItemId = rc.AssyId

LEFT JOIN Item i3 
    ON i3.ItemId = rc.RMId

/* PO directly linked with RouteCard */
LEFT JOIN MfgPo mp 
    ON mp.PoId = rc.RefPoId

LEFT JOIN MfgPoSub mps 
    ON mps.PoId = mp.PoId 

/* DC may or may not exist */
LEFT JOIN MfgDcSub mds 
    ON mds.RefPoSubId = mps.PoSubId

LEFT JOIN MfgDc md 
    ON md.DcId = mds.DcId

LEFT JOIN LabourDcOutgoingSub lds 
    ON lds.RefPoSubId = mps.PoSubId

LEFT JOIN LabourDcOutgoing ld 
    ON ld.DCId = lds.DCId

/* Invoice may or may not exist */
LEFT JOIN MfgInvSub mis 
    ON mis.RefDcSubId = mds.DcSubId
       OR mis.RefPoSubId = mps.PoSubId

LEFT JOIN MfgInv mi
    ON mi.InvId = mis.InvId

LEFT JOIN LabInvSub lis 
    ON lis.RefDcSubId = lds.DcSubId

LEFT JOIN LabInv li
    ON li.LabInvId = lis.LabInvId


--WHERE

--    CAST(rc.RCDate AS DATE) >= CAST(@FromDate AS DATE)

--    AND CAST(rc.RCDate AS DATE) <= CAST(@ToDate AS DATE)

--    AND (@CustId = 0 OR rc.CustId = @CustId)

--    AND (@RCId = 0 OR rc.RCId = @RCId)

WHERE
    rc.RCDate >= CAST(@FromDate AS DATE)
    AND rc.RCDate < DATEADD(DAY,1,CAST(@ToDate AS DATE))

--    AND 
--    (
--        @CustId = 0 
--        OR (@CustId = -1 AND rc.IsManual = 1)
--        OR (@CustId > 0 AND rc.CustId = @CustId)
--    )

--    AND (@RCId = 0 OR rc.RCId = @RCId)

--	AND
--    (
--        @Status = -1 OR rc.RcStatus = @Status
--    )

ORDER BY rc.RCId


END
