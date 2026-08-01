using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InspectionViewModel.IncomingInspectionVM
{
    public class IncomingInspectionHelpSubVM
    {
        public int? ItemId { get; set; }

        public string? ItemName { get; set; } = string.Empty;

        public string? ItemCode { get; set; } = string.Empty;

        public string? Uom { get; set; } = string.Empty;

        public decimal? Qty { get; set; }
        public int? processId { get; set; }
        public string? processname {get; set;} = string.Empty;

        public int?  CommonGrnSubId { get; set; }
    }
}
