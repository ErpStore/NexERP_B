using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAccountsService;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.AccountsService
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _userService;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public CurrencyService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggingService loggingService,
            CurrentUserService userService,
            ForeignKeyUsageChecker foreignKeyUsageChecker)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logs = loggingService;
            _userService = userService;
            _fkChecker = foreignKeyUsageChecker;
        }

        public async Task<(List<CurrencyVM> currencyVMs, int TotalCount)> SearchWithDynamicFilterAsync(
            int pageNumber,
            int pageSize,
            Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.Currencyis
                            .GetQueryable()
                            .AsNoTracking();

                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = CurrencyFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
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
                throw new InvalidOperationException("Failed to load Currency list.", ex);
            }
        }

        public async Task<CurrencyVM?> GetByIdAsync(int currId)
        {
            var entity = await _unitOfWork.Currencyis.GetAsync(currId);
            return entity == null ? null : _mapper.Map<CurrencyVM>(entity);
        }

        public async Task<(bool Success, string Message, CurrencyVM? Currency)> CreateAsync(CurrencyVM vm)
        {
            try
            {
                bool isDuplicate = await _unitOfWork.Currencyis
                    .ExistsByNameAsync("CurrName", vm.CurrName, "CurrId", null);

                if (isDuplicate)
                    return (false, "Currency name already exists.", null);

                var entity = _mapper.Map<Currency>(vm);
                entity.CurrId = 0;
                entity.CreatedBy = await _userService.GetUsernameAsync();
                entity.CreatedDate = DateTime.Now;
                entity.ModifiedBy = null;
                entity.ModifiedDate = null;

                await _unitOfWork.Currencyis.CreateAsync(entity);
                await _unitOfWork.SaveAsync();

                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "Currency",
                    action: $"Currency Created Successfully {entity.CurrName}",
                    additionalInfo: $"CurrencyName: {entity.CurrName}");

                return (true, "Currency Created Successfully", _mapper.Map<CurrencyVM>(entity));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error creating Currency");
                throw;
            }
        }

        public async Task<(bool Success, string Message, CurrencyVM? Currency)> UpdateAsync(int currId, CurrencyVM vm)
        {
            try
            {
                var existing = await _unitOfWork.Currencyis.GetAsync(currId);
                if (existing == null)
                    return (false, "Currency not found.", null);

                if (existing.IsSystemDefined)
                    return (false, "You cannot modify a system-defined Currency.", null);

                bool isDuplicate = await _unitOfWork.Currencyis
                    .ExistsByNameAsync("CurrName", vm.CurrName, "CurrId", currId);

                if (isDuplicate)
                    return (false, "Currency name already exists.", null);

                existing.CurrName = vm.CurrName;
                existing.CurrSub = vm.CurrSub;
                existing.Symbol = vm.Symbol;
                existing.ModifiedBy = await _userService.GetUsernameAsync();
                existing.ModifiedDate = DateTime.Now;

                await _unitOfWork.Currencyis.UpdateAsync(existing);
                await _unitOfWork.SaveAsync();

                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "Currency",
                    action: $"Currency Updated Successfully {existing.CurrName}",
                    additionalInfo: $"CurrencyName: {existing.CurrName}");

                return (true, "Currency Updated Successfully", _mapper.Map<CurrencyVM>(existing));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error updating Currency: {currId}");
                throw;
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

                return field switch
                {
                    "CurrName" => query.Where(x =>
                        x.CurrName != null &&
                        EF.Functions.Like(x.CurrName, $"%{val}%")),
                    "CreatedBy" => query.Where(x =>
                        x.CreatedBy != null &&
                        EF.Functions.Like(x.CreatedBy, $"%{val}%")),
                    "FromDate" when DateTime.TryParse(val, out var fromDate)
                        => query.Where(x => x.CreatedDate >= fromDate.Date),
                    "ToDate" when DateTime.TryParse(val, out var toDate)
                        => query.Where(x =>
                            x.CreatedDate <= toDate.Date.AddDays(1).AddTicks(-1)),
                    _ => query
                };
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
                    return (false, $"'{currency.CurrName}' is a system-defined currency and cannot be deleted.");

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
                    return false;

                await _unitOfWork.Currencyis.DeleteAsync(currency);
                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "Currency List",
                    action: $"Deleted Currency: {currency.CurrName}",
                    additionalInfo: $"Currency Id: {currency.CurrId}");

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
