using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour.Export;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ExpInv_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.MfgInvVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.SalesStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService
{
    public interface IExpInvService
    {
        // 🔹 Customer + Item data reused from Common Service
        Task<List<CustomerVM>> GetCustomersAsync(int? custId = null);

        Task<CustomerVM?> GetCustomerByIdAsync(int custId);

        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);

        Task<int> GetScreenCodeByScreenNameAsync(string screenName);

        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);

        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);

        Task<List<TermsAndConditions>> GetTermsAsync();

        Task<List<Currency>> GetCurrenciesAsync();

        Task<Currency?> GetCurrencyByIdAsync(int currId);

        Task<decimal?> GetLatestCurrencyValueAsync(int currId);

        Task<int> GetDecimalPlacesAsync();

        Task<List<ContactPerson>> GetContactPersonsAsync(int custId);

        Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId);

        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds);

        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);

        Task<Companydetails?> GetCompanyDetailsAsync();

        Task<decimal> GetStockQtyFromStockManager(int ItemId, int StoreId);

        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);

        Task<IEnumerable<Store>> GetAllActiveStoresAsync();

        //-------------------ExpInvoice operation -----------------------------

        Task<(List<ExpInvVM> expInvVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters);

        Task<bool> HasAnyItemOrExpInvoiceCancelAsync(int ExpInvId);

        Task<bool> DeleteExpInvoiceByInvIdAsync(int InvId, int screenCode);

        Task<int> GetPendingDcCountAsync(int custId);

        Task<int> GetPendingPoCountAsync(int custId);

        Task<ExpInvVM?> GetExpInvoiceByExpInvIdAsync(int ExpInvId);

        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int custId);

        Task<string> GetPrefixFromDb();

        Task<string> GetExpInvoiceNumberAsync(string suffix);

        Task<ExpInv?> GetLastExpInvoiceAsync(int custId);

        Task<decimal> GetDcItemBalQtyFromDcSubId(int DcSubId);

        Task<decimal> GetPoItemBalQtyFromDcSubId(int PoSubId);

        Task<ExpInvSubVM?> GetInvSubItemDetailByInvSubIdAsync(int ExpInvSubId);

        Task<decimal> GetAvailableStockByItemIdAndRcAndScreenAsync(int itemId, int? storeId, int rcSubId);

        Task<List<ExpInvSub>> GetExpInvoiceSubByInvIdAsync(int ExpInvId);

        Task DeleteAndResequenceAsync(ExpInvSubVM subitem, ExpInvVM expinv, int screenCode);

        Task ValidateExpInvoiceBalanceBeforeRevertAsync(ExpInvSub sub);

        Task UpdatedCancelStatusAndAddOrRevertQty(ExpInvVM expinvoiceVM, int screenCode);

        Task<List<Dictionary<string, object>>> GetDcDetailsByCustId(int custId);

        Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId, int storeIssId);

        Task<List<ExpInvSubVM>> GetExpInvoiceDetailsSubByInvIdAsync(int ExpInvId);

        Task<bool> IsDuplicateExportNoAsync(string ExpInvNo, string suffix, int? currentExpInvId = null, int? CustId = null);

        Task<ExpInvVM> UpsertInvoiceAsync(ExpInvVM invoiceVM, int screenCode);


        Task<ExpInvVM> UpdateExportInvShortCloseAsync(ExpInvVM expInvVM);

        Task<List<ExpInvVM>> GetPackingInvoiceDetailsAsync(int ExpInvId);

        Task<ExpInvVM> UpdatePackingInvoiceDetailsAsync(ExpInvVM expinv);


        Task<string> GenerateQrBase64(string signedQrText);

        Task<bool> CheckPrefixValid(string ScreenName);

        Task<bool> CheckEwayRequired(string ScreenName);

        Task<bool> HasAnyItemOrInvoiceCreditNoteAsync(int RefSubInvId);

        Task<List<ExportInvoiceStatusVM>> GetExportInvoiceStatusListAsync(string status);
    }
}
