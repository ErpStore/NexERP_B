using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.PlanningViewModel.AssyReqAnalysisVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService
{
    public interface IAssemblyRequirementService
    {
        Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int? storeId);
        Task<List<ItemVM>> GetAllItemsByCategoryCode(int Id);

        Task<List<AssemblyDefVM>> GetAllAssemblyItemsAsync();

        Task<List<AssemblyDefVM>> GetAssemblyByItemIdAsync(int itemID);

        List<TreeNode> BuildBomTree(int parentAssemblyId, List<AssemblyDefVM> rows, int level = 1);

        Task<Dictionary<int, decimal>> GetItemRatesByItemIdsAsync(List<int> itemIds);
    }
}
