using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel
{
    public class PoPendingDetailsVM
    {

        public long SlNo { get; set; }
        public string Name { get; set; }
        public string PoNo { get; set; }
        public DateTime PoDate { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal BalQty { get; set; }
        public decimal StockQty { get; set; }
        public DateTime DueDate { get; set; }
        public string HSNCode { get; set; }

        public decimal ROL { get; set; }
        public decimal MOL { get; set; }

        public string Type { get; set; }
        public string Specification { get; set; }
        public string UOM { get; set; }
    }
}
