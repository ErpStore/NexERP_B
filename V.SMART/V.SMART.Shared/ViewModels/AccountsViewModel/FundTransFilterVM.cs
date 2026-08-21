
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Accounts_Module;

namespace V.SMART.Shared.ViewModels.AccountsViewModel
{
    public class FundTransFilter
    {
        public int? FundTransId { get; set; }
        public string TransNo { get; set; }
        public DateTime TransDate { get; set; } 

        public int? PaymentRefId { get; set; }
        public int? ReceiptRefId { get; set; }
        public int? AdvanceRefId { get; set; }


        public string? BankName { get; set; }     // NEW
        public string? ExpenseName { get; set; }  // NEW

        // Navigation. Retyped by M2-B04 from the Razor component `Bank` (declared in the
        // Bank_Pages UI namespace) to the EF entity `Banks`
        // (V.SMART/V.SMART.Shared/Data/Master/Accounts_Module/Banks.cs). The property is
        // read-but-never-assigned outside this VM, so the retype is behaviour-neutral.
        public Banks? Bank { get; set; }
        public Expense? Expense { get; set; }
        public int? ExpenseCode { get; set; }

        public string? TransType { get; set; }
        // PAYMENT / RECEIPT / CONTRA / JOURNAL / TRANSFER
        public string? TrasactionMode { get; set; }
        // CASH / BANK
        public int? LedgerId { get; set; }         // Vendor / Customer / Any Account
        public string? LedgerName { get; set; }
        public int? BankId { get; set; }           // If Mode = BANK
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public decimal BillAmount { get; set; }      // Original
        public decimal PaidOrReceived { get; set; }  // Effect
        public decimal BalanceAfter { get; set; }    // Running Balance
        public decimal RunningBalance { get; set; }   // Updated balance after this movement
        public decimal AdvanceBalance { get; set; }

        
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; } 

       
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string? SearchTerm { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? TransactionType { get; set; }
        public string? TransactionMode { get; set; }
        public string? LedgerType { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }

        public decimal? totalCredits { get ; set; }
        public decimal? totalDebits { get ; set; }
        public decimal? netBalance { get ; set; }

        public bool SortDescending { get; set; } = true;
        public string? SortBy { get; set; } = "TransDate";
   
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;


    }
}
