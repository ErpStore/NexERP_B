
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Accounts_Module;

namespace V.SMART.Shared.Data.AccountsModule
{
    public class Receipts
    {
        [Key]
        public int ReceiptId { get; set; }
        public string? ReceiptNo { get; set; }
        public string? Suffix { get; set; }
        public DateTime ReceiptDate { get; set; } = DateTime.Now;
        public DateTime ReceiptDateNow { get; set; } = DateTime.Now;
        public int? IncomeCode { get; set; }

        [ForeignKey(nameof(IncomeCode))]
        public Income? Income { get; set; }
        public string? Description { get; set; }
        public string? PaymentMode { get; set; }
        public int PaymentTypeId { get; set; }
        public string? PaymentTypeName { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; } = DateTime.Today;
        public int? BankId { get; set; }

        [ForeignKey(nameof(BankId))]
        public virtual Banks? Banks { get; set; }
        public int? PayFromRefCode { get; set; }
        public string? PayFromName { get; set; }

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
        public virtual ICollection<ReceiptsSub> ReceiptsSubs { get; set; } = new List<ReceiptsSub>();
    }
}
