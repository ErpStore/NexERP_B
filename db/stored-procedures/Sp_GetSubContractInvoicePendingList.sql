CREATE OR ALTER   PROCEDURE [dbo].[Sp_GetSubContractInvoicePendingList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
Select ROW_NUMBER() OVER (ORDER BY  SIsub.InvSubId) AS SlNo, V.VendorName as Vendor,
CONCAT(SI.InvNo,SI.Suffix) InvNo,SI.InvDate,
CONCAT(Sc.SCNNo,Sc.Suffix) SCNNo,Sc.SCNDate,Sc.MainRemark,
--CASE  WHEN ISNULL(Pq.PurchOrSub, 0) = 1 THEN 'Purchase' ELSE 'SubContract'  END AS Type,
CASE WHEN ISNULL(SI.InvTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(SI.InvCancel, 0) = 1 THEN 'Cancelled' 
	 ELSE 'Pending' END AS SubContractInvoiceStatus,
SI.CancelReason as CancelReason,Ct.CurrName as Curreny,SI.TotalBasicAmount as BasicAmount,Si.GrandTotal,
SI.CreatedBy,SI.CreatedDate,SI.ModifiedBy,SI.ModifiedDate,
--Items
I.ItemCode,I.ItemName,I.Specification,I.MeasureUnit,I.HSNCode,
ISNULL(SIsub.Qty, 0) AS Qty,ISNULL(SIsub.UnitPrice, 0) AS UnitPrice,ISNULL(SIsub.RejectQty, 0) AS RejectedQty,
ISNULL(SIsub.ReworkQty, 0) AS ReworkQty,ISNULL(SIsub.BalQty, 0) AS BalQty,
--Esub.Qty,Esub.UnitPrice,Esub.BalQty,
CASE  WHEN ISNULL(SIsub.ItemCancel, 0) = 1 THEN 'Cancelled'  ELSE 'Pending' END AS ItemStatus,
SIsub.ItemCancelReason as ItemCancelReason,SIsub.Remarks 

from SubConInv SI inner join SubConInvsub SIsub on SIsub.Invid=SI.Invid
Inner join Vendor V on V.VendorCode=SI.VendorCode
inner Join Item I on I.Itemid=SIsub.Itemid
inner join Currency Ct on Ct.Currid = SI.Currid
left join SubConSCNSub Scsub on Scsub.ScnSubid=SIsub.refScnSubid
left join SubConSCN Sc on Sc.ScnId = Scsub.ScnId 

WHERE 
(@Status IS NULL  OR @Status = '' OR @Status = 'All' OR
( CASE  WHEN ISNULL(SI.InvTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(SI.InvCancel, 0) = 1 THEN 'Cancelled'
 ELSE 'Pending'  END = @Status ))
END
