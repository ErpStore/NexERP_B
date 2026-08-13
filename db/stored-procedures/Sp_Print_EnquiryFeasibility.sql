CREATE OR ALTER     Procedure [dbo].[Sp_Print_EnquiryFeasibility]
@FesId int
As
Select ES.FesId,concat(ES.FesNo,'',Suffix)[FesNo],FesDate,FesDateNow,ES.Remark,RefEnqNo,RefEnqDate
,CustName,CustAddr,GSTNo,PANNo,ContactNo,i.ItemId,i.ItemCode,i.ItemName,i.MeasureUnit[UOM],HSNCode
,ESB.SlNo,ESB.Remarks,esb.TypeOfProject,esb.ToolRequired,esb.SpclReqOutsource,esb.TechCapaBilities,
esb.DelvryTrms,esb.ManHrReq,esb.RiskIfAny,esb.SpclReqCustomer,es.QuotRefNo,es.PreparedBy from EnquiryFeasibility ES
inner join EnquiryFeasibilitySub ESB on ES.FesId=ESB.FesId
inner join Item i on i.ItemId=ESB.ItemId
inner join Customer c on c.CustId=ES.CustId
where ES.FesId=@FesId order by ESB.FesSubId
