using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IHRMasterservice;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository;
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
    public class LeaveTypeService : ILeaveTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _userService;
        private readonly ICommonService _commonService;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public LeaveTypeService(IUnitOfWork unitOfWork, IMapper mapper, ILoggingService loggingService,
                            CurrentUserService userService, ICommonService commonService,
                            ForeignKeyUsageChecker foreignKeyUsageChecker)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logs = loggingService;
            _userService = userService;
            _commonService = commonService;
            _fkChecker = foreignKeyUsageChecker;
        }

        public async Task<(List<LeaveTypeVM> leaveTypeVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.LeaveTypes
                    .GetQueryable()
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
                    .OrderByDescending(x => x.LeaveTypeId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<LeaveTypeVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Machine)");
                throw new InvalidOperationException("Failed to load Machine list.", ex);
            }
        }

        public static class LeaveTypeFilterBuilder
        {
            public static IQueryable<LeaveType> ApplyFilter(
                IQueryable<LeaveType> query,
                string field,
                object value)
            {
                if (value == null) return query;

                var val = value.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(val))
                    return query;

                switch (field)
                {
                    case "LeaveName":
                        return query.Where(x =>
                            x.LeaveName != null &&
                            EF.Functions.Like(x.LeaveName, $"%{val}%"));

                    case "CreatedBy":
                        return query.Where(x =>
                            x.CreatedBy != null &&
                            EF.Functions.Like(x.CreatedBy, $"%{val}%"));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x =>
                                x.CreatedDate >= fromDate.Date);
                        return query;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x =>
                                x.CreatedDate <= toDate.Date
                                    .AddDays(1)
                                    .AddTicks(-1));
                        return query;

                    default:
                        return query;
                }
            }
        }


        public async Task<(bool CanDelete, string Message)> CanDeleteLeaveTypeAsync(int typeId)
        {
            try
            {
                var leaveType = await _unitOfWork.LeaveTypes
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.LeaveTypeId == typeId);

                if (leaveType == null)
                    return (false, "Leave Type not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<LeaveType>(typeId);

                if (usedIn != null)
                    return (false, $"Cannot delete Leave Type '{leaveType.LeaveName}' because it is used in {usedIn} Screen.");

                return (true, $"Leave Type Id '{leaveType.LeaveTypeId}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Leave Type delete validation failed: {typeId}");
                return (false, "Unexpected error occurred while validating Leave Type.");
            }
        }


        public async Task<bool> DeleteLeaveTypeByLeaveTypeIdAsync(int typeId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var leaveType = await _unitOfWork.LeaveTypes
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.LeaveTypeId == typeId);

                if (leaveType == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                // Delete the Machine
                await _unitOfWork.LeaveTypes.DeleteAsync(leaveType);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "Leave Type List",
                    action: $"Deleted Leave Type: {leaveType.LeaveName}",
                    additionalInfo: $"Leave Type Id: {leaveType.LeaveTypeId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Leave Type: {typeId}");
                throw;
            }
        }

        public async Task<LeaveTypeVM> GetByAttendanceCode(string code)
        {
            code = code.Trim().ToUpper();

            var data = await _unitOfWork.LeaveTypes.GetAllAsync();

            var leaveType = data
                .Where(x => !string.IsNullOrWhiteSpace(x.AttendanceCode) &&
                            x.AttendanceCode.Trim().ToUpper() == code)
                .Select(x => new LeaveTypeVM
                {
                    LeaveTypeId = x.LeaveTypeId,
                    LeaveName = x.LeaveName,
                    AttendanceCode = x.AttendanceCode,
                    PaidLeave = x.PaidLeave,
                    DeductSalary = x.DeductSalary
                })
                .FirstOrDefault();

            return leaveType;
        }
    }
}
