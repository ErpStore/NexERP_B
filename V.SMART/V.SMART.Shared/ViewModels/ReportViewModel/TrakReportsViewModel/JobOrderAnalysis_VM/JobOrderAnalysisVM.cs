using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.AnalysisViewModel
{
    public  class JobOrderAnalysisVM
    {
        public int? SlNo { get; set; }

        public string? CustName { get; set; }
        public string? PoNo { get; set; }
        public DateTime? PODate { get; set; }

        public string? JobNo { get; set; }
        public DateTime? JobDate { get; set; }

        public decimal? JobQty { get; set; }
        public decimal? JobBalQty { get; set; }

        public string? Assembly { get; set; }
        public string? Item { get; set; }

        public string? AssemblyIssNo { get; set; }
        public decimal? IssQty { get; set; }
        public decimal? IssBalQty { get; set; }

        public string? GRNNo { get; set; }
		public string? ReturnItem { get; set; }

		public decimal? ReturnQty { get; set; }
        public decimal? ReturnBalQty { get; set; }

        public string? AssSCNNo { get; set; }
        public decimal? AccQty { get; set; }
        public decimal? ReworkQty { get; set; }
        public decimal? RejQty { get; set; }
        public decimal? SCNBalQty { get; set; }

        public string? DcNo { get; set; }
        public string? InvNo { get; set; }

        public string? LabourGrnNo { get; set; }
        public string? LabourScnNo { get; set; }
        public string? DcOutgoingNo { get; set; }
        public string? LabInvNo { get; set; }
       
       
    }

}
