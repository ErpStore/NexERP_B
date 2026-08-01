using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM
{
    public class PurchaseEnquiryPendingList
    {
        public long SlNo { get; set; }
        public string? Vendor { get; set; }
        public string? Prefix { get; set; }
        public string? EnquiryNo { get; set; }
        public DateTime? EnquiryDate { get; set; }
        public string? MainRemarks { get; set; }
        //public decimal TotalValue { get; set; }
        public string? Type { get; set; }
        public string? EnquiryStatus { get; set; }
        public string? CancelReason { get; set; }
        public string? CancelBy { get; set; }
        public DateTime? Canceldate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? PocName { get; set; }
        public string? PocContactNo { get; set; }
        public DateTime? ExpactedReplayDate { get; set; }


        //itemDeatails
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Specification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }
        //subtable
        public decimal? Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? BalQty { get; set; }
        public string? ItemCancel { get; set; }
        public string? ItemCancelReason { get; set; }
        public string? ItemRemarks { get; set; }
    }
}
