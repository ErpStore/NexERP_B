using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PlanningViewModel.MaterialReqAnalysisVM
{
    public class MRAPoItems
    {
        public bool IsSelected { get; set; }
        public string? CategoryName { get; set; }

        public int? CustId { get; set; }
        public string? custName { get; set; }

        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }

        public int RefPoSubId { get; set; }
        public int RefPoId { get; set; }
        public string RefPoNo { get; set; }
        public DateTime RefPoDate { get; set; }

        public decimal? POQty { get; set; }
        public decimal? IndentBalQty { get; set; }
        public decimal? RequiredQty { get; set; }
        public decimal? StockQty { get; set; }


        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }
        public string? Type { get; set; }
        public bool IsLabourMaterial { get; set; }
        public string MaterialType => IsLabourMaterial ? "Customer Supplied" : "In-House";
    }
}
