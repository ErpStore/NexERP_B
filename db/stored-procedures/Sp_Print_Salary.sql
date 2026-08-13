
CREATE OR ALTER    PROCEDURE [dbo].[Sp_Print_Salary]
@RowId int
AS
BEGIN
	Select St.StaffName,St.PFCodeNo,St.ESICodeNo,St.AccNo,St.IFSC,St.DOJ,Sa.WorkingDays,Sa.PayableDays,
Sa.LeaveDays,Sa.AbsentDays,Sa.OT,Sa.EarnedBasic,Sa.EarnedDA,Sa.EarnedEA,Sa.EarnedHRA,Sa.EarnedLTA,
Sa.EarnedMR,Sa.EarnedSplAllow,EarnedAttBonus,Sa.EarnedTA,Sa.Miscellaneous,Sa.Others,Sa.EarnedOT,
Sa.TotalEarnings,Sa.TotalDed,Sa.paidAmt,Sa.TDS,Sa.Society,Sa.Insurance,Sa.SalAdvance,Sa.Fines,
Sa.DamageLoss,Sa.OtherDed,Sa.LoanAmtDed,
--DATENAME(MONTH, DATEFROMPARTS(Sa.SalYear, Sa.SalMonth, 1)) 
DATENAME(MONTH, DATEFROMPARTS(Sa.SalYear, Sa.SalMonth, 1)) AS MonthName,
+ ' ' + CAST(Sa.SalYear AS VARCHAR) AS SalMonthYear,Sa.NetPay
from Salary Sa
Inner Join Staff St ON St.StaffID=sa.StaffId
Inner Join Attendance Ad ON Ad.StaffId=Sa.StaffId
Where Sa.RowId=@RowId
END

