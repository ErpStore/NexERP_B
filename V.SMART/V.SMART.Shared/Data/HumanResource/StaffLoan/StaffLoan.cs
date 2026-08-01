using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.HumanResource.StaffLoan
{
    public class StaffLoan
    {
        [Key]
        public int LoanId { get; set; }
        [Required, StringLength(50)]
        public string LoanNo { get; set; }

        [Required, StringLength(50)]
        public int StaffId { get; set; }

        public DateTime DateOfIssue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public int NumberOfMonthsRecoveries { get; set; }

        [MaxLength(250)]
        public string? LoanType { get; set; }

        public bool IsFinished { get; set; }
        public string Status { get; set; } = "Active";

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAmount { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
