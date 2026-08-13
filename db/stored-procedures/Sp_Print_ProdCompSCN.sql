CREATE OR ALTER   PROCEDURE [dbo].[Sp_Print_ProdCompSCN]  
@SCNId int	
AS
BEGIN
	SELECT PSC.SCNNo,CONVERT(varchar,CAST(PSC.SCNDate as date),103)[SCNDate],PSC.MainRemark,SA.StoreName as StoreAddName,
	SI.StoreName as StoreIssueName,I.ItemName,I.ItemCode,I.MeasureUnit,PSCS.SlNo,PSCS.AccQty,PSCS.RejQty,PSCS.RewQty,
	PSCS.NCRNo,PSCS.Remark,PRC.ReturnNo,CONVERT(nvarchar,CAST(PRC.ReturnDate as date),103)[ReturnDate],
	CC.CostCenterName,PSCS.InsNO,PSCS.UnitPrice
	from ProductionSCNComp PSC 
	Inner Join ProductionSCNCompSub PSCS ON PSCS.SCNId=PSC.SCNId
	Inner Join Stores SA ON SA.StoreId=PSC.AddStoreId
	Inner Join Stores SI ON SI.StoreId=PSC.IssueStoreId
	Inner Join Item I ON I.ItemId=PSCS.ItemId
	Inner Join ProductionReturnCompSub PRCS ON PRCS.ReturnSubId=PSCS.RefReturnSubId
	Inner Join ProductionReturnComp PRC ON PRC.ReturnId=PRCS.ReturnId
	Left Join CostCenter CC ON CC.Id=PSCS.CostId
	Where PSC.SCNId=@SCNId
END
