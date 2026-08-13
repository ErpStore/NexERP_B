CREATE OR ALTER   PROCEDURE [dbo].[Sp_Print_ProductionIssueComp]
@IssueId int
AS
BEGIN
	SELECT PC.IssueNo,CONVERT(varchar,CAST(PC.IssueDate as date),103)[IssueDate],PC.Suffix,C.CustName,PC.IssueQty,
	PC.NoOfItems,PC.IssueToWhom,PC.MainRemark,PC.CreatedBy,CONVERT(varchar,CAST(PC.CreatedDate as date),103)[CreatedDate],
	PC.ModifiedBy,CONVERT(varchar,CAST(PC.ModifiedDate as date),103)[ModifiedDate],s.StoreName,PC.IssueTally,
	I.ItemCode,I.ItemName,I.MeasureUnit,PCS.SlNo,PCS.Qty,PCS.BalQty,PCS.BatchNo,PCS.Remark,PCS.TransType,
	PCS.UnitPrice,PCS.MachineId,PCS.HeatNo,PCS.ProcessId,MP.PONo,CONVERT(varchar,CAST(MP.PODate as date),103)[PODate],
	RC.RCNo,RC.RcQty
	from ProductionIssueComp PC 
	Inner Join  ProductionIssueCompSub PCS ON PCS.IssueId=PC.IssueId
	Inner Join Item I ON I.ItemId=PCS.ItemId
	Inner Join Stores S ON S.StoreId=PC.StoreIssId
	Inner Join Customer C ON C.CustId=PC.CustId
	Left Join CostCenter CC ON CC.ProjectTypeId=PCS.CostId
	Left Join MfgPo MP ON MP.PoId=RefPoSubId
	Left Join RouteCard RC ON RC.RCId=PC.IssueId
	where PC.IssueId=@IssueId
END
