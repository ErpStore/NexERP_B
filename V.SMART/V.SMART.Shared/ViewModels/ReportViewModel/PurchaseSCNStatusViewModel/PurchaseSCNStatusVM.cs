using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.PurchaseSCNStatusViewModel
{
    public class PurchaseSCNStatusVM
    {
        public long SlNo { get; set; }
        public string? VendorSupplier { get; set; }
        public string? SCNNo { get; set; }
        public DateTime SCNDate { get; set; }

        public string? RefPoNo { get; set; }
        public string? RefPoDt { get; set; }
        public string? RefGRNNo { get; set; }
        public string? RefGRNDt { get; set; }

        public string? Remarks { get; set; }
        public string? SCNStatus { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }
        public decimal Qty { get; set; }
        public decimal RejectQty { get; set; }
        public decimal BalQty { get; set; }
        public decimal RejectBalQty { get; set; }
        public decimal RewQty { get; set; }
        public decimal UnitPrice { get; set; }
        public string? ItemRemarks { get; set; }
    }
}
