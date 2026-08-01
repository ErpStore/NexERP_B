using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.ToolCribIssueStatusViewModel
{
    public class ToolCribIssueStatusVM
    {
        public long SlNo { get; set; }
        public string? TCIssueNo { get; set; }
        public DateTime TCIssueDate { get; set; }
        public string? Remarks { get; set; }
        public string? TCIssueStatus { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }
        public decimal QtyOut { get; set; }
        public decimal BalQty { get; set; }
        public decimal UnitPrice { get; set; }

        public string? ItemRemarks { get; set; }
    }
}
