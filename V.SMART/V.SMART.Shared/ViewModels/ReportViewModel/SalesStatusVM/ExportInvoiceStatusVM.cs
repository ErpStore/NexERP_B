using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.SalesStatusVM
{
    public class ExportInvoiceStatusVM
    {
        public long SlNo { get; set; }

        public string? Customer { get; set; }

        public string? ExpInvoiceNo { get; set; }
        public string? ExpInvoiceDate { get; set; }

        public string? RefDCNo { get; set; }
        public string? RefDCDate { get; set; }


        public string? RefPoNo { get; set; }
        public string? RefPoDate { get; set; }

        public string? MainRemarks { get; set; }

        public string? InvoiceStatus { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        public decimal Qty { get; set; }
        public decimal BalQty { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal LineGross { get; set; }
        public decimal LineDiscountAmount { get; set; }
        public decimal LineBasicAmount { get; set; }

        public decimal LineCGSTRate { get; set; }
        public decimal LineSGSTRate { get; set; }
        public decimal LineIGSTRate { get; set; }



        public string? ItemRemarks { get; set; }

        public bool ItemCancel { get; set; }

        public decimal GrandTotal { get; set; }
        public decimal Balance { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public string? ModifiedDate { get; set; }
    }
}
