using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.AnalysisViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IAnalysisReportService
{
    public  interface IJobOrdeAnalysisService
    {
       // Task<List<AnalysisVM>> GetAnalysis();
        Task<List<JobOrderAnalysisVM>> GetAnalysis(bool type, DateTime Fromdate, DateTime Todate);

        Task<List<JobOrderAnalysisVM>> GetAnalysis(string type, DateTime Fromdate, DateTime Todate);
    }
}
