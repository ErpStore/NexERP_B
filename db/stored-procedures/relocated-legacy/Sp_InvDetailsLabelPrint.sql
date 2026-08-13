CREATE OR ALTER procedure [dbo].[Sp_InvDetailsLabelPrint]
     @INVID int
AS
BEGIN
SELECT 
    c.CustName,
    im.LabInvNo,
    FORMAT(im.LabInvDate, 'dd/MM/yyyy') AS LabInvDate,
    gm.grnno,
    it.itemcode,
    it.itemname,
    gs.ItemSpecification AS OutputPartName,
    isub.Qty,
    co.Image
FROM LabInv im

INNER JOIN LabInvSub isub 
    ON im.LabInvId = isub.LabInvId

-- 🔥 Correct bridge table
INNER JOIN LabourDcReturnCompTrack rt
    ON isub.RefDcSubId = rt.RefDcSubId

-- GRN linkage
INNER JOIN LabourGRNSub gs 
    ON rt.RefGrnSubId = gs.GrnSubId

INNER JOIN LabourGRN gm 
    ON gs.GrnId = gm.GrnId

-- Item
INNER JOIN Item it 
    ON gs.ItemId = it.ItemId

INNER JOIN Customer c 
    ON im.CustId = c.CustId

OUTER APPLY (
    SELECT TOP 1 Image
    FROM Correspondences co
    WHERE co.ReferenceId = gs.ItemId
      AND co.DocumentType = 'Drawing'
    ORDER BY co.Id DESC
) co

WHERE im.LabInvId = @INVID;

END