using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseInvoiceVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.PurchaseStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchase_Invoice_Service
{
    public interface IPurchaseInvoiceService
    {

        #region Common Service

        //🔹 Screen
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);

        //🔹 Vendor
        Task<VendorVM?> GetVendorByIdAsync(int VendorCode);

        Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText);

        Task<List<VendorContact>> GetContactPersonsVendorAsync(int Vendorcode);

        Task<List<VendorInDirect>> GetConsigneeAddressesAsync(int VendorCode);

        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);

        // 🔹 Correspond 
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);

        Task<List<Currency>> GetCurrenciesAsync();

        // 🔹 Decimal places
        Task<int> GetDecimalPlacesAsync();

        Task<Currency?> GetCurrencyByIdAsync(int currId);

        Task<decimal?> GetLatestCurrencyValueAsync(int currId);

        Task<Companydetails?> GetCompanyDetailsAsync();

        Task<List<PurchaseInvoiceSubVM>> GetDistinctRefSCNByPurchaseInvIdAsync(int invId);

        #endregion


        #region Invoice Operation

        Task<PurchaseSCNSubVM> GetSCNQtyDetailsFromSCNSubId(int SCNSubId);
        Task<List<Dictionary<string, object>>> GetSCNDetailsByVendorCode(int VendorCode);
        Task<(bool CanDelete, string Message)> CanDeletePurchaseInvAsync(int invId);
        Task<bool> IsInvTransactionsMatchedAsync(int invId, PurchaseInvoiceVM invoiceVMs);
        Task<List<PurchaseInvoiceSub>> GetInvSubByInvIdAsync(int InvId);
        Task ValidatePurchaseSCNBalanceBeforeRevertAsync(PurchaseInvoiceSub sub);
        Task LoadPOHeaderToInvoiceAsync(PurchaseInvoiceVM vm, int poId);
        Task<List<int>> GetPOIdsByPOSubIdsAsync(List<int> poSubIds);
        Task<bool> IsInvoiceExistsForVendorAsync(int vendorCode);
        Task LoadPreviousInvoiceHeaderAsync(PurchaseInvoiceVM vm);
        Task<(List<PurchaseInvoiceVM> invs, int totalCount)> GetPagedInvoiceAsync(int pageNumber, int pageSize, string search);

        Task<PurchaseInvoice?> GetLastInvAsync(int VendorCode);

        Task UpsertPurchaseInvShortCloseAsync(PurchaseInvoiceVM purchaseInvoiceVM);
        Task<PurchaseInvoiceVM> GetPurchaseInvoiceByIdAsync(int InvId);
        Task<bool> DeletePurchaseInvoiceByIdAsync(int InvId);
        Task DeleteAndResequenceAsync(PurchaseInvoiceSubVM subitem, PurchaseInvoiceVM InvVM);
        Task<int> GetPendingSCNCountAsync(int VendorCode);
        Task<List<PurchaseInvoiceSubVM>> GetInvoiceSubByInvIdAsync(int InvId);
        Task<bool> HasAnyItemOrPurchaseInvoiceCancelAsync(int InvId);
        Task<bool> GetPurchaseInvoiceByIdIsCancelAsync(int InvId);
        Task<decimal> GetExistingPurchInvoiceQtyByInvSubId(int InvSubId);
        Task<decimal> GetSCNItemInvoiceBalQtyFromSCNSubId(int SCNSubId);
        Task<PurchaseInvoiceSubVM?> GetInvSubItemDetailByInvSubIdAsync(int InvSubId);
        Task UpdatedCancelStatusAndAddOrRevertQty(PurchaseInvoiceVM InvVM, int screenCode);
        Task<bool> IsDuplicateInvoiceAsync(string InvNo, string suffix, int? currentInvId = null, int? VendorCode = null);
        Task<PurchaseInvoiceVM> UpsertPurchaseInvoice(PurchaseInvoiceVM purchgInvoiceVM, int screenCode);

        #endregion


        #region TDS Deducts
        //----TDS------------------
        Task<bool> UpdateTDSAmountAsync(PurchaseInvoiceVM purchgInvoiceVM);
        #endregion

        Task<(List<PurchaseInvoiceVM> invVms, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<bool> HasAnyItemOrInvoiceDebitNoteAsync(int RefSubInvId);

        Task<List<PurchaseInvoiceStatusVM>> GetPurchaseInvoicePendingListAsync(string status);

        Task<bool> IsDocumentUploaded(int invId);
    }
}
