using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.PurchaseStatusVM
{
    public class PurchaseInvoiceStatusVM
    {
        public long SlNo { get; set; }

        public string? InvoiceNo { get; set; }
        public string? InvoiceDate { get; set; }

        public string? Vendor { get; set; }

        public string? MainRemark { get; set; }

        public string? InvoiceStatus { get; set; }

        public string? RefSCNNo { get; set; }
        public string? RefSCNDate { get; set; }
        public string? RefPoNo { get; set; }
        public string? RefPoDate { get; set; }

       

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        public decimal Qty { get; set; }
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
