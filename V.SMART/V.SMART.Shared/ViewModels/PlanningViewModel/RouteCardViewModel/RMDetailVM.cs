using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel
{
    public class RMDetailsVM
    {
        public decimal RequiredQty { get; set; }
        public decimal StockQty { get; set; }
        public decimal Weight { get; set; }
        public string BatchNo { get; set; } = "";
        public string Remarks { get; set; } = "";
    }

}
