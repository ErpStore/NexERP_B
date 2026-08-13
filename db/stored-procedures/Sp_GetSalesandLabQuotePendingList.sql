CREATE OR ALTER     PROCEDURE [dbo].[Sp_GetSalesandLabQuotePendingList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
Select ROW_NUMBER() OVER (ORDER BY  MqSub.QuotesubId) AS SlNo, C.CustName as Customer,Mq.Prefix,
CONCAT(Mq.QuoteNo,Mq.Suffix) QuoteNo,Mq.QuoteDate,Mq.MainRemark,Mq.DueDate,
CONCAT(E.EnquiryNo,E.EnquiryDate) EnquiryNo,E.EnquiryDate,
CASE  WHEN ISNULL(Mq.MfgOrLab, 0) = 1 THEN 'Manufacturing' ELSE 'Labour'  END AS Type,
CASE WHEN ISNULL(Mq.QuotationTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(Mq.IsCancel, 0) = 1 THEN 'Cancelled' 
     WHEN ISNULL(Mq.ShortClose, 0) = 1 THEN 'Short Closed' WHEN ISNULL(Mq.IsRejected, 0) = 1 THEN 'Rejected'
	 ELSE 'Pending' END AS QuotationStatus,
Mq.PayTerms,Mq.DeliveryTems,Mq.CancelReason as CancelReason,Mq.Canceldate,Mq.CancelBy,Mq.RejectReason,
mq.TotalBasicAmount as BasicAmount,mq.GrandTotal as TotalValue,
Mq.CreatedBy,Mq.CreatedDate,Mq.ModifiedBy,Mq.ModifiedDate,
--Items
I.ItemCode,I.ItemName,I.Specification,I.MeasureUnit,I.HSNCode,
ISNULL(MqSub.Qty, 0) AS Qty,ISNULL(MqSub.UnitPrice, 0) AS UnitPrice,ISNULL(MqSub.BalQty, 0) AS BalQty,MqSub.LineGross as LineGrossAmount,
--Esub.Qty,Esub.UnitPrice,Esub.BalQty,
CASE  WHEN ISNULL(MqSub.ItemCancel, 0) = 1 THEN 'Cancelled' WHEN ISNULL(MqSub.Quotetally, 0) = 1 THEN 'Completed' ELSE 'Pending' END AS ItemStatus,
MqSub.ItemCancelReason,MqSub.Remarks as ItemRemarks


from MfgQuote Mq inner join MfgQuoteSub MqSub on MqSub.QuoteId=Mq.QuoteId
Inner join Customer C on C.CustId=Mq.CustId
inner Join Item I on I.Itemid=MqSub.Itemid
inner Join Currency Ct on Ct.CurrId=Mq.CurrId
Left join EnquirySalesSub Esub  on Esub.EnquirySubId=MqSub.refEnqSubid
Left join EnquirySales E on E.Enquiryid=Esub.Enquiryid
WHERE 
(@Status IS NULL  OR @Status = '' OR
( CASE  WHEN ISNULL(Mq.QuotationTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(Mq.IsCancel, 0) = 1 THEN 'Cancelled'
 WHEN ISNULL(Mq.ShortClose, 0) = 1 THEN 'Short Closed' WHEN ISNULL(Mq.IsRejected, 0) = 1 THEN 'Rejected'
 ELSE 'Pending'  END = @Status ))
END
