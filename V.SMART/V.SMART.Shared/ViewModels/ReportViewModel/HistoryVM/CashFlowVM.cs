using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.HistoryVM
{
    public class CashFlowVM
    {
        public long SlNo { get; set; }

        public DateTime TranDate { get; set; }

        public string? VoucherNo { get; set; }

        public string? OutFlow { get; set; }

        public string? PayTo { get; set; }

        public string? InFlow { get; set; }

        public string? PayFrom { get; set; }

        public string? PaymentMode { get; set; }

        public string? ChequeNo { get; set; }

        public DateTime? ChequeDate { get; set; }

        public decimal Amount { get; set; }

        public string? CreatedBy { get; set; }

        public string? ModifiedBy { get; set; }
    }
}
