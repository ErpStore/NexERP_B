using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.AnalysisReportViewModel
{
    public class StockAnalysisVM
    {
        public long SlNo { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string MeasureUnit { get; set; }
        public string CategoryName { get; set; }
        public decimal UnitRate { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OpeningStockQty { get; set; }
        public decimal OpeningStockValue { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal ReceivedQtyValue { get; set; }
        public decimal IssuedQty { get; set; }
        public decimal IssuedQtyValue { get; set; }
        public decimal ClosingStockQty { get; set; }
        public decimal ClosingStockValue { get; set; }

    }
}
