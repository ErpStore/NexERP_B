using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.PurchaseSalesTrackViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IPurchaseSalesTrackReportServices
    {
        //Task<List<PurchaseSalesTrackVM>> GetPurchaseSalesTrack(DateTime fromDate, DateTime toDate, int? custId, int? itemId);
        Task<List<PurchaseSalesTrackVM>> GetPurchaseSalesTrack(DateTime fromDate, DateTime toDate);
    }
}
