CREATE OR ALTER Procedure Sp_Print_LabourInv
@LabInvId int
As
BEGIN

select 
    Concat(li.Prefix, ' ',li.LabInvNo, ' ',li.Suffix) as LabInvNo ,
    convert(varchar,cast(li.LabInvDate as date),103) as LabInvDate,

    -- 🔥 GRN Details Added
    gm.GrnNo,
    convert(varchar,cast(gm.GrnDate as date),103) as GrnDate,
    gs.ItemSpecification,

    ld.DcNo,
    convert(varchar,cast(ld.DcDate as date),103) as DcDate,
    
    lis.SlNo,
    c.CustName,
    c.CustAddr,
    c.VendorCode,
    c.StateId,
    c.GSTNo,
    c.PANNo,
    c.ContactNo,

    i.ItemCode,
    i.ItemName,
    i.MeasureUnit,
    i.HSNCode,

    mp.PONo,
    convert(varchar,cast(mp.PODate as date),103) as PODate,

    lis.Qty,
    lis.UnitPrice,
    lis.LineGross,
    lis.LineDiscountPercent,
    lis.LineBasicAmount,

    li.TotalGrossAmount,
    cu.CurrSub,
    cu.Symbol,

    li.OtherCharges,
    li.RoundOff,
    li.FreightCharges,
    li.TotalTaxable,
    li.CGstRate,
    li.TotalCGSTAmount,
    li.SGstRate,
    li.TotalSGSTAmount,
    li.IGstRate,
    li.TotalIGSTAmount,
    li.GrandTotal,
    li.TotalBasicAmount,
    li.MainRemark,
    convert(varchar,cast(li.ACKNODate as date),103) as ACKNODate,
	li.ACKNO,
	li.IRNNo,
	li.SignedInvoiceQrCode,
	ci.ContactPerson,
	c.StateName,

    CASE 
        WHEN ld.TransportMode = '1' THEN 'ByRoad'
        WHEN ld.TransportMode = '2' THEN 'ByRailway'
        WHEN ld.TransportMode = '3' THEN 'ByAirway'
        WHEN ld.TransportMode = '4' THEN 'BySeaway'
        ELSE ''
    END AS TransMode,

    CASE 
        WHEN li.InsuranceAmtOrPer = 1 THEN cu.Symbol
        ELSE CONCAT(CAST(li.InsurancePercent AS decimal(10,2)),' %') 
    END AS InsuranceSymbol,

    FORMAT(li.InsuranceCharges ,'N2') as InsuranceAmount,

    CASE 
        WHEN li.TCSAmtOrPer = 1 THEN cu.Symbol
        ELSE CONCAT(CAST(li.TCSPercent AS decimal(10,2)),' %') 
    END AS TcsSymbol,

    FORMAT(li.TCSAmount, 'N2') AS TcsAmount,

    CASE 
        WHEN li.PackingAmtOrPer = 1 THEN cu.Symbol
        ELSE CONCAT(CAST(li.PackingPercent AS decimal(10,2)),' %') 
    END AS PackSymbol,

    FORMAT(li.PackingCharges ,'N2') as PackAmount,

    CASE 
        WHEN li.DiscAmtOrPer = 1 THEN cu.Symbol
        ELSE CONCAT(CAST(li.DiscountPercent AS decimal(10,2)),' %') 
    END AS DiscSymbol,

    FORMAT(li.DiscountAmount ,'N2') as DiscountAmount,

    CASE 
        WHEN CI.AltCustId IS NOT NULL THEN CI.AltCustName
        ELSE C.CustName
    END As PrintCustName,

    CASE 
        WHEN CI.AltCustId IS NOT NULL 
        THEN CONCAT(CI.AltCustAddr1,CHAR(10),CI.AltCustAddr2,',',CI.City,',',CI.Pincode)
        ELSE C.CustAddr
    END As PrintCustAddr,

    CASE 
        WHEN CI.ContactNo IS NOT NULL THEN CI.ContactNo 
        ELSE C.ContactNo
    END As PrintContactNo,

    CASE 
        WHEN CI.GSTNo IS NOT NULL THEN CI.GSTNo 
        ELSE C.GSTNo
    END As PrintGSTNo,

    CASE 
        WHEN CI.PANNo IS NOT NULL THEN CI.PANNo 
        ELSE C.PANNo
    END As PrintPANNo,

    CASE 
        WHEN CI.State IS NOT NULL THEN CI.State 
        ELSE C.StateId
    END As PrintState,

        -- FINAL OUTPUT
    ISNULL(REFDC.RefDcDetails, '') AS RefDcDetails,
    ISNULL(REFQTY.RefQtyDetails, '') AS RefQtyDetails,
    ISNULL(REFBATCH.RefBatchDetails, '') AS RefBatchDetails

from LabInv li

join LabInvSub lis 
    on li.LabInvId = lis.LabInvId

-- 🔥 ADD THIS BLOCK (GRN LINK)
left join LabourDcReturnCompTrack rt 
    on lis.RefDcSubId = rt.RefDcSubId

left join LabourGRNSub gs 
    on rt.RefGrnSubId = gs.GrnSubId

left join LabourGRN gm 
    on gs.GrnId = gm.GrnId

-- Existing joins
left join LabourDcOutgoingSub lds 
    on lds.DcSubId = lis.RefDcSubId

left join LabourDcOutgoing ld 
    on ld.DcId = lds.DcId

left join Customer c 
    on c.CustId = li.CustId

left join CustomerIndirect ci 
    on ci.CustId = ld.CustId

left join item i 
    on i.ItemId = lis.ItemId

left join MfgPoSub mps 
    on mps.PoSubId = lds.RefPoSubId

left join MfgPo mp 
    on mp.PoId = mps.PoId

left join Currency cu 
    on cu.CurrId = li.CurrId

--------------------------------------------------
-- GRN APPLY
--------------------------------------------------
OUTER APPLY
(
    SELECT TOP 1
        gm.GrnNo,
        CONVERT(VARCHAR, CAST(gm.GrnDate AS DATE), 103) AS GrnDate,
        gm.RefDcNo,
        CONVERT(VARCHAR, CAST(gm.RefDcDate AS DATE), 103) AS RefDcDate
    FROM LabourDcReturnCompTrack rt
    JOIN LabourGRNSub gs ON rt.RefGrnSubId = gs.GrnSubId
    JOIN LabourGRN gm ON gs.GrnId = gm.GrnId
    WHERE rt.RefDcSubId = lis.RefDcSubId
) GRN

--------------------------------------------------
-- REF DC
--------------------------------------------------
OUTER APPLY
(
    SELECT 
        STUFF((
            SELECT DISTINCT 
                CHAR(10) +
                'PO: ' + CAST(mp2.PONo AS VARCHAR) +
                ' IDC: ' + CAST(gm2.RefDcNo AS VARCHAR) +
                ' (' + CONVERT(VARCHAR, gm2.RefDcDate, 103) + ')'
            FROM LabourDcReturnCompTrack rt2
            JOIN LabourGRNSub gs2 ON rt2.RefGrnSubId = gs2.GrnSubId
            JOIN LabourGRN gm2 ON gs2.GrnId = gm2.GrnId
            LEFT JOIN LabourDcOutgoingSub lds2 ON lds2.DcSubId = rt2.RefDcSubId
            LEFT JOIN MfgPoSub mps2 ON mps2.PoSubId = lds2.RefPoSubId
            LEFT JOIN MfgPo mp2 ON mp2.PoId = mps2.PoId
            WHERE rt2.RefDcSubId = lis.RefDcSubId
            FOR XML PATH(''), TYPE
        ).value('.', 'NVARCHAR(MAX)'),1,1,'') AS RefDcDetails
) REFDC

--------------------------------------------------
-- REF QTY
--------------------------------------------------
OUTER APPLY
(
    SELECT 
        STRING_AGG(CAST(gs2.Qty AS VARCHAR), CHAR(10)) AS RefQtyDetails
    FROM LabourDcReturnCompTrack rt2
    JOIN LabourGRNSub gs2 ON rt2.RefGrnSubId = gs2.GrnSubId
    WHERE rt2.RefDcSubId = lis.RefDcSubId
) REFQTY

--------------------------------------------------
-- REF BATCH
--------------------------------------------------
OUTER APPLY
(
    SELECT 
        STRING_AGG('Batch No: ' + CAST(gm2.GrnNo AS VARCHAR), CHAR(10)) AS RefBatchDetails
    FROM LabourDcReturnCompTrack rt2
    JOIN LabourGRNSub gs2 ON rt2.RefGrnSubId = gs2.GrnSubId
    JOIN LabourGRN gm2 ON gs2.GrnId = gm2.GrnId
    WHERE rt2.RefDcSubId = lis.RefDcSubId
) REFBATCH

where li.LabInvId = @LabInvId

END