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
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.ProdAssStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService
{
    public interface IProductionReturnAssyService
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

        Task<decimal?> GetLatestCurrencyValueAsync(int currId);
        Task<int> GetDecimalPlacesAsync();

        Task<List<ContactPerson>> GetContactPersonsAsync(int custId);
        Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId);

        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<List<Store>> GetAllActiveStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);



        // 🔹 Production Assembly - specific logic

        Task<(decimal AddQty, decimal BalQty)> GetQtyBalQtyByStockAddAsync(
                        int screenCode,
                        int storeId,
                        int itemId,
                        int subItemRefId);

        Task<(bool CanDelete, string Message)> CanDeleteProductionReturnAssyAsync(int returnId, int screenCode);
        Task<int> GetPendingJobOrdersCountAsync();
        Task<List<ProductionReturnAssySubVM>> GetDistinctRefJobNoByReturnIdAsync(int returnId);
        Task<(bool IsValid, string Message)> ValidateDeleteAsync(int jobId, int itemId, decimal qtyReturned, int addStoreId);
        Task<(List<ProductionReturnAssyVM> returnAssyVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters);
        Task<decimal> GetIssueBalQtyByIssueSubId(int issueSubId);
        Task<List<Dictionary<string, object>>> GetAllProductionIssuedItemsAsync();
        Task<List<Dictionary<string, object>>> GetAllOpenJobOrdersAsync();
        Task<List<ProductionReturnAssyTrackVM>> GetAllExistingJobOrderAsync();
        Task<ProductionReturnAssyVM?> GetProductionByReturnIdAsync(int returnId);
        Task<string> GetProductionReturnNumberAsync(string suffix);
        Task<List<ProductionReturnAssySubVM>> GetProductionReturnSubByReturnIdAsync(int returnId);
        Task<decimal> GetJobOrderBalQtyFromJobId(int jobId);
        Task<ProductionReturnAssySubVM?> GetProductionReturnSubItemDetailByReturnSubIdAsync(int returnSubId);
        Task<ProductionReturnAssyVM> UpsertProductionReturnAsync(ProductionReturnAssyVM prodReturnAssyVM, int screenCode);
        Task<ProductionReturnAssySubVM?> GetProdReturnSubItemDetailByReturnSubIdAsync(int returnSubId);
        Task<bool> DeleteProdAssyReturnByReturnIdAsync(int returnId, int screenCode);
        Task<string> GetProductionIssueNumberAsync(string suffix);
        Task DeleteAndResequenceAsync(ProductionReturnAssySubVM subitem, ProductionReturnAssyVM productionReturnAssyVM, int screenCode);
        Task<ProductionReturnAssyVM> GetJobOrderDetailsByIdAsync(int jobId);
        Task<bool> HasAnyItemOrQuoteCancelAsync(int quoteId);
        Task<IEnumerable<MfgQuoteVM>> GetAllQuoteAsync();
        Task<MfgQuote?> GetLastQuoteAsync(int custId);
        Task<bool> HasAnyPOTransactionMadeAsync(int quoteId);
        Task<List<Dictionary<string, object>>> GetEnquiryDetailsByCustId(int custId);
        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds);
        Task<string> GetPrefixFromDb();
        Task<bool> DeleteQuotationByQuoteIdAsync(int QuoteId);
        Task UpdateItemCancelAndAddorRevertAsync(MfgQuoteSubVM subItem, int quoteId);
        Task<decimal> GetEnquiryItemBalQtyFromEnqSubId(int enqSubId);
        Task UpdatedCancelStatusAndAddOrRevertQty(MfgQuoteVM quoteVM);

        Task<List<ProductionReturnAssyStatusVM>> GetProductionReturnAssyStatusListAsync(string status);

    }
}
