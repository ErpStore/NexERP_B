CREATE OR ALTER     PROCEDURE [dbo].[Sp_Print_ToolCribIssueNote]
@TCIssueId int	
AS
BEGIN
	Select TCI.TCIssueNo,TCI.Suffix,CONVERT(varchar,CAST(TCI.TCIssueDate as date),103)[TCIssueDate],
	TCI.IssuedToWhom,TCI.MainRemarks,TCI.Description,TCI.CreatedBy,TCIS.Remark,I.ItemCode,I.ItemName,
	I.Specification,I.MeasureUnit,I.HSNCode,CONVERT(varchar,CAST(TCI.CreatedDate as date),103)[CreatedDate],
	TCI.ModifiedBy,CONVERT(varchar,CAST(TCI.ModifiedDate as date),103)[ModifiedDate],TCIS.SlNo,TCIS.UnitPrice,
	TCIS.QtyOut,C.CategoryName,S.StoreName
	from ToolCribIssue TCI
	Inner Join ToolCribIssueSub TCIS ON TCIS.TCIssueId=TCI.TCIssueId
	Inner Join Category C ON C.CategoryCode=TCI.CategoryCode
	Inner Join Stores S ON S.StoreId=TCI.StoreId
	Inner Join Item I ON I.ItemId=TCIS.ItemId
	Where TCI.TCIssueId=@TCIssueId
END
