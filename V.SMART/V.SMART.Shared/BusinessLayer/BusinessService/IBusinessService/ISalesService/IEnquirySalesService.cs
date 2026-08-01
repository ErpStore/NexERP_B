using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.FeasibilityVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesEnquiryVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.MaterialRequisitionViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService
{
    public interface IEnquirySalesService
    {
        #region Company & Master Data
        Task<Companydetails> GetCompanyDetailsAsync();
        Task<int> GetDecimalPlacesAsync();
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText, int? custId = null);
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);
        Task<List<ContactPerson>> GetContactPersonsAsync(int custId);
        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        #endregion

        Task<byte[]> CreateTemplateAsync(string uploadtype);

        #region Enquiry Operations

        Task<bool> IsEnquiryTransactionsMatchedAsync(int enquiryId, EnquirySalesVM enqVMs);
        Task<(List<EnquirySalesVM> enquirySalesVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<(bool CanDelete, string Message)> CanDeleteSalesEnquiryAsync(int enqId);
        Task UpsertSalesEnquiryShortCloseAsync(EnquirySalesVM enquirySalesVM);
        Task<List<EnquiryFeasibilityVM>> GetFeasibilityDetailsByEnqIdAsync(string enquiryNo);
        Task<EnquirySalesVM?> GetEnquiryDetailsByEnqIdAsync(int enquiryId);
        Task<string> GetPrefixFromDbAsync();
        Task<EnquirySalesVM> UpsertEnquirySalesAsync(EnquirySalesVM enquirySalesVM);
        Task<IEnumerable<EnquirySalesVM>> GetAllEnquirySalesAsync();
        Task<bool> HasAnyQuotationTransactionMadeAsync(int enquiryId);
        Task<bool> HasAnyEnqFesibilityTransactionMadeAsync(int enquiryId);
        Task<bool> DeleteEnquiryAsync(int enquiryId);
        #endregion

        #region Enquiry Subitem Operations

        Task<(bool CanItemCancel, string Message)> CanEnquiryItemCancelCheckAsync(EnquirySalesSubVM subItem);
        Task UpdatedCancelStatusAndAddOrRevertQty(EnquirySalesVM enquirySalesVM);
        Task UpdateItemCancelAndAddorRevertAsync(EnquirySalesSubVM subItem, int enqId);
        Task<bool> IsDuplicateEnquiryAsync(string enquiryNo, string suffix, int? enquiryId = null, int? custId = null);
        Task DeleteEnqSubItemAndResequenceAsync(EnquirySalesSubVM subitem, EnquirySalesVM enquiry);
        Task<List<EnquirySalesSubVM>> GetEnqSubByEnquiryIdAsync(int enqId);
        Task<EnquirySalesSub?> GetEnqSubItemDetailByEnqSubIdAsync(int subEnqId);
        #endregion

        Task<byte[]?> GenerateAndOpenPdf(int RefId, string refTemplatefrx, string ParamName, bool Cancel, string ScreenName, string ProcedureName);

        Task<DataTable> GetSalesandLabeourEnquiryPendingList(string status);
    }

}
