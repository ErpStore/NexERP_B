using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService
{
    public class Profit_LossAccountsService : IProfit_LossAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IReportExecutor _report;


        public Profit_LossAccountsService(
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

        public async Task<List<ProfitLossMonthlyVM>> GetMonthlyProfitAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var parameters = new[]
                {
                      new SqlParameter("@FromDate", fromDate),
                      new SqlParameter("@ToDate", toDate)
        };

                var result = await _report.ExecuteAsync<ProfitLossMonthlyVM>(
                    "SP_ProfitLossMonthly",
                    parameters
                );

                return result ?? new List<ProfitLossMonthlyVM>();
            }
            catch (Exception ex)
            {
                _logs.LogDeveloperError(
                    ex,
                    "Error executing stored procedure SP_ProfitLossAccount"
                );

                return new List<ProfitLossMonthlyVM>();
            }
        }
        

        public async Task<List<Profit_LossAccountsVM>> GetProfitLossAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var parameters = new[]
                {
                      new SqlParameter("@FromDate", fromDate),
                      new SqlParameter("@ToDate", toDate)
        };

                var result = await _report.ExecuteAsync<Profit_LossAccountsVM>(
                    "SP_ProfitLossAccount",
                    parameters
                );

                return result ?? new List<Profit_LossAccountsVM>();
            }
            catch (Exception ex)
            {
                _logs.LogDeveloperError(
                    ex,
                    "Error executing stored procedure SP_ProfitLossAccount"
                );

                return new List<Profit_LossAccountsVM>();
            }
        }

       
    }
}
