using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM
{
    public class SubConInvoicePendingVM
    {
        public long SlNo { get; set; }
        public string? Vendor { get; set; }
        public string? InvNo { get; set; }
        public DateTime? InvDate { get; set; }
        public string? SCNNo { get; set; }
        public DateTime? SCNDate { get; set; }

        public string? MainRemark { get; set; }

        public string? SubContractInvoiceStatus { get; set; }
        public string? CancelReason { get; set; }
        public string? Curreny { get; set; }
        public decimal? BasicAmount { get; set; }
        public decimal? GrandTotal { get; set; }
      
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Specification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

       
        public decimal? Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? RejectedQty { get; set; }
        public decimal? ReworkQty { get; set; }
        public decimal? BalQty { get; set; }
        public string? ItemStatus { get; set; }
        public string? ItemCancelReason { get; set; }
        public string? Remarks { get; set; }
        
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
