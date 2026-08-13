CREATE OR ALTER   PROCEDURE [dbo].[Sp_Print_JobOrder]
@JobId int
AS
BEGIN
	SELECT C.CustName,MP.PONo,CONVERT(varchar,CAST(MP.PODate as date),103)[PODate],JO.UtilQty,
	JO.JobNo,CONVERT(varchar,CAST(JO.JobDate as date),103)[JobDate],JO.POQty,JO.JobOrderQty,
    JOS.SlNo,I.ItemCode,I.ItemName,I.Specification,I.MeasureUnit,JOS.UtilQty AS UtilityQty,
	JOS.ReqQty,CC.CostCenterName,(SELECT ItemCode FROM Item WHERE ItemId=JO.AssyId) AS Assembly
	from JobOrder JO
	Inner Join JobOrderSub JOS ON JOS.JobId=JO.JobId
	Inner Join Customer C ON C.CustId=JO.CustId
	Inner Join MfgPo MP ON MP.PoId=JO.RefPoId
	Inner Join Item I ON I.ItemId=JOS.ItemId
	Left Join CostCenter CC ON CC.Id=JOS.CostId
	Where JO.JobId=@JobId
END
