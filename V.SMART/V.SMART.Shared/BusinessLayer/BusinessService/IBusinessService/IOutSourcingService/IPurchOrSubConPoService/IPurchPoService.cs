using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchOrSubConEnquiryVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchOrSubConPoService
{
    public interface IPurchPoService
    {
        #region Company & Master Data
        Task<Companydetails> GetCompanyDetailsAsync();
        Task<int> GetDecimalPlacesAsync();
        Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<VendorVM?> GetVendorByIdAsync(int VendorCode);
        Task<List<Currency>> GetCurrenciesAsync();
        Task<Currency?> GetCurrencyByIdAsync(int currId);
        Task<decimal?> GetLatestCurrencyValueAsync(int currId);
        Task<List<TermsAndConditions>> GetTermsAsync();
        Task<List<VendorContact>> GetContactPersonsVendorAsync(int VendorCode);
        Task<List<VendorInDirect>> GetConsigneeAddressesVendorAsync(int VendorCode);
        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int VendorCode, HashSet<int> usedCostCenterIds);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);



        #endregion

        #region purchpo Operations
        Task UpdatedCancelStatusAndAddOrRevertQty(PurchPoVM poVM);
        Task<string> GetPrefixFromDbAsync(bool IsPurchase);
        Task<IEnumerable<PurchPoVM>> GetAllPurchPoAsync();
        Task<PurchPoVM?> GetPurchPoByIdAsync(int poId);
        Task<bool> DeletePOByPOIdAsync(int enquiryId);
        Task<string> GeneratePoNumberAsync();
        Task<PurchPo?> GetLastPoAsync(int VendorCode);
        Task<PurchPoVM> UpsertPoAsync(PurchPoVM purchPoVM);

        #endregion

        #region purchposub Subitem Operations

        Task<PurchPoSubVM?> GetPurchPoSubItemDetailByPoSubIdAsync(int poSubid);
        Task<decimal> GetQuoteItemBalQtyFromQuoteSubId(int QuoteSubId);
        Task<(bool CanItemCancel, string Message)> CanPoGRNItemCancelCheckAsync(PurchPoSubVM subItem);
        Task<List<PurchPoSub>> GetPoSubByPoIdAsync(int poId);
        Task ValidateBeforeRevertAsync(int poSubId);
        Task DeleteAndResequenceAsync(PurchPoSubVM subitem, PurchPoVM poVM);
        Task UpdateItemCancelAndAddorRevertAsync(PurchPoSubVM subItem, int poId);
        Task<bool> IsPoTransactionsMatchedAsync(int poId, PurchPoVM PurchPoVMs);
        Task<PurchPoSubVM?> GetPoSubItemDetailByPoSubIdAsync(int PoSubId);
        Task<(bool CanDelete, string Message)> CanDeletePurchaseOrderAsync(int poId);
        Task<List<PurchPoSubVM>> GetPOSubByPoIdAsync(int poId);
        Task<bool> IsDuplicatePoAsync(string poNo, string suffix, int? currentPoId = null, int? VendorCode = null,int? RevisionNo=null);
        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int VendorCode);
        Task<bool> DeletePOIdAsync(int Posubid, int poid, int MatReqSubId);
        Task<bool> DeletePOIdQuoteAsync(int Posubid, int poid, int RefQuoteSubid);
        Task<PurchPoVM> RevisePODraftAsync(int poid);
        Task<PurchPoVM> UpsertPurchaseOrderShortCloseAsync(PurchPoVM Purchpo);
        Task<List<RateComparisonVM>> GetRateComparisonAsync();

        #endregion

        #region Material Requisition and Quatation Operation

        Task<int> GetPendingQuoteCountAsync(int VendorCode);
        Task<List<Dictionary<string, object>>> GetQuoteDetailsByVendorCode(int VendorCode);
        Task<decimal> GetmatReqItemBalQtyFromMreqSubId(int MatreqSubId);
        Task<List<Dictionary<string, object>>> GetMaterialReqsByVendorCode(int VendorCode);
        Task UpdateOldMaterialReqSubBalQtyByID(int matReqSubId, decimal qtyToAdd);
        Task UpdateOldQuotationSubBalQtyByID(int Quotesubid, decimal qtyToAdd);
       Task<(List<PurchPoVM> purchPos, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        #endregion

        Task<bool> IsItemROlDisplayEnabledAsync();
        Task<Dictionary<int, decimal>> GetROlForItemsAsync(List<int> itemIds);

        Task<List<PurchasePoPendingListVM>> GetPurchasePosPendingListAsync(string status);
        Task<List<ProcessFlowChartVM>> GetProcessByCompAsync(int itemId);
        Task<List<RouteCardVM>> GetRcdetailsByCompIdAsync(int itemId);
        Task<List<ProcessFlowChartVM>> GetProcessByRcIdAsync(int rcId);
        Task<int?> GetRcdetailsByCompIdRcsubidAsync(int itemId, int processId, int rcid);
        Task<decimal?> GetRcBalQtyyCompIdRcsubidAsync(int rcSubId);
        Task<string?> GetPoNoAndSuffixByRcSubAsync(int rcSubId);
    }
}
