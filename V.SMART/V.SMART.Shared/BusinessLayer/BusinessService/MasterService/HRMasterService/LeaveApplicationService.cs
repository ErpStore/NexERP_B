using DocumentFormat.OpenXml.Vml.Office;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IHRMasterservice;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.HRMasterService
{
    public class LeaveApplicationService : ILeaveApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public LeaveApplicationService(IUnitOfWork unitOfWork, ILoggingService loggingService, CurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<LeaveApplication> GetLeaveApplicationWithDetailsAsync(int leaveAppID)
        {
            return await _unitOfWork.LeaveApplications.GetWithIncludeAsync(
                l => l.LeaveAppID == leaveAppID,
                l => l.DailyLeaveRecords
            );
        }

        public async Task<(IEnumerable<Staff> Staffs, IEnumerable<LeaveType> LeaveTypes, IEnumerable<EmployeeLeaveBalance> EmployeeLeaveBalances, string LeaveApplicationNo)> GetFormDataAsync()
        {
            var staffs = await _unitOfWork.Staffs.GetAllAsync();
            var leaveTypes = await _unitOfWork.LeaveTypes.GetAllAsync();
            var leaveBalances = await _unitOfWork.EmployeeLeaveBalances.GetAllAsync();
            var appNo = await _unitOfWork.LeaveApplications.GenerateLeaveApplicationNoAsync();

            return (staffs, leaveTypes, leaveBalances, appNo);
        }

        public async Task<string> CreateOrUpdateLeaveApplicationAsync(LeaveApplication leaveApplication)
        {
            if (leaveApplication.LeaveStatus == "Approved")
                return "This Application Already Approved You can't Modify this record";

            if (leaveApplication.FromDate == null || leaveApplication.ToDate == null)
                return "From Date and To Date are required.";

            if (leaveApplication.FromDate > leaveApplication.ToDate)
                return "To Date cannot be before From Date.";

            if (leaveApplication.EmployeeID == 0)
                return "Employee is required.";

            if (leaveApplication.LeaveTypeId == 0)
                return "Leave Type is required.";

            if (string.IsNullOrWhiteSpace(leaveApplication.Reason))
                return "Reason for leave is required.";

            if (leaveApplication.BalDaysBeforeLeaveApp <= 0)
                return "Leave balance not found for this employee.";

            // ✅ Recalculate total days + generate daily records
            RecalculateTotalDays(leaveApplication);

            try
            {
                if (leaveApplication.LeaveAppID == 0)
                {
                    // CREATE
                    leaveApplication.LeaveStatus = "Pending";
                    leaveApplication.CreatedBy = await _currentUserService.GetUsernameAsync();
                    leaveApplication.CreatedDate = DateTime.Now;

                    await _unitOfWork.LeaveApplications.CreateAsync(leaveApplication);
                    await _unitOfWork.SaveAsync(); // Save parent first to generate ID

                    // Add children
                    foreach (var record in leaveApplication.DailyLeaveRecords)
                    {
                        record.DailyLeaveRecordID = 0;
                        record.LeaveApplicationID = leaveApplication.LeaveAppID;
                        await _unitOfWork.DailyLeaveRecords.CreateAsync(record);
                    }
                    await _unitOfWork.SaveAsync();

                    // 🔹 Update EmployeeLeaveBalance Pooja Commented to correct the leavebalanace
                    // await UpdateEmployeeLeaveBalance(leaveApplication, isUpdate: false);

                    await _loggingService.LogUserAction(
                        UserName: leaveApplication.CreatedBy,
                        Machine: _currentUserService.MachineName,
                        IP_Address: _currentUserService.IpAddress,
                        screen: "LeaveApplication",
                        action: "User Created a new Leave Application",
                        additionalInfo: $"LeaveApplicationNo: {leaveApplication.LeaveApplicationNo}"
                    );

                    return "Leave Application Created Successfully.";
                }
                else
                {
                    // UPDATE
                    leaveApplication.ModifiedBy = await _currentUserService.GetUsernameAsync();
                    leaveApplication.ModifiedDate = DateTime.Now;

                    // 🔹 Check if the leave was previously Rejected
                    var existingLeave = await _unitOfWork.LeaveApplications
                        .FirstOrDefaultAsync(l => l.LeaveAppID == leaveApplication.LeaveAppID);

                    if (existingLeave == null)
                        return "Leave Application not found.";

                    if (existingLeave.LeaveStatus == "Rejected")
                    {
                        // Reset approval status
                        leaveApplication.LeaveStatus = "Pending";
                        leaveApplication.Authorized = false;
                        leaveApplication.ApprovedBy = null;
                        leaveApplication.ApprovalDate = null;

                        leaveApplication.Level1 = false;
                        leaveApplication.Level2 = false;
                        leaveApplication.Level3 = false;

                        leaveApplication.Level1_Sign = null;
                        leaveApplication.Level2_sign = null;
                        leaveApplication.Level3_sign = null;
                    }

                    await _unitOfWork.LeaveApplications.UpdateAsync(leaveApplication);

                    // 🔹 Fetch existing child records
                    var existingRecords = (await _unitOfWork.DailyLeaveRecords
                        .GetAllAsync(r => r.LeaveApplicationID == leaveApplication.LeaveAppID))
                        .ToList();

                    // 🔹 Update or Insert children
                    foreach (var record in leaveApplication.DailyLeaveRecords)
                    {
                        record.LeaveApplicationID = leaveApplication.LeaveAppID;

                        if (record.DailyLeaveRecordID == 0)
                        {
                            await _unitOfWork.DailyLeaveRecords.CreateAsync(record);
                        }
                        else
                        {
                            var existing = existingRecords
                                .FirstOrDefault(r => r.DailyLeaveRecordID == record.DailyLeaveRecordID);
                            if (existing != null)
                            {
                                existing.Date = record.Date;
                                existing.IsWeekend = record.IsWeekend;
                                existing.DayType = record.DayType;

                                await _unitOfWork.DailyLeaveRecords.UpdateAsync(existing);
                            }
                        }
                    }

                    // 🔹 Delete records that were removed
                    var incomingIds = leaveApplication.DailyLeaveRecords
                        .Where(r => r.DailyLeaveRecordID > 0)
                        .Select(r => r.DailyLeaveRecordID)
                        .ToHashSet();

                    var toDelete = existingRecords
                        .Where(r => !incomingIds.Contains(r.DailyLeaveRecordID))
                        .ToList();

                    if (toDelete.Any())
                        await _unitOfWork.DailyLeaveRecords.DeleteRangeAsync(toDelete);

                    await _unitOfWork.SaveAsync();

                    //pooja commented this for correct leave balance after approved
                    //await UpdateEmployeeLeaveBalance(leaveApplication, isUpdate: true);

                    await _loggingService.LogUserAction(
                        UserName: leaveApplication.ModifiedBy,
                        Machine: _currentUserService.MachineName,
                        IP_Address: _currentUserService.IpAddress,
                        screen: "LeaveApplication",
                        action: "User Updated a Leave Application",
                        additionalInfo: $"LeaveAppID: {leaveApplication.LeaveAppID}"
                    );

                    return "Leave Application Updated Successfully.";
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "CreateOrUpdateLeaveApplicationAsync error");
                return "An unexpected error occurred while saving leave application.";
            }
        }



        private async Task UpdateEmployeeLeaveBalance(LeaveApplication leaveApplication, bool isUpdate)
        {
            // 🔹 Determine leave year
            var year = leaveApplication.FromDate.Year;

            // 🔹 Get leave balance for the employee and leave type for that year
            var balance = await _unitOfWork.EmployeeLeaveBalances
                .GetQueryable()
                .Where(b => b.EmployeeID == leaveApplication.EmployeeID &&
                            b.LeaveTypeId == leaveApplication.LeaveTypeId &&
                            b.Year == year)
                .FirstOrDefaultAsync();


            if (balance == null) return;

            if (isUpdate)
            {
                // 🔹 Sum all approved/pending leaves for the same year
                var totalTaken = await _unitOfWork.LeaveApplications
                    .GetQueryable()
             

                .Where(l => l.EmployeeID == leaveApplication.EmployeeID &&
                            l.LeaveTypeId == leaveApplication.LeaveTypeId &&
                            l.LeaveStatus == "Approved" &&
                            l.FromDate.Year == year)
                .SumAsync(l => l.TotalDays);

                balance.LeavesTaken = totalTaken;
                balance.Balance = balance.TotalAllocatedLeave - totalTaken;
            }
            else
            {
                // 🔹 New leave application — just adjust the balance
                balance.LeavesTaken += leaveApplication.TotalDays;
                balance.Balance -= leaveApplication.TotalDays;
            }

            await _unitOfWork.EmployeeLeaveBalances.UpdateAsync(balance);
            await _unitOfWork.SaveAsync();
        }


        // Moved from component
        public void RecalculateTotalDays(LeaveApplication leaveApplication)
        {
            double totalDays = leaveApplication.DailyLeaveRecords
                .Where(r => !r.IsWeekend)
                .Sum(r => r.DayType == "Full Day" ? 1.0 : r.DayType == "Half Day" ? 0.5 : 0);

            leaveApplication.TotalDays = (decimal)totalDays;
        }

        public async Task<List<EmployeeLeaveBalance>> GetLeaveHistoryByStaffIdAsync(int employeeId)
        {
            return await _unitOfWork.EmployeeLeaveBalances
                .GetQueryable()
                .Where(x => x.EmployeeID == employeeId)
                .ToListAsync();
        }
    }
}
