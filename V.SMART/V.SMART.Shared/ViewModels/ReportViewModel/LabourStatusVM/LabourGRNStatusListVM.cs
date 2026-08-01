using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.GRNPendingVM
{
    public class LabourGRNStatusListVM
    {
        public long SlNo { get; set; }
        public string? Customer { get; set; }

        public string? GRNNo { get; set; }
        public string? GRNDate { get; set; }

        public string? RefDcNo { get; set; }
        public string? RefDcDate { get; set; }

        public string? StoreName { get; set; }
        public string? Remarks { get; set; }

        public string? GRNStatus { get; set; }

        //Out
        public string? ItemCode_Out { get; set; }
        public string? ItemName_Out { get; set; }
        public string? ItemSpecification_Out { get; set; }
        public string? MeasureUnit_out { get; set; }
        public string? HSNCode_Out { get; set; }

        public decimal Qty_Out { get; set; }
        public decimal BalQty_Out { get; set; }
        public decimal UnitPrice_out { get; set; }


        // ItemIn Details
        public string? ItemCode_In { get; set; }
        public string? ItemName_In { get; set; }
        public string? ItemSpecification_In { get; set; }
        public string? MeasureUnit_In { get; set; }
        public string? HSNCode_In { get; set; }

        public decimal ReceivedQty { get; set; }
        public decimal ReceivedBalQty { get; set; }
        public decimal UnitPrice { get; set; }
       
       
        public string? ItemRemarks { get; set; }
        public string? RefPoNo { get; set; }
        public string? RefPoDt { get; set; }

        public int LineNo { get; set; }

        public bool ItemCancel { get; set; }


        public string? CreatedBy { get; set; }
        public string? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public string? ModifiedDate { get; set; }



    }
}
