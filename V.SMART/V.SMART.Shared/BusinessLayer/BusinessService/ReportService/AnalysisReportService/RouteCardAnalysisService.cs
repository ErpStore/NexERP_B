using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using FastReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IAnalysisReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.AnalysisReportViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.AnalysisReportService
{
    public class RouteCardAnalysisService : IRouteCardAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IReportExecutor _report;

        public RouteCardAnalysisService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs, IReportExecutor report, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _report = report;
            _mapper = mapper;
        }

        

        public async Task<List<RouteCardAnalysisVM>> GetRouteCardAnalysisAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                return await _report.ExecuteAsync<RouteCardAnalysisVM>(
                "Sp_RouteCardAnalysisSummary",
                new SqlParameter("@FromDate", fromDate),
                new SqlParameter("@ToDate", toDate));
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<List<RouteCardAnalysisDetailsVM>> GetRouteCardAnalysisDetailsAsync(int rcid)
        {
            try
            {
                return await _report.ExecuteAsync<RouteCardAnalysisDetailsVM>(
                "Sp_RouteCardAnalysisDetails",
                new SqlParameter("@RcId", rcid));
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<List<RouteCardVM>> GetAllRCCustomersAsync(DateTime fromDate,DateTime toDate)
        {
            try
            {
                var query = _unitOfWork.RouteCards.GetQueryable();

                query = query.Where(x =>
                    x.RCDate >= fromDate.Date &&
                    x.RCDate < toDate.Date.AddDays(1));


                var customers = await _unitOfWork.Customers.GetQueryable()
                    .Where(c => query.Any(r => r.CustId == c.CustId))
                    .Select(c => new RouteCardVM
                    {
                        CustId = c.CustId,
                        CustName = c.CustName
                    })
                    .OrderBy(x => x.CustName)
                    .ToListAsync();


                if (await query.AnyAsync(x => x.IsManual))
                {
                    customers.Add(new RouteCardVM
                    {
                        CustId = -1,
                        CustName = "Manual"
                    });
                }

                return customers;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetAllRCCustomersAsync on RouteCardAnalysis Load");
                return (new List<RouteCardVM>());
            }
        }
    }
}
