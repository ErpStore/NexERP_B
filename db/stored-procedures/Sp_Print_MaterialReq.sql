CREATE OR ALTER   PROCEDURE [dbo].[Sp_Print_MaterialReq]
@MreqId int
AS
BEGIN
	SELECT MR.MReqNo,CONVERT(varchar,CAST(MR.MreqDate as date),103)[PRDate],MR.Department,MR.KindAttn,MR.MainRemark,
	MR.CreatedBy,CONVERT(varchar,CAST(MR.CreatedDate as date),103)[CreatedDate],I.ItemCode,I.ItemName,I.MeasureUnit,I.HSNCode,
	I.Weight,CONVERT(varchar,CAST(MRS.DueDate as date),103)[DueDate],MRS.Remark,MRS.SlNo,MRS.UnitPrice,
	MRS.TotalGross,MRS.PurchQty,MP.PONo,C.CustName,V.VendorName,CC.ProjectNo
	from MaterialReq MR 
	Inner Join MaterialReqSub MRS ON MRS.MreqId=MR.MreqId
	Inner Join Item I ON I.ItemId=MRS.ItemId
	Left Join MfgPo MP ON MP.PoId=MRS.RefPoId
	Inner Join Customer C ON C.CustId=MRS.CustId
	Left Join Vendor V ON V.VendorCode=MRS.VendorCode
	Left Join CostCenter CC ON CC.ProjectTypeId=MRS.CostId
	Where MR.MreqId=@MreqId
	Order by SlNo
END
