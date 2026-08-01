using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.PlanningViewModel.JobOrderViewModel;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService
{
    public interface IRouteCardService
    {
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<decimal> GetAvailableStockByItemIdAsync(int itemId, int? storeId);
        Task<bool> GetScreenPermissionsAsync();
        Task<List<VendorVM>> GetAllVendorsAsync();
        Task<List<Machine>> GetAllMachinesAsync();
        Task<IEnumerable<Process>> GetAllProcessesAsync();


        // Route card specific logic

        Task<decimal> GetRemainingQtyByAssyPoCompTrack(int rcReleaseId, int assyId, int compItemId);
        Task<List<ItemVM>> GetComponentsByParentAsync(int parentItemId, int rcReleaseId);
        Task<List<RouteCardReleaseVM>> GetPendingAssyRcReleasesByCustomerAsync(int custId);
        Task<List<ItemVM>> GetAssemblyItemsByRcRelease(int? rcReleaseId);
        Task<(bool CanDelete, string Message)> CanDeleteRoutecardAsync(int rcId);
        Task<bool> DeleteRouteCardByRCIdAsync(int rcId);
        Task<bool> HasAnyRCCancelAsync(int rcId);
        Task<bool> HasAnyRcTransactionMadeAsync(int rcId);
        Task<bool> IsPoExists(int poId);
        Task<List<RouteCardSubVM>> GetRouteCardSubsByRCIdAsync(int rcId);
        Task UpsertRouteCardSubAsync(RouteCardSubVM subVM);
        Task<decimal?> GetExistingRCQtyByRoutecard(int rcId);
        Task<decimal?> GetRcSubProcessBalQtyByRcId(int rcId);
        Task<decimal?> GetMfgPoRCBalQtyByRefPoIdAndItemId(int poId, int itemId);
        Task<List<ProcessFlowChart>> GetProcessFlowAsync(int compItemId, int rmId);
        Task<List<ItemVM>> GetRawMaterialByCompItemId(int compItemId);

        Task<RouteCardVM> UpsertRouteCardAsync(RouteCardVM routeCardVM);

        Task<MfgPoSub?> GetPOQtyAndRCBalQtyByPoIdAndItemId(int poId, int compId);

        Task<RMDetailsVM?> GetRMDetails(int rmId, int compItemId, decimal rcQty);


        Task<List<ItemVM>> GetComponentItemsByPO(int? poId);
        Task<RouteCardVM?> GetRouteCardByRCIdAsync(int rcId);

        Task<List<ItemVM>> GetSubAssyByParentAsync(int parentItemId);
        Task<int> GetPreviewRcNoAsync(string suffix);
        Task<List<Dictionary<string, object>>> GetLatestBOMFromAssemblyDefAsync(int AssyId);
        Task<bool> UpdatedCancelStatusAsync(JobOrderVM jobVM, string cancelReason);
        Task<bool> DeleteJobOrdersRecursiveAsync(List<int> jobIds, JobOrderVM? parentJobVM);
        Task<List<JobOrder>> GetReferenceJobListByParentJobId(int parentJobId, int refPoSubId);
        Task<bool> CancelJobOrdersRecursiveAsync(List<int> jobIds, JobOrderVM ParentJobOrderVM, string cancelReason);

        Task AdjustPoRouteCardBalanceAsync(int? poId, int? compItemId, decimal oldQty, decimal newQty, string context);


        Task<(List<RouteCardVM> routeCardVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        //Task<List<int>> GetBomItemIdsWithCategory7Async(List<int> itemIds);
        //Task<int?> GetJobIdByMfgPoAndItemIdAsync(int poId, int poSubId, int itemId);

        Task<bool> HasProductionIssueForJobAsync(int itemId, int jobId);
        Task UpdateItemCancelAsync(JobOrderSubVM subItem);
        Task<List<JobOrderSubVM>> GetJobOrderSubByJobIdAsync(int jobId);
        Task<List<MfgPoVM>> GetPendingPOsByCustomerAsync(int custId,string type);
        Task<List<JobOrderAssyPoItemVM>> GetAssemblyItemsByPoIdAsync(int poId);
        Task<List<JobOrderPreviewVM>> GetBOMHierarchyAsync(List<int> assyIds, List<JobOrderAssyPoItemVM> selectedAssys);
        Task<List<BOMItem>> GetBOMItemsByAssyIdAsync(int assyId);
        Task<List<JobOrderSubVM>> GetItemDetailsByAssySubAssyIdAsync(int assyId, decimal joQty);
        Task<bool> DeleteJobOrderByJobIdAsync(int jobId);
        Task<(bool CanDelete, string Message)> CanDeleteJobOrderAsync(int jobId);
        //Task<List<BOMItem>> GetBOMSubAssyByAssyIdAsync(int assyId);
        Task<List<Dictionary<string, object>>> GetExistingBOMDetailsAsync(int jobId);

    }


}
