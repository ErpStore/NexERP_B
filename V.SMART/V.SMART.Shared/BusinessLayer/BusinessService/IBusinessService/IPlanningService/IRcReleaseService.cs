using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService
{
    public interface IRcReleaseService
    {
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);

        Task<decimal> GetPendingRcBalQtyByPoSubIdAsync(int poSubId);
        Task<bool> IsRcReleaseTransactionsMatchedAsync(int RcreleaseId, RouteCardReleaseVM rcReleaseVMs);
        Task UpsertRcReleaseShortCloseAsync(RouteCardReleaseVM rcRelaseVM);
        Task UpdatedCancelStatusAndAddOrRevertQty(RouteCardReleaseVM RcReleaseVM);
        Task<List<RouteCardReleaseSub>> GetRcReleaseSubByRcReleaseIdAsync(int RcReleaseId);


        Task<decimal> GetExactRcReleaseQtyByRcReleaseIdAsync(int rcReleaseId);
        Task<RouteCardReleaseVM?> GetRcReleaseByRcIdAsync(int rcReleaseId);
        Task<RouteCardReleaseVM> UpsertRouteCardReleaseAsync(RouteCardReleaseVM releaseVM);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<List<MfgPoSubVM>> GetPendingPoItemsAsync(int poId);
        Task<List<MfgPoVM>> GetPendingPOsByCustomerAsync(int custId);
        Task<List<RouteCardReleaseSubVM>> GetAssemblyComponentsForReleaseAsync(int assemblyId, decimal releaseQty);
        Task<(List<RouteCardReleaseVM> rcReleaseVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<(bool CanDelete, string Message)> CanDeleteRcReleaseAsync(int rcReleaseId);
        Task<bool> DeleteRouteCardReleaseByRcReleaseIdAsync(int rcReleaseId);
        Task<string> GetRcReleaseNoAsync(string suffix);

        Task<List<CustomerVM>> GetAllActiveCustomersAsync();
    }
}
