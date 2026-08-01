using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.PoPendingModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService
{
    public interface IMfgPoService
    {
        // 🔹 Customer + Item data reused from Common Service
        Task<int> GetDecimalPlacesAsync();



        Task<bool> GetLineIdByCustomerAsync(int custId);
        Task<List<CustomerVM>> GetCustomersAsync(int? custId = null);
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);


        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText, int? custId = null);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<List<TermsAndConditions>> GetTermsAsync();
        Task<List<Currency>> GetCurrenciesAsync();
        Task<Currency?> GetCurrencyByIdAsync(int currId);

        Task<decimal?> GetLatestCurrencyValueAsync(int currId);

        Task<List<ContactPerson>> GetContactPersonsAsync(int custId);
        Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId);

        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<bool> CheckAssemblyExistsAsync(int itemId);


        // 🔹 PO-specific logic

        Task ValidateBeforeRevertAsync(int poSubID);
        Task ValidateQuotationBalanceBeforeRevertAsync(MfgPoSub? sub);
        Task<bool> IsPoTransactionsMatchedAsync(int poId, MfgPoVM mfgPoVMs);
        Task UpsertSalesOrderShortCloseAsync(MfgPoVM mfgPoVM);
        Task<(List<MfgPoVM> mfgPoVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<(bool CanDelete, string Message)> CanDeleteSalesOrderAsync(int poId);
        Task<(bool CanItemCancel, string Message)> CanSalesOrderItemCancelCheckAsync(MfgPoSubVM subItem);

        Task UpdateItemCancelAndAddorRevertAsync(MfgPoSubVM subItem, int poId);
        Task UpdatedCancelStatusAndAddOrRevertQty(MfgPoVM poVM);

        Task<bool> HasAnyItemOrPoCancelAsync(int poId);
        Task<int> GetPendingQuoteCountAsync(int custId);
        Task<bool> IsDuplicatePoAsync(string poNo, string suffix, int? currentPoId = null, int? custId = null);
        Task<bool> GetAutoGenerateSalesOrderFlagAsync();
        Task<string> GenerateNextWoNoAsync(IEnumerable<MfgPoSubVM> existingSubItems);
        Task<List<PoType>> GetAllPoTypesAsync();
        Task<IEnumerable<MfgPoVM>> GetAllPODetailsAsync();
        Task<bool> DeletePOByPOIdAsync(int poId);
        Task<MfgPoVM?> GetPoByPoIdAsync(int poId);
        Task<MfgPo?> GetLastPoAsync(int custId);
        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int custId);
        Task<decimal> GetQuoteItemBalQtyFromQuoteSubId(int quoteSubId);
        Task<MfgPoSubVM?> GetPoSubItemDetailByPoSubIdAsync(int PoSubId);
        Task<MfgPoVM> UpsertPoAsync(MfgPoVM mfgPoVM);
        Task DeleteAndResequenceAsync(MfgPoSubVM subitem, MfgPoVM poVM);
        Task<List<MfgPoSub>> GetPOSubByPoIdAsync(int poId);
        Task<List<Dictionary<string, object>>> GetQuoteDetailsByCustId(int custId, bool mfgOrLab);
        Task<string> GetNextSaleOrderNoAsync(int poTypeId);
        Task<string> GetNextOANumberAsync(string suffix);
        Task<string> GetLastOATermsAsync();
        Task<List<MfgPoSubVM>> GetDistinctRefQuoteByPoIdAsync(int poId);
        Task<byte[]> CreateTemplateAsync(string uploadtype);

        Task<bool> IsLabourPoByDefault();

        Task<List<MfgPoPendingVM>> GetMfgPosPendingList(string status);
        Task<bool> IsPoTransactionsMatchedRowRemoveAsync(int? PoSubId, MfgPoVM mfgPoVMs);

        Task<bool> IsDocumentUploaded(int poid);

    }
}
