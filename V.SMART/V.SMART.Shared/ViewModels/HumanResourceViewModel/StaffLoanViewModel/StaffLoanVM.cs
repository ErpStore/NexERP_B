using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.HumanResourceViewModel.StaffLoanViewModel
{
    public class StaffLoanVM
    {
        [Required(ErrorMessage = "Select Staff Name")]
        public int StaffId { get; set; }

        public bool IsFinished { get; set; }

        public string? LoanNo { get; set; }

        public DateTime DateOfIssue { get; set; } = DateTime.Today;

        public string? Description { get; set; }

        // ===== ROW PROPERTIES =====

        public int? LoanId { get; set; }

        public string? LoanType { get; set; }

        public bool IsEditable { get; set; } = true;

        // ===== GRID COLLECTION =====
        public int SlNo { get; set; }
        public decimal Amount { get; set; }

        public int NumberOfMonthsRecoveries { get; set; }
        public decimal EMIAmount { get; set; }
        public decimal BalanceAmount { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // ===== NEW: Loan Recovery Details for display =====
        public int LoanDedId { get; set; }      // Deduction record ID
        //public int LoanId { get; set; }         // FK to Loan
        public DateTime SalDate { get; set; }   // Salary date
        public decimal LoanAmtDed { get; set; } // Deducted amount

        // Row type: "LoanItem" or "Recovery"
        public string RowType { get; set; } = "LoanItem";

        // Collections for mapping
        public List<StaffLoanVM> LoanRows { get; set; } = new();
        public List<StaffLoanVM> LoanRecoveries { get; set; } = new();
        
    }
}
