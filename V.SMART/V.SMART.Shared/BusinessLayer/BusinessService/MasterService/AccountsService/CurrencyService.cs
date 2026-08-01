

using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAccountsService;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.AccountsService
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _userService;
        private readonly ICommonService _commonService;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public CurrencyService(IUnitOfWork unitOfWork, IMapper mapper, ILoggingService loggingService,
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

        public async Task<(List<CurrencyVM> currencyVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.Currencyis
                            .GetQueryable()
                            .AsNoTracking();

                // Apply Filters
                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = CurrencyFilterBuilder
                            .ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.CurrId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<CurrencyVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Currency)");
                throw new InvalidOperationException("Failed to load Income list.", ex);
            }
        }

        public static class CurrencyFilterBuilder
        {
            public static IQueryable<Currency> ApplyFilter(
                IQueryable<Currency> query,
                string field,
                object value)
            {
                if (value == null) return query;

                var val = value.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(val))
                    return query;

                switch (field)
                {
                    case "CurrName":
                        return query.Where(x =>
                            x.CurrName != null &&
                            EF.Functions.Like(x.CurrName, $"%{val}%"));

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


        public async Task<(bool CanDelete, string Message)> CanDeleteCurrencyAsync(int id)
        {
            try
            {
                var currency = await _unitOfWork.Currencyis
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.CurrId == id);

                if (currency == null)
                    return (false, "Currency not found or already removed.");

                if (currency.IsSystemDefined)
                    return (false, $"'{currency.CurrName}' is a system-defined store and cannot be deleted.");

                var usedIn = await _fkChecker.GetUsageTableAsync<Currency>(id);

                if (usedIn != null)
                    return (false, $"Cannot delete Currency '{currency.CurrName}' because it is used in {usedIn} Screen.");

                return (true, $"Currency '{currency.CurrId}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Currency delete validation failed: {id}");
                return (false, "Unexpected error occurred while validating Currency.");
            }
        }


        public async Task<bool> DeleteCurrencyByCurrIdAsync(int currId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var currency = await _unitOfWork.Currencyis
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.CurrId == currId);

                if (currency == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                await _unitOfWork.Currencyis.DeleteAsync(currency);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "Currency List",
                    action: $"Deleted Currency: {currency.CurrName}",
                    additionalInfo: $"Currency Id: {currency.CurrId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Currency Code: {currId}");
                throw;
            }
        }







    }
}
