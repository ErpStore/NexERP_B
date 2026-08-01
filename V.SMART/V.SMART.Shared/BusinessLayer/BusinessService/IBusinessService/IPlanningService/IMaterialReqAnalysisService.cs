using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.PlanningViewModel.MaterialReqAnalysisVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService
{
    public interface IMaterialReqAnalysisService
    {
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int? storeId);
        Task<List<MfgPo>> GetPendingPOsByCustomersAsync(List<int> custIds);
        Task<List<MRAPoItems>> GetPOItemsAsync(List<int> poIds, int? storeId = null);
        Task<List<BOMItem>> GetBOMHierarchyAsync(int assyId);
        Task<List<CompMasterVM>> GetRawMaterialsForComponentAsync(int componentItemId);
        Task<MaterialReq> AutoGenerateMaterialReqAsync(
                    string selectedFilter,
                    List<MRAPoItems> poItems,
                    List<MRAPoSubItemsVM> mraPOSubItems);
        Task<Dictionary<int, List<BOMItem>>> GetBOMHierarchiesBatchAsync(IEnumerable<int> itemIds);
        Task<Dictionary<int, List<CompMasterVM>>> GetRawMaterialsForComponentsBatchAsync(IEnumerable<int> componentIds);
        Task<bool> IsItemROlDisplayEnabledAsync();

        Task<List<CustomerVM>> GetAllActiveCustomersAsync();
        Task<List<BOMItem>> GetDirectBOMItemsAsync(int assyId);
    }
}
