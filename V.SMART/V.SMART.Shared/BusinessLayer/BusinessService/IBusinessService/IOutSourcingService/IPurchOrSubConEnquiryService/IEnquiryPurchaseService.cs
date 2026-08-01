using DocumentFormat.OpenXml.Vml.Office;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchOrSubConEnquiryVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using V.SMART.Shared.ViewModels.PlanningViewModel.JobOrderViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchOrSubConEnquiryService
{
    public interface IEnquiryPurchaseService
    {
        #region Company & Master Data
        Task<Companydetails> GetCompanyDetailsAsync();
        Task<int> GetDecimalPlacesAsync();
        Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<VendorVM?> GetVendorByIdAsync(int VendorCode);
        Task<List<VendorContact>> GetContactPersonsVendorAsync(int VendorCode);
        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int VendorCode, HashSet<int> usedCostCenterIds);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        #endregion


        #region Enquiry Operations

        Task<List<EnquiryPurchaseVendorAssignVM>> GetRefVendors(int EnquiryId);
        Task<List<EnquiryPurchaseVendorAssignVM>> GetAssignedVendorsByEnquiryIdAsync(int enquiryId);
        Task ValidateBeforeRevertAsync(int enqSubId);
        Task<EnquiryPurchaseSubVM?> GetEnqSubItemDetailByEnquirySubIdAsync(int enqSubId);
        Task<decimal> GetMreqBalQtyByMreqSubId(int mreqSubId);
        Task<bool> IsEnquiryTransactionsMatchedAsync(int enquiryId, EnquiryPurchaseVM enqVMs);
        Task<List<EnquiryPurchaseSub>> GetEnqPurchSubByEnqIdAsync(int enqId);
        Task<List<Dictionary<string, object>>> GetMreqDetails(bool isPurchOrSubCon);
        Task<string> GenerateEnquiryNumberAsync(string suffix);
        Task<EnquiryPurchaseVM?> GetEnquiryDetailsByEnqIdAsync(int enquiryId);
        Task<string> GetPrefixFromDbAsync();
        Task<EnquiryPurchaseVM> UpsertEnquiryPurchaseAsync(EnquiryPurchaseVM enquiryPurchaseVM);
        Task<EnquiryPurchaseVM> UpsertEnquiryPurchaseShortCloseAsync(EnquiryPurchaseVM enquiryPurchaseVM);
        Task<IEnumerable<EnquiryPurchaseVM>> GetAllEnquirySalesAsync();
        Task<bool> DeleteEnquiryAsync(int enquiryId);
        Task DeleteAndResequenceAsync(EnquiryPurchaseSubVM subitem, EnquiryPurchaseVM enquiryPurchaseVM);
        Task UpdatedCancelStatusAndAddOrRevertQty(EnquiryPurchaseVM enquiryPurchaseVM);
        Task ValidateMreqBalanceBeforeRevertAsync(EnquiryPurchaseSub sub);

        Task<(List<EnquiryPurchaseVM> enquiryPurchaseVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters);
        Task UpdateItemCancelAndAddorRevertAsync(EnquiryPurchaseSubVM subItem, int enqId);
        Task<(bool CanDelete, string Message)> CheckAnyTransectionsExists(int enquirySubId);
        Task<(bool CanDelete, string Message)> CanDeleteEnquiryAsync(int EnquiryId);
        Task<(bool CanDelete, string Message)> CanRemoveEnquiryAsync(int EnquiryId,int enqsubid);

        #endregion


        Task<List<PurchaseEnquiryPendingList>> GetPurchaseandSuContractEnquiryPendingList(string status);
    }

}
