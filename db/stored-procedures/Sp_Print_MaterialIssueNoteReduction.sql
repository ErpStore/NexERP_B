CREATE OR ALTER   PROCEDURE [dbo].[Sp_Print_MaterialIssueNoteReduction]
@MINId int	
AS
BEGIN
	SELECT MN.IssueNo,MN.Suffix,CONVERT(varchar,CAST(MN.IssueDate as date),103)[IssueDate],MN.ToWhom,MN.MainRemark,
	MN.CreatedBy,MN.CreatedDate,MN.ModifiedBy,CONVERT(varchar,CAST(MN.ModifiedDate as date),103)[ModifiedDate],
	S.StoreName,MNS.SlNo,MNS.IssueQty,MNS.UnitPrice,MNS.Remark,I.ItemCode,I.ItemName,I.Specification,I.MeasureUnit
	from MaterialIssNote MN 
	Inner Join MaterialIssNoteSub MNS ON MNS.MINId=MN.MINId
	Inner Join Item I ON I.ItemId=MNS.ItemId
	Inner Join Stores S ON S.StoreId=MN.StoreIssId
	Left Join CostCenter CC ON CC.ProjectTypeId=MNS.CostId
	Where MN.MINId=@MINId
END
