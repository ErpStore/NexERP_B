using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.PurchaseGRNStatusViewModel
{
    public class PurchaseGRNStatusVM
    {
        public long SlNo { get; set; }
        public string? VendorSupplier { get; set; }
        public string? GRNNo { get; set; }
        public DateTime GRNDate { get; set; }
        public string? Type { get; set; }

        public string? RefDcNo { get; set; }
        public string? RefDcDt { get; set; }
        public string? RefInvNo { get; set; }
        public string? RefInvDt { get; set; }

        public string? Remarks { get; set; }
        public string? GRNStatus { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }
        public decimal Qty { get; set; }
        public decimal BalQty { get; set; }
        public decimal UnitPrice { get; set; }
       
        public string? ItemRemarks { get; set; }

        public string? RefPoNo { get; set; }
        public string? RefPoDt { get; set; }

    }
}
