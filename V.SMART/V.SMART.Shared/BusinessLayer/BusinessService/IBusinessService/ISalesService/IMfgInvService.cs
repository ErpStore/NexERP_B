using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.EWayModel;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.MfgInvVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.SalesStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService
{
    public interface IMfgInvService
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



        Task<string> GenerateQrBase64(string signedQrText);

        Task<bool> CheckPrefixValid(string ScreenName);

        Task<bool> CheckEwayRequired(string ScreenName);


        // 🔹 Mfg Invoice -specific logic

        Task<(List<MfgInvVM> mfgInvVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters);



        Task<bool> HasAnyItemOrInvoiceCancelAsync(int InvId);

        Task<int> GetPendingDcCountAsync(int custId);

        Task<int> GetPendingPoCountAsync(int custId);

        Task<MfgInvVM?> GetInvoiceByInvIdAsync(int InvId);

        Task<MfgInv?> GetLastInvoiceAsync(int custId);

        Task<bool> IsDuplicateMfgInvoiceNoAsync(string InvNo, string suffix, int? currentInvId = null, int? CustId = null);
        Task<MfgInvVM> UpsertInvoiceAsync(MfgInvVM quote, int screenCode);

        Task<bool> HasAnyPOTransactionMadeAsync(int quoteId);

        Task DeleteAndResequenceAsync(MfgInvSubVM subitem, MfgInvVM quote,int screenCode);

        Task<List<MfgInvSubVM>> GetInvoiceSubByInvIdAsync(int InvId);

        Task<List<Dictionary<string, object>>> GetDcDetailsByCustId(int custId);

        Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId, int StoreIssId);

        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int custId);

        Task<string> GetPrefixFromDb();

        Task<string> GetInvoiceNumberAsync(string suffix);

        Task<bool> DeleteInvoiceByInvIdAsync(int InvId, int screenCode);

        Task UpdateItemCancelAndAddorRevertAsync(MfgInvSubVM subItem, int screenCode);

        Task<decimal> GetDcItemBalQtyFromDcSubId(int DcSubId);

        Task<decimal> GetPoItemBalQtyFromDcSubId(int PoSubId);

        Task<MfgInvSubVM?> GetInvSubItemDetailByInvSubIdAsync(int InvSubId);

        Task<List<MfgInvSubVM>> GetDistinctRefEnquiriesByQuoteIdAsync(int quoteId);


        Task UpdatedCancelStatusAndAddOrRevertQty(MfgInvVM invoiceVM, int screenCode);

        Task<List<MfgInvSub>> GetInvoiceSubDetailsByInvIdAsync(int InvId);

        Task ValidateInvoiceBalanceBeforeRevertAsync(MfgInvSub sub);

        Task<bool> UpdateTDSAmountAsync(MfgInvVM mfgInvoiceVM);

        Task<bool> UpdateLcDetailsAsync(MfgInvVM mfgInvoiceVM);

        Task<decimal> GetAvailableStockByItemIdAndRcAndScreenAsync(int itemId, int? storeId, int rcSubId);

        Task<MfgInvVM> UpdateMfgInvShortCloseAsync(MfgInvVM mfgInvVM);



        Task<bool> HasAnyItemOrInvoiceCreditNoteAsync(int RefSubInvId);

        Task<List<ManufacturingInvoiceStatusListVM>> GetMfgInvoiceStatusListAsync(string status);


        //------------**********Invoice Details for EWAY *****-----------------------\\

        Task<List<EWayDocument>> GetMfgInvByCustidAsync(int custId);

        Task<string> GetBasicDirectory();
    }
}
