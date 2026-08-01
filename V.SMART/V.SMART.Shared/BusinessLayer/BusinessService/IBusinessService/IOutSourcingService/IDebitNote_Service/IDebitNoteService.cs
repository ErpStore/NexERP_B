

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.OutSourcing.Debit_Note;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.DebitNote_VM;
using V.SMART.Shared.ViewModels.ReportViewModel.DebitNoteListViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IDebitNote_Service
{
    public interface IDebitNoteService
    {
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);

        Task<List<Currency>> GetCurrenciesAsync();

        Task<Currency?> GetCurrencyByIdAsync(int currId);

        Task<decimal?> GetLatestCurrencyValueAsync(int currId);

        Task<int> GetDecimalPlacesAsync();

        Task<List<VendorVM>> GetVendorsAsync(int? VendorCode = null);

        Task<IEnumerable<VendorVM>> SearchVendorAsync(string searchText);

        Task<VendorVM?> GetVendorsByIdAsync(int VendorCode);

        Task<List<VendorContact>> GetContactPersonsAsync(int VendorCode);

        Task<List<VendorInDirect>> GetConsigneeAddressesAsync(int VendorCode);

        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds);

        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);

        Task<Companydetails?> GetCompanyDetailsAsync();

        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);

        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);

        Task<string> GenerateQrBase64(string signedQrText);

        Task<bool> CheckPrefixValid(string ScreenName);


        //DebitNote Operation

        Task<(List<DebitNoteVM> debitNoteVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters);

        Task<string> GetPrefixFromDb();

        Task<string> GetDebitNoteNumberAsync(string suffix);

        Task<int> GetPendingInvCountAsync(int VendorCode, string type);

        Task<DebitNote?> GetLastdebitNoteAsync(int VendorCode);

        Task<bool> DeleteDebitNoteByDbIdAsync(int DbId, int screenCode, string type);

        Task<DebitNoteVM?> GetDebitNoteByDbIdAsync(int DbId);

        Task<decimal> GetInvItemBalQtyFromDcSubId(int invSubId, string type);


        Task<List<DebitNoteSubVM>> GetDebitNoteSubByDbIdAsync(int DbId);

        Task<DebitNoteSubVM?> GetDebitNoteSubItemDetailByDbSubIdAsync(int DbSubId, string reason);

        Task DeleteAndResequenceAsync(DebitNoteSubVM subitem, DebitNoteVM quote, int screenCode, string type);

        Task<List<Dictionary<string, object>>> GetPurchaseInvoiceDetailsByVendorCode(int VendorCode);

        Task<List<Dictionary<string, object>>> GetSubContractInvoiceDetailsByVendorCode(int VendorCode);

        Task<DebitNoteVM> UpsertDebitNoteAsync(DebitNoteVM invoiceVM, int screenCode, string type);

        Task<List<DebitNoteListVM>> GetDebitNoteListAsync(string status);


        //Cancel DebitNote
        Task ValidateCreditNoteBalanceBeforeRevertAsync(DebitNoteSub sub);

        Task<List<DebitNoteSub>> GetCreditSubDetailsByCrIdAsync(int DbId);

        Task UpdatedCancelStatusAndAddOrRevertQty(DebitNoteVM debitNoteVM, int screenCode);
    }
}
