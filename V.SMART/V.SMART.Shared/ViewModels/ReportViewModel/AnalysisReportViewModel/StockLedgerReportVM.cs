using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.AnalysisReportViewModel
{
    public class StockLedgerReportVM
    {
        public long SlNo { get; set; }
        public DateTime Date { get; set; }
        public string ItemCode { get; set; }
        public string DocNo { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal IssueQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public string Screen { get; set; }
        public decimal StockQty { get; set; }
        public decimal OpeningBalance { get; set; }
        //public decimal ClosingBalance { get; set; }
    }
}
