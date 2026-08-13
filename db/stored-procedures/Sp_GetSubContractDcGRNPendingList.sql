CREATE OR ALTER    PROCEDURE [dbo].[Sp_GetSubContractDcGRNPendingList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
Select ROW_NUMBER() OVER (ORDER BY  GSub.GRnsubid) AS SlNo, V.VendorName as Vendor,
CONCAT(Sg.GRNNO,Sg.Suffix) GRNNo,Sg.GRNDate,Sg.PartyDC,sg.PartyDcDate,
CONCAT(Ds.Dcno,Ds.Suffix) DcNo,Ds.DcDate,Sg.MainRemark,

--CASE  WHEN ISNULL(Pq.PurchOrSub, 0) = 1 THEN 'Purchase' ELSE 'SubContract'  END AS Type,
CASE WHEN ISNULL(Sg.GrnTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(Sg.Cancel, 0) = 1 THEN 'Cancelled' 
     WHEN ISNULL(Sg.ShortClose, 0) = 1 THEN 'Short Closed' 
	 ELSE 'Pending' END AS SubContractGRNStatus,
Sg.CancelReason as CancelReason,Sg.Canceldate,Sg.CancelBy,

Sg.CreatedBy,Sg.CreatedDate,Sg.ModifiedBy,Sg.ModifiedDate,

--Items
I.ItemCode,I.ItemName,I.Specification,I.MeasureUnit,I.HSNCode,
ISNULL(GSub.Qty, 0) AS Qty,ISNULL(GSub.UnitPrice, 0) AS UnitPrice,ISNULL(GSub.BalQty, 0) AS BalQty,
--Esub.Qty,Esub.UnitPrice,Esub.BalQty,
CASE  WHEN ISNULL(GSub.ItemCancel, 0) = 1 THEN 'Cancelled'  ELSE 'Pending' END AS ItemStatus,
GSub.ItemCancelReason,GSub.Remarks as ItemRemarks

from SubConGRN Sg inner join SubConGRNsub GSub on GSub.GRNID=Sg.GRNID
Inner join Vendor V on V.VendorCode=Sg.VendorCode
inner Join Item I on I.Itemid=GSub.Itemid
left join SubConDcOutSub Dsub on Dsub.DcSubId=GSub.refDcsubId
left join SubConDcOut Ds on Ds.Dcid = Dsub.Dcid 
--left join PurchPoSub Psub on psub.PoSubId = DSub.RefPoSubId
--left join PurchPo P on P.PoId = psub.PoId
--left join Routecardsub Rcsub on Rcsub.RcsubId=Sgub.refRcSubid
--left join Routecard Rc on Rc.Rcid= Rcsub.Rcid
WHERE 
(@Status IS NULL  OR @Status = '' OR @Status = 'All' OR
( CASE  WHEN ISNULL(Sg.GrnTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(Sg.Cancel, 0) = 1 THEN 'Cancelled'
 WHEN ISNULL(Sg.ShortClose, 0) = 1 THEN 'Short Closed' 
 ELSE 'Pending'  END = @Status ))
END
