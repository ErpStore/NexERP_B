using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PlanningViewModel.MaterialReqAnalysisVM
{
    public class MRAPoSubItemsVM
    {
        public int RefPoId { get; set; }
        public int RefPoSubId { get; set; }
        
        public int CustId { get; set; }
        public string? CustName { get; set; }
        

        public string? RefPoNo { get; set; }
        public DateTime RefPoDate { get; set; }

        public int AssyCompItemId { get; set; }
        public string? AssyCompCode { get; set; }
        public string? AssyCompName { get; set; }

        [Precision(18,3)]
        public decimal? AssyCompUtilityQty { get; set; }

        [Precision(18, 3)]
        public decimal? AssyCompReqQty { get; set; }

        [Precision(18, 3)]
        public decimal? AssyCompStokQty { get; set; }

        [Precision(18, 3)]
        public decimal? AssyCompDiffQty { get; set; }

        public int? RMItemId { get; set; }
        public string? RMItemCode { get; set; }

        [Precision(18, 3)]
        public decimal? RMUtilityQty { get; set; }

        [Precision(18, 3)]
        public decimal? RMReqQty { get; set; }

        [Precision(18, 3)]
        public decimal? RMStockQty { get; set; }

        [Precision(18, 3)]
        public decimal? RMDiffQty { get; set; }
        public string? Type { get; set; }


        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }
        public bool IsLabourMaterial { get; set; }
        public string MaterialType => IsLabourMaterial ? "Customer Supplied" : "In-House";

    }

}
