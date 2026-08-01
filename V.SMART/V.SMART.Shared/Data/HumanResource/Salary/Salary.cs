using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.HumanResource.Salary
{
    public class Salary
    {
        [Key]
        public int RowId { get; set; }

        [Required]
        public int? SalYear { get; set; }
        [Required]
        public int? SalMonth { get; set; }
        public DateTime SalDate { get; set; }= DateTime.Now;
        [Required]
        public int? StaffId { get; set; }
        [Required]
        public string Dept { get; set; }

        public decimal WorkingDays { get; set; }
        public decimal PayableDays { get; set; }
        public decimal LeaveDays { get; set; }
        public decimal PresentDays { get; set; }
        public decimal AbsentDays { get; set; }
        public decimal Holidays { get; set; }
        public decimal TotalEarnings { get; set; }

        public decimal Basic { get; set; }
        public decimal EarnedBasic { get; set; }

        public decimal DA { get; set; }
        public decimal EarnedDA { get; set; }

        public bool IsOTunderBasic { get; set; }
        public bool IsOTPerHour {  get; set; }
        public decimal OT_times_now { get; set; }
        public decimal OT { get; set; }
        public decimal EarnedOT { get; set; }

        public bool IsAttBonus { get; set; }
        public decimal EarnedAttBonus { get; set; }

        public bool IsHRAPerDay { get; set; }
        public bool IsHRAPerMonth { get; set; }
        public decimal HRA { get; set; }
        public decimal EarnedHRA { get; set; }

        public bool IsTAPerDay { get; set; }
        public bool IsTAPerMonth { get; set; }
        public decimal TA { get; set; }
        public decimal EarnedTA { get; set; }

        public bool IsSplAllowPerDay { get; set; }
        public bool IsSplAllowPerMonth { get; set; }
        public decimal SplAllow { get; set; }
        public decimal EarnedSplAllow { get; set; }

        public decimal Miscellaneous { get; set; }
        public decimal Others { get; set; }

        public bool IsLTAPerDay { get; set; }
        public bool IsLTAPerMonth { get; set; }
        public decimal LTA { get; set; }
        public decimal EarnedLTA { get; set; }

        public bool IsMRPerDay { get; set; }
        public bool IsMRPerMonth { get; set; }
        public decimal MR { get; set; }
        public decimal EarnedMR { get; set; }

        public bool IsEAPerDay { get; set; }
        public bool IsEAPerMonth { get; set; }
        public decimal EA { get; set; }
        public decimal EarnedEA { get; set; }

        //public decimal PresentDays { get; set; }
        //public decimal AbsentDays { get; set; }
        //public decimal Holidays { get; set; }
        //public decimal TotalEarnings { get; set; }
        //Deduction
        public decimal Society { get; set; }
        public decimal Insurance { get; set; }
        public decimal SalAdvance { get; set; }
        public decimal Fines { get; set; }
        public decimal DamageLoss { get; set; }
        public decimal OtherDed { get; set; }
        public bool IsPF { get; set; }
        public decimal EarnedPF { get; set; }
        public bool IsESI { get; set; }
        public decimal EarnedESI { get; set; }
        public bool IsPT { get; set; }
        public decimal PT { get; set; }
        public decimal TDS { get; set; }
        public bool chckOT { get; set; }

        public int? LoanId { get; set; }
        public decimal LoanAmtDed { get; set; }
        public decimal LoanBalB4 { get; set; }

        public decimal TotalDed { get; set; }
        public decimal NetPay { get; set; }

        public decimal? OldBal { get; set; }
        public decimal paidAmt { get; set; }

        public decimal AttBonus { get; set; }
        public int? LoanDedId { get; set; }

        public bool IsDAtoPF { get; set; }
       

        // ESI Details
        public string? ESIBankCode { get; set; }
        public string? ESIBankName { get; set; }
        public DateTime? ESICQDate { get; set; }
        public string? ESICQNo { get; set; }
        public DateTime? ESIDate { get; set; }
        public string? ESIPytMode { get; set; }

        // PF Details
        public string? PFBankName { get; set; }
        public DateTime? PFCQDate { get; set; }
        public string? PFCQNo { get; set; }
        public DateTime? PFDate { get; set; }
        public string? PFDepositor { get; set; }
        public string? PFPytMode { get; set; }

        // Audit Fields
        public string? Createdby { get; set; }
        public DateTime? Createddate { get; set; }
        public string? Modifiedby { get; set; }
        public DateTime? Modifieddate { get; set; }

      
        // Attendance Extra Fields
        public decimal PH { get; set; }
        public decimal W { get; set; }
        public decimal LOP { get; set; }
        public decimal P { get; set; }
        public decimal O { get; set; }

    }

}
