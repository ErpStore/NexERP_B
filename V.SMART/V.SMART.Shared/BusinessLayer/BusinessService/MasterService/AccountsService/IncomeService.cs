using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAccountsService;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.AccountsService
{
    public class IncomeService : IIncomeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _userService;
        private readonly ICommonService _commonService;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public IncomeService(IUnitOfWork unitOfWork, IMapper mapper, ILoggingService loggingService,
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

        public async Task<(List<IncomeVM> incomeVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.Incomes
                            .GetQueryable()
                            .AsNoTracking();

                // Apply Filters
                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = IncomeFilterBuilder
                            .ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.IncomeCode)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<IncomeVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Income)");
                throw new InvalidOperationException("Failed to load Income list.", ex);
            }
        }

        public static class IncomeFilterBuilder
        {
            public static IQueryable<Income> ApplyFilter(
                IQueryable<Income> query,
                string field,
                object value)
            {
                if (value == null) return query;

                var val = value.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(val))
                    return query;

                switch (field)
                {
                    case "IncomeName":
                        return query.Where(x =>
                            x.IncomeName != null &&
                            EF.Functions.Like(x.IncomeName, $"%{val}%"));

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


        public async Task<(bool CanDelete, string Message)> CanDeleteIncomeAsync(int id)
        {
            try
            {
                var income = await _unitOfWork.Incomes
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.IncomeCode == id);

                if (income == null)
                    return (false, "Income not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<Income>(id);

                if (usedIn != null)
                    return (false, $"Cannot delete Income '{income.IncomeName}' because it is used in {usedIn} Screen.");

                return (true, $"Income '{income.IncomeCode}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Income delete validation failed: {id}");
                return (false, "Unexpected error occurred while validating Income.");
            }
        }


        public async Task<bool> DeleteIncomeByIncomeIdAsync(int incomeCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var income = await _unitOfWork.Incomes
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.IncomeCode == incomeCode);

                if (income == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                // Delete the Machine
                await _unitOfWork.Incomes.DeleteAsync(income);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "Income List",
                    action: $"Deleted Income: {income.IncomeName}",
                    additionalInfo: $"Income Id: {income.IncomeCode}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Income Code: {incomeCode}");
                throw;
            }
        }







    }
}
