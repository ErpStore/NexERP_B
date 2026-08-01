using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.RatingsVM;


namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IRatingService
    {
        Task<List<CustomerVM>> GetAllCustomersAsync(
        string type,
        DateTime fromdate,
        DateTime todate);

        Task<List<VendorVM>> GetAllVendorsAsync(
            string type,
            DateTime fromdate,
            DateTime todate);

        Task<List<RatingVM>> GetRatings(
            bool DetailsView,
            string partyType,
            string partyId,
            DateTime? fromDate,
            DateTime? toDate);
    }
}
