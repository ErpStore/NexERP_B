using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel
{
    public class LabourDcInOutTallyVM
    {
        public long SlNo { get; set; }

        // Customer
        public string? Customer { get; set; }

        // Labour GRN
        public string? GRNNo { get; set; }
        public string? GRNDate { get; set; }

        // Reference DC
        public string? RefDcNo { get; set; }
        public string? RefDcDate { get; set; }
        public string? TransType { get; set; }

        // Item Details
        public string? ItemCode_In { get; set; }
        public string? ItemName { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        // Incoming Qty
        public decimal ReceivedQty { get; set; }
        public decimal BalQty { get; set; }

        // SCN Details
        public string? SCNNo { get; set; }
        public string? SCNDate { get; set; }

        public decimal WOPQty { get; set; }
        public decimal RejQty { get; set; }
        public decimal RewQty { get; set; }

        // PO Details
        public string? PONo { get; set; }
        public string? PODate { get; set; }

        public int? LineNo { get; set; }

        // DC Out
        public string? DcNoOut { get; set; }
        public string? DcDate { get; set; }
        public decimal Qty_Out { get; set; }

        ////OutItem
        //public string? ItemCode_Out { get; set; }
        //public string? ItemName_Out { get; set; }
        //public string? MeasureUnit_Out { get; set; }
        //public string? HSNCode_Out { get; set; }

        // Invoice
        public string? InvoiceNo { get; set; }
        public string? InvDate { get; set; }
        public decimal Qty { get; set; }
        public string? BatchNo { get; set; }

        public string? Process { get; set; }

        public int? Duedays { get; set; }
    }
}
