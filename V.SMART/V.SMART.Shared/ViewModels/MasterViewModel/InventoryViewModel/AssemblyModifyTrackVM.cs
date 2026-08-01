using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel
{
    public class AssemblyModifyTrackVM
    {
        public int RecNo { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string MeasureUnit { get; set; } = string.Empty;
        public decimal UtilityQty { get; set; }
        public string? Cancel { get; set; }
        public string? ProcessName { get; set; }
        public string? Remark { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
