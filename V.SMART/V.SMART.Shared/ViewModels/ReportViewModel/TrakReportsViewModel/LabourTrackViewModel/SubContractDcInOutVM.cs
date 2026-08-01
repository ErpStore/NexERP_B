using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel
{
    public class SubContractDcInOutVM
    {
        public long SlNo { get; set; }

        // =========================================
        // SUPPLIER
        // =========================================
        public string? Supplier { get; set; }

        // =========================================
        // DC OUT
        // =========================================
        public string? DcNo { get; set; }

        public string? DcDate { get; set; }
        public string? TransType { get; set; }

        // =========================================
        // ITEM DETAILS
        // =========================================
        public string? ItemCodeOut { get; set; }

        public string? ItemName { get; set; }

        public string? MeasureUnit { get; set; }

        public string? HSNCode { get; set; }

        // =========================================
        // QTY DETAILS
        // =========================================
        public decimal QtyOut { get; set; }

        public decimal BalQty { get; set; }

        // =========================================
        // GRN IN
        // =========================================
        public string? GRNNo { get; set; }

        public string? GRNDate { get; set; }

        public string? RefDcNo { get; set; }

        public string? RefDcDate { get; set; }
        public decimal QtyIn { get; set; }

        // =========================================
        // SCN
        // =========================================
        public string? SCNNo { get; set; }

        public string? SCNDate { get; set; }

        public decimal WOPQty { get; set; }

        public decimal RejQty { get; set; }

        public decimal RewQty { get; set; }

        // =========================================
        // PO
        // =========================================
        public string? RefPoNo { get; set; }

        public string? RefPoDate { get; set; }

        // =========================================
        // INVOICE
        // =========================================
        public string? InvoiceNo { get; set; }

        public string? InvoiceDate { get; set; }

        public decimal InvQty { get; set; }
    }
}
