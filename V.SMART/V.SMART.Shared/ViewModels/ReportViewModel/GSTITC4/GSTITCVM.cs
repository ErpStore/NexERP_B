using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.NewFolder
{
    public class GSTITCVM
    {

            public DateTime FromDate { get; set; } = DateTime.Today.AddMonths(-1);

            public DateTime ToDate { get; set; } = DateTime.Today;

            public int ReportType { get; set; } = -1;
        
    }
}
