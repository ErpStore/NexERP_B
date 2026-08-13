
CREATE OR ALTER   PROCEDURE [dbo].[Sp_Print_RouteCard]
    @RCId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        concat(RC.RCNo, ' ',rc.suffix) as RCNo,
        CONVERT(VARCHAR, CAST(RC.RCDate AS DATE), 103) AS RCDate,
        RC.RcQty,RC.SelectedItemType,RC.MainRemarks,RC.RMReqQty,RC.RMWeight,C.CustName,MP.PONo,MPS.Qty AS POQty,I.ItemCode AS ComponentName,
        IR.ItemCode AS RawMaterialName,IIn.ItemCode AS ItemIn,IOut.ItemCode AS ItemOut,RCS.SlNo,RCS.TotalQty,RCS.AccQty,RCS.RejQty,
        RCS.CycleTime,RCS.SettingTime,RCS.ProcessCost,M.MachineName,V.VendorName,P.ProcessName,RC.BarCodeImage,RC.QrCodeImage,
		i.ItemName,mps.DueDate
		
		
    FROM RouteCard RC
        INNER JOIN RouteCardSub RCS
            ON RCS.RCId = RC.RCId
        LEFT JOIN Customer C
            ON C.CustId = RC.CustId
        LEFT JOIN Process P
            ON P.ProcessId = RCS.ProcessId
        LEFT JOIN Machine M
            ON M.MachineId = RCS.MachineId
        LEFT JOIN Vendor V
            ON V.VendorCode = RCS.VendorId
        LEFT JOIN Item I
            ON I.ItemId = RC.CompItemId
        LEFT JOIN Item IR
            ON IR.ItemId = RC.RMId
        LEFT JOIN Item IIn
            ON IIn.ItemId = RCS.ItemIdIn
        LEFT JOIN Item IOut
            ON IOut.ItemId = RCS.ItemIdOut
        LEFT JOIN MfgPo MP
            ON MP.PoId = RC.RefPoId
        LEFT JOIN MfgPoSub MPS
            ON MPS.PoId = MP.PoId
    WHERE RC.RCId = @RCId;

END
