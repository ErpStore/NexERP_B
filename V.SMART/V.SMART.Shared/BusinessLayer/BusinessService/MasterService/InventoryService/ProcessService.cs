using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.InventoryService
{
    public class ProcessService : IProcessService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ForeignKeyUsageChecker _fkChecker;
        public ProcessService(
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

        public async Task<(List<Process> processes, int TotalCount)>
            SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
            Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.Processes.GetQueryable();

            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = DynamicWhereBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.ProcessId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var list = entities;

            return (list, total);
        }


        public static class DynamicWhereBuilder
        {
            public static IQueryable<Process> ApplyFilter(
                IQueryable<Process> query, string field, object value)
            {
                if (value == null) return query;

                switch (field)
                {
                    case "ProcessName":
                        return query.Where(x => x.ProcessName.Contains(value.ToString()));

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

        public async Task<(bool CanDelete, string Message)> CanDeleteProcessAsync(int processId)
        {
            try
            {
                var process = await _unitOfWork.Processes
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.ProcessId == processId);

                if (process == null)
                    return (false, "Process not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<Process>(processId);

                if (usedIn != null)
                    return (false, $"Cannot delete store '{process.ProcessName}' because it is used in {usedIn}.");

                return (true, $"Process '{process.ProcessName}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Process delete validation failed: {processId}");
                return (false, "Unexpected error occurred while validating Process.");
            }
        }


        public async Task<bool> DeleteProcessAsync(int processId)
        {
            try
            {
                var process = await _unitOfWork.Processes.GetAsync(processId);

                if (process == null)
                    return false;

                await _unitOfWork.Processes.DeleteAsync(process);
                await _unitOfWork.SaveAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Process List",
                    action: $"Deleted Process: {process.ProcessName}",
                    additionalInfo: $"Process Name: {process.ProcessName}"
                );
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error deleting Process with Id {processId}");
                return false;
            }
        }


    }
}
