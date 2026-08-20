using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAccountsService
{
    public interface ICurrencyService
    {
        Task<(List<CurrencyVM> currencyVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        /// <summary>
        /// M2-B02 — the same paged search, with an explicit <paramref name="sort"/>.
        ///
        /// <para><b>Additive overload, not a replacement.</b> The three-argument member above is
        /// unchanged and every existing caller (e.g. <c>CurrencyList.razor:344-348</c>) still
        /// binds to it; it now delegates here with <c>sort: null</c>, so a request without a sort
        /// produces byte-identical results to before.</para>
        ///
        /// <para><paramref name="sort"/> is a comma-separated list of camel-case field names, each
        /// optionally prefixed with <c>-</c> for descending (<c>-createdDate,currName</c>).
        /// <c>null</c> or empty means "keep this service's existing default ordering". An
        /// unrecognised field name <b>throws</b> (see <c>CurrencyService.CurrencySortBuilder</c>)
        /// — deliberately, because the rejected alternative of passing sort through the filter
        /// dictionary would be swallowed by <c>_ =&gt; query</c> and would appear to succeed while
        /// sorting nothing. The API validates against a per-resource allow-list and answers 400
        /// first, so a throw here is a wiring defect, not a user error. ADR-002 §2 addendum
        /// (M2-B02).</para>
        /// </summary>
        Task<(List<CurrencyVM> currencyVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters, string? sort);

        Task<CurrencyVM?> GetByIdAsync(int currId);
        Task<(bool Success, string Message, CurrencyVM? Currency)> CreateAsync(CurrencyVM vm);
        Task<(bool Success, string Message, CurrencyVM? Currency)> UpdateAsync(int currId, CurrencyVM vm);
        Task<(bool CanDelete, string Message)> CanDeleteCurrencyAsync(int id);
        Task<bool> DeleteCurrencyByCurrIdAsync(int currId);
    }
}
