
using Microsoft.Data.SqlClient;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService
{
    public class ItemWiseReportService : IItemWiseReportService
    {
        private readonly IReportExecutor _report;

        private readonly ICommonService _commonService;

        public ItemWiseReportService(IReportExecutor report,ICommonService commonService)
        {
            _report = report;

            _commonService = commonService;
        }

        public async Task<List<ItemWiseReportVM>> GetItemWiseReport(int ItemId, DateTime fromDate, DateTime toDate)
        {
            var parameters = new[]
            {
                new SqlParameter("@ItemId", ItemId),
                new SqlParameter("@FromDate", fromDate),
                new SqlParameter("@ToDate", toDate)
            };

            try
            {
                var result = await _report.ExecuteAsync<ItemWiseReportVM>("Sp_ItemWiseHistoryReport",parameters);

                return result ?? new List<ItemWiseReportVM>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<ItemVM>> SearchItemAsync(string searchText)
        {
            return await _commonService.SearchItemsAsync(searchText);
        }
    }
}