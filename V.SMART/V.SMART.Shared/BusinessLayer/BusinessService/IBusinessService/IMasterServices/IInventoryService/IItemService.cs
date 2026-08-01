using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService
{
    public interface IItemService
    {
        Task<ItemVM> CopyItemAsync(int itemId);
        Task<ItemVM> GenerateNewRevisionAsync(int itemId);
        Task<ItemSub> UpsertItemRateAssign(ItemSub sub);
        Task<IEnumerable<VendorVM>> SearchVenorsAsync(string searchText);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<List<UOM>> GetAllUOMAsync();
        Task<List<HSNMaster>> GetAllHSNAsync();
        Task<List<RawMaterial>> GetAllRMAsync();
        Task<List<VendorVM>> GetAllActiveVendorsAsync();
        Task<List<CustomerVM>> GetCustomersAsync();

        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<(bool CanDelete, string Message)> CanDeleteItemAsync(int itemId);
        Task<VendorVM?> GetVendorByVendorIdAsync(int vendorCode);
        Task<CustomerVM?> GetCustomerByCustIdAsync(int custId);
        Task<bool> DeleteItemRateAssign(ItemSub sub);
        Task<bool> DeleteItemProductAssign(ItemProductAssign assign);
        Task<ItemProductAssign> UpsertItemSubstitution(ItemProductAssign assign);
        Task<List<ItemProductAssign>> LoadItemProductAssignAsync(int? itemId);
        Task<List<ItemSub>> LoadItemSubAsync(int? itemId);
        Task<List<CompMasterVM>> GetPrevProcessFlowRMAsyncByCompItemId(int compItemId);
        Task<List<ProcessFlowChartVM>> GetProcessFlowChartByCompMasterIdAndRMIdAsync(int compItemId, int rmId);
        Task<List<ProcessFlowChartVM>> GetAllProcessByGroupingId(
                    int groupingId,
                    int compItemId,
                    int rmId,
                    decimal weight);
        Task<List<Grouping>> GetFrcNoByFactorIdAsync(int factorId);
        Task<(bool Success, List<Factor> Factors)> ToggleGroupingAsync(bool enable);
        Task EnsureSingleDefaultRMAsync(int compItemId);
        Task<int?> GetComIdbyRMIdAsync(int rmId, int CompItemId);
        Task<List<ProcessFlowChartVM>> DeleteAndRecalculateProcessFlowAsync(int processFlowId, int compMasterId);
        Task<CompMasterVM> GetCompMasterByIdAsync(int id);
        Task<List<Process>> GetAllProcessAsync();
        Task<List<Machine>> GetAllMachineAsync();
        Task<int> GetCompMasterCountAsync(int itemId);
        Task<List<CompMasterVM>> GetCompMastersByItemIdAsync(int itemId);
        Task<IEnumerable<ItemVM>> SearchRawMaterialItemsAsync(string searchText);
        Task<bool> DeleteCompMasterWithProcessFlowAsync(int compMasterId);


        Task<CompMasterVM> UpsertProcessFlow(CompMasterVM compVM);
        Task<ItemVM> UpsertItem(ItemVM vm);
        Task<ScreenManagement?> GetWeightConfigAsync();
        Task<bool> CheckItemExistsAsync(string itemCode);
        Task<ItemVM?> GetItemByItemIdAsync(int itemId);
        Task<(List<ItemVM> Items, int TotalCount)> SearchWithDynamicFilterAsync(
            int pageNumber,
            int pageSize,
            Dictionary<string, object>? filters);


        Task DeleteItemAsync(int itemId);
        Task<(List<Item> Items, int TotalCount)> GetPagedItemsAsync(int pageNumber, int pageSize, string searchTerm);

        Task<string> GetAssignedCustomer(int itemId);

        Task<byte[]> GetDrawingPath(int itemId);

        Task<IEnumerable<Process>> GetAllProcessDetailsAsync();

    }

}
