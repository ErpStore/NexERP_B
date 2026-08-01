using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.RatingsVM
{
    public class FPYVM
    {
        public string Itemcode { get; set; }
        public string PartyName { get; set; }
        public string ItemName { get; set; }

        public decimal ProductionQty { get; set; }
        public decimal RejectionQty { get; set; }
        public decimal ReworkQty { get; set; }

        public decimal FPYQty { get; set; }
        public decimal FPYPercentage { get; set; }
    }
}
