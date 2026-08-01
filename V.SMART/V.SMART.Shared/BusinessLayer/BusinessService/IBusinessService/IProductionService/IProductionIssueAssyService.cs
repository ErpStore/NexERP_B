using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.PlanningViewModel.JobOrderViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.ProdAssStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService
{
    public interface IProductionIssueAssyService
    {
        // 🔹 Customer + Item data reused from Common Service
        Task<List<CustomerVM>> GetCustomersAsync(int? custId = null);
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<List<TermsAndConditions>> GetTermsAsync();
        Task<List<Currency>> GetCurrenciesAsync();
        Task<Currency?> GetCurrencyByIdAsync(int currId);
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        Task<decimal?> GetLatestCurrencyValueAsync(int currId);
        Task<int> GetDecimalPlacesAsync();

        Task<List<ContactPerson>> GetContactPersonsAsync(int custId);
        Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId);

        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<List<Store>> GetAllActiveStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);

        Task<decimal> GetStockForItemAsync(int itemId, int storeId);
        Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int storeId);



        // 🔹 Production SCN Assembly - specific logic
        Task<int> GetPendingJobOrdersCountAsync();
        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds);
        Task<List<Dictionary<string, object>>> GetAllOpenJobOrdersAsync(int storeId);
        Task<bool> HasAnyProdReturnAssyTransactionMadeAsync(int issueId);
        Task<bool> DeleteProdAssyIssueByIssueIdAsync(int issueId, int screenCode);

        Task<(List<ProductionIssueAssyVM> issueAssyVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters);
        Task<ProductionIssueAssyVM> UpsertProdIssueAssy(ProductionIssueAssyVM issueVM, int screenCode, bool IsMonthwiseNumber);
        Task<ProductionIssueAssyVM?> GetProductionByIssueIdAsync(int issueId);
        Task<string> GetProductionIssueNumberAsync(string suffix);
        Task DeleteAndResequenceAsync(ProductionIssueAssySubVM subitem, ProductionIssueAssyVM productionIssueAssyVM, int screenCode);
        Task<ProductionIssueAssySubVM?> GetProductionIssueSubItemDetailByIssueSubIdAsync(int issueSubId);
        Task<ProductionIssueAssyVM> GetJobOrderDetailsByIdAsync(int jobId, int storeId);
        Task<List<ProductionIssueAssySubVM>> GetProductionIssueSubByIssueIdAsync(int issueId);
        Task<decimal> GetJobOrderItemBalQtyFromJobSubId(int jobSubId);
        Task<bool> HasAnyItemOrQuoteCancelAsync(int quoteId);
        Task<IEnumerable<MfgQuoteVM>> GetAllQuoteAsync();
        Task<MfgQuote?> GetLastQuoteAsync(int custId);

        Task<List<ProductionIssueAssySubVM>> GetDistinctRefJobNoByIssueIdAsync(int issueId);

        Task<List<Dictionary<string, object>>> GetEnquiryDetailsByCustId(int custId);
        Task<string> GetPrefixFromDb();
        Task<bool> DeleteQuotationByQuoteIdAsync(int QuoteId);
        Task UpdateItemCancelAndAddorRevertAsync(MfgQuoteSubVM subItem, int quoteId);
        Task<decimal> GetEnquiryItemBalQtyFromEnqSubId(int enqSubId);
        Task<MfgQuoteSubVM?> GetQuoteSubItemDetailByQuoteSubIdAsync(int quoteSubId);
        Task UpdatedCancelStatusAndAddOrRevertQty(MfgQuoteVM quoteVM);
        Task<bool> IsMonthwiseNumberGenerationEnabledAsync();
        Task<string> GetMonthWiseProductionIssueNumberAsync(string suffix);
        Task<string> GetDerpartmentCodeByCurrentuserAsync();
        Task<List<StaffVM>> GetStaffDetailsByJobOrderIdAsync(List<int> jobOrderIds);

        Task<List<ProductionIssueAssyStatusVM>> GetProductionIssueAssyStatusListAsync(string status);

    }
}
