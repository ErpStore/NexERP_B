CREATE OR ALTER   Procedure [dbo].[Sp_Print_EnquirySales]
@EnquiryId int
As
Select s.EnquiryId,concat(s.Prefix,' ',s.EnquiryNo,'',Suffix)[EnquiryNo],s.EnquiryDate,EnquiryDateNow,s.MainRemarks,POCName,POCContactNo,ExpectedReplyDate,KindOfAttention,cast(case when s.MfgORlab=0 then 'Labour' else 'Manufacturing' end as Varchar)[Type]
,CustName,CustAddr,GSTNo,PANNo,ContactNo,i.ItemId,i.ItemCode,i.ItemName,i.MeasureUnit[UOM],HSNCode,
sb.Qty,sb.BalQty,sb.SlNo,sb.Remark,ct.CostCenterName,ct.ProjectNo,Cancel,CancelDate from EnquirySales s 
inner join EnquirySalesSub sb on s.EnquiryId=sb.EnquiryId
inner join Item i on i.ItemId=sb.ItemId
inner join Customer c on c.CustId=s.CustId
left join CostCenter ct on ct.Id=sb.CostCenterId
where s.EnquiryId=@EnquiryId order by sb.EnquirySubId
