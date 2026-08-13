
CREATE OR ALTER     PROCEDURE [dbo].[Sp_GetPurchasesandSubcontractEnquiryPendingList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
Select ROW_NUMBER() OVER (ORDER BY  Esub.EnquirySubId) AS SlNo, V.VendorName as Vendor,Es.Prefix,
CONCAT(Es.EnquiryNo,Es.Suffix) EnquiryNo,Es.EnquiryDate,Es.MainRemarks,
CASE  WHEN ISNULL(Es.PurchOrSubCon, 0) = 1 THEN 'Purchase' ELSE 'SubContract'  END AS Type,
CASE WHEN ISNULL(Es.EnquiryTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(Es.Cancel, 0) = 1 THEN 'Cancelled' 
   WHEN ISNULL(Es.EnquiryShortClose, 0) = 1 THEN 'Short Closed' ELSE 'Pending' END AS EnquiryStatus,
Es.CancellationRemarks as CancelReason,Es.Canceldate,Es.CancelBy,Es.CreatedBy,Es.CreatedDate,Es.ModifiedBy,Es.ModifiedDate,Es.PocName,Es.PocContactNo,
Es.ExpactedReplayDate,
--Items
I.ItemCode,I.ItemName,I.Specification,I.MeasureUnit,I.HSNCode,
ISNULL(Esub.Qty, 0) AS Qty,ISNULL(Esub.UnitPrice, 0) AS UnitPrice,ISNULL(Esub.BalQty, 0) AS BalQty,
--Esub.Qty,Esub.UnitPrice,Esub.BalQty,
CASE  WHEN ISNULL(Esub.ItemCancel, 0) = 1 THEN 'Cancelled' ELSE '' END AS ItemCancel,
Esub.ItemCancelReason,Esub.Remark as ItemRemarks

from EnquiryPurchaseVendorAssign Epv 
inner join EnquiryPurchase Es on Es.EnquiryId=Epv.EnquiryId
inner join EnquiryPurchasesub Esub on Esub.EnquiryId = Es.EnquiryId
inner join Vendor V on V.VendorCode= Epv.VendorCode
inner join Item I on I.ItemId = Esub.ItemId
WHERE 
(@Status IS NULL  OR @Status = '' OR  @Status = 'All' OR
( CASE  WHEN ISNULL(Es.EnquiryTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(Es.Cancel, 0) = 1 THEN 'Cancelled'
 WHEN ISNULL(Es.EnquiryShortClose, 0) = 1 THEN 'Short Closed' ELSE 'Pending'  END = @Status ))
END
