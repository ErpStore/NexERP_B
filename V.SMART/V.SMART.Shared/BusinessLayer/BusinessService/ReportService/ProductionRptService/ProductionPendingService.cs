using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.ReportViewModel.PoPendingModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService
{
    public class ProductionPendingService : IProductionPendingService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IReportExecutor _report;


        public ProductionPendingService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IReportExecutor report)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _report = report;

        }


        public async Task<List<ProductionPendingVM>> GetPendingStatements(bool DetailsView,string partyType, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@DetailsView", DetailsView != null ? (object)DetailsView : DBNull.Value),
                    new SqlParameter("@partyType", partyType ?? (object)DBNull.Value),
                    new SqlParameter("@FromDate", fromDate ?? (object)DBNull.Value),
                    new SqlParameter("@ToDate", toDate ?? (object)DBNull.Value)

                };

                var result = await _report.ExecuteAsync<ProductionPendingVM>(
                    "sp_GetProductionPendingReport",
                    parameters
                );

                return result ?? new List<ProductionPendingVM>();
            }
            catch (Exception ex)
            {
                _logs.LogDeveloperError(
                    ex,
                    "Error executing stored procedure SP_BillPending"
                );

                return new List<ProductionPendingVM>();
            }
        }
    }

}

