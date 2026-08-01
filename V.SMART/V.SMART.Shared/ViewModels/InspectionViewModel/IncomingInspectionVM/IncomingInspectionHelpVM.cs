using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InspectionViewModel.IncomingInspectionVM
{
    public class IncomingInspectionHelpVM
    {
        public long? productionLogId {  get; set; }
        public string GRNNo { get; set; } = string.Empty;
        public string Suffix {  get; set; } = string.Empty;
        public string DcNo { get; set; } = string.Empty;
        public DateTime? DcDate { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; } = string.Empty;
        public int? ItemId { get; set; }
        public string? ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; } = string.Empty;
        public string Uom { get; set; } = string.Empty;
        public decimal Qty { get; set; }

        public int? processId {get; set; }
        public string? processname { get; set; } = string.Empty;

        public List<IncomingInspectionHelpSubVM> Items { get; set; } = new List<IncomingInspectionHelpSubVM>();
    }
}
