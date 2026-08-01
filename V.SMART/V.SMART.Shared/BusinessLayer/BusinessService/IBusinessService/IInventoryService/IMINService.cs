using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.MaterialIssueNoteVM;
using V.SMART.Shared.ViewModels.InventoryViewModel.SCNGenViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService
{
    public interface IMINService
    {
        // 🔹 Customer + Item data reused from Common Service
        Task<int> GetDecimalPlacesAsync();
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<List<ItemVM>> GetItemVMsByItemIdsAsync(List<int> itemIds);
        Task<Dictionary<string, int>> GetItemIdsByItemCodesAsync(List<string> itemCodes);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<IEnumerable<Store>> GetAllActiveStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<List<ItemVM>> GetAllAssembliesAsync();
        Task<List<ItemVM>> GetAllSubAssembliesAsync();
        Task<List<ItemVM>> GetAllSubAssembliesByAssyIdAsync(int assyId);
        Task<List<CostCenterVM>> GetAllCostCenterDetails();
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int storeId);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<List<UOM>> GetUOMsAsync();
        Task<Companydetails?> GetCompanyDetailsAsync();

        // 🔹 Material Issue Note specific logic+

        Task<(List<MaterialIssNoteVM> minVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<List<string>> GetAllExistingUserNamesinMINAsync();
        Task<IEnumerable<MaterialIssNoteVM>> GetAllMINDetailsAsync();
        Task<(bool CanDelete, string Message)> CanDeleteMINAsync(int minId);
        Task<bool> DeleteMINByMINIdAsync(int minId, int screenCode);
        Task<MaterialIssNoteVM?> GetMINByMINIdAsync(int minId);
        Task<string> GetMINIssueNumberAsync(string suffix);
        Task<List<MaterialIssNoteSubVM>> GetAssemblyRelatedItemsAsync(int assyId, decimal ReqQty, int storeId);
        Task<MaterialIssNoteSubVM?> GetMINSubItemDetailByMINSubIdAsync(int minSubId);
        Task DeleteAndResequenceAsync(MaterialIssNoteSubVM subitem, MaterialIssNoteVM minVM, int screenCode);
        Task<List<MaterialIssNoteSubVM>> GetMINSubByMINIdAsync(int minId, int storeId);
        Task<MaterialIssNoteVM> UpsertMINAsync(MaterialIssNoteVM minVM, int screenCode);
        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds);
        Task<List<Dictionary<string, object>>> GetStockIssueReqDetails();

    }

}
