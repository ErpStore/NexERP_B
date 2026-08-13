CREATE OR ALTER     PROCEDURE [dbo].[Sp_GetSalesandlabourPendingList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
Select ROW_NUMBER() OVER (ORDER BY  Esub.EnquirySubId) AS SlNo,C.CustName as Customer,Es.Prefix,
CONCAT(Es.EnquiryNo,Es.Suffix) EnquiryNo,Es.EnquiryDate,Es.MainRemarks,Es.TotalValue,
CASE  WHEN ISNULL(Es.MfgORLab, 0) = 1 THEN 'Manufacturing' ELSE 'Labour'  END AS Type,
CASE WHEN ISNULL(Es.EnquiryTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(Es.Cancel, 0) = 1 THEN 'Cancelled' 
   WHEN ISNULL(Es.ShortClose, 0) = 1 THEN 'Short Closed' ELSE 'Pending' END AS EnquiryStatus,
Es.CancelReason,Es.Canceldate,Es.CancelBy,Es.CreatedBy,Es.CreatedDate,Es.ModifiedBy,Es.ModifiedDate,Es.PocName,Es.PocContactNo,
Es.ExpectedReplyDate,
--Items
I.ItemCode,I.ItemName,I.Specification,I.MeasureUnit,I.HSNCode,
Esub.Qty,Esub.UnitPrice,Esub.BalQty,CASE  WHEN ISNULL(Esub.ItemCancel, 0) = 1 THEN 'Cancelled' END AS ItemCancel,
Esub.ItemCancelReason,Esub.Remark as ItemRemarks

from EnquirySales Es inner join EnquirySalesSub Esub on Esub.EnquiryId = Es.EnquiryId
inner join Customer C on C.CustId= Es.CustId
inner join Item I on I.ItemId = Esub.ItemId
inner join Currency cs on Cs.CurrId= Es.CustId
WHERE 
(@Status IS NULL  OR @Status = '' OR
( CASE  WHEN ISNULL(Es.EnquiryTally, 0) = 1 THEN 'Completed'  WHEN ISNULL(Es.Cancel, 0) = 1 THEN 'Cancelled'
 WHEN ISNULL(Es.ShortClose, 0) = 1 THEN 'Short Closed' ELSE 'Pending'  END = @Status ))
  
END
