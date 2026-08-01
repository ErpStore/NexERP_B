using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM
{
    public class PurchaseQuotationPendingList
    {
        public long SlNo { get; set; }
        public string? Vendor { get; set; }
        //public string? Prefix { get; set; }
        public string? QuoteNo { get; set; }
        public DateTime? QuoteDate { get; set; }
        public string? EnquiryNo { get; set; }
        public DateTime? EnquiryDate { get; set; }
        //public DateTime? DueDate { get; set; }
        public string? MainRemark { get; set; }
        public string? Type { get; set; }
        public string? QuotationStatus { get; set; }
        //public string? PayTerms { get; set; }
        //public string? DeliveryTems { get; set; }
        public string? CancelReason { get; set; }
        public DateTime? Canceldate { get; set; }
        public string? CancelBy { get; set; }
        //public string? RejectReason { get; set; }
        public decimal? BasicAmount { get; set; }
        public decimal? TotalValue { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }


        //itemDeatails
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Specification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        //subtable
        public decimal? Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? LineBasicAmount { get; set; }
        public decimal? BalQty { get; set; }
        public string? ItemStatus { get; set; }
        public string? ItemCancelReason { get; set; }
        public string? ItemRemarks { get; set; }
    }
}
