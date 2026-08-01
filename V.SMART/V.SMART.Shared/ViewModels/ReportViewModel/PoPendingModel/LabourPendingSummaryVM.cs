using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.AccountsVM
{
    public class LabourPendingSummaryVM
    {

        public long? slno { get; set; }
        public int? CustId { get; set; }
        public string? CustName { get; set; }

        //Doc Details
        public string? DocNo { get; set; }
        public string? DocDate { get; set; }
        public string? RefDcNo { get; set; }

        public string? RefDcDate { get; set; }

        public string? BatchNo { get; set; }
        public int? NoofItems { get; set; }

        //Item Details
        public int? LineNo { get; set; } = 0;
        public int? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ChildItemCode { get; set; }
        public string? childItemName { get; set; }

        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public decimal? Qty { get; set; }
        public decimal? BalQty { get; set; }
        public decimal? UtilQty { get; set; }
        public decimal? RequiredQty { get; set; }
        public decimal? ReceivedQty { get; set; }
        public decimal? PendingQty { get; set; }
        public string? TransType { get; set; }

        public string? PONo { get; set; }

        public string? PODate { get; set; }
        public string? createdBy { get; set; }
        public string? CreatedDate { get; set; }
    }

}
