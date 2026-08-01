using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PlanningViewModel.JobOrderViewModel
{
    public class JobOrderPreviewVM
    {
        public string TempJobNo { get; set; }

        public int SubAssyId { get; set; }
        public string SubAssyCode { get; set; }
        public string MeasureUnit { get; set; }

        public decimal UtilQty { get; set; }
        public decimal ReqQty { get; set; }
        public decimal StockQty { get; set; }

        public bool IsSelected { get; set; }

        // ✅ PO Link
        public int RefPoSubId { get; set; }
        public string RefPoNo { get; set; }
        public decimal PoQty { get; set; }

        public decimal JobOrderQty { get; set; }

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }
        public int? LineNo { get; set; }
        public int? StaffID { get; set; }
        public string? StaffName { get; set; }
        public string? DepartmentCode { get; set; }

    }

}
