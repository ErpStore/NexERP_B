
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.AccountsModule
{
    public class AdvaceadjustmentVM
    {
        public int AdjumentId { get; set; }
        public string? AdvaceadjustmentNo { get; set; }
        public string? Suffix { get; set; }
        public DateTime AdjumentDate { get; set; } = DateTime.Today;
        public DateTime AdjumentDateNow { get; set; } = DateTime.Now;
        public int? ExpenseCode { get; set; }

        public string? ExpenseName { get; set; }

        public int? IncomeCode { get; set; }

        public string? IncomeName { get; set; }

        public string? Description { get; set; }
        public string? PaymentMode { get; set; }
        public int PaymentTypeId { get; set; }
        public string? PaymentTypeName { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; } = DateTime.Today;
        public int? BankId { get; set; }
        public int? PayToRefCode { get; set; }
        public string? PayToName { get; set; }
        public decimal Amount { get; set; }

        public bool IsIncome { get; set; }

        public string? TransactionType { get; set; }

        // 🔹 Audit Fields
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public virtual List<AdvaceadjustmentSubVM?> AdvaceadjustmentSubVM { get; set; } = new List<AdvaceadjustmentSubVM>();
        
    }
}
