

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.AccountsViewModel
{
    public class PaymentsVM
    {
        public int PaymentId { get; set; }
        public string? PaymentNo { get; set; }
        public string Suffix { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Today;
        public DateTime PaymentDateNow { get; set; } = DateTime.Today;
        public int? ExpenseCode { get; set; }
        public string? Description { get; set; }
        public string? PaymentMode { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; } = DateTime.Today;
        public int? BankId { get; set; }
        public int? PayToRefCode { get; set; }
        public string PayToName { get; set; }
        public string? ExpenseName { get; set; }
        public int? PaymentTypeId { get; set; }
        public string? PaymentTypeName { get; set; }

        public string? TransactionType { get; set; }
        public decimal Amount { get; set; }
        // 🔹 Audit Fields
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public virtual List<PaymentSubVM?> PaymentSubVM { get; set; } = new List<PaymentSubVM>();
      

    }
}
