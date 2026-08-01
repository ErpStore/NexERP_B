using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService
{
    public interface IMfgQuotationService
    {
        // 🔹 Customer + Item data reused from Common Service
        Task<List<CustomerVM>> GetCustomersAsync(int? custId = null);
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);

        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText, int? custId = null);
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




        // 🔹 Quotation-specific logic


        Task ValidateBeforeRevertAsync(int quoteSubId);
        Task ValidateEnquiryBalanceBeforeRevertAsync(MfgQuoteSub sub);
        Task<(bool CanItemCancel, string Message)> CanQuoteItemCancelCheckAsync(MfgQuoteSubVM subItem);
        Task<bool> IsQuoteTransactionsMatchedAsync(int quoteId, MfgQuoteVM quoteVMs);
        Task UpsertSalesQuoteShortCloseAsync(MfgQuoteVM mfgQuoteVM);
        Task<(List<MfgQuoteVM> mfgQuoteVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<(bool CanDelete, string Message)> CanDeleteSalesQuotationAsync(int quoteId);
        Task<bool> HasAnyItemOrQuoteCancelAsync(int quoteId);
        Task<int> GetPendingEnquiryCountAsync(int custId);
        Task<MfgQuoteVM?> GetQuotationByQuoteIdAsync(int quoteId);
        Task<IEnumerable<MfgQuoteVM>> GetAllQuoteAsync();
        Task<MfgQuote?> GetLastQuoteAsync(int custId);
        Task<MfgQuoteVM> UpsertQuoteAsync(MfgQuoteVM quote);

        Task<bool> HasAnyPOTransactionMadeAsync(int quoteId);
        Task DeleteAndResequenceAsync(MfgQuoteSubVM subitem, MfgQuoteVM quote);
        Task<List<MfgQuoteSub>> GetQuoteSubByQuoteIdAsync(int quoteId);

        Task<List<Dictionary<string, object>>> GetEnquiryDetailsByCustId(int custId ,bool mfgOrLab);

        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int custId);
        Task<string> GetPrefixFromDb();
        Task<string> GetQuotationNumberAsync(string suffix);
        Task<bool> DeleteQuotationByQuoteIdAsync(int QuoteId);
        Task UpdateItemCancelAndAddorRevertAsync(MfgQuoteSubVM subItem, int quoteId);
        Task<decimal> GetEnquiryItemBalQtyFromEnqSubId(int enqSubId);
        Task<MfgQuoteSubVM?> GetQuoteSubItemDetailByQuoteSubIdAsync(int quoteSubId);
        Task<List<MfgQuoteSubVM>> GetDistinctRefEnquiriesByQuoteIdAsync(int quoteId);
        Task UpdatedCancelStatusAndAddOrRevertQty(MfgQuoteVM quoteVM);

        Task<byte[]> CreateTemplateAsync(string uploadtype);

        //Revise Quotation Draft
        Task<MfgQuoteVM> ReviseQuotationDraftAsync(int QuoteId);

        Task<bool> CheckreviseQuoteByQuoteIdAsyn(string quoteNo);

        Task<DataTable> GetMfgLabQuotePendingList(string status);//Shankar
    }

}
