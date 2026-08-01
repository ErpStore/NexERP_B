using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel
{
    public class PendingBillVM
  
    {
        // Customer Details
        public int? CustId { get; set; }
        public string? CustomerName { get; set; }

        // Invoice Details
        public int InvId { get; set; }
        public string? InvNo { get; set; }
        public string? InvDate { get; set; }

        // Bill Type
        public string? BillType { get; set; }

        // Amount Details
        public decimal? GrandTotal { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? Balance { get; set; }

        // Status
        public string? BillStatus { get; set; }


        public decimal? Credit { get; set; }

      
        public decimal? TDSAmount { get; set; }

        public int? OverDueDays { get; set; }


    }
}
