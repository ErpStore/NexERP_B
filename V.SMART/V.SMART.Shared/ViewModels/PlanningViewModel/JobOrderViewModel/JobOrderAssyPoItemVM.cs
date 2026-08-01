using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PlanningViewModel.JobOrderViewModel
{
    public class JobOrderAssyPoItemVM
    {
        public bool IsSelected { get; set; }
        public int? RefPoSubId { get; set; }
       
        public string? RefPoNo { get; set; }
        public DateTime? RefPoDate { get; set; }

        public int? AssyItemId { get; set; }
        public string? AssyItemCode { get; set; }
        public string? AssyItemName { get; set; }

        [Precision(18,3)]
        public decimal PoQty { get; set; }

        [Precision(18, 3)]
        public decimal WoBalQty { get; set; }

        [Precision(18, 3)]
        public decimal ReqQty { get; set; }

        [Precision(18, 3)]
        public decimal StockQty { get; set; }

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }
        public int? LineNo { get; set; }
        public int? StaffID { get; set; }
        public string? StaffName { get; set; }
        public string? DepartmentCode { get; set; }


    }
}
