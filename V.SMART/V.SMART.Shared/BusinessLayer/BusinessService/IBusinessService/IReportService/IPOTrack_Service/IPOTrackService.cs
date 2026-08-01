using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.POTrackVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IPOTrack_Service
{
    public interface IPOTrackService
    {
        Task<List<CustomerVM>> GetCustomersByDate(DateTime fromDate, DateTime toDate);

        Task<List<VendorVM>> GetSupplierByDate(DateTime fromDate, DateTime toDate);

        Task<List<InwardPOTrack>> GetInwordPOTrack(DateTime fromDate, DateTime toDate);

        Task<List<PurchasePOTrack>> GePurchasePOTrack(DateTime fromDate, DateTime toDate);
    }
}
