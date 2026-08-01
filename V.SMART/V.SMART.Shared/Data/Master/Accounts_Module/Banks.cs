using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Master.Accounts_Module
{
    public class Banks
    {
        [Key]
        public int BankId { get; set; }

        [Required(ErrorMessage = "Bank Name is required.")]
        [StringLength(250)]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Account Number is required.")]
        [StringLength(50, MinimumLength = 9, ErrorMessage = "Account Number must be between 9 and 18 digits.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Account Number must be numeric.")]
        public string AccountNo { get; set; }

        [Required(ErrorMessage = "Please Enter IFSC Code..")]
        [StringLength(20)]
        [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Enter Valid IFSC Code..")]
        public string IFSCCode { get; set; }

        [Required(ErrorMessage = "Please Enter IFSC Code..")]
        [StringLength(100, ErrorMessage = "Branch Name Cannot Be Exceed 100 Characters")]
        public string BranchName { get; set; }

        [Required(ErrorMessage = "Please Bank Address")]
        [StringLength(250, ErrorMessage = "Bank Address Cannot be Exceed 250 Characters")]
        public string BankAddress { get; set; }

        [StringLength(100, ErrorMessage = "Bank State cannot be Exceed 100 Characters")]
        public string? BankState { get; set; }

        [Precision(18, 2)]
        public decimal BankOPBal { get; set; }

        [Precision(18, 2)]
        public decimal CurrentBalance { get; set; }




        [Required(ErrorMessage = "Please specify Debit or Credit")]
        public bool IsDebit { get; set; }

        public string BankType => IsDebit ? "Debit" : "Credit";

        [Precision(18, 2)]
        public decimal CreditLimit { get; set; }


        [StringLength(250)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [StringLength(250)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
        // ➕ New fields
        public string UpiId { get; set; } = "";
        public string UpiName { get; set; } = "";

        public string? LogoUrl { get; set; }

        // Local UPI validation (regex)
        public bool IsValidUpiId()
        {
            if (string.IsNullOrWhiteSpace(UpiId))
                return true;

            string pattern = @"^[a-zA-Z0-9.\-_]{2,256}@[a-zA-Z]{2,64}$";
            return System.Text.RegularExpressions.Regex.IsMatch(UpiId, pattern);
        }
    }
}
