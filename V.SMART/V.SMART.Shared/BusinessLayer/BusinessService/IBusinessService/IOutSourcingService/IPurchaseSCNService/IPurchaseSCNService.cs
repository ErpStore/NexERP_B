using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.PurchaseSCNStatusViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchaseSCNService
{
    public interface IPurchaseSCNService
    {
        //Company
        Task<Companydetails> GetCompanyDetailsAsync();
        //ScreenCode
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        // 🔹 Vendors
        Task<VendorVM?> GetVendorByIdAsync(int VendorCode);
        Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText);
        Task<List<VendorContact>> GetContactPersonsVendorAsync(int Vendorcode);
        //Deci
        Task<int> GetDecimalPlacesAsync();
        //Item
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        //Correspond
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<IEnumerable<Store>> GetAllAddStoresAsync();
        Task<List<Store>> GetAllIssueStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<decimal> GetStockQtyFromStockManager(int ItemId, int StoreId);

        //SCN Operation

        Task ValidateBeforeRevertAsync(int scnSubId);
        Task<bool> IsSCNTransactionsMatchedAsync(int ScnId, PurchaseSCNVM scnVMs);
        Task<(bool CanDelete, string Message)> CanDeletePurchaseSCNAsync(int scnId);
        Task UpsertSCNShortCloseAsync(PurchaseSCNVM purchaseSCNVM);
        Task<(bool CanItemCancel, string Message)> CanSCNItemCancelCheckAsync(PurchaseSCNSubVM subItem);
        Task<List<PurchaseSCNSub>> GetSCNSubBySCNIdAsync(int scnId);
        Task ValidateGRNBalanceBeforeRevertAsync(PurchaseSCNSub sub);
        Task<(List<PurchaseSCNVM> scns, int totalCount)> GetPagedSCNAsync(int pageNumber, int pageSize, string search);
        Task<string> GetSCNNumberAsync(string suffix);
        Task<PurchaseSCNVM> GetPurchaseSCNByIdAsync(int SCNId);
        Task<IEnumerable<PurchaseSCNVM>> GetAllPurchaseSCNDetailsAsync();
        Task<int> GetPendingGRNCountAsync(int VendorCode);
        Task<List<Dictionary<string, object>>> GetGRNDetailsByVendorCode(int VendorCode, int StoreId);
        Task<decimal> GetGRNItemBalQtyFromGRNSubId(int GRNSubId);
        Task<PurchaseSCNSubVM?> GetSCNSubItemDetailBySCNSubIdAsync(int SCNSubId);
        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int VendorCode);
        Task<PurchaseSCNVM> UpsertSCNAsync(PurchaseSCNVM purchscnVM, int screenCode);
        Task DeleteAndResequenceAsync(PurchaseSCNSubVM subitem, PurchaseSCNVM scnVM, int screenCode);
        Task<bool> HasAnyItemOrPurchaseSCNCancelAsync(int SCNId);
        Task<bool> DeletePurchaseSCNByIdAsync(int SCNId, int screenCode);
        Task<(bool CanDelete, string Message)> ToCheckStockQtyIssued(int SCNId, int screenCode, string refNo);
        Task<bool> GetPurchaseSCNByIdIsCancelAsync(int SCNId);
        Task UpdateItemCancelAndAddorRevertAsync(PurchaseSCNSubVM subItem, PurchaseSCNVM scnVM, int screenCode);
        Task UpdatedCancelStatusAndAddOrRevertQty(PurchaseSCNVM scnVM, int screenCode);
        Task<List<PurchaseSCNSubVM>> GetDistinctRefGRNBySCNIdAsync(int SCNId);

        Task<(List<PurchaseSCNVM> scnVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<(bool CanDelete, string Message)> NeedTocheckRejection(int refGRNSubId, decimal rejQty);

        Task<List<PurchaseSCNStatusVM>> GetPurchaseSCNsStatusListAsync(string status);
        Task<bool> GetRejectionSelectionEnableAsync();
        Task<List<RejectionMasterVM>> GetAllRejectionReasonAsync();

        Task<bool> IsDocumentUploaded(int invId);

    }
}
