using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.ProdCompStatusVM
{
    public class ProductionIssueCompStatusVM
    {
        public long SlNo { get; set; }

        public string? IssueNo { get; set; }
        public string? IssueDate { get; set; }

        public string? IssueTo { get; set; }
        public string? Customer { get; set; }
        public string? StoreName { get; set; }

        public string? MainRemark { get; set; }

        public string? IssueStatus { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        public decimal Qty { get; set; }
        public decimal BalQty { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal ProcessCost { get; set; }


        public string? ItemRemarks { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public string? ModifiedDate { get; set; }
    }
}
