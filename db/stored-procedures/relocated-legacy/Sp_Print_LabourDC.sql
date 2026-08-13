CREATE OR ALTER Procedure Sp_Print_LabourDC
@DcId INT
AS
BEGIN

SELECT 
    ld.DcNo,
    CONVERT(VARCHAR, CAST(ld.DcDate AS DATE), 103) AS DcDate,

    -- PO Details
    mp.PONo,
    CONVERT(VARCHAR, CAST(mp.PODate AS DATE), 103) AS PODate,

    -- GRN Details
    MAX(gm.GrnNo) AS GrnNo,
    CONVERT(VARCHAR, MAX(CAST(gm.GrnDate AS DATE)), 103) AS GrnDate,
    MAX(gm.RefDcNo) AS RefDcNo,
    CONVERT(VARCHAR, MAX(CAST(gm.RefDcDate AS DATE)), 103) AS RefDcDate,

    MAX(gs.ItemSpecification) AS ItemSpecification,

    ld.VehicleNo,
    ld.EwayNo,
	CASE 
    WHEN ld.EwayDate = '1900-01-01' THEN ''
    ELSE CONVERT(VARCHAR, CAST(ld.EwayDate AS DATE), 103) END AS EwayDate,
    -- Customer
    c.CustName,
    c.CustAddr,
    c.VendorCode,
    c.GSTNo,
    c.PANNo,
    c.StateId,
    c.ContactNo,
    c.StateName,

    -- Consignee
    ci.AltCustName,
    CONCAT(ci.AltCustAddr1, ' ', ci.AltCustAddr2) AS ConsigneeAddress,
    ci.GSTNo AS congst,
    ci.ContactNo,
    ci.State,
    ci.PANNo AS congpan,

    -- Item
    lds.SlNo,
    i.ItemCode,
    i.ItemName,
    i.MeasureUnit,
    i.HSNCode,

    SUM(DISTINCT lds.Qty) AS Qty,
    lds.UnitPrice,
    SUM(DISTINCT (lds.Qty * lds.UnitPrice)) AS Itemamount,

    ld.Remarks,
    lds.DcRowRem,

    -----------------------------------
    -- 🔥 IDC DETAILS (MULTILINE)
    -----------------------------------
    STUFF((
        SELECT DISTINCT 
            CHAR(10) +
            'PO: ' + CAST(mp2.PONo AS VARCHAR) +
            ' IDC: ' + CAST(gm2.RefDcNo AS VARCHAR) +
            ' (' + CONVERT(VARCHAR, gm2.RefDcDate, 103)+')'

        FROM LabourDcOutgoingSub lds2
        LEFT JOIN LabourDcReturnCompTrack rt2 
            ON lds2.DcSubId = rt2.RefDcSubId
        LEFT JOIN LabourGRNSub gs2 
            ON rt2.RefGrnSubId = gs2.GrnSubId
        LEFT JOIN LabourGRN gm2 
            ON gs2.GrnId = gm2.GrnId
        LEFT JOIN MfgPoSub mps2 
            ON mps2.PoSubId = lds2.RefPoSubId
        LEFT JOIN MfgPo mp2 
            ON mp2.PoId = mps2.PoId

        WHERE lds2.DcId = ld.DcId
        AND lds2.SlNo = lds.SlNo

        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS IDC_Details,

    -----------------------------------
    -- 🔥 QTY STRING
    -----------------------------------
    REFQTY.RefQtyDetails,

    -----------------------------------
    -- 🔥 BATCH NO STRING (NEW)
    -----------------------------------
    BATCH.BatchDetails,

    CASE 
        WHEN ld.TransportMode = '1' THEN 'ByRoad'
        WHEN ld.TransportMode = '2' THEN 'ByRailway'
        WHEN ld.TransportMode = '3' THEN 'ByAirway'
        WHEN ld.TransportMode = '4' THEN 'BySeaway'
        ELSE ''
    END AS TransMode

FROM LabourDcOutgoing ld

INNER JOIN LabourDcOutgoingSub lds 
    ON ld.DcId = lds.DcId

LEFT JOIN LabourDcReturnCompTrack rt 
    ON lds.DcSubId = rt.RefDcSubId

LEFT JOIN LabourGRNSub gs 
    ON rt.RefGrnSubId = gs.GrnSubId

LEFT JOIN LabourGRN gm 
    ON gs.GrnId = gm.GrnId

LEFT JOIN MfgPoSub mps 
    ON mps.PoSubId = lds.RefPoSubId

LEFT JOIN MfgPo mp 
    ON mp.PoId = mps.PoId

LEFT JOIN Customer c 
    ON c.CustId = ld.CustId

LEFT JOIN CustomerIndirect ci 
    ON ci.CustId = ld.CustId 
   AND ci.AltCustId = ld.ShippingId

LEFT JOIN Item i 
    ON i.ItemId = lds.ItemId

-----------------------------------
-- 🔥 QTY APPLY
-----------------------------------
OUTER APPLY
(
    SELECT 
        STRING_AGG(CAST(gs2.Qty AS VARCHAR), CHAR(10)) AS RefQtyDetails
    FROM LabourDcReturnCompTrack rt2
    JOIN LabourGRNSub gs2 
        ON rt2.RefGrnSubId = gs2.GrnSubId
    JOIN LabourDcOutgoingSub lds2 
        ON lds2.DcSubId = rt2.RefDcSubId
    WHERE lds2.DcId = ld.DcId
      AND lds2.SlNo = lds.SlNo
) REFQTY

-----------------------------------
-- 🔥 BATCH APPLY (GRN NO AS BATCH)
-----------------------------------
OUTER APPLY
(
    SELECT 
        STRING_AGG(
            'Batch No: ' + CAST(gm2.GrnNo AS VARCHAR),
            CHAR(10)
        ) AS BatchDetails
    FROM LabourDcReturnCompTrack rt2
    JOIN LabourGRNSub gs2 
        ON rt2.RefGrnSubId = gs2.GrnSubId
    JOIN LabourGRN gm2 
        ON gs2.GrnId = gm2.GrnId
    JOIN LabourDcOutgoingSub lds2 
        ON lds2.DcSubId = rt2.RefDcSubId

    WHERE lds2.DcId = ld.DcId
      AND lds2.SlNo = lds.SlNo
) BATCH

WHERE ld.DcId =  @DcId

GROUP BY 
    ld.DcId,
    ld.DcNo, ld.DcDate,
    mp.PONo, mp.PODate,
    ld.VehicleNo, ld.EwayNo,ld.EwayDate,
    c.CustName, c.CustAddr, c.VendorCode, c.GSTNo, c.PANNo, c.StateId, c.ContactNo, c.StateName,
    ci.AltCustName, ci.AltCustAddr1, ci.AltCustAddr2, ci.GSTNo, ci.ContactNo, ci.State, ci.PANNo,
    lds.SlNo, i.ItemCode, i.ItemName, i.MeasureUnit, i.HSNCode,
    lds.UnitPrice, ld.Remarks, lds.DcRowRem, ld.TransportMode,
    REFQTY.RefQtyDetails,
    BATCH.BatchDetails

ORDER BY lds.SlNo;

END