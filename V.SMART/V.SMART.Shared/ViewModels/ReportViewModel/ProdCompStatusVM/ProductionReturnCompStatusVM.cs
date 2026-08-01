using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.ProdCompStatusVM
{
    public class ProductionReturnCompStatusVM
    {
        public long SlNo { get; set; }

        public string? ReturnNo { get; set; }
        public string? ReturnDate { get; set; }

        public string? ReturnBy { get; set; }
        public string? Customer { get; set; }
        public string? StoreName { get; set; }

        public string? MainRemark { get; set; }

        public string? ReturnStatus { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        public decimal Qty { get; set; }
        public decimal BalQty { get; set; }
        public decimal UnitPrice { get; set; }

        public string? CategoryName { get; set; }

        public string? RefIssueNo { get; set; }
        public string? RefIssueDate { get; set; }

        public string? RefPoNo { get; set; }
        public string? RefPoDate { get; set; }

        public string? RCNo { get; set; }
        public string? RCDate { get; set; }

        public string? ItemRemarks { get; set; }


        public string? CreatedBy { get; set; }
        public string? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public string? ModifiedDate { get; set; }
    }
}
