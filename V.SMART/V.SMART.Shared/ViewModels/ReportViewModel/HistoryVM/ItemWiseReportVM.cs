using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel
{
    public class ItemWiseReportVM
    {
            public long SlNo { get; set; }
            
            public DateTime Date { get; set; }

            public string? Heading { get; set; }

            public string? DocNo { get; set; }

            public string? FromToWhom { get; set; }

            public string? ItemCode { get; set; }

            public decimal? QtyOut { get; set; }

            public decimal? QtyIn { get; set; }

            public decimal? RejQty { get; set; }

            public decimal? ReworkQty { get; set; }

            public decimal? Rate { get; set; }
        
    }
}
