using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.ToolCribViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.ToolCribIssueStatusViewModel;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService
{
    public interface IToolCribIssueService
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<bool> HasAnyTCReturnTransactionMadeAsync(int tcIssueId);
        Task<IEnumerable<ItemVM>> SearchItemsByCategoryAsync(int categoryCode, string searchText);

        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int storeId);
        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds);
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<ToolCribIssueVM?> GetToolCribIssueByTCIdAsync(int tcId);
        Task<ToolCribIssueSubVM?> GetToolCribIssueSubItemDetailByTCSubIdAsync(int TCSubId);
        Task<ToolCribIssueVM> UpsertToolCribisueAsync(ToolCribIssueVM tcIssueVM, int screenCode);
        Task<bool> HasAnyToolCribReturnTransactionMadeAsync(int tcId);
        Task<List<ToolCribIssueSubVM>> GetToolCribIssueSubByTCIdAsync(int tcId);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<IEnumerable<Store>> GetAllIssueStoresAsync();
        Task<string> GetTCNumberAsync(string suffix);

        Task<List<string>> GetAllExistingUserNamesinTCIssueAsync();
        Task<ToolCribIssueSubVM?> GetTCSubItemDetailByTCIssueSubIdAsync(int issueSubId);
        Task<bool> DeleteToolCribIssueByTCIssueIdAsync(int tcIssueId, int screenCode);

        Task DeleteAndResequenceAsync(ToolCribIssueSubVM subitem, ToolCribIssueVM tcIssueVM, int screenCode);

        Task<List<ToolCribIssueStatusVM>> GetToolCribIssueStatusListAsync(string status);

        Task<(List<ToolCribIssueVM> issueVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);


        Task<List<Staff>> GetAllStaffAsync();
    }

}
