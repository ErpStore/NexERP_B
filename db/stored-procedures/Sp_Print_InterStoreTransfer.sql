
CREATE OR ALTER   PROCEDURE [dbo].[Sp_Print_InterStoreTransfer]
@ISTId int
AS
BEGIN
	SELECT SIT.ISTNo,SIT.Suffix,CONVERT(varchar,CAST(SIT.ISTDate as date),103)[ISTDate],SIT.MainRemark,
	SA.StoreName as AddStoreName,SI.StoreName as IssueStoreName,I.ItemCode,I.ItemName,I.Specification,I.MeasureUnit,SITS.SlNo,SITS.Qty,SITS.UnitPrice,
	SITS.Remark,SIT.CreatedBy,SIT.ModifiedBy,CONVERT(varchar,CAST(SIT.CreatedDate as date),103)[CreatedDate],
	CONVERT(varchar,CAST(SIT.ModifiedDate as date),103)[ModifiedDate]
	from StoreInterTrans SIT 
	Inner Join StoreInterTransSub SITS ON SITS.ISTId=SIT.ISTId
	Inner Join Stores SA ON SA.StoreId=SIT.StoreAddId
	Inner Join Stores SI ON SI.StoreId=SIT.StoreIssueId
	Inner Join Item I ON I.ItemId=SITS.ItemId
	Where SIT.ISTId=@ISTId
	Order by SlNo
END
