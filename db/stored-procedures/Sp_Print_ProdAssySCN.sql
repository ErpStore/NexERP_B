CREATE OR ALTER   PROCEDURE [dbo].[Sp_Print_ProdAssySCN]
@SCNId int
AS
BEGIN
	SELECT PSA.SCNNo,CONVERT(varchar,CAST(PSA.SCNDate as date),103)[SCNDate],PSA.MainRemark,SA.StoreName as StoreAddName,
	SI.StoreName as StoreIssueName,PSAS.SlNo,PSAS.AccQty,PSAS.RejQty,PSAS.RejReason,PSAS.RewQty,PSAS.RewReason,
	PRA.ReturnNo,CONVERT(varchar,Cast(PRA.ReturnDate as date),103)[ReturnDate],PSAS.UnitPrice,PSAS.InsNO,PSAS.NCRNo,
	PSAS.Remark,I.ItemCode,I.ItemName,I.MeasureUnit
	from ProductionSCNAssy PSA
	Inner Join ProductionSCNAssySub PSAS ON PSAS.SCNId=PSA.SCNId
	Inner Join Stores SA ON SA.StoreId=PSA.AddStoreId
	Inner Join Stores SI ON SI.StoreId=PSA.IssueStoreId
	Inner Join Item I ON I.ItemId=PSAS.ItemId
	Inner Join ProductionReturnAssySub PRAS ON PRAS.ReturnSubId=PSAS.RefReturnSubId
	Inner Join ProductionReturnAssy PRA ON PRA.ReturnId=PRAS.ReturnId
	Left Join CostCenter CC ON CC.Id=PSAS.CostId
	Where PSA.SCNId=@SCNId
	order by SlNo
END
