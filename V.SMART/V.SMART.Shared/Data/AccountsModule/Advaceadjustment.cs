
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Accounts_Module;

namespace V.SMART.Shared.Data.AccountsModule
{
    public class Advaceadjustment
    {
        [Key]
        public int AdjumentId { get; set; }
        public string? AdvaceadjustmentNo { get; set; }
        public string? Suffix { get; set; }
        public DateTime AdjumentDate { get; set; } = DateTime.Today;
        public DateTime AdjumentDateNow { get; set; } = DateTime.Now;
        public int? ExpenseCode { get; set; }

        [ForeignKey(nameof(ExpenseCode))]
        public Expense? Expense { get; set; }

        public int? IncomeCode { get; set; }

        [ForeignKey(nameof(IncomeCode))]
        public Income? Income { get; set; }

        public string? Description { get; set; }
        public string? PaymentMode { get; set; }
        public int PaymentTypeId { get; set; }
        public string? PaymentTypeName { get; set; }

        public int? PayToRefCode { get; set; }
        public string? PayToName { get; set; }
        public bool IsIncome { get; set; } 

        [MaxLength(50)]
        public string? TransactionType { get; set; }
        public decimal Amount { get; set; }
        public int? BankId { get; set; }

        [ForeignKey(nameof(BankId))]
        public virtual Banks? Banks { get; set; }

        // 🔹 Audit Fields
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
        public virtual ICollection<AdvaceadjustmentSub> AdvaceadjustmentSubs { get; set; } = new List<AdvaceadjustmentSub>();
    }
}
