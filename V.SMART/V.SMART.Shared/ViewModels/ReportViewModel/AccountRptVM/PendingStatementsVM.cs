using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel
{
    public class PendingStatementsVM
    {

        public int CustId { get; set; }

     

        public string PartyName { get; set; }


        public decimal PendingBalance { get; set; }

        public decimal Total { get; set; }
    }
}
