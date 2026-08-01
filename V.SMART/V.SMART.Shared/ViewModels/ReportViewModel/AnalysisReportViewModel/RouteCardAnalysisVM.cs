using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;

namespace V.SMART.Shared.ViewModels.ReportViewModel.AnalysisReportViewModel
{
    public class RouteCardAnalysisVM
    {
        public int SlNo { get; set; }
        public int RCId { get; set; }
        public int? CustId { get; set; }
        public string? PONo { get; set; }

        public string? PODate { get; set; }
        public string RCNo { get; set; }

        public string Suffix { get; set; }
        public string RCDate { get; set; }

        public decimal RCQty{get; set; }

        public string? Customer { get; set; }

        public string? Assembly { get; set; }
        public string Component { get; set; }
        public string RM { get; set; }
        public decimal RMQty { get; set; }
        public bool IsManual { get; set; }
        public string? MfgOrLab{ get; set; }

        public int? RCStatus { get; set; }

        public string? DCNo { get; set; }

        public string? DCDate  { get; set; }

        public string? InvNo { get; set; }

        public string? InvDate { get; set; }

       
    }

    public class RouteCardAnalysisDetailsVM
    {
        public long SlNo { get; set; }

        public int SeqNo { get; set; }

        public string ProcessName { get; set; }

        public string TransType { get; set; }

        public decimal TotalQty { get; set; }
        public decimal IssuedQty { get; set; }

        public decimal AccQty { get; set; }

        public decimal RejQty { get; set; }
        public decimal RewQty { get; set; }
        public string IssueNo { get; set; }
        public string GRNNo { get; set; }
        public string SCNNo { get; set; }
        public int ProcessStatus { get; set; }
        
    }

    

    public class StatusVM
    {
        public int StatusId { get; set; }

        public string StatusName { get; set; }
    }

}
