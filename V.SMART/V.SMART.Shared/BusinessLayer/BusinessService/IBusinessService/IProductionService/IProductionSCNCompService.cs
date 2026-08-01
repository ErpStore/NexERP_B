using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionSCNAssyViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.ProdCompStatusVM;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService
{
    public interface IProductionSCNCompService
    {

        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<int> GetDecimalPlacesAsync();
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<List<Store>> GetAllIssueStoresAsync();
        Task<List<Store>> GetAllAddStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int storeId);
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);


        // 🔹 Quotation-specific logic

        Task<List<Dictionary<string, object>>> GetProductionCompReturnDetails();
        Task<ProductionSCNCompVM> UpsertproductionSCNAsync(ProductionSCNCompVM prodCompVM, int screenCode);
        Task<ProductionSCNCompVM?> GetProductionSCNByScnIdAsync(int scnId);
        Task<(List<ProductionSCNCompVM> scnCompVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters);
        Task<bool> CheckIfTransactionsMadeAsync(
                List<int> itemIds,
                int storeId,
                int screenCode,
                List<int> refSubItemIds);
        Task<bool> DeleteProdAssySCNBySCNIdAsync(int scnId, int screenCode);
      
        Task<ProductionSCNCompSubVM?> GetSCNCompItemDetailByScnSubIdAsync(int scnSubId);
        Task DeleteAndResequenceAsync(ProductionSCNCompSubVM subitem, ProductionSCNCompVM prodScn, int screenCode);
        Task<List<ProductionSCNCompSubVM>> GetProdSCNSubBySCNIdAsync(int scnId);
        Task<decimal> GetProdReturnItemBalQtyFromReturnSubId(int returnSubId);
        Task<ProductionSCNCompSubVM?> GetProdScnSubItemDetailByScnSubIdAsync(int scnSubId);
        Task<string> GetSCNNumberAsync(string suffix);
        Task<int> GetReturnPendingCountAsync();

        Task<List<ProductionSCNCompSubVM>> GetDistinctRefGRNNOsbySCNIdAsync(int SCNId);

        Task<List<ProductionSCNCompStatusVM>> GetProductionSCNComponentStatusListAsync(string status);
        Task<bool> GetRejectionSelectionEnableAsync();
        Task<List<RejectionMasterVM>> GetAllRejectionReasonAsync();

        Task<(bool CanDelete, string Message)> CanDeleteProductionSCNAssyAsync(int SCNId, int screenCode, string refNo);

        Task AdjustReturnCompBalanceAsync(int? refReturnSubId, decimal oldQty, decimal newQty, string context);
        Task SyncRCSubProcessQtyAsync(int? rcSubId,
                   decimal oldAccQty, decimal oldRejQty, decimal oldRewQty,
                   decimal newAccQty, decimal newRejQty, decimal newRewQty,
                   string context);


        Task<List<Staff>> GetAllStaffAsync();

        Task<bool> IsProductionLogAsync();
    }
}
