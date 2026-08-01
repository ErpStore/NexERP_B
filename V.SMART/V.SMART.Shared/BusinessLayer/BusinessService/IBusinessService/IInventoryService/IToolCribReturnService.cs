using V.SMART.Shared.Data.Inventory_Stock_.ToolCrib;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.ToolCribViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.ToolCribReturnStatusViewModel;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService
{
    public interface IToolCribReturnService
    {

        Task<(decimal AddQty, decimal BalQty)> GetQtyBalQtyByStockAddAsync(
                        int screenCode,
                        int storeId,
                        int itemId,
                        int subItemRefId);

        Task<(bool CanDelete, string Message)> CanDeleteToolCribReturnAsync(int tcReturnId, int screenCode);

        Task<bool> DeleteToolCribReturnByDcIdAsync(int tcReturnId, int screenCode);
        Task<decimal> GetTCIssueItemBalQtyFromTCIssueSubId(int tcIssueSubId);
        Task<int> GetPendingIssueCountAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        Task<ToolCribReturnVM?> GetToolCribReturnByDcIdAsync(int dcId);
        Task<ToolCribReturnVM> UpsertToolCribReturnAsync(ToolCribReturnVM tcReturnVM, int screenCode);
        Task DeleteAndResequenceAsync(ToolCribReturnSubVM subitem, ToolCribReturnVM tcrVM, int screenCode);
        Task<List<ToolCribReturnSubVM>> GetToolCribReturnSubByDcIdAsync(int dcId);

        Task<string> GetToolCribReturnNumberAsync(string suffix);
        Task<List<ToolCribReturnSubVM>> GetDistinctRefToolCribIssuesByDcIdAsync(int dcId);

        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);

        Task<ToolCribReturnSubVM?> GetTCReturnSubItemDetailByTCReturnSubIdAsync(int tcReturnSubId);

        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<IEnumerable<Store>> GetAllReturnStoresAsync();
        Task<IEnumerable<ToolCribIssue>> GetAllToolCribIssueNosAsync();

        Task<List<Dictionary<string, object>>> GetAllOpenToolCribIssuesAsync();

        Task<List<ToolCribReturnStatusVM>> GetToolCribReturnStatusListAsync(string status);

        Task<(List<ToolCribReturnVM> returnVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<List<Staff>> GetAllStaffAsync();
    }

}
