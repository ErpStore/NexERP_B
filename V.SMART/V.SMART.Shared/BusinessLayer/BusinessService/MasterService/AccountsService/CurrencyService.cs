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

        public Task<(List<CurrencyVM> currencyVMs, int TotalCount)> SearchWithDynamicFilterAsync(
            int pageNumber,
            int pageSize,
            Dictionary<string, object>? filters)
            // M2-B02: unchanged signature, unchanged behaviour. It delegates to the sort-aware
            // overload with sort: null, which takes exactly the previous ordering path
            // (OrderByDescending(x => x.CurrId)). Existing callers — CurrencyList.razor:344-348 —
            // are untouched.
            => SearchWithDynamicFilterAsync(pageNumber, pageSize, filters, sort: null);

        /// <summary>
        /// M2-B02 — the paged search with an explicit <paramref name="sort"/>. See
        /// <c>ICurrencyService</c> for the contract; ordering is delegated to
        /// <see cref="CurrencySortBuilder"/>, and a <c>null</c>/empty sort keeps the historical
        /// <c>OrderByDescending(x =&gt; x.CurrId)</c> exactly.
        /// </summary>
        public async Task<(List<CurrencyVM> currencyVMs, int TotalCount)> SearchWithDynamicFilterAsync(
            int pageNumber,
            int pageSize,
            Dictionary<string, object>? filters,
            string? sort)
        {
            // Parsed OUTSIDE the try on purpose: an unsupported sort field is a wiring defect and
            // must surface as itself, not be relabelled "Failed to load Currency list." by the
            // catch below. Failing loudly here is the whole reason sort is not smuggled through
            // the filter dictionary, where CurrencyFilterBuilder's `_ => query` would discard it.
            var sortTerms = CurrencySortBuilder.Parse(sort);

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

                var list = await CurrencySortBuilder
                    .ApplyOrder(query, sortTerms)
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

        /// <summary>
        /// M2-B02 — the ordering counterpart of <see cref="CurrencyFilterBuilder"/>. New class; the
        /// filter builder above is untouched, because its predicates are the behaviour being
        /// preserved.
        ///
        /// <para>Sort is a comma-separated list of camel-case field names, each optionally prefixed
        /// with <c>-</c> for descending. The mapping below is an explicit allow-list — a
        /// <c>switch</c> over string literals, never reflection over property names — so the set of
        /// sortable columns is a reviewable compile-time fact.</para>
        ///
        /// <para><b>An unknown field throws.</b> That is the opposite of the filter builder's
        /// <c>_ =&gt; query</c>, and it is deliberate: a request that silently sorts nothing while
        /// answering 200 is worse than one that fails. The API layer validates the field names
        /// against <c>CurrencyQuery.Sortable</c> and answers 400 before calling, so this throw is
        /// reachable only from a mis-wired caller.</para>
        /// </summary>
        public static class CurrencySortBuilder
        {
            /// <summary>The sortable field names, camel-case as they appear on the wire.</summary>
            public static readonly IReadOnlyList<string> SortableFields = new[]
            {
                "currId", "currName", "currSub", "symbol", "isSystemDefined", "createdBy", "createdDate"
            };

            /// <summary>
            /// Splits and validates a sort expression. A null/whitespace value yields an empty list,
            /// which <see cref="ApplyOrder"/> reads as "keep the historical default ordering".
            /// </summary>
            /// <exception cref="ArgumentException">A term names a field that is not sortable.</exception>
            public static IReadOnlyList<(string Field, bool Descending)> Parse(string? sort)
            {
                if (string.IsNullOrWhiteSpace(sort))
                    return Array.Empty<(string, bool)>();

                var terms = new List<(string Field, bool Descending)>();

                foreach (var raw in sort.Split(',', StringSplitOptions.TrimEntries))
                {
                    var descending = raw.StartsWith('-');
                    var name = descending ? raw[1..].Trim() : raw.Trim();

                    var canonical = SortableFields.FirstOrDefault(
                        f => string.Equals(f, name, StringComparison.OrdinalIgnoreCase));

                    if (canonical is null)
                    {
                        throw new ArgumentException(
                            $"Unsupported Currency sort field '{name}'. Permitted values: "
                            + string.Join(", ", SortableFields) + ".",
                            nameof(sort));
                    }

                    terms.Add((canonical, descending));
                }

                return terms;
            }

            /// <summary>
            /// Applies the parsed terms. With no terms the ordering is
            /// <c>OrderByDescending(x =&gt; x.CurrId)</c> — the ordering this service has always
            /// used, preserved verbatim so an unsorted request is unchanged.
            /// </summary>
            public static IOrderedQueryable<Currency> ApplyOrder(
                IQueryable<Currency> query,
                IReadOnlyList<(string Field, bool Descending)> terms)
            {
                if (terms == null || terms.Count == 0)
                    return query.OrderByDescending(x => x.CurrId);

                IOrderedQueryable<Currency>? ordered = null;

                foreach (var (field, descending) in terms)
                {
                    ordered = ordered is null
                        ? Order(query, field, descending)
                        : Then(ordered, field, descending);
                }

                // Paging over a non-unique sort key can otherwise repeat or drop rows between
                // pages, because SQL Server is free to break ties differently per query. CurrId is
                // the primary key, so appending it makes every page deterministic.
                return terms.Any(t => string.Equals(t.Field, "currId", StringComparison.OrdinalIgnoreCase))
                    ? ordered!
                    : ordered!.ThenByDescending(x => x.CurrId);
            }

            private static IOrderedQueryable<Currency> Order(IQueryable<Currency> q, string field, bool desc) => field switch
            {
                "currId" => desc ? q.OrderByDescending(x => x.CurrId) : q.OrderBy(x => x.CurrId),
                "currName" => desc ? q.OrderByDescending(x => x.CurrName) : q.OrderBy(x => x.CurrName),
                "currSub" => desc ? q.OrderByDescending(x => x.CurrSub) : q.OrderBy(x => x.CurrSub),
                "symbol" => desc ? q.OrderByDescending(x => x.Symbol) : q.OrderBy(x => x.Symbol),
                "isSystemDefined" => desc ? q.OrderByDescending(x => x.IsSystemDefined) : q.OrderBy(x => x.IsSystemDefined),
                "createdBy" => desc ? q.OrderByDescending(x => x.CreatedBy) : q.OrderBy(x => x.CreatedBy),
                "createdDate" => desc ? q.OrderByDescending(x => x.CreatedDate) : q.OrderBy(x => x.CreatedDate),
                _ => throw new ArgumentException($"Unsupported Currency sort field '{field}'.", nameof(field))
            };

            private static IOrderedQueryable<Currency> Then(IOrderedQueryable<Currency> q, string field, bool desc) => field switch
            {
                "currId" => desc ? q.ThenByDescending(x => x.CurrId) : q.ThenBy(x => x.CurrId),
                "currName" => desc ? q.ThenByDescending(x => x.CurrName) : q.ThenBy(x => x.CurrName),
                "currSub" => desc ? q.ThenByDescending(x => x.CurrSub) : q.ThenBy(x => x.CurrSub),
                "symbol" => desc ? q.ThenByDescending(x => x.Symbol) : q.ThenBy(x => x.Symbol),
                "isSystemDefined" => desc ? q.ThenByDescending(x => x.IsSystemDefined) : q.ThenBy(x => x.IsSystemDefined),
                "createdBy" => desc ? q.ThenByDescending(x => x.CreatedBy) : q.ThenBy(x => x.CreatedBy),
                "createdDate" => desc ? q.ThenByDescending(x => x.CreatedDate) : q.ThenBy(x => x.CreatedDate),
                _ => throw new ArgumentException($"Unsupported Currency sort field '{field}'.", nameof(field))
            };
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
