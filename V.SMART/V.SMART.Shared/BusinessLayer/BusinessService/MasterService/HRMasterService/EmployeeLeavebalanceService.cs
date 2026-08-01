using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IHRMasterservice;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.HRMasterService
{
    public class EmployeeLeavebalanceService : IEmployeeLeavebalanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ForeignKeyUsageChecker _fkChecker;
        public EmployeeLeavebalanceService(
            IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService,
            IMapper mapper,
            ForeignKeyUsageChecker fkChecker)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _fkChecker = fkChecker;
        }


        public async Task<(List<EmployeeLeaveBalanceVM> leaveBalanceVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.EmployeeLeaveBalances
                    .GetQueryable()
                    .Include(x => x.Staff)
                    .Include(x => x.LeaveType)
                    .AsNoTracking();

                // Apply Filters
                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = LeaveTypeFilterBuilder
                            .ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.LeaveBalanceID)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<EmployeeLeaveBalanceVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Employee Leave Balance)");
                throw new InvalidOperationException("Failed to load Employee Leave Balance list.", ex);
            }
        }

        public static class LeaveTypeFilterBuilder
        {
            public static IQueryable<EmployeeLeaveBalance> ApplyFilter(
                IQueryable<EmployeeLeaveBalance> query, string field, object value)
            {
                if (value == null) return query;

                switch (field)
                {
                    case "Leave Year":
                        if (int.TryParse(value.ToString(), out int year))
                            return query.Where(x => x.Year == year);
                        break;

                    case "LeaveType":
                        return query.Where(x => x.LeaveType.LeaveName.Contains(value.ToString()));

                    case "EmployeeName":
                        return query.Where(x => x.Staff.StaffName.Contains(value.ToString()));

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(value.ToString()));

                    case "FromDate":
                        return query.Where(x => x.CreatedDate >= DateTime.Parse(value.ToString()));

                    case "ToDate":
                        return query.Where(x => x.CreatedDate <= DateTime.Parse(value.ToString()));
                }

                return query;
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteEmpLeaveBalAsync(int leaveBalId)
        {
            try
            {
                var employeeLeaveBalance = await _unitOfWork.EmployeeLeaveBalances
                    .GetQueryable()
                    .Include(x => x.Staff)
                    .FirstOrDefaultAsync(s => s.LeaveBalanceID == leaveBalId);

                if (employeeLeaveBalance == null)
                    return (false, "Employee Leave Balance not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<EmployeeLeaveBalance>(leaveBalId);

                if (usedIn != null)
                    return (false, $"Cannot delete Employee Leave balance '{employeeLeaveBalance.Staff.StaffName}' because it is used in {usedIn}.");

                return (true, $"Employee Leave Balance '{employeeLeaveBalance.Staff.StaffName}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Employee Leave balance delete validation failed: {leaveBalId}");
                return (false, "Unexpected error occurred while validating Employee Leave balance.");
            }
        }


        public async Task<bool> DeleteEmpLeaveBalAsync(int leaveBalId)
        {
            try
            {
                var empLeavebal = await _unitOfWork.EmployeeLeaveBalances.GetAsync(leaveBalId);

                if (empLeavebal == null)
                    return false;

                await _unitOfWork.EmployeeLeaveBalances.DeleteAsync(empLeavebal);
                await _unitOfWork.SaveAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Employee Leave balance List",
                    action: $"Deleted Employee Leave Balance: {empLeavebal.Staff.StaffName}",
                    additionalInfo: $"Employee Name: {empLeavebal.Staff.StaffName}"
                );
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error deleting Process with LeavebalId {leaveBalId}");
                return false;
            }
        }

        public async Task<EmployeeLeaveBalanceVM?> GetBalanceAsync(int employeeId, int leaveTypeId, int year)
        {
            var entity = await _unitOfWork.EmployeeLeaveBalances
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.EmployeeID == employeeId &&
                    x.LeaveTypeId == leaveTypeId &&
                    x.Year == year);

            if (entity == null)
                return null;

            return _mapper.Map<EmployeeLeaveBalanceVM>(entity);
        }

       
        public async Task UpdateLeaveBalanceFromAttendanceAsync(int year)
        {
            var balances = await _unitOfWork.EmployeeLeaveBalances
                .GetQueryable()
                .Where(x => x.Year == year)
                .ToListAsync();

            foreach (var bal in balances)
            {
                if (!bal.LeaveTypeId.HasValue)
                    continue;

                var leaveType = await _unitOfWork.LeaveTypes.GetAsync(bal.LeaveTypeId.Value);

                if (leaveType == null)
                    continue;

                string code = leaveType.AttendanceCode.Trim().ToUpper();

                var attendances = await _unitOfWork.Attendances
                    .GetQueryable()
                    .Where(x => x.StaffId == bal.EmployeeID &&
                                x.Year == year)
                    .ToListAsync();

                decimal taken = 0m;

                foreach (var att in attendances)
                {
                    if (string.IsNullOrWhiteSpace(att.Ldetail))
                        continue;

                    foreach (var value in att.Ldetail.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        string status = value.Trim().ToUpper();

                        // Full-day leave
                        if (status == code)
                        {
                            if (code == "L")
                            {
                                // Only one L can be deducted from balance in a month
                                if (taken < 1m)
                                    taken += 1m;
                            }
                            else
                            {
                                // SL, CL, EL... normal calculation
                                taken += 1m;
                            }
                        }
                        // Half-day leave entered as 1/2L
                        //else if (status == $"1/2{code}")
                        //{
                        //    taken += 0.5m;
                        //}
                        else if (status == $"1/2{code}")
                        {
                            if (code == "L")
                            {
                                if (taken < 1m)
                                {
                                    taken += 0.5m;

                                    if (taken > 1m)
                                        taken = 1m;
                                }
                            }
                            else
                            {
                                taken += 0.5m;
                            }
                        }
                        // Half-day leave entered as M
                        //else if (code == "L" && status == "M")
                        //{
                        //    taken += 0.5m;
                        //}
                        else if (code == "L" && status == "M")
                        {
                            if (taken < 1m)
                            {
                                taken += 0.5m;

                                if (taken > 1m)
                                    taken = 1m;
                            }
                        }
                    }
                }

                bal.LeavesTaken = taken;
                bal.Balance = Math.Max(0m, bal.TotalAllocatedLeave - taken);

                await _unitOfWork.EmployeeLeaveBalances.UpdateAsync(bal);
            }

            await _unitOfWork.SaveAsync();
        }
    }
}
