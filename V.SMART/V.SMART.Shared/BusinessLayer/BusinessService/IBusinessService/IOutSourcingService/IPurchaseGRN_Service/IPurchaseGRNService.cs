using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.PurchaseGRNStatusViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchaseGRN_Service
{
    public interface IPurchaseGRNService
    {
        // 🔹 Vendor + Item data reused from Common Service
        Task<VendorVM?> GetVendorByIdAsync(int VendorCode);
        Task<List<VendorContact>> GetContactPersonsVendorAsync(int Vendorcode);
        Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<int> GetDecimalPlacesAsync();
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        Task<Companydetails> GetCompanyDetailsAsync();
        Task<List<Store>> GetAllAddStoresAsync();
        Task<IEnumerable<Store>> GetAllActiveStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);


        // 🔹 GRN-specific logic
        Task<(List<PurchaseGRNVM> grnVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task ValidateBeforeRevertAsync(int grnSubId);
        Task<(bool CanDelete, string Message)> CanDeleteGRNAsync(int grnId, int screenCode);
        Task UpsertPurchGRNShortCloseAsync(PurchaseGRNVM purchaseGRNVM);
        Task<(bool CanItemCancel, string Message)> CanGRNItemCancelCheckAsync(PurchaseGRNSubVM subItemVM);
        Task<List<PurchaseGRNSub>> GetGRNSubByGRNIdAsync(int grnId);
        Task ValidatePurchasePoBalanceBeforeRevertAsync(PurchaseGRNSub sub);
        Task<bool> IsGRNTransactionsMatchedAsync(int poId, PurchaseGRNVM PurchPoVMs, int screenCode);
        Task<(List<PurchaseGRNVM> grns, int totalCount)> GetPagedGRNAsync(int pageNumber, int pageSize, string search);
        Task<string> GetGRNnNumberAsync(string suffix);
        Task<IEnumerable<PurchaseGRNVM>> GetAllPurchaseGRNDetailsAsync();
        Task<PurchaseGRNVM> GetPurchaseGRNByIdAsync(int GRNId);
        //Task<int> GetPendingPoCountAsync(int VendorCode);
        Task<decimal> GetPoItemBalQtyFromPoSubId(int poSubId);
        Task<PurchaseGRNSubVM?> GetGRNSubItemDetailByGRNSubIdAsync(int GRNSubI);
        Task UpdateItemCancelAsync(PurchaseGRNSubVM subItem);
        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int VendorCode);
        Task<List<Dictionary<string, object>>> GetPoDetailsByVendorCode(int VendorCode);
        Task<bool> IsOpenPoAsync(int poSubId);
        Task<PurchaseGRNVM> UpsertGRNAsync(PurchaseGRNVM purchgrn, int screenCode);
        Task<bool> IsDuplicateDcNoAsync(int vendorCode, string dcNo, string suffix);
        Task<bool> IsDuplicateInvNoAsync(int vendorCode, string InvNo, string suffix);
        Task<bool> DeletePurchaseGRNByIdAsync(int GRNId, int screenCode);
        Task<(bool CanDelete, string Message)> ToCheckStockQtyIssued(int GRNId, int screenCode, string refNo);
        Task UpdateItemCancelAndAddorRevertAsync(PurchaseGRNSubVM subItem, PurchaseGRNVM purchaseGRNVM, int screenCode);
        Task UpdatedCancelStatusAndAddOrRevertQty(PurchaseGRNVM purchaseGRNVM, int screenCode);
        Task<bool> HasAnyItemOrPurchaseGRNCancelAsync(int GRNId);
        Task DeleteAndResequenceAsync(PurchaseGRNSubVM subitem, PurchaseGRNVM grnVM, int screenCode);
        Task<bool> GetPurchaseGRNByIdIsCancelAsync(int GRNId);

        Task<List<PurchaseGRNStatusVM>> GetPurchaseGRNsStatusListAsync(string status);
    }
}
