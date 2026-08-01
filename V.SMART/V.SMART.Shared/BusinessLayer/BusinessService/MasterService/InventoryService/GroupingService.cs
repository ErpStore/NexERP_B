using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.InventoryService
{
    public class GroupingService : IGroupingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ForeignKeyUsageChecker _fkChecker;
        public GroupingService(
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

        public async Task<(List<GroupingVM> groupingVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.Groupings
                    .GetQueryable()
                    .Include(x => x.GroupingSubs)
                        .ThenInclude(s => s.Factor)
                    .Include(x => x.GroupingSubs)
                        .ThenInclude(s => s.Process)
                    .AsQueryable();

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = GroupingFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.ID)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<GroupingVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Grouping)");
                throw new InvalidOperationException("Failed to load Grouping list.", ex);
            }
        }

        public static class GroupingFilterBuilder
        {
            public static IQueryable<Grouping> ApplyFilter(
                IQueryable<Grouping> query,
                string field,
                object value)
            {
                try
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return query;

                    string val = value.ToString()!.Trim();

                    switch (field)
                    {
                        case "FRCNo":
                            {
                                return query.Where(x => (string.IsNullOrEmpty(val) || x.FRCNo.StartsWith(val)));
                            }

                        case "ProcessName":
                            return query.Where(x =>
                                x.GroupingSubs.Any(s =>
                                    s.Process != null &&
                                    s.Process.ProcessName.Contains(val)));

                        case "ItemCode":
                            return query.Where(x =>
                                x.GroupingSubs.Any(s =>
                                    s.Factor != null &&
                                    s.Factor.Factors.Contains(val)));

                        case "CreatedBy":
                            return query.Where(x =>
                                x.CreatedBy != null &&
                                x.CreatedBy.Contains(val));

                        case "FromDate":
                            if (DateTime.TryParse(val, out var fromDate))
                                return query.Where(x => x.CreatedDate >= fromDate.Date);
                            return query;

                        case "ToDate":
                            if (DateTime.TryParse(val, out var toDate))
                                return query.Where(x =>
                                    x.CreatedDate <= toDate.Date.AddDays(1).AddTicks(-1));
                            return query;
                    }

                    return query;
                }
                catch
                {
                    return query;
                }
            }
           
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteGroupingAsync(int grouppingId)
        {
            try
            {
                var grouping = await _unitOfWork.Groupings
                                .GetQueryable()
                                .Include(e => e.GroupingSubs)
                                .Where(e => e.ID == grouppingId).FirstOrDefaultAsync();

                if (grouping == null)
                    return (true, "Groouping can be safely deleted.");

               
                return (true, "Grouping can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in CanDeleteGroupingAsync for GroupId: {grouppingId}");
                throw new Exception("Error checking Sales Grouping delete eligibility", ex);
            }
        }

        public async Task<bool> DeleteGroupingByGroupingIdAsync(int groupingId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get the quotation with its sub-items
                var grouping = await _unitOfWork.Groupings
                    .GetQueryable()
                    .Include(e => e.GroupingSubs)
                    .FirstOrDefaultAsync(e => e.ID == groupingId);

                if (grouping == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                await _unitOfWork.Groupings.DeleteAsync(grouping);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Grouping List",
                    action: $"Deleted FRC: {grouping.FRCNo}",
                    additionalInfo: $"Grouping Id: {grouping.ID}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to delete GroupingId: {groupingId}");
                throw;
            }
        }




    }
}
