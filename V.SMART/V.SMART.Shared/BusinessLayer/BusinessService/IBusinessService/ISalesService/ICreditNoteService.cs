using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.SalesAndLabour.Credit_Note;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.CreditNote_VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.CreditNoteListViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService
{
    public interface ICreditNoteService
    {
        //GetScreenCode
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);

        Task<int> GetDecimalPlacesAsync();

        Task<List<Currency>> GetCurrenciesAsync();

        Task<Currency?> GetCurrencyByIdAsync(int currId);

        Task<decimal?> GetLatestCurrencyValueAsync(int currId);

        Task<List<CustomerVM>> GetCustomersAsync(int? custId = null);

        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);

        Task<CustomerVM?> GetCustomerByIdAsync(int custId);

        Task<List<ContactPerson>> GetContactPersonsAsync(int custId);

        Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId);

        Task<CustomerVM?> GetExportCustomerByIdAsync(int custId);

        Task<IEnumerable<CustomerVM>> SearchExportCustomersAsync(string searchText);

        Task<List<CustomerVM>> GetExportCustomerByIdAsync(int? custId = null);

        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds);

        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);

        Task<Companydetails?> GetCompanyDetailsAsync();

        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);

        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);

        Task<string> GenerateQrBase64(string signedQrText);

        Task<bool> CheckPrefixValid(string ScreenName);





        //CreditNote Operation

        Task<(List<CreditNoteVM> creditNoteVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters);

        Task<string> GetPrefixFromDb();

        Task<string> GetCreditNoteNumberAsync(string suffix);

        Task<bool> DeleteCreditNoteByCrIdAsync(int CrId, int screenCode, string type);

        Task<int> GetPendingInvCountAsync(int custId, string type);

        Task<CreditNote?> GetLastcreditNoteAsync(int custId);

        Task<CreditNoteVM?> GetCreditNoteBycrIdAsync(int CrId);


        Task<CreditNoteSubVM?> GetCreditNoteSubItemDetailByCrSubIdAsync(int CrSubId, string reason);

        Task<decimal> GetInvItemBalQtyFromDcSubId(int invSubId, string type);

        Task<List<CreditNoteSubVM>> GetCreditNoteSubByCrIdAsync(int CrId);

        Task DeleteAndResequenceAsync(CreditNoteSubVM subitem, CreditNoteVM quote, int screenCode, string type);

        Task<List<Dictionary<string, object>>> GetExportInvoiceDetailsByCustId(int custId);

        Task<List<Dictionary<string, object>>> GetLabourInvoiceDetailsByCustId(int custId);

        Task<List<Dictionary<string, object>>> GetMfgInvoiceDetailsByCustId(int custId);

        Task<CreditNoteVM> UpsertCreditNoteAsync(CreditNoteVM invoiceVM, int screenCode, string type);

        Task<bool> HasRejectReworkPoByPoSubidAsync(int PosubId);

        Task<List<CreditNoteListVM>> GetCreditNoteListAsync(string status);


        //CreditNote Cancel Operation
        Task ValidateCreditNoteBalanceBeforeRevertAsync(CreditNoteSub sub);
        Task<List<CreditNoteSub>> GetCreditSubDetailsByCrIdAsync(int CrId);

        Task UpdatedCancelStatusAndAddOrRevertQty(CreditNoteVM ceditNoteVM, int screenCode);

    }
}
