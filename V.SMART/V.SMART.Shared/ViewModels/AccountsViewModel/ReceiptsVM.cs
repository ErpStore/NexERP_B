
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.AccountsViewModel
{
    public   class ReceiptsVM
    {
        public int ReceiptId { get; set; }
        public string? ReceiptNo { get; set; }
        public string? Suffix { get; set; }
        public DateTime ReceiptDate { get; set; } = DateTime.Today;
        public DateTime ReceiptDateNow { get; set; } = DateTime.Now;
        public int? IncomeCode { get; set; }

        public string? Description { get; set; }
        public string? PaymentMode { get; set; }
        public int PaymentTypeId { get; set; }
        public string? PaymentTypeName { get; set; }

        public string? InComeName { get; set; }
   
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; } = DateTime.Today;
        public int? BankId { get; set; }

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

        public virtual List<ReceiptsSubVM?> ReceiptsSubVMs { get; set; } = new List<ReceiptsSubVM>();
       
    }
}
