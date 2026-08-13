CREATE OR ALTER   PROCEDURE [dbo].[Sp_Print_ProductionIssueAss]
@IssueId int	
AS
BEGIN
	SELECT PA.IssueNo,CONVERT(varchar,CAST(PA.IssueDate as date),103)[IssueDate],PA.Suffix,PA.IssueToWhom,
	PA.MainRemark,PA.CreatedBy,CONVERT(varchar,CAST(PA.CreatedDate as date),103)[CreatedDate],PA.ModifiedBy,
	PA.ModifiedDate,PA.StoreIssId,PA.IssueQty,PA.NoOfItems,S.StoreName,PAS.AssyId,PAS.RefJobId,
	PAS.RefJobOrderSubId,PAS.Remark,PAS.SlNo,PAS.UnitPrice,PAS.BatchNo,PAS.IssueQty AS IssueQtySub,PAS.BalQty,I.ItemCode,
	I.ItemName,I.Specification,I.MeasureUnit,I.HSNCode,I.SACCode,JO.JobNo,CONVERT(varchar,CAST(JO.JobDate as date),103)[JobDate],
	JOS.ReqQty,CC.ProjectNo,SUM(PAS.IssueQty) as SumQty,(SELECT ItemCode FROM Item WHERE ItemId=PAS.AssyId) AS Assembly
	from ProductionIssueAssy PA
	Inner Join ProductionIssueAssySub PAS ON PAS.IssueId=PA.IssueId
	Inner Join Item I ON I.ItemId=PAS.ItemId
	Inner Join Stores S ON S.StoreId=PA.StoreIssId
	Left Join CostCenter CC ON CC.ProjectTypeId=PAS.CostId
	Left JOIN JobOrder JO ON JO.JobId = PAS.RefJobId
    Left JOIN JobOrderSub JOS ON JOS.JobSubId = PAS.RefJobOrderSubId
	where PA.IssueId=@IssueId
	GROUP BY PA.IssueNo,PA.IssueDate,PA.Suffix, PA.IssueToWhom,PA.MainRemark,PA.CreatedBy,PA.CreatedDate,
    PA.ModifiedBy, PA.ModifiedDate,PA.StoreIssId,PA.IssueQty, PA.NoOfItems,S.StoreName,PAS.AssyId,PAS.RefJobId,
	PAS.RefJobOrderSubId,PAS.Remark,PAS.SlNo,PAS.UnitPrice,PAS.BatchNo,PAS.IssueQty,PAS.BalQty,I.ItemCode, 
	I.ItemName, I.Specification,I.MeasureUnit,I.HSNCode,I.SACCode,JO.JobNo,JO.JobDate,JOS.ReqQty,CC.ProjectNo
END
