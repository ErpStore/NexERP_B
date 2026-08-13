CREATE OR ALTER    PROCEDURE [dbo].[Sp_GetPurchandSubQuotePendingList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
Select ROW_NUMBER() OVER (ORDER BY  PqSub.QuotesubId) AS SlNo, V.VendorName as Vendor,
CONCAT(Pq.QuoteNo,Pq.Suffix) QuoteNo,Pq.QuoteDate,Pq.MainRemark,
CONCAT(E.EnquiryNo,E.EnquiryDate) EnquiryNo,E.EnquiryDate,
CASE  WHEN ISNULL(Pq.PurchOrSub, 0) = 1 THEN 'Purchase' ELSE 'SubContract'  END AS Type,
CASE WHEN ISNULL(Pq.QuotationTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(Pq.IsCancel, 0) = 1 THEN 'Cancelled' 
     WHEN ISNULL(Pq.QuoteShortClose, 0) = 1 THEN 'Short Closed' 
	 ELSE 'Pending' END AS QuotationStatus,
Pq.CancelReason as CancelReason,Pq.Canceldate,Pq.CancelledBy as CancelBy,
Pq.TotalBasicAmount as BasicAmount,Pq.GrandTotal as TotalValue,
Pq.CreatedBy,Pq.CreatedDate,Pq.ModifiedBy,Pq.ModifiedDate,
--Items
I.ItemCode,I.ItemName,I.Specification,I.MeasureUnit,I.HSNCode,
ISNULL(PqSub.Qty, 0) AS Qty,ISNULL(PqSub.UnitPrice, 0) AS UnitPrice,ISNULL(PqSub.BalQty, 0) AS BalQty,PqSub.LineBasicAmount as LineBasicAmount,
--Esub.Qty,Esub.UnitPrice,Esub.BalQty,
CASE  WHEN ISNULL(PqSub.ItemCancel, 0) = 1 THEN 'Cancelled' WHEN ISNULL(PqSub.Quotetally, 0) = 1 THEN 'Completed' 
WHEN ISNULL(PqSub.ItemShortClose, 0) = 1 THEN 'Short Closed' ELSE '' END AS ItemStatus,
PqSub.ItemCancelReason,PqSub.Remarks as ItemRemarks


from PurchaseQuote Pq inner join PurchaseQuoteSub PqSub on PqSub.QuoteId=Pq.QuoteId
Inner join Vendor V on V.VendorCode=Pq.VendorCode
inner Join Item I on I.Itemid=PqSub.Itemid
inner Join Currency Ct on Ct.CurrId=Pq.CurrId
Left join EnquiryPurchaseVendorAssign EVA  on EVA.EnqPurchVendorId=PqSub.RefEnqVendorAssignId
Left join EnquiryPurchase E on E.Enquiryid=EVA.Enquiryid
Left join EnquiryPurchaseSub Esub on Esub.Enquiryid=E.Enquiryid
WHERE 
(@Status IS NULL  OR @Status = '' OR   @Status = 'All' OR
( CASE  WHEN ISNULL(Pq.QuotationTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(Pq.IsCancel, 0) = 1 THEN 'Cancelled'
 WHEN ISNULL(Pq.QuoteShortClose, 0) = 1 THEN 'Short Closed' 
 ELSE 'Pending'  END = @Status ))
END
