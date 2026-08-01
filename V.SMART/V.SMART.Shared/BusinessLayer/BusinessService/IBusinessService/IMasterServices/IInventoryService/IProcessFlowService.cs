using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService
{
    public interface IProcessFlowService
    {
        Task<IEnumerable<ItemVM>> SearchComponentItemsAsync(string searchText);
        Task<(List<CompMasterVM> compMasterVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                     Dictionary<string, object>? filters);
        Task<List<CompMasterVM>> GetRawMaterialsByComponentId(int compId);
        Task<CompMasterVM?> GetCompMasterAsync(int compId);
        Task<bool> IsDuplicateCategoryNameAsync(string categoryName, int? categoryCode = null);
        Task<CompMasterVM> UpsertProcessflowAsync(CompMasterVM vm);
        Task<IEnumerable<Category>> GetAllCategoriesAsync(string? keyword = null);
        Task<bool> DeleteCategoryAsync(int categoryCode);
    }
}
