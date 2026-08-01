using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService
{
    public interface IRawMaterialService
    {
        Task<(List<RawMaterialVM> rawMaterialVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<(bool CanDelete, string Message)> CanDeleteRMAsync(int rmId);
        Task<bool> DeleteRmAsync(int rmId);
        Task<RawMaterialVM?> GetRawMaterialByIdAsync(int id);

        Task<bool> IsDuplicateRawMaterialAsync(string rmName);
        Task<RawMaterialVM> UpsertRMAsync(RawMaterialVM vm);
    }
}
