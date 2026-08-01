using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PlanningViewModel.AssyReqAnalysisVM
{
    public class RequirementRow
    {
        public string OrderNo { get; set; }
        public int Level { get; set; }
        public string ItemCode { get; set; }
        public string UOM { get; set; }
        public string ItemName { get; set; }

        public decimal Qty { get; set; }

        public decimal UtilQty { get; set; }

        public decimal ReqQty { get; set; }

        public decimal StkQty { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }

        public string Category { get; set; }

    }
}
