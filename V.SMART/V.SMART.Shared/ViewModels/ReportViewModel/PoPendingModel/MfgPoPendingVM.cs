using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.PoPendingModel
{
    public class MfgPoPendingVM
    {
        public long SlNo { get; set; }
        public int LineNo { get; set; }

        public string? Customer { get; set; }

        public string? PONo { get; set; }
        public DateTime PODate { get; set; }

        public string? Currency { get; set; }

        public string? Type { get; set; }
        public string? PaymentTerms { get; set; }

        public string? DeliveryTerms { get; set; }
        public string? Remarks { get; set; }

        public bool Rejection { get; set; }
        public string? POStatus { get; set; }

        // Item Details
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        public decimal Qty { get; set; }
        public decimal BalQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountPerce { get; set; }
        public decimal BasicAmt { get; set; }

        public string? DueDate { get; set; }
        public string? PoGetDate { get; set; }

       // public string? PoItemStatus { get; set; }

        public string? ItemRemarks { get; set; }
        public string? RefQuoteNo { get; set; }
        public string? RefQuoteDt { get; set; }

       // public bool ItemCancel { get; set; }
    }
}
