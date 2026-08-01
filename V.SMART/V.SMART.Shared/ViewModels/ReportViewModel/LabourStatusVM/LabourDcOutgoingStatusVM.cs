using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.GRNPendingVM
{
    public class LabourDcOutgoingStatusVM
    {
        public long SlNo { get; set; }

        public string? Customer { get; set; }

        public string? DCNo { get; set; }
        public string? DCDate { get; set; }

        public string? RefGRNNo { get; set; }
        public string? RefGRNDate { get; set; }

        public string? RefPoNo { get; set; }
        public string? RefPoDate { get; set; }

        public string? IssueFromStore { get; set; }

        public string? TransferFrom { get; set; }

        public string? TransferTo { get; set; }
        public string? VehicleNo { get; set; }
        public string? TransportMode { get; set; }

        public string? MainRemarks { get; set; }

        public string? DCStatus { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        public decimal Qty { get; set; }
        public decimal BalQty { get; set; }
        public decimal UnitPrice { get; set; }

       

        public string? ItemRemarks { get; set; }

        public bool ItemCancel { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public string? ModifiedDate { get; set; }
    }
}
