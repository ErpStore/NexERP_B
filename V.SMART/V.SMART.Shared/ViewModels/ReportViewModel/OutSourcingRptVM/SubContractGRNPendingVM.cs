using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM
{
    public class SubContractGRNPendingVM
    {
        public long SlNo { get; set; }
        public string? Vendor { get; set; }
        public string? GRNNo { get; set; }
        public DateTime? GRNDate { get; set; }
        public string? PartyDC { get; set; }
        public DateTime? PartyDcDate { get; set; }
        public string? DcNo { get; set; }
        public DateTime? DcDate { get; set; }
       
        public string? MainRemark { get; set; }

        public string? SubContractGRNStatus { get; set; }
        public string? CancelReason { get; set; }
        public DateTime? Canceldate { get; set; }
        public string? CancelBy { get; set; }

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

        public string? ItemStatus { get; set; }
        public string? ItemCancelReason { get; set; }
        public string? ItemRemarks { get; set; }
        //
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
