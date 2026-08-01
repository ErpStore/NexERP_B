using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour_Module;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourInvoiceVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.LabourStatusVM;
using V.SMART.Shared.Data.Master.Inventory;

using V.SMART.Shared.ViewModels.AccountsViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ILabourServices
{
    public interface ILabourInvoiceService
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

        // 🔹 Labour-specific logic

        Task<bool> GetPriceLockStatusAsync();
        Task<int> GetPendingLabourDcOutCountAsync(int custId);
        Task<LabInvVM?> GetLabourInvByLabInvIdAsync(int? LabInvId);
        Task<IEnumerable<LabInvVM>> GetAllLabInvoiceAsync();
        Task<LabInv?> GetLastinvoiceNoAsync(int custId);
        Task<LabInvVM> UpsertLabourInvoiceAsync(LabInvVM LabInvVM);

        Task DeleteAndResequenceAsync(LabInvSubVM subitem, LabInvVM LabInvVM);
        Task<List<LabInvSubVM>> GetLabourInvSubByInvIdAsync(int LabInvId);

        Task<List<Dictionary<string, object>>> GetDcDetailsByCustId(int custId);

        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int custId);
        Task<string> GetPrefixFromDb();
        Task<string> GetInvoiceNumberAsync(string suffix);

        Task<bool> DeleteLabourInvoiceByLabInvIdAsync(int Labinvid);

        Task<decimal> GetLabourdcOutItemBalQtyFromEnqSubId(int dcsubid);

       // Task<List<LabInvSubVM>> GetInvoiceSubItemDetailByinvoiceSubIdAsync(int LabInvId);
        Task<LabInvSubVM?> GetInvoiceSubItemDetailByinvoiceSubIdAsync(int InvSubId);
         Task<(List<LabInvVM> LabInv, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
               Dictionary<string, object>? filters);
        Task UpsertlabourInvoiceShortCloseAsync(LabInvVM LabInvVMs);

        Task UpdatedCancelStatusAndAddOrRevertQty(LabInvVM LabInvVM);
        Task ValidateInvoiceBalanceBeforeRevertAsync(LabInvSub sub);
        Task<List<LabInvSub>> GetLabInvSubDetailsByInvIdAsync(int InvId);
        Task UpdateItemCancelAndAddorRevertAsync(LabInvSubVM subItem);
        Task<bool> HasAnyItemOrLabourInvoiceCancelAsync(int labinvid);
        Task<PoTaxDetailVM?> GetPoTaxDataByRefDcSubIdAsync(int refDcSubId);
        Task<bool> GetLabourInvoiceByIdIsCancelAsync(int InvId);

        Task<bool> UpdateTDSAmountAsync(LabInvVM LabInvVM);

        Task<decimal> GetBulkLastUnitPricesItemIdAsync(int itemId, int custId, int refdcsubid);

        Task<List<LabInvSubVM>> GetDistinctRefDcNosByInvIdIdAsync(int LabinvId);



        //--------------------------------------------------
        Task<string> GenerateQrBase64(string signedQrText);

        Task<bool> CheckPrefixValid(string ScreenName);

        Task<bool> CheckEwayRequired(string ScreenName);
        //---------------------------------------------------

        Task<bool> HasAnyItemOrInvoiceCreditNoteAsync(int RefSubInvId);

        Task<List<LabourInvoiceStatusVM>> GetLabourInvoiceStatusListAsync(string status);

        Task<IEnumerable<Process>> SearchProcessAsync(string searchText);

        Task<bool> EinvoiceCancelOption();
        Task<Process> GetProcessByIdAsync(int processId);
        Task<bool> ProcessSelectOption();

        Task<bool> IsDocumentUploaded(int labourInvoiceId);

        Task<List<BillsVM>> GetBillsByCustomerAsync(int CustId, string billType);

        Task<bool> UpdateMultipleTDSAmountAsync(List<BillsVM> labInvVMs);
    }
}
