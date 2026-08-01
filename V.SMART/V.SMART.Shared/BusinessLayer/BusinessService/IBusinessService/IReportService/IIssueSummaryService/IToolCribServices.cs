

using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IToolCribServices
    {
        Task<List<ToolCribVM>> GetReport(DateTime? fromDate,
            DateTime? toDate,
            string? type);
    }
}