using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.ToolCribReturnStatusViewModel
{
    public class ToolCribReturnStatusVM
    {
        public long SlNo { get; set; }
        public string? TCReturnNo { get; set; }
        public DateTime TCReturnDate { get; set; }
        public string? Remarks { get; set; }
        public string? TCReturnStatus { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }
        public decimal QtyOut { get; set; }
        public decimal RejectQty { get; set; }
        public string? RejectRemark { get; set; }
        public decimal ReworkQty { get; set; }
        public string? ReworkRemark { get; set; }

        public string? ItemRemarks { get; set; }
    }
}
