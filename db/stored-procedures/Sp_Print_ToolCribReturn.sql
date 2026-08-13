CREATE OR ALTER     PROCEDURE [dbo].[Sp_Print_ToolCribReturn]
@TCReturnId int
AS
BEGIN
	SELECT TCR.TCReturnNo,TCR.Suffix,CONVERT(varchar,CAST(TCR.TCReturnDate as date),103)[TCReturnDate],
	TCR.FromWhom,TCR.MainRemarks,TCR.BreakageRemarks,I.ItemCode,I.ItemName,I.Specification,I.MeasureUnit,
	TCR.CreatedBy,CONVERT(varchar,CAST(TCR.CreatedDate as date),103)[CreatedDate],TCR.ModifiedBy,
	CONVERT(nvarchar,CAST(TCR.ModifiedDate as date),103)[ModifiedDate],S.StoreName,TCRS.SlNo,
	TCRS.Remark,TCRS.AccpQty,TCRS.RejQty,TCRS.RejRemark,TCRS.RewQty,TCRS.RewRemark
	from ToolCribReturns TCR
	Inner Join ToolCribReturnSubs TCRS ON TCRS.TCReturnId=TCR.TCReturnId
	Inner Join Item I ON I.ItemId=TCRS.ItemId
	Inner Join Stores S ON S.StoreId=TCR.StoreId
	Where TCR.TCReturnId=@TCReturnId
END
