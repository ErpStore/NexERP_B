using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static V.SMART.Shared.BusinessLayer.BusinessService.MasterService.InventoryService.StoreService;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService
{
    public interface IStoreService
    {
        Task<(List<StoreVM> storeVMs, int TotalCount)>
            SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
            Dictionary<string, object>? filters);
        Task<(bool CanDelete, string Message)> CanDeleteStoreAsync(int storeId);
        Task<bool> DeleteStoreAsync(int storeId);

        Task<StoreVM?> GetStoreDataByStoreId(int storeId);
        Task<ServiceResult> UpsertStoreAsync(StoreVM vm);
    }
}
