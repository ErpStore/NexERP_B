using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionLogViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionSCNAssyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService
{
    public interface IProductionLogService
    {
        Task<List<ShiftAllocation>> GetAllShiftAsync();
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<int> GetDecimalPlacesAsync();
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<List<Store>> GetAllIssueStoresAsync();
        Task<List<Store>> GetAllAddStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int storeId);
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        Task<decimal> GetAvailableStockByItemIdAsync(int itemId, int? storeId);
        Task<decimal> GetAvailableStockByItemIdAndRcAndScreenAsync(int itemId, int? storeId, int rcSubId);

        Task<List<Machine>> GetAllActiveMachinesAsync();
        Task<List<string>> GetAllUsersAsync();

        // 🔹 Production Logs logic

        Task<int?> GetPendingProductionLogIdAsync(string operatorName);
        Task<(bool CanDelete, string Message)> CanDeleteProductionLog(long logId, int screenCode);
        Task<bool> DeleteProductionLogByLogId(long logId, int screenCode);
        Task<ProductionLogVM?> GetProductionLogByLogIdAsync(long logId, int screenCode);
        Task<List<ProductionLogSubVM>> GetProductionLogSubByLogId(long logId, int ScreenCode);
        Task<ProductionLogVM?> GetActiveProductionLogAsync(int rcId, int processId, int screenCode);
        Task<ProductionLogVM> GetRCProcessDetailsByRcIdAndProcessId(int rcId, int processId, int issueStoreid, int screenCode);
        Task<List<Process>> LoadNextHierarchyAsync(int rcId);
        Task<List<RouteCardVM>> GetPendingRouteCardsAsync();
        Task<List<ProductionLogSetting>> GetAllActiveProdLogSettingsAsync();
        Task<ProductionLogVM> UpsertproductionLogAsync(ProductionLogVM prodLogVM, int screenCode);
        Task<string> GetLogNumberAsync(string suffix);
        Task<(List<ProductionLogVM> logVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters);
        Task<bool> CheckIfTransactionsMadeAsync(List<int> itemIds,int storeId,int screenCode,List<int> refSubItemIds);
        Task<bool> DeleteProdAssySCNBySCNIdAsync(int scnId, int screenCode);
        Task<(bool CanDelete, string Message)> CanDeleteProductionSCNAssyAsync(int scnId, int screenCode);
        Task<ProductionSCNAssySubVM?> GetSCNAssyItemDetailByScnSubIdAsync(int scnSubId);
        Task DeleteAndResequenceAsync(ProductionSCNAssySubVM subitem, ProductionSCNAssyVM prodScn, int screenCode);
        Task<List<ProductionSCNAssySubVM>> GetProdSCNSubBySCNIdAsync(int scnId);
        Task<decimal> GetProdReturnItemBalQtyFromReturnSubId(int returnSubId);
        Task<ProductionSCNAssySubVM?> GetProdScnSubItemDetailByScnSubIdAsync(int scnSubId);
        Task<List<Dictionary<string, object>>> GetProductionAssyReturnDetails();
        Task<int> GetReturnPendingCountAsync();
        Task<List<ProductionSCNAssySubVM>> GetDistinctRefGRNNOsbySCNIdAsync(int SCNId);
        Task<bool> GetIsReworkProcessAsync(int RcId, int ProcessId);
        Task<List<RouteCardSub>> GetReworkProcessesAsync(int itemId, int RcId, int processId);

        Task UpdateRouteCardSubRejQtyAsync(int itemIdIn, int processId, decimal rejectQty, int currentRcSubId);
        Task UpdateRouteCardSubAsync(int rcSubId, int itemIdIn, int reworkProcessId, decimal reworkNewQty, decimal reworkOldQty, int CurrentRcsubid);
        Task<List<int>> GetIssueSourceRCSubIdsAsync(int rcId, int seqNo, int currentRcSubId);
        Task<int?> GetEffectivePreviousSeqNoAsync(int rcId, int currentSeqNo);
        Task<decimal> GetPrevSeqMinNextQtyAsync(int rcId, int prevSeqNo);
    }
}
