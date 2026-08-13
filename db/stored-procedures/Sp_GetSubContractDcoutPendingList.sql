CREATE OR ALTER    PROCEDURE [dbo].[Sp_GetSubContractDcoutPendingList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
Select ROW_NUMBER() OVER (ORDER BY  DSub.DcSubId) AS SlNo, V.VendorName as Vendor,Sd.Prefix,
CONCAT(Sd.DcNo,Sd.Suffix) DcNo,Sd.DcDate,Sd.MainRemark,
CONCAT(P.PONo,P.Suffix) PoNo,P.PODate,
--CASE  WHEN ISNULL(Pq.PurchOrSub, 0) = 1 THEN 'Purchase' ELSE 'SubContract'  END AS Type,
CASE WHEN ISNULL(Sd.DcTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(Sd.Cancel, 0) = 1 THEN 'Cancelled' 
     WHEN ISNULL(Sd.ShortClose, 0) = 1 THEN 'Short Closed' 
	 ELSE 'Pending' END AS SubContractDcStatus,
Sd.CancelReason as CancelReason,Sd.Canceldate,Sd.CancelBy,
Sd.VehicleNo,Sd.TransFrom,Sd.TransTo,Sd.EwayBillNumber,Sd.EwayBillDate,Sd.EwayTransName as TransName,
Sd.CreatedBy,Sd.CreatedDate,Sd.ModifiedBy,Sd.ModifiedDate,

--Items
I.ItemCode,I.ItemName,I.Specification,I.MeasureUnit,I.HSNCode,
ISNULL(DSub.Qty, 0) AS Qty,ISNULL(DSub.UnitPrice, 0) AS UnitPrice,ISNULL(DSub.BalQty, 0) AS BalQty,
--Esub.Qty,Esub.UnitPrice,Esub.BalQty,
CASE  WHEN ISNULL(DSub.ItemCancel, 0) = 1 THEN 'Cancelled'  ELSE 'Pending' END AS ItemStatus,
DSub.ItemCancelReason,DSub.Remark as ItemRemarks
from SubConDcOut Sd inner join SubConDcOutSub DSub on DSub.DcId=Sd.DcId
Inner join Vendor V on V.VendorCode=Sd.VendorCode
inner Join Item I on I.Itemid=DSub.Itemid
left join PurchPoSub Psub on psub.PoSubId = DSub.RefPoSubId
left join PurchPo P on P.PoId = psub.PoId
WHERE 
(@Status IS NULL  OR @Status = '' OR @Status = 'All' OR
( CASE  WHEN ISNULL(Sd.DcTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(Sd.Cancel, 0) = 1 THEN 'Cancelled'
 WHEN ISNULL(Sd.ShortClose, 0) = 1 THEN 'Short Closed' 
 ELSE 'Pending'  END = @Status ))
END
