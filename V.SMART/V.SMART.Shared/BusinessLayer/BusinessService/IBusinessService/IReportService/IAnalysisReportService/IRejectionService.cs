
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IRejectionService
    {
        // MAIN REPORT
        Task<List<RejectionVM>> GetRejectionAnalysisAsync(
            int reportType,
            DateTime fromDate,
            DateTime toDate,
            int partyId = 0,
            int itemId = 0);

        // DROPDOWNS
        Task<List<CustomerVM>> GetCustomersAsync(DateTime fromDate, DateTime toDate, string typeOfCustomer);
        Task<List<VendorVM>> GetVendorsAsync(DateTime fromDate, DateTime toDate, string typeOfVendor);
        Task<List<ItemVM>> GetItemsAsync(DateTime fromDate, DateTime toDate, string typeOfCustomer, int Custid);
    }
}