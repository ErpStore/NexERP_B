
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IItemWiseReportService
    {
        Task<List<ItemWiseReportVM>> GetItemWiseReport(int ItemId, DateTime fromDate, DateTime toDate);

        Task<IEnumerable<ItemVM>> SearchItemAsync(string searchText);
    }
}