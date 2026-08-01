
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
    public class Payments
    {
        [Key]
        public int PaymentId { get; set; }
        public string? PaymentNo { get; set; }
        public string Suffix { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public DateTime PaymentDateNow { get; set; } = DateTime.Now;
        public int? ExpenseCode { get; set; } 

        [ForeignKey(nameof(ExpenseCode))]
        public Expense? Expense { get; set; }
        public string? Description { get; set; }
        public string? PaymentMode { get; set; }
        public int PaymentTypeId { get; set; }
        public string? PaymentTypeName { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; } = DateTime.Today;
        public int? BankId { get; set; }

        [ForeignKey(nameof(BankId))]
        public virtual Banks? Banks { get; set; }
        public int? PayToRefCode { get; set; }
        public string? PayToName { get; set; }

        [MaxLength(50)]
        public string? TransactionType { get; set; }
        public decimal Amount { get; set; }

        // 🔹 Audit Fields
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
        public virtual ICollection<PaymentsSub> PaymentsSubs { get; set; }=new List<PaymentsSub>();

    }

}
