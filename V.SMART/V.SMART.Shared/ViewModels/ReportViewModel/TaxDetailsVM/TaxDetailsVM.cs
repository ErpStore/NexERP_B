using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TaxDetailsReportViewModel
{
    public class TaxDetailsVM
    {
        // ================= COMMON =================
        public string? InvType { get; set; }   // MFG / LABOUR / EXPORT / PURCHASE / SUBCON

        public int SlNo { get; set; }

        // ================= PARTY (Customer / Vendor) =================
        public string? CustName { get; set; }
        public string? GSTNo { get; set; }

        // ================= INVOICE =================

        public int? InvId { get; set; }
        public string? InvNo { get; set; }
        public DateTime? InvDate { get; set; }

        // ================= PO =================
        public string? PONo { get; set; }
        public DateTime? PODate { get; set; }

        public int? LineNo { get; set; }

        // ================= DC / GRN =================
        public string? RefDCNo { get; set; }
        public DateTime? RefDcDate { get; set; }

        // ================= GST =================
        public decimal? CGstRate { get; set; }
        public decimal? SGstRate { get; set; }
        public decimal? IGstRate { get; set; }

        public decimal? TotalCGSTAmount { get; set; }
        public decimal? TotalSGSTAmount { get; set; }
        public decimal? TotalIGSTAmount { get; set; }

        public decimal? TotalBasicAmount { get; set; }
        public decimal? GrandTotal { get; set; }

        // ================= E-INVOICE =================
        public string? ACKNO { get; set; }
        public string? IRNNo { get; set; }
        public string? Process { get; set; }

        // ================= ITEM =================
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Specification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        public decimal? Qty { get; set; }

        // ================= DISCOUNT =================
        public decimal? DiscountPercent { get; set; }
        public decimal? DiscountAmount { get; set; }

        // ================= ITEM GST =================
        public decimal? LineBasicAmount { get; set; }
        public decimal? UnitPrice { get; set; }

        public decimal? LineCGSTRate { get; set; }
        public decimal? LineSGSTRate { get; set; }
        public decimal? LineIGSTRate { get; set; }
        public decimal? LineDiscountPercent { get; set; }
        public decimal? LineDiscountAmount { get; set; }

    }
}
