using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IViewTallyDCInOutTrackService
    {
        Task<List<CustomerVM>> GetCustomersByDate(DateTime fromDate, DateTime toDate);
        Task<List<VendorVM>> GetSupplierByDate(DateTime fromDate, DateTime toDate);

        Task<List<ItemVM>> GetItemsByCustomer(DateTime fromDate, DateTime toDate,int custId);

        Task<List<ItemVM>> GetItemsBySupplier(DateTime fromDate, DateTime toDate, int VendorCode);

        Task<List<LabourDcInOutTallyVM>> GetViewTallyDcInOutByCustomerAsync(DateTime fromDate, DateTime toDate, int? custId, int? itemId);
        Task<List<SubContractDcInOutVM>> GetViewTallyDcInOutBySupplierAsync(DateTime fromDate, DateTime toDate, int? custId, int? itemId);
    }
}
