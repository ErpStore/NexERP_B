CREATE OR ALTER    PROCEDURE [dbo].[Sp_GetSubContractScnPendingList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
Select ROW_NUMBER() OVER (ORDER BY  Scsub.SCNSubid) AS SlNo, V.VendorName as Vendor,
CONCAT(Sc.SCNNo,Sc.Suffix) SCNNo,Sc.SCNDate,Sg.PartyDC,sg.PartyDcDate,
CONCAT(Sg.GRNNo,Sg.Suffix) GRNNo,Sg.GRNDate,Sc.MainRemark,

--CASE  WHEN ISNULL(Pq.PurchOrSub, 0) = 1 THEN 'Purchase' ELSE 'SubContract'  END AS Type,
CASE WHEN ISNULL(Sc.SCNTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(Sc.SCNCancel, 0) = 1 THEN 'Cancelled' 
	 ELSE 'Pending' END AS SubContractScnStatus,
Sc.CancelReason as CancelReason,
Sc.CreatedBy,Sc.CreatedDate,Sc.ModifiedBy,Sc.ModifiedDate,
--Items
I.ItemCode,I.ItemName,I.Specification,I.MeasureUnit,I.HSNCode,
ISNULL(Scsub.AccQty, 0) AS AccQty,ISNULL(GSub.UnitPrice, 0) AS UnitPrice,ISNULL(Scsub.RejQty, 0) AS RejQty,
ISNULL(Scsub.RewQty, 0) AS RewQty,ISNULL(Scsub.BalQty, 0) AS BalQty,
--Esub.Qty,Esub.UnitPrice,Esub.BalQty,
--CASE  WHEN ISNULL(Scsub.ItemCancel, 0) = 1 THEN 'Cancelled'  ELSE 'Pending' END AS ItemStatus,
Scsub.RejReason as RejectedReason,Scsub.RewReason as ReworkReason
from SubConSCN Sc inner join SubConSCNsub Scsub on Scsub.SCNId=Sc.SCNId
Inner join Vendor V on V.VendorCode=Sc.VendorCode
inner Join Item I on I.Itemid=Scsub.Itemid
left join SubConGRNsub GSub on GSub.GRNSubId=Scsub.RefGRNSubId
left join SubConGRN Sg on Sg.GRNId = GSub.GRNId 
--left join PurchPoSub Psub on psub.PoSubId = DSub.RefPoSubId
--left join PurchPo P on P.PoId = psub.PoId
--left join Routecardsub Rcsub on Rcsub.RcsubId=Sgub.refRcSubid
--left join Routecard Rc on Rc.Rcid= Rcsub.Rcid
WHERE 
(@Status IS NULL  OR @Status = '' OR @Status = 'All' OR
( CASE  WHEN ISNULL(Sc.SCNTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(Sc.SCNCancel, 0) = 1 THEN 'Cancelled'
 ELSE 'Pending'  END = @Status ))
END



select * from SubConSCNSub
