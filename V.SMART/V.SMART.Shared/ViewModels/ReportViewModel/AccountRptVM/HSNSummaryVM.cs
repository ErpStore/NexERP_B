using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.AccountsReportViewModel
{
    public class HSNSummaryVM
    {
        public int SlNo { get; set; }

        public string? Customer { get; set; }

        public string? InvoiceNo { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public string? Type { get; set; }

        public string? HSNCode { get; set; }

        public string? PartNo { get; set; }

        public string? PartName { get; set; }

        public string? UOM { get; set; }

        public decimal? Qty { get; set; }

    }
}
