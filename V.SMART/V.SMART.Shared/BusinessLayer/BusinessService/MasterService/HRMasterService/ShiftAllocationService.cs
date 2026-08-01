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
    public class ShiftAllocationService : IShiftAllocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _userService;
        private readonly ICommonService _commonService;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public ShiftAllocationService(IUnitOfWork unitOfWork, IMapper mapper, ILoggingService loggingService,
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

        public async Task<(List<ShiftAllocationVM> shiftAllocationVMs, int TotalCount)> 
            SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.ShiftAllocations
                    .GetQueryable()
                    .AsNoTracking();

                // Apply Filters
                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = ShiftAllocationFilterBuilder
                            .ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.ShiftId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<ShiftAllocationVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Shift Allocation)");
                throw new InvalidOperationException("Failed to load Shift Allocation list.", ex);
            }
        }

        public static class ShiftAllocationFilterBuilder
        {
            public static IQueryable<ShiftAllocation> ApplyFilter(
                IQueryable<ShiftAllocation> query,
                string field,
                object value)
            {
                if (value == null) return query;

                var val = value.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(val))
                    return query;

                switch (field)
                {
                    case "ShiftCode":
                        return query.Where(x =>
                            x.ShiftCode != null &&
                            EF.Functions.Like(x.ShiftCode, $"%{val}%"));

                    case "ShiftName":
                        return query.Where(x =>
                            x.ShiftName != null &&
                            EF.Functions.Like(x.ShiftName, $"%{val}%"));

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


        public async Task<(bool CanDelete, string Message)> CanDeleteShiftAllocationAsync(int shiftId)
        {
            try
            {
                var shiftAllocation = await _unitOfWork.ShiftAllocations
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.ShiftId == shiftId);

                if (shiftAllocation == null)
                    return (false, "Shift Allocation not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<ShiftAllocation>(shiftId);

                if (usedIn != null)
                    return (false, $"Cannot delete Shift Allocation '{shiftAllocation.ShiftName}' because it is used in {usedIn} Screen.");

                if (shiftAllocation.CreatedBy == "System")
                    return (false, $"The Shift Allocation '{shiftAllocation.ShiftName}' cannot be deleted because it is a system-generated record.");


                return (true, $"Shift Code '{shiftAllocation.ShiftCode}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Shift Allocation delete validation failed: {shiftId}");
                return (false, "Unexpected error occurred while validating Shift Allocation.");
            }
        }


        public async Task<bool> DeleteShiftAllocationByShiftIdAsync(int shiftId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var shiftAllocation = await _unitOfWork.ShiftAllocations
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.ShiftId == shiftId);

                if (shiftAllocation == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                // Delete the Machine
                await _unitOfWork.ShiftAllocations.DeleteAsync(shiftAllocation);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "Shift Allocation List",
                    action: $"Deleted Shift Name: {shiftAllocation.ShiftName}",
                    additionalInfo: $"Shift Id: {shiftAllocation.ShiftId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete ShiftId: {shiftId}");
                throw;
            }
        }




    }
}
