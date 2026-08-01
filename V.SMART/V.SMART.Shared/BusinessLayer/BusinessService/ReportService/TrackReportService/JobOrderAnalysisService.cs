using AutoMapper;
using FastReport;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IAnalysisReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.ReportViewModel.AnalysisViewModel;


namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.AnalysisService
{
    public class JobOrderAnalysisService : IJobOrdeAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IReportExecutor _report;

        string filteredData;


        public JobOrderAnalysisService(IUnitOfWork unitOfWork, ICommonService commonService, CurrentUserService userService, ILoggingService logs, IMapper mapper, IReportExecutor report)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
            _report = report;
        }
       

        public async Task<List<JobOrderAnalysisVM>> GetAnalysis(bool type,DateTime Fromdate,DateTime Todate)
        {
         
            try
            {
                var result = await _report.ExecuteAsync<JobOrderAnalysisVM>(
                    "Sp_BomAnalysis",
                    new SqlParameter("@Type", type),
                    new SqlParameter("@FromDate", Fromdate),

                    new SqlParameter("@ToDate", Todate)

                    
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    "Failed to load GetViewTallyDcInOutAsync");

                return new List<JobOrderAnalysisVM>();
            }

        }


        public async Task<List<JobOrderAnalysisVM>> GetAnalysis(string type, DateTime Fromdate, DateTime Todate)
        {
           
            try
            {
                var result = await _report.ExecuteAsync<JobOrderAnalysisVM>(
                    "Sp_JobOrderTrack",
                    new SqlParameter("@Type", type),
                    new SqlParameter("@FromDate", Fromdate.ToString("yyyy-MM-dd 00:00:000.00")),

                    new SqlParameter("@ToDate", Todate.ToString("yyyy-MM-dd 23:59:59.00"))


                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    "Failed to load GetViewTallyDcInOutAsync");

                return new List<JobOrderAnalysisVM>();
            }

        }
    }
}
