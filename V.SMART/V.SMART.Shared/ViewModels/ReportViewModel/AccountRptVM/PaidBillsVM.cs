using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel
{
    public class PaidBillsVM
    {
        // Customer Details
        public int? CustId { get; set; }
        public string? CustomerName { get; set; }

        // Invoice Details
        public int InvId { get; set; }
        public string? DocumentNo { get; set; }

        public string? DocumentDate { get; set; }

        public string? DocumentInvNo { get; set; }

        public string? DocumentInvDate { get; set; }

        public decimal? GrandTotal { get; set; }

        public decimal? AdjustAmount { get; set; }

        public string? PaymentTypeName { get; set; }

        public string? BankName { get; set; }

        public string? ChequeNo { get; set; }

        public string? ChequeDate { get; set; }

        public string? Description { get; set; }

       


    }
}
