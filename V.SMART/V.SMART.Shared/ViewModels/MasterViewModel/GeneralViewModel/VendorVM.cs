using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel
{
    public class VendorVM
    {
        public int VendorCode { get; set; }

        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(150, ErrorMessage = "Supplier name cannot exceed 150 characters")]
        public string VendorName { get; set; }

        [Required(ErrorMessage = "Supplier address is required")]
        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters")]
        public string VendorAddress { get; set; }

        [StringLength(10, ErrorMessage = "PAN number cannot exceed 10 characters")]
        public string? PANNo { get; set; }

        [StringLength(15, ErrorMessage = "Contact number cannot exceed 15 digits")]
        public string? ContactNo { get; set; }

        [StringLength(250, ErrorMessage = "Scope cannot exceed 250 characters")]
        public string? Scope { get; set; }

        [Required(ErrorMessage = "GST number is required")]
        [StringLength(15, ErrorMessage = "GST number must be 15 characters")]
        public string GSTNo { get; set; }

        [StringLength(15, ErrorMessage = "Customer code cannot exceed 15 characters")]
        public string? CustomerCode { get; set; }

        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Business type is required")]
        [StringLength(50, ErrorMessage = "Business type cannot exceed 50 characters")]
        public string BusiType { get; set; }

        public bool Inactive { get; set; }
        public bool SSI { get; set; }

        // ===================== FINANCE =====================

        [Range(0, double.MaxValue, ErrorMessage = "Opening balance cannot be negative")]
        public decimal? OpenBal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Pending opening balance cannot be negative")]
        public decimal? OpenBalPndg { get; set; }

        public DateTime? OpenBalDate { get; set; }

        public bool TdsDeduct { get; set; }

        [Range(0, 100, ErrorMessage = "TDS percentage must be between 0 and 100")]
        public decimal TDS { get; set; }

        public decimal TDSExmptdAmt { get; set; }
        public decimal TDSBillval { get; set; }
        public decimal TDSDeductper { get; set; }

        [Required(ErrorMessage ="Currencey Name selection is required")]
        public int? CurrId { get; set; }
        public string? CurrencyName { get; set; } 

        // ===================== BANK =====================

        [StringLength(100, ErrorMessage = "Bank name cannot exceed 100 characters")]
        public string? BankName { get; set; }

        [StringLength(20, ErrorMessage = "Account number cannot exceed 20 characters")]
        public string? BankAccountNo { get; set; }

        [StringLength(11, ErrorMessage = "IFSC code must be 11 characters")]
        public string? BankIFSCCode { get; set; }

        [StringLength(100, ErrorMessage = "Branch name cannot exceed 100 characters")]
        public string? BankBranchName { get; set; }

        // ===================== LOCATION =====================

        [Required(ErrorMessage = "State name is required")]
        [StringLength(100, ErrorMessage = "State name cannot exceed 100 characters")]
        public string StateName { get; set; }

        [Required(ErrorMessage = "State is required")]
        public int StateCode { get; set; }

        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Pin code is required")]
        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "Enter a valid 6-digit pin code")]
        public string PinCode { get; set; }

        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters")]
        public string? Country { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Distance cannot be negative")]
        public double Distance { get; set; }


        // ===================== COMPANY =====================

        [StringLength(50, ErrorMessage = "TAN number cannot exceed 50 characters")]
        public string? TANNo { get; set; }

        [StringLength(50, ErrorMessage = "CIN number cannot exceed 50 characters")]
        public string? CINNo { get; set; }

        [StringLength(50, ErrorMessage = "Company type cannot exceed 50 characters")]
        public string? CompnyType { get; set; }

        [StringLength(50, ErrorMessage = "Year of establishment cannot exceed 50 characters")]
        public string? YearOfEstd { get; set; }

        [StringLength(50, ErrorMessage = "Turnover cannot exceed 50 characters")]
        public string? PreviousYearTurnover { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Number of employees cannot be negative")]
        public int NoofEmployee { get; set; }


        // ===================== QUALITY / SUPPLY =====================

        public string? ProdMfg { get; set; }
        public string? MajorCust { get; set; }
        public string? MfgFacility { get; set; }
        public string? InspectionFacility { get; set; }
        public string? AnyQualSysFoll { get; set; }
        public string? AnyProdApprvl { get; set; }

        [StringLength(5)]
        public string? CouldMeetQuantityReq { get; set; }

        [StringLength(5)]
        public string? ManintngDocs { get; set; }

        [StringLength(5)]
        public string? FAIMaintained { get; set; }

        [StringLength(5)]
        public string? FODMaintained { get; set; }


        // ===================== PURCHASE =====================

        public bool IsitPurch { get; set; } = true;
        public bool IsitSubCon { get; set; } = true;

        public bool RejectionRetract { get; set; }
        public bool IsAllowAccRejRew { get; set; }


        // ===================== OTHER =====================

        public bool TaxValidate { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        public string? Remarks { get; set; }

        [StringLength(500, ErrorMessage = "Terms & Conditions cannot exceed 500 characters")]
        public string? TermsAndCondition { get; set; }

        public decimal? OpeningBalance { get; set; }
        public decimal? OpeningBalancePending { get; set; }
        public decimal? Advance { get; set; }

        // ===================== AUDIT =====================

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // ===================== CHILD COLLECTIONS =====================

        public List<VendorContactVM> VendorContacts { get; set; } = new();
        public List<VendorInDirectVM> VendorInDirects { get; set; } = new();
        public List<ItemSubVM> ItemSubs { get; set; } = new();
    }
}
