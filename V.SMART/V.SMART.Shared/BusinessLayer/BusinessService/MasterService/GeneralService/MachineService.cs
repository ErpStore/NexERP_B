using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IGeneralService;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.GeneralService
{
    public class MachineService : IMachineService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _userService;
        private readonly ICommonService _commonService;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public MachineService(IUnitOfWork unitOfWork, IMapper mapper, ILoggingService loggingService,
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

        public async Task<(List<MachineVM> machineVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.Machines
                    .GetQueryable()
                    .AsNoTracking();

                // Apply Filters
                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = MachineFilterBuilder
                            .ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.MachineId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<MachineVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Machine)");
                throw new InvalidOperationException("Failed to load Machine list.", ex);
            }
        }

        public static class MachineFilterBuilder
        {
            public static IQueryable<Machine> ApplyFilter(
                IQueryable<Machine> query,
                string field,
                object value)
            {
                if (value == null) return query;

                var val = value.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(val))
                    return query;

                switch (field)
                {
                    case "Machine":
                        return query.Where(x =>
                            x.MachineName != null &&
                            EF.Functions.Like(x.MachineName, $"%{val}%"));

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


        public async Task<(bool CanDelete, string Message)> CanDeleteMachineAsync(int machineId)
        {
            try
            {
                var machine = await _unitOfWork.Machines
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.MachineId == machineId);

                if (machine == null)
                    return (false, "Machine not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<Machine>(machineId);

                if (usedIn != null)
                    return (false, $"Cannot delete Machine '{machine.MachineName}' because it is used in {usedIn} Screen.");

                return (true, $"Machine '{machine.MachineId}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Customer delete validation failed: {machineId}");
                return (false, "Unexpected error occurred while validating Machine.");
            }
        }


        public async Task<bool> DeleteMachineByMachineIdAsync(int machineId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var machine = await _unitOfWork.Machines
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.MachineId == machineId);

                if (machine == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                // Delete the Machine
                await _unitOfWork.Machines.DeleteAsync(machine);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "Machine List",
                    action: $"Deleted Machine: {machine.MachineName}",
                    additionalInfo: $"Machine Id: {machine.MachineId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Machine: {machineId}");
                throw;
            }
        }




    }
}
