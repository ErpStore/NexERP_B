using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.PurchaseAndSubcontract.Purchase_Quote;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.PurchAndSubConViewModel.Purch_QuotationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPurchaseService
{
    public interface IPurchaseQuoteService
    {

        // 🔹 Vendor + Item data reused from Common Service
        Task<List<VendorVM>> GetVendorAsync(int? VendorCode = null);
        Task<VendorVM?> GetVendorByIdAsync(int vendorCode);
        Task<IEnumerable<VendorVM>> SearchVendorAsync(string searchText);

        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<List<TermsAndConditions>> GetTermsAsync();
        Task<List<Currency>> GetCurrenciesAsync();
        Task<Currency?> GetCurrencyByIdAsync(int currId);

        Task<decimal?> GetLatestCurrencyValueAsync(int currId);
        Task<int> GetDecimalPlacesAsync();

        Task<List<VendorContact>> GetVenorContactPersonsAsync(int VenoderCode);
        Task<List<VendorInDirect>> GetConsigneeAddressesAsync(int custId);
        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<Companydetails?> GetCompanyDetailsAsync();



        // 🔹 Quotation-specific logic

        Task<(List<PurchaseQuoteVM> purchaseQuoteVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task ValidateBeforeRevertAsync(int quoteSubId);
        Task UpsertPurchaseQuoteShortCloseAsync(PurchaseQuoteVM PurchQuoteVM);
        Task<(bool CanItemCancel, string Message)> CanQuoteItemCancelCheckAsync(PurchaseQuoteSubVM subItem);
        Task<List<PurchaseQuoteSub>> GetQuoteSubByQuoteIdAsync(int quoteId);
        Task ValidatePurchaseEnquiryBalanceBeforeRevertAsync(PurchaseQuoteSub sub);
        Task<bool> IsQuoteTransactionsMatchedAsync(int quoteId, PurchaseQuoteVM purchQuoteVms);
        Task<int> GetPendingEnquiryCountAsync(int vendorCode);
        Task<PurchaseQuoteVM> GetQuotationByQuoteIdAsync(int quoteId);
        Task<IEnumerable<PurchaseQuoteVM>> GetAllQuoteAsync();
        Task<PurchaseQuote?> GetLastQuoteAsync(int VendorCode);
        Task<bool> IsDuplicateQuoteAsync(string quoteNo, string suffix, int VendorCode, int? quotId = null);
        Task<PurchaseQuoteVM> UpsertQuoteAsync(PurchaseQuoteVM quoteVM);
        Task DeleteAndResequenceAsync(PurchaseQuoteSubVM subitem, PurchaseQuoteVM quote);

        Task UpdatedCancelStatusAndAddOrRevertQty(PurchaseQuoteVM quoteVM);
        Task<List<Dictionary<string, object>>> GetEnquiryDetailsByVendorCode(int VendorCode, bool purchOrSub);
        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int VendorCode);
        Task<string> GetQuotationNumberAsync(string suffix);
        Task<bool> DeleteQuotationByQuoteIdAsync(int QuoteId);
        Task<PurchaseQuoteSubVM?> GetQuoteSubItemDetailByQuoteSubIdAsync(int quoteSubId);
        //Task<List<PurchaseQuoteSubVM>> GetDistinctRefEnquiriesByQuoteIdAsync(int quoteId);
        Task UpdateItemCancelAndAddorRevertAsync(PurchaseQuoteSubVM subItem);
        Task<(bool CanDelete, string Message)> CanDeletePurchaseQuote(int QuoteId);
        Task<(bool CanDelete, string Message)> CanRemoveQuoteAsync(int QuoteId, int QuoteSubId);
        Task<decimal> GetEnquiryItemBalQtyFromVendorAssignId(int enqAssignId);


        Task<List<PurchaseQuotationPendingList>> GetPurchSubQuotePendingList(string status);//Shankar
    }
}
