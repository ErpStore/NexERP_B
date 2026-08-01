using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionSCNAssyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.ProdAssStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService
{
    public interface IProductionSCNAssyservice
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

        Task<List<Dictionary<string, object>>> GetProductionAssyReturnDetails();
        Task<(List<ProductionSCNAssyVM> scnAssyVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters);
        Task<bool> CheckIfTransactionsMadeAsync(
                List<int> itemIds,
                int storeId,
                int screenCode,
                List<int> refSubItemIds);
        Task<bool> DeleteProdAssySCNBySCNIdAsync(int scnId, int screenCode);
        Task<(bool CanDelete, string Message)> CanDeleteProductionSCNAssyAsync(int scnId, int screenCode);
        Task<ProductionSCNAssySubVM?> GetSCNAssyItemDetailByScnSubIdAsync(int scnSubId);
        Task<ProductionSCNAssyVM?> GetProductionSCNByScnIdAsync(int scnId);
        Task DeleteAndResequenceAsync(ProductionSCNAssySubVM subitem, ProductionSCNAssyVM prodScn, int screenCode);
        Task<List<ProductionSCNAssySubVM>> GetProdSCNSubBySCNIdAsync(int scnId);
        Task<decimal> GetProdReturnItemBalQtyFromReturnSubId(int returnSubId);
        Task<ProductionSCNAssySubVM?> GetProdScnSubItemDetailByScnSubIdAsync(int scnSubId);
        Task<ProductionSCNAssyVM> UpsertproductionSCNAsync(ProductionSCNAssyVM prodAssyVM, int screenCode);
        Task<string> GetSCNNumberAsync(string suffix);
        Task<int> GetReturnPendingCountAsync();

        Task<List<ProductionSCNAssySubVM>> GetDistinctRefGRNNOsbySCNIdAsync(int SCNId);

        Task<List<ProductionSCNAssyStatusVM>> GetProductionSCNAssyStatusListAsync(string status);
        Task<bool> GetRejectionSelectionEnableAsync();
        Task<List<RejectionMasterVM>> GetAllRejectionReasonAsync();
    }
}
