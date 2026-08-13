CREATE OR ALTER   PROCEDURE [dbo].[Sp_Print_LeaveApplication]
@LeaveAppID int
AS
BEGIN
	SELECT LA.LeaveApplicationNo,CONVERT(varchar,CAST(LA.AppliedDate as date),103)[AppliedDate],LA.ApprovedBy,
	CONVERT(varchar,CAST(LA.ApprovalDate as date),103)[ApprovalDate],LA.BalDaysBeforeLeaveApp,
	LA.LeaveStatus,LA.Department,LA.Reason,LA.FromDate,LA.ToDate,LA.TotalDays,S.StaffName,LT.LeaveName,
	LT.LeaveDescription,LT.CreatedBy,CONVERT(varchar,CAST(LT.CreatedDate as date),103)[CreatedDate]
	from LeaveApplication LA
	Inner Join Staff S ON S.StaffID=LA.EmployeeID
	Inner Join LeaveType LT ON LT.LeaveTypeId=LA.LeaveTypeId
	Where LA.LeaveAppID=@LeaveAppID
END
