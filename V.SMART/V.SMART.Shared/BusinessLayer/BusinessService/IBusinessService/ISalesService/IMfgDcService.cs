using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.EWayModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService
{
    public interface IMfgDcService
    {
        // 🔹 Customer + Item data reused from Common Service
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);

        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);

        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText, int? custId = null);

        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);

        Task<int> GetDecimalPlacesAsync();

        Task<List<ContactPerson>> GetContactPersonsAsync(int custId);

        Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId);

        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);

        Task<Companydetails?> GetCompanyDetailsAsync();

        Task<List<Store>> GetAllIssueStoresAsync();

        Task<IEnumerable<Store>> GetAllActiveStoresAsync();

        Task<decimal> GetStockQtyFromStockManager(int ItemId, int StoreId);

        Task<int> GetScreenCodeByScreenNameAsync(string screenName);

        // 🔹 DC-specific logic
        Task<bool> IsDuplicateDcNoAsync(string DcNo, string suffix, int? currentDcId = null, int? CustId = null);
        Task<(List<MfgDcVM> mfgInvVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                   Dictionary<string, object>? filters);

        Task<string> GetPrefixFromDb();

        Task<string> GetDcOutgoingNoAsync(string suffix);

        Task<List<MfgDcSubVM>> GetDistinctRefPoByDcIdAsync(int dcId);

        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);

        Task<IEnumerable<MfgDcVM>> GetAllDcDetailsAsync();

        Task<bool> DeleteMfgDcByDcIdAsync(int dcId, int screenCode);

        Task<MfgDcVM?> GetMfgDcByDcIdAsync(int dcId);

        Task<int> GetPendingPoCountAsync(int custId);

        Task<decimal> GetPoItemBalQtyFromPoSubId(int poSubId);

        Task<MfgDcSubVM?> GetDcSubItemDetailByDcSubIdAsync(int dcSubId);

        Task UpdateItemCancelAndAddorRevertAsync(MfgDcSubVM subItem, int screenCode);

        Task UpdatedCancelStatusAndAddOrRevertQty(MfgDcVM dcVM, int screenCode);

        Task<List<MfgDcSubVM>> GetDcSubByDcIdAsync(int dcId);

        Task<MfgDcVM> UpsertDCAsync(MfgDcVM mfgDcVM, int screenCode);

        Task DeleteAndResequenceAsync(MfgDcSubVM subitem, MfgDcVM dcVM, int screenCode);

        Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId, int storeIssId);

        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int custId);

        Task<bool> IsOpenPoAsync(int poSubId);

        Task<decimal> GetAvailableStockByItemIdAndRcAndScreenAsync(int itemId, int? storeId, int rcSubId);

        Task<bool> HasAnyItemOrDcCancelAsync(int DcId);

        Task ValidateDcBalanceBeforeRevertAsync(MfgDcSub sub);

        Task<List<MfgDcSub>> GetDcDetailsSubByDcIdAsync(int dcId);

        Task<MfgDcVM> UpdateMfgDcShortCloseAsync(MfgDcVM mfgDcVM);


        Task<MfgDcVM> UpdateMfgDCRejectRework(MfgDcVM mfgDcVM);

        Task<bool> CanMfgDcOutgoingTrasByDcsubIdAsync(List<int> dcSubIds);

        Task<MfgDcSubVM?> GetRejReworkDcSubItemDetailByDcSubIdAsync(int dcSubId);



        
        Task<int> GetRejectionCountAsync(int vendorCode);
        Task<List<Dictionary<string, object>>> GetGRNDetailsByVendorCode(int VendorCode, int storeIssId);
        Task<decimal> GetSCNItemBalQtyFromSCNSubId(int ScnSubId);



        //------------**********DC Details for EWAY *****-----------------------\\

        Task<List<EWayDocument>> GetMfgDcByCustidAsync(int custId);
    }
}
