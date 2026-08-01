using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAccountsService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IGeneralService;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.AccountsService
{

    public class CostCenterService : ICostCenterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _userService;
        private readonly ICommonService _commonService;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public CostCenterService(IUnitOfWork unitOfWork, IMapper mapper, ILoggingService loggingService,
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

        public async Task<(List<CostCenterVM> costCenterVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.CostCenters
                    .GetQueryable()
                    .Include(x=> x.ProjectTypeMaster)
                    .Include(x=> x.Customer)
                    .AsNoTracking();

                // Apply Filters
                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = CostCenterFilterBuilder
                            .ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<CostCenterVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Cost Cenetr)");
                throw new InvalidOperationException("Failed to load Cost Center list.", ex);
            }
        }

        public static class CostCenterFilterBuilder
        {
            public static IQueryable<CostCenter> ApplyFilter(
                IQueryable<CostCenter> query,
                string field,
                object value)
            {
                if (value == null) return query;

                var val = value.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(val))
                    return query;

                switch (field)
                {
                    case "ProjNo":
                        return query.Where(x =>
                            x.ProjectNo != null &&
                            EF.Functions.Like(x.ProjectNo, $"%{val}%"));

                    case "CostCenterName":
                        return query.Where(x =>
                            x.CostCenterName != null &&
                            EF.Functions.Like(x.CostCenterName, $"%{val}%"));

                    case "TypeOfProject":
                        return query.Where(x =>
                            x.ProjectTypeMaster != null &&
                            EF.Functions.Like(x.ProjectTypeMaster.TypeOfProject, $"%{val}%"));

                    case "Customer":
                        return query.Where(x =>
                            x.Customer != null &&
                            EF.Functions.Like(x.Customer.CustName, $"%{val}%"));

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
                            return query.Where(x => x.CreatedDate <= toDate.Date.AddDays(1).AddTicks(-1));
                        return query;

                    default:
                        return query;
                }
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteCostCenterAsync(int CostId)
        {
            try
            {
                var costCenter = await _unitOfWork.CostCenters
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.Id == CostId);

                if (costCenter == null)
                    return (false, "Cost Center not found or already removed.");

                if(costCenter.IsDispatch)
                    return (false, $"'{costCenter.CostCenterName}' is a dispatched project and cannot be deleted.");

                if(costCenter.IsClosed)
                    return (false, $"'{costCenter.CostCenterName}' is a closed project and cannot be deleted.");

                var usedIn = await _fkChecker.GetUsageTableAsync<CostCenter>(CostId);

                if (usedIn != null)
                    return (false, $"Cannot delete Cost Center '{costCenter.CostCenterName}' because it is used in {usedIn} Screen.");

                return (true, $"Cost Center '{costCenter.Id}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Cost Center delete validation failed: {CostId}");
                return (false, "Unexpected error occurred while validating CostCenter.");
            }
        }

        public async Task<bool> DeleteCostCenterByCostIdAsync(int costId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var costCenter = await _unitOfWork.CostCenters
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.Id == costId);

                if (costCenter == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                // Delete the Machine
                await _unitOfWork.CostCenters.DeleteAsync(costCenter);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "Cost Center List",
                    action: $"Deleted Cost Center: {costCenter.CostCenterName}",
                    additionalInfo: $"Cost Center Id: {costCenter.Id}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Cost Center: {costId}");
                throw;
            }
        }






    }

}
