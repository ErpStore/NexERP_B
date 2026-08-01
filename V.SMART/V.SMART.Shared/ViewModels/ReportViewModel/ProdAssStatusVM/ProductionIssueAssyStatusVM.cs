using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.ProdAssStatusVM
{
    public class ProductionIssueAssyStatusVM
    {
        public long SlNo { get; set; }

        public string? IssueNo { get; set; }
        public string? IssueDate { get; set; }

        public string? IssueToWhom { get; set; }
        public string? StoreName { get; set; }

        public string? DepartmentCode { get; set; }
        public string? MonthCode { get; set; }

        public string? MainRemark { get; set; }

        public string? IssueStatus { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        public decimal IssueQty { get; set; }
        public decimal BalQty { get; set; }
        public decimal UnitPrice { get; set; }

        public string? BatchNo { get; set; }
        public string? ItemRemark { get; set; }

        public string? RefJobNo { get; set; }

        public string? RefJobDate { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public string? ModifiedDate { get; set; }
    }
}
