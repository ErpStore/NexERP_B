using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.General;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.Company_Module
{
    public class Companydetails
    {
        [Key]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Company Name is required.")]
        [MaxLength(150, ErrorMessage = "Company Name cannot exceed 150 characters.")]
        public string CompanyName { get; set; }

        [MaxLength(20, ErrorMessage = "Company Code cannot exceed 20 characters.")]
        public string? CompanyCode { get; set; }

        [MaxLength(100, ErrorMessage = "Business Type cannot exceed 100 characters.")]
        public string? BusinessType { get; set; }

        [MaxLength(100, ErrorMessage = "Industry Type cannot exceed 100 characters.")]
        public string? IndustryType { get; set; }

        [Required(ErrorMessage = "Registered Address is required.")]
        [MaxLength(300, ErrorMessage = "Registered Address cannot exceed 300 characters.")]
        public string RegisteredAddress { get; set; }

        [Required(ErrorMessage = "Billing Address is required.")]
        [MaxLength(300, ErrorMessage = "Billing Address cannot exceed 300 characters.")]
        public string BillingAddress { get; set; }

        [MaxLength(300, ErrorMessage = "Shipping Address cannot exceed 300 characters.")]
        public string? ShippingAddress { get; set; }

        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string? City { get; set; }

        [Required(ErrorMessage = "State is required.")]
        [MaxLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
        public string StateName { get; set; }

        [ForeignKey(nameof(StateCode))]
        public State State { get; set; }

        public int StateCode { get; set; }

        [MaxLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
        public string? Country { get; set; }

        [Required(ErrorMessage = "Zip Code / Pin Code is required.")]
        [StringLength(10, ErrorMessage = "Zip Code cannot exceed 10 characters.")]
        [RegularExpression(@"^\d{4,10}$", ErrorMessage = "Zip Code must be numeric and between 4 to 10 digits.")]
        public string ZipCode { get; set; }


        [MaxLength(20, ErrorMessage = "Phone Number cannot exceed 20 characters.")]
        [RegularExpression(@"^\+?[0-9\- ]{7,20}$", ErrorMessage = "Phone Number must contain only digits, spaces, dashes or an optional '+' sign.")]
        public string? PhoneNumber { get; set; }

        [MaxLength(15, ErrorMessage = "Mobile Number cannot exceed 15 digits.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Mobile Number must be a valid 10-digit Indian number starting with 6-9.")]
        public string? MobileNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string? EmailId { get; set; }

        [MaxLength(100, ErrorMessage = "Website cannot exceed 100 characters.")]
        public string? Website { get; set; }

        [Required(ErrorMessage = "GSTIN is required.")]
        [MaxLength(15, ErrorMessage = "GSTIN must be 15 characters.")]
        [RegularExpression(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}[Z]{1}[0-9A-Z]{1}$", ErrorMessage = "Invalid GSTIN format.")]
        public string GSTIN { get; set; }

        [Required(ErrorMessage = "PAN is required.")]
        [MaxLength(10, ErrorMessage = "PAN must be 10 characters.")]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN format.")]
        public string PAN { get; set; }

        [MaxLength(30, ErrorMessage = "IEC Number cannot exceed 30 characters.")]
        public string? IECNumber { get; set; }

        public bool IsEOURegistered { get; set; }

        [MaxLength(50, ErrorMessage = "EOU Type cannot exceed 50 characters.")]
        public string? EOUType { get; set; }

        [MaxLength(50, ErrorMessage = "LUT Number cannot exceed 50 characters.")]
        public string? LUTNumber { get; set; }

        [MaxLength(30, ErrorMessage = "Bank AD Code cannot exceed 30 characters.")]
        public string? BankAuthorizedDealerCode { get; set; }

        [MaxLength(50, ErrorMessage = "EOU Registration No. cannot exceed 50 characters.")]
        public string? EOURegistrationNo { get; set; }

        public DateTime? EOURegistrationDate { get; set; }

        [MaxLength(20, ErrorMessage = "EOU Status cannot exceed 20 characters.")]
        public string? EOUStatus { get; set; }

        public DateTime? EOUValidityTill { get; set; }

        [Required(ErrorMessage = "Select Financial Year Type.")]
        [MaxLength(20, ErrorMessage = "Financial Year Type cannot exceed 20 characters.")]
        public string FinancialYearType { get; set; } // e.g., Apr-Mar, Jan-Dec

        [Range(0, double.MaxValue, ErrorMessage = "Opening Balance must be 0 or a positive value.")]
        [Precision(18, 2)]
        public decimal OpeningBalance { get; set; }

        [Required(ErrorMessage = "Opening Balance Date is required.")]
        public DateTime? OpeningBalanceDate { get; set; } = DateTime.Now;

        public string? LogoUrl { get; set; }
        public string? LogoPath { get; set; }
        public string? IsoLogoUrl { get; set; }
        public string? IsoLogoPath { get; set; }

        // Foreign key property
        [Required(ErrorMessage = "Please select Currency.")]
        public int CurrencyCode { get; set; }

        // Navigation property
        [ForeignKey(nameof(CurrencyCode))]
        public Currency? Currency { get; set; }

        [MaxLength(100, ErrorMessage = "TimeZone cannot exceed 100 characters.")]
        public string? TimeZone { get; set; } // e.g., Asia/Kolkata

        [MaxLength(21, ErrorMessage = "CIN cannot exceed 21 characters.")]
        public string? CIN { get; set; }

        [MaxLength(15, ErrorMessage = "TIN cannot exceed 15 characters.")]
        public string? TIN { get; set; }

        // Bank Details

        [MaxLength(100, ErrorMessage = "Bank Name cannot exceed 100 characters.")]
        public string? BankName { get; set; }

        [MaxLength(100, ErrorMessage = "Bank Branch cannot exceed 100 characters.")]
        public string? BankBranch { get; set; }

        [MaxLength(20, ErrorMessage = "Account Number cannot exceed 20 characters.")]
        public string? BankAccountNumber { get; set; }

        [MaxLength(20, ErrorMessage = "Account Type cannot exceed 20 characters.")]
        public string? AccountType { get; set; }

        [MaxLength(11, ErrorMessage = "IFSC Code must be 11 characters.")]
        [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Invalid IFSC Code format.")]
        public string? IFSCCode { get; set; }

        [StringLength(300)]
        public string? BankAddress { get; set; }

        public string? BankCity { get; set; }
        public string? BankState { get; set; }

        [MaxLength(9, ErrorMessage = "MICR Code cannot exceed 9 characters.")]
        public string? MICRCode { get; set; }

        [MaxLength(100, ErrorMessage = "SWIFT Code cannot exceed 100 characters.")]
        public string? SWIFTCode { get; set; }

        //. Legal / Government Registrations 
        [MaxLength(30, ErrorMessage = "MSME/Udyam Number cannot exceed 30 characters.")]
        public string? UdyamNumber { get; set; }

        [MaxLength(25, ErrorMessage = "PF Registration Number cannot exceed 25 characters.")]
        public string? PFRegistrationNumber { get; set; }

        [MaxLength(25, ErrorMessage = "ESI Registration Number cannot exceed 25 characters.")]
        public string? ESIRegistrationNumber { get; set; }

        [MaxLength(30, ErrorMessage = "Shops & Establishment Reg. Number cannot exceed 30 characters.")]
        public string? ShopEstablishmentNumber { get; set; }

        [MaxLength(30, ErrorMessage = "Excise Registration Number cannot exceed 30 characters.")]
        public string? ExciseRegistrationNumber { get; set; }

        [MaxLength(30, ErrorMessage = "SEZ Registration Number cannot exceed 30 characters.")]
        public string? SEZRegistrationNumber { get; set; }

        [MaxLength(30, ErrorMessage = "Customs Registration Number cannot exceed 30 characters.")]
        public string? CustomsRegistrationNumber { get; set; }

        [MaxLength(100, ErrorMessage = "Alternate Email cannot exceed 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid Alternate Email format.")]
        public string? AlternateEmail { get; set; }

        [MaxLength(100, ErrorMessage = "Contact Person Name cannot exceed 100 characters.")]
        public string? ContactPersonName { get; set; }

        public string? CustomFinancialYearType { get; set; }
        public string? CustomBusinessType { get; set; }
        public string? CustomIndustryType { get; set; }

        public string? APIEinvoiceLicenseKey { get; set; }

        public bool IsActive { get; set; } = true;

        public int DecimalPlaces { get; set; } = 2;


        //***********Choose DC  Outgoing Type(for Auto Running No.)**********\\
        public byte BookTypeDc { get; set; }

        //Option 1, Sales, Labour, SubContract - ALL SEPARATE BOOKS

        //Option 2, Sales ,Labour - ONE COMBINED BOOK, SubContract - ONE BOOK

        //Option 3, Sales, Labour, SubContract - ALL IN ONE BOOK

        //***********************************************************************


        //***********Choose Inv. Type (for Auto Running No.)**********\\
        public byte BookTypeInvoice { get; set; }

        //Option 1, Sales && Labour - ONE COMBINED BOOK

        //Option 2, Sales && Labour - SEPARATE BOOKS

        //Option 3, Sales,Export,Labour-ONE COMBINE BOOK
        //***********************************************************************

        [StringLength(80)]
        public string? EwayUser { get; set; }

        [StringLength(80)]
        public string? EwayPassword { get; set; }

        [StringLength(30)]
        public string? EwayCommodityCode { get; set; }

        [StringLength(200)]
        public string? EwayURL { get; set; }

        [StringLength(250)]
        public string? GSTAddress { get; set; }

        [StringLength(150)]
        public string? LegalName { get; set; }


        [MaxLength(100, ErrorMessage = "CreatedBy cannot exceed 100 characters.")]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [MaxLength(100, ErrorMessage = "ModifiedBy cannot exceed 100 characters.")]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
