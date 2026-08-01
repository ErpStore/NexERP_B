using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Master.Accounts
{
    public class Expense
    {
        [Key]
        public int ExpenseCode { get; set; }

        [Required(ErrorMessage = "Expense Name is required.")]
        [StringLength(250, ErrorMessage = "Expense Name cannot exceed 250 characters.")]
        public string ExpenseName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Expense Description cannot exceed 500 characters.")]
        public string? ExpenseDesc { get; set; }

        [Required(ErrorMessage = "Please indicate whether the expense is direct or indirect.")]
        public bool IsDirect { get; set; }

        public string ExpenseType => IsDirect ? "Direct" : "Indirect";

        public bool IsDefault { get; set; } = false;
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }

}
