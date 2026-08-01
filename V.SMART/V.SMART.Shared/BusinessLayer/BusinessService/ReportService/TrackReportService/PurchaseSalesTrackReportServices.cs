using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.PurchaseSalesTrackViewModel;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using Microsoft.Data.SqlClient;





namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService
{
    public class PurchaseSalesTrackReportServices : IPurchaseSalesTrackReportServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IReportExecutor _report;

        public PurchaseSalesTrackReportServices(
            IUnitOfWork unitOfWork,
            CurrentUserService userService,
            ICommonService commonService,
            ILoggingService logs, IReportExecutor report)
        {

            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _unitOfWork = unitOfWork;
            _logs = logs;
            _report = report;
        }
        public async Task<List<PurchaseSalesTrackVM>> GetPurchaseSalesTrack(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var result = await _report.ExecuteAsync<PurchaseSalesTrackVM>(
                    "Sp_PurchaseSalesTrack",

                    new SqlParameter("@FromDate", fromDate),

                    new SqlParameter("@ToDate", toDate)
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    "Failed to load GetViewTallyDcInOutAsync");

                return new List<PurchaseSalesTrackVM>();
            }
        }
    }
}
