using System;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel
{
    public class ToolCribVM
    {
        public long SlNo { get; set; }

        public string? IssueNo { get; set; }

        public DateTime TCIssueDate { get; set; }

        public string? IssuedToWhom { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }

        public decimal QtyOut { get; set; }

        public string? CreatedBy { get; set; }

        public bool IsReturn { get; set; }

        public string? MainRemarks { get; set; }

        public string? CategoryName { get; set; }

        public string? StoreName { get; set; }

        public string? UOM { get; set; }

        public string? HSNCode { get; set; }

        public decimal BalQty { get; set; }

        public decimal Rate { get; set; }
    }
}