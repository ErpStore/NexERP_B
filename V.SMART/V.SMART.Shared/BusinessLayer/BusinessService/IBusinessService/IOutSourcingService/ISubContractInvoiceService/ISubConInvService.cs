using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.OutSourcing.SubContractInvoice;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.ISubContractInvoiceService
{
    public interface ISubConInvService
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

        Task<List<SubConInvSubVM>> GetDistinctRefSCNByPurchaseInvIdAsync(int invId);

        #endregion



        #region Invoice Operation
        Task<(List<SubConInvVM> invs, int totalCount)> GetPagedInvoiceAsync(int pageNumber, int pageSize, string search);

        // Task<List<Dictionary<string, object>>> GetSCNDetailsByVendorCode(int VendorCode);
        Task<List<Dictionary<string, object>>> GetSCNDetailsByVendorCode(int VendorCode, bool AcptRejRewQtRequired);

        Task<SubConInv?> GetLastInvAsync(int VendorCode);

        Task<SubConInvVM> GetPurchaseInvoiceByIdAsync(int InvId);

        Task<bool> DeletePurchaseInvoiceByIdAsync(int InvId, int screenCode);

        Task DeleteAndResequenceAsync(SubConInvSubVM subitem, SubConInvVM InvVM);

        Task<int> GetPendingSCNCountAsync(int VendorCode);

        Task<List<SubConInvSubVM>> GetInvoiceSubByInvIdAsync(int InvId);

        Task<bool> HasAnyItemOrPurchaseInvoiceCancelAsync(int InvId);

        Task<bool> GetPurchaseInvoiceByIdIsCancelAsync(int InvId);

        Task<decimal> GetSCNItemPerformaBalQtyFromSCNSubId(int SCNSubId);

        Task<decimal> GetExistingPurchInvoiceQtyByInvSubId(int InvSubId);

        Task<decimal> GetSCNItemInvoiceBalQtyFromSCNSubId(int SCNSubId);

        Task<SubConInvSubVM?> GetInvSubItemDetailByInvSubIdAsync(int InvSubId);

        Task UpdateItemCancelAndAddorRevertAsync(SubConInvSubVM subItem, int screenCode, string InvNo, DateTime InvDate);

        Task UpdatedCancelStatusAndAddOrRevertQty(SubConInvVM InvVM, int screenCode);

        Task<bool> IsDuplicateInvoiceAsync(string InvNo, string suffix, int? currentInvId = null, int? VendorCode = null);

        Task<SubConInvVM> UpsertPurchaseInvoice(SubConInvVM purchgInvoiceVM, int screenCode);



        #endregion


        #region TDS Deducts
        //----TDS------------------
        Task<bool> UpdateTDSAmountAsync(SubConInvVM purchgInvoiceVM);
        #endregion

        Task<(List<SubConInvVM> Invies, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<bool> HasAnyItemOrInvoiceDebitNoteAsync(int RefSubInvId);

        Task<bool> IsDocumentUploaded(int Dcid);

        Task<List<SubConInvoicePendingVM>> GetSubContractInvoicePendingList(string status);
    }
}
