using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAccountsService;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;

namespace V.SMART.Api.Tests.Infrastructure
{
    /// <summary>
    /// A hand-written stand-in for <see cref="ICurrencyService"/>.
    ///
    /// It reproduces the real service's <b>signalling shape</b> — the two-element delete guard
    /// <c>(bool CanDelete, string Message)</c> and the three-element create/update tuple
    /// <c>(bool Success, string Message, CurrencyVM? Currency)</c> — and nothing else. No
    /// business logic is reimplemented here: the messages a test feeds in are copied verbatim
    /// from <c>CurrencyService.cs</c> at the call site, so the assertions are about transport,
    /// not about rules.
    /// </summary>
    internal sealed class FakeCurrencyService : ICurrencyService
    {
        public (List<CurrencyVM> Items, int TotalCount) SearchResult { get; set; } = (new List<CurrencyVM>(), 0);
        public CurrencyVM? GetByIdResult { get; set; }
        public (bool Success, string Message, CurrencyVM? Currency) CreateResult { get; set; } = (true, "Currency Created Successfully", null);
        public (bool Success, string Message, CurrencyVM? Currency) UpdateResult { get; set; } = (true, "Currency Updated Successfully", null);
        public (bool CanDelete, string Message) CanDeleteResult { get; set; } = (true, "ok");
        public bool DeleteResult { get; set; } = true;

        public Task<(List<CurrencyVM> currencyVMs, int TotalCount)> SearchWithDynamicFilterAsync(
            int pageNumber, int pageSize, Dictionary<string, object>? filters)
            => Task.FromResult((SearchResult.Items, SearchResult.TotalCount));

        public Task<CurrencyVM?> GetByIdAsync(int currId) => Task.FromResult(GetByIdResult);

        public Task<(bool Success, string Message, CurrencyVM? Currency)> CreateAsync(CurrencyVM vm)
            => Task.FromResult(CreateResult);

        public Task<(bool Success, string Message, CurrencyVM? Currency)> UpdateAsync(int currId, CurrencyVM vm)
            => Task.FromResult(UpdateResult);

        public Task<(bool CanDelete, string Message)> CanDeleteCurrencyAsync(int id)
            => Task.FromResult(CanDeleteResult);

        public Task<bool> DeleteCurrencyByCurrIdAsync(int currId) => Task.FromResult(DeleteResult);
    }
}
