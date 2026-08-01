using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel
{
    public class DaybookVM
    {
        public long SlNo { get; set; }
        public DateTime DocDate { get; set; }
        public string? Heading { get; set; } 
        public string? DocNo { get; set; } 
        public string? FromToWhom { get; set; } 
        public string? Item { get; set; } 
        public decimal? Qty { get; set; }
        public string? CreatedBy { get; set; } 
        public string? ModifiedBy { get; set; } 
    }
}
