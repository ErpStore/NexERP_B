using FastReport.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.AccountsReportViewModel
{
    public class TDSReportVM
    {

        public int SlNo { get; set; }

        public string Customer { get; set; } 

        public string Type { get; set; } 

        public string InvoiceNo { get; set; } 

        public DateTime? InvoiceDate { get; set; } 

        public decimal InvoiceValue { get; set; }

        public decimal TotalBasicAmount { get; set; }

        public string? CreditDebitNoteNo { get; set; }

        public DateTime? CreditDebitNoteDate { get; set; }

        public decimal? CreditDebitNoteAmt { get; set; }

        public decimal? TDSPercent { get; set; }

        public decimal? TDSAmt { get; set; }

        public decimal? TotalAmount { get; set; }

        public decimal? InvoiceBalance { get; set; }

        public string? Remarks { get; set; }

        public string? PANNo { get; set; }
    }
}
