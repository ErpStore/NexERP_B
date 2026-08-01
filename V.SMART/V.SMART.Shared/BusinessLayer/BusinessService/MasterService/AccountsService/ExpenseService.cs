using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAccountsService;
using V.SMART.Shared.Data.Master.Accounts;
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
    public class ExpenseService : IExpenseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _userService;
        private readonly ICommonService _commonService;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public ExpenseService(IUnitOfWork unitOfWork, IMapper mapper, ILoggingService loggingService,
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


        public async Task<(List<ExpenseVM> expenseVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.Expenses
                            .GetQueryable()
                            .AsNoTracking();

                // Apply Filters
                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = ExpenseFilterBuilder
                            .ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.ExpenseCode)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<ExpenseVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Expense)");
                throw new InvalidOperationException("Failed to load Expense list.", ex);
            }
        }

        public static class ExpenseFilterBuilder
        {
            public static IQueryable<Expense> ApplyFilter(
                IQueryable<Expense> query,
                string field,
                object value)
            {
                if (value == null) return query;

                var val = value.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(val))
                    return query;

                switch (field)
                {
                    case "ExpenseName":
                        return query.Where(x =>
                            x.ExpenseName != null &&
                            EF.Functions.Like(x.ExpenseName, $"%{val}%"));

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

        public async Task<(bool CanDelete, string Message)> CanDeleteExpenseAsync(int id)
        {
            try
            {
                var expense = await _unitOfWork.Expenses
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.ExpenseCode == id);

                if (expense == null)
                    return (false, "Expense not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<Expense>(id);

                if (usedIn != null)
                    return (false, $"Cannot delete Expense '{expense.ExpenseName}' because it is used in {usedIn} Screen.");

                return (true, $"Expense '{expense.ExpenseCode}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Expense delete validation failed: {id}");
                return (false, "Unexpected error occurred while validating Expense.");
            }
        }


        public async Task<bool> DeleteExpenseByExpenseCodeAsync(int expenseCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var expense = await _unitOfWork.Expenses
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.ExpenseCode == expenseCode);

                if (expense == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                // Delete the Machine
                await _unitOfWork.Expenses.DeleteAsync(expense);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "Expense List",
                    action: $"Deleted Expense: {expense.ExpenseName}",
                    additionalInfo: $"Expense Id: {expense.ExpenseCode}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Expense Code: {expenseCode}");
                throw;
            }
        }








    }
}
