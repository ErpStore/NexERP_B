using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.GRNPendingVM
{
    public class LabourSCNStatusListVM
    {
        public long SlNo { get; set; }

        public string? Customer { get; set; }

        public string? SCNNo { get; set; }
        public string? SCNDate { get; set; }

        public string? RefGRNNo { get; set; }
        public string? RefGRNDate { get; set; }

        public string? IssueFromStore { get; set; }
        public string? AddToStore { get; set; }

        public string? MainRemarks { get; set; }

        public string? SCNStatus { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        public decimal AcceptQty { get; set; }
        public decimal BalQty { get; set; }
        public decimal RejectQty { get; set; }
        public decimal ReworkQty { get; set; }
        public decimal UnitPrice { get; set; }

        public string? ItemRemarks { get; set; }

        public bool ItemCancel { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public string? ModifiedDate { get; set; }

    }
}
