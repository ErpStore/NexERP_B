CREATE OR ALTER       PROCEDURE [dbo].[Sp_Print_PurchaseEnquiry]
@EnquiryId int,  
@VendCode int
AS 
BEGIN
    SELECT EP.EnquiryNo,CONVERT(varchar,CAST(EP.EnquiryDate as date),103)[EnquiryDate],EP.KindOfAttention,
	EP.MainRemarks,EP.Prefix,EP.Suffix,EP.CreatedBy,EP.CreatedDate,EP.ModifiedBy,EP.ModifiedDate,EP.POCName,
	EP.POCContactNo,EP.Cancel,EP.CancelDate,CONVERT(varchar,CAST(EP.ExpactedReplayDate as date),103)[ExpectedReplyDate],
	EP.NoOfItems,EPS.SlNo,EPS.Qty,EPS.BalQty,EPS.UnitPrice,CONVERT(varchar,CAST(EPS.DueDate as date),103)[ItemWiseDueDate],
	EPS.Remark,MR.MReqNo,MR.MReqDate,EPS.DueDate,I.ItemCode,I.ItemName,I.HSNCode,I.SACCode,I.MeasureUnit,V.VendorName,
	V.VendorAddress,V.GSTNo,V.PANNo,V.ContactNo,VC.ContactPerson,VC.ContactNo as VContactNo
	from EnquiryPurchase EP
	Inner Join EnquiryPurchaseSub EPS ON EPS.EnquiryId=EP.EnquiryId
	LEFT JOIN EnquiryPurchaseVendorAssign EVA ON EVA.EnquiryId = EP.EnquiryId  
	Inner Join Vendor V ON V.VendorCode=EVA.VendorCode
	Left Join VendorContact VC ON VC.VendorCode=V.VendorCode
	Inner Join Item I ON I.ItemId=EPS.ItemId
	Left Join MaterialReqSub MRS ON MRS.MreqId=EPS.RefMReqSubId
	Left Join MaterialReq MR ON MR.MreqId=MRS.MreqId
	where EP.EnquiryId=@EnquiryId and EVA.VendorCode=@VendCode
	Group by EP.EnquiryNo,EP.EnquiryDate,EP.KindOfAttention,EP.MainRemarks,EP.Prefix,EP.Suffix,EP.CreatedBy,
	EP.CreatedDate,EP.ModifiedBy,EP.ModifiedDate,EP.POCName,EP.POCContactNo,EP.Cancel,EP.CancelDate,
	EP.ExpactedReplayDate,EP.NoOfItems,EPS.SlNo,EPS.Qty,EPS.BalQty,EPS.UnitPrice,EPS.DueDate,EPS.Remark,
	EPS.DueDate,I.ItemCode,I.ItemName,I.HSNCode,I.SACCode,I.MeasureUnit,V.VendorName,V.VendorAddress,V.GSTNo,
	V.PANNo,V.ContactNo,VC.ContactPerson,VC.ContactNo,MR.MReqNo,MR.MReqDate

END
