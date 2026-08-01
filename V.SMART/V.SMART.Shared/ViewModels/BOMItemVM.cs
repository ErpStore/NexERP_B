using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels
{
    public class BOMItem
    {
        public int Id { get; set; }
        public int AssmblyID { get; set; }
        public int ItemID { get; set; }
        public decimal UtilQty { get; set; }
        public decimal CumulativeUtilQty { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int? categoryCode { get; set; }
        public string MeasureUnit { get; set; }

        [Precision(18,3)]
        public decimal ReqQty { get; set; }


        // New PO fields
        public int RefPoSubId { get; set; }
        public string RefPoNo { get; set; }
        public decimal PoQty { get; set; }
        public decimal JobOrderQty { get; set; }
        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }
        public bool IsLabourMaterial { get; set; }
        public int? LineNo { get; set; }
        public int? StaffID { get; set; }
        public string? StaffName { get; set; }
        public string? DepartmentCode { get; set; }
        public string MaterialType => IsLabourMaterial ? "Customer Supplied" : "In-House";
    }
}
