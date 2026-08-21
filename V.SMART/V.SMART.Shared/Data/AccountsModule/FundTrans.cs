
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
    public class FundTrans
    {
        public int FundTransId { get; set; }

        [StringLength(150)]
        public string? TransNo { get; set; }

        public DateTime TransDate { get; set; } = DateTime.Today;

        public int? PaymentRefId { get; set; }

        [ForeignKey(nameof(PaymentRefId))]
        public Payments? Payments { get; set; }
        public int? ReceiptRefId { get; set; }

        [ForeignKey(nameof(ReceiptRefId))]
        public  Receipts? Receipts { get; set; }
        public int? AdvanceRefId { get; set; }

        [ForeignKey(nameof(AdvanceRefId))]
        public Advaceadjustment? Advaceadjustment { get; set; }

        public int? ExpenseCode { get; set; }

        [ForeignKey(nameof(ExpenseCode))]
        public Expense? Expense { get; set; }

        public int? IncomeCode { get; set; }

        [ForeignKey(nameof(IncomeCode))]
        public Income? Income { get; set; }


        [StringLength(150)]
        public string? TransType { get; set; }

        /// <summary>
        [StringLength(150)]
        public string? TransactionType { get; set; }
        /// </summary>

        // PAYMENT / RECEIPT / CONTRA / JOURNAL / TRANSFER
        [StringLength(25)]
        public string? TrasactionMode { get; set; }
        // CASH / BANK
        public int? LedgerId { get; set; }
        // Vendor / Customer / Any Account

        [StringLength(150)]
        public string? LedgerName { get; set; }

        [StringLength(100)]
        public string? LedgerType { get; set; }
        // Make BankId nullable and add ForeignKey attribute
        public int? BankId { get; set; }

        [ForeignKey("BankId")]
        public Banks? Banks { get; set; }

        [StringLength(50)]
        public string? BankName { get; set; }     // NEW
        [StringLength(100)]
        public string? ExpenseName { get; set; }  // NEW
        [StringLength(100)]
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public decimal BillAmount { get; set; }      // Original
        public decimal PaidOrReceived { get; set; }  // Effect
        public decimal BalanceAfter { get; set; }    // Running Balance
        public decimal RunningBalance { get; set; }   // Updated balance after this movement
        public decimal AdvanceBalance { get; set; }

        [StringLength(250)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [StringLength(250)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

    }

}
