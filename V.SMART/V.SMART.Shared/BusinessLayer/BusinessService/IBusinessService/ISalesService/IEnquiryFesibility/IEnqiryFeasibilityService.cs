using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.SalesAndLabour.Fesibility;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.FeasibilityVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesEnquiryVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService.IEnquiryFesibility
{
    public interface IEnqiryFeasibilityService
    {

        #region Company & Master Data
        Task<IEnumerable<string>> ToGetActiveUsers();
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<Companydetails> GetCompanyDetailsAsync();
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        #endregion

        #region Enquiry Operations

        Task<(List<EnquiryFeasibilityVM> enqFesVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<IEnumerable<EnquiryFeasibilityVM>> GetAllEnquiryFeasibilityAsync();
        Task<EnquiryFeasibilityVM> GetEnquiryDetailsByEnqIdAsync(int enquiryId);
        Task<string> GetFeasibilityNumberAsync(string suffix);
        Task<IEnumerable<EnquirySalesVM>> GetAvailableEnquiriesAsync();
        Task<EnquiryFeasibilityVM?> GetEnquiryFeasibilityDetailsByFesIdAsync(int FesId);
        Task<bool> DeleteEnquiryAsync(int FesId);
        Task<EnquiryFeasibilityVM> UpsertEnquiryFeasibilityAsync(EnquiryFeasibilityVM vm);
        #endregion




    }
}
