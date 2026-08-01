using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.ProdAssStatusVM
{
    public class ProductionReturnAssyStatusVM
    {
        public long SlNo { get; set; }

        public string? ReturnNo { get; set; }
        public string? ReturnDate { get; set; }

        public string? ReturnBy { get; set; }
        public string? StoreName { get; set; }

        public string? MainRemark { get; set; }

        public string? ReturnStatus { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        public decimal QtyReturned { get; set; }
        public decimal BalQty { get; set; }
        public decimal UnitPrice { get; set; }

        public string? RefIssueNo { get; set; }
        public string? RefIssueDate { get; set; }

        public string? RefJobNo { get; set; }
        public string? RefJobDate { get; set; }

        public string? ItemRemarks { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public string? ModifiedDate { get; set; }
    }
}
