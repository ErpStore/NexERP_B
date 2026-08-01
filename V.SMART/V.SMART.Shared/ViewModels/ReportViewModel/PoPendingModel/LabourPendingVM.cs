using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.AccountsVM
{
    public class LabourPendingVM
    {
        //Customer
        public long slno { get; set; } 
        public int CustId { get; set; }
        public string? CustName { get; set; }

        //Doc Details
        public string? DocNo { get; set; }
        public string? DocDate { get; set; }
        public string? RefDcNo { get; set; }

        public string? RefDcDate { get; set; }

        public string? BatchNo { get; set; }
        public int? NoofItems { get; set; }

        public string? createdBy { get; set; }

        public string? CreatedDate { get; set; }
        
    }
}
