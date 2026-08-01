using AutoMapper;
using DocumentFormat.OpenXml.Vml.Office;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMaintenanceService;
using V.SMART.Shared.Data.Maintenance.BreakdownMaintenance;
using V.SMART.Shared.Data.Maintenance.MaintenanceSchedule;
using V.SMART.Shared.Repository;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MaintenanceViewModel.BreakdownMaintenanceVM;
using V.SMART.Shared.ViewModels.MaintenanceViewModel.MaintenanceScheduleVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MaintenanceService
{
    public class BreakdownMaintenanceService : IBreakdownMaintenanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        public BreakdownMaintenanceService(
                                          IUnitOfWork unitOfWork,
                                          ICommonService commonService,
                                          CurrentUserService userService,
                                          ILoggingService logs,
                                          IMapper mapper,
                                          ILoggingService js)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
        }

        #region GetBreakdownMaintenanceById
        public async Task<BreakdownMaintenanceVM> GetBreakdownMaintenanceById(int id)
        {
            try
            {
                var Entity = await _unitOfWork.BreakdownMaintenances.GetAsync(id);
                var MaintenanceVMs = _mapper.Map<BreakdownMaintenanceVM>(Entity);
                return MaintenanceVMs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in Loading GetBreakdownMaintenanceById");
                return new BreakdownMaintenanceVM();
            }
        }
        #endregion

        #region Find Duplicate MaintenanceRecords
        public async Task<bool> FindDuplicateMaintenanceRecords(int machineId, DateTime breakdownDate, int? excludeId = null)
        {

            try
            {
                return await _unitOfWork.BreakdownMaintenances.GetQueryable()
                                      .AnyAsync(b => b.MachineId == machineId
                                                     && b.BreakdownDate.Date == breakdownDate.Date
                                                     && (excludeId == null || b.BreakdownId != excludeId));

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in Loading FindDuplicateMaintenanceRecords");
                return false;
            }
        }

        #endregion

        #region Create BreakdownMaintenance
        public async Task CreateBreakdownMaintenance(BreakdownMaintenanceVM breakdownMaintenance)
        {
            try
            {
                if (breakdownMaintenance == null)
                {
                    throw new ArgumentNullException(nameof(breakdownMaintenance));
                }
                BreakdownMaintenance scheduleEntity = _mapper.Map<BreakdownMaintenance>(breakdownMaintenance);
                await _unitOfWork.BreakdownMaintenances.CreateAsync(scheduleEntity);
                await _unitOfWork.SaveAsync();


            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in Creating Breakdown Maintenance");

            }
        }
        #endregion

        #region UpdateBreakdownMaintenance
        public async Task<BreakdownMaintenanceVM?> UpdateBreakdownMaintenanceAsync(
                    BreakdownMaintenanceVM breakdownMaintenance)
        {
            try
            {
                if (breakdownMaintenance == null)
                    throw new ArgumentNullException(nameof(breakdownMaintenance));

                var entity = _mapper.Map<BreakdownMaintenance>(breakdownMaintenance);

                var updatedEntity = await _unitOfWork
                    .BreakdownMaintenances
                    .UpdateAsync(entity);

                await _unitOfWork.SaveAsync();

                return _mapper.Map<BreakdownMaintenanceVM>(updatedEntity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in Updating Breakdown Maintenance");
                return null;
            }
        }

        #endregion

        #region Delete BreakdownMaintenance
        public async Task<bool> DeleteBreakdownMaintenance(int id)
        {
            try
            {
                var result = await _unitOfWork.BreakdownMaintenances.DeleteAsync(id);
                if (result)
                {
                    await _unitOfWork.SaveAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in Deleting Breakdown Maintenance");
                return false;
            }
        }
        #endregion

        public async Task<(List<BreakdownMaintenanceVM> BreakdownVMs, int TotalCount)>
                 SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.BreakdownMaintenances
                    .GetQueryable()
                    .Include(x => x.Machine)
                    .AsQueryable();

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = BreakdownFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.BreakdownId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<BreakdownMaintenanceVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync");
                throw;
            }
        }

        public static class BreakdownFilterBuilder
        {
            public static IQueryable<BreakdownMaintenance> ApplyFilter(
                IQueryable<BreakdownMaintenance> query,
                string field,
                object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString()!.Trim();

                switch (field)
                {
                    case "Machine":
                        return query.Where(x =>
                            x.Machine != null &&
                            x.Machine.MachineName.Contains(val));

                    case "Breakdown":
                        return query.Where(x => x.BreakdownDescription != null && x.BreakdownDescription.Contains(val));

                    case "Resolution":
                        return query.Where(x => x.ResolutionDescription != null && x.ResolutionDescription.Contains(val));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.BreakdownDate >= fromDate.Date);
                        return query;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x =>
                                x.BreakdownDate <= toDate.Date.AddDays(1).AddTicks(-1));
                        return query;

                    case "CreatedBy":
                        return query.Where(x =>
                            x.CreatedBy != null &&
                            x.CreatedBy.Contains(val));
                }

                return query;
            }
        }

        public async Task<Decimal> GetMachinePrice(int machineId)
        {
            try
            {
                var machine = await _unitOfWork.Machines.GetAsync(machineId);
                if (machine != null)
                {
                    return machine.MachineHrRate;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetMachinePrice");
                throw;
            }

        }

    }
}
