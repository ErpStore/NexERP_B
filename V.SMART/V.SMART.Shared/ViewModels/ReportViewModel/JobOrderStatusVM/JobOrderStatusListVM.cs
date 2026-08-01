using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.JobOrderStatusVM
{
    public class JobOrderStatusListVM
    {
        public long SlNo { get; set; }
        
        public string? JobNo { get; set; }

        public string? JobDate { get; set; }

        public string? RefPoNo { get; set; }

        public string? RefPoDate { get; set; }

        public string? Customer { get; set; }

        public int? LineNo { get; set; }
        public string? Assembly { get; set; }

        public string? SubAssembly { get; set; }

        public string? JobType { get; set; }

       // public string? ParentJobNo { get; set; }

        public decimal? POQty { get; set; }

        public decimal? JobOrderQty { get; set; }

        public decimal? JobOrderBalQty { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        public decimal? UtilQty { get; set; }

        public decimal? RequiredQty { get; set; }

        public decimal? BalQyy { get; set; }

        public string? Status { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public string? ModifiedDate { get; set; }

    }
}
