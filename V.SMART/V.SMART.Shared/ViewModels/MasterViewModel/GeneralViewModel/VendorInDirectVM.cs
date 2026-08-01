using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel
{
    public class VendorInDirectVM
    {
        public int AltVendorCode { get; set; }

        public int? VendorCode { get; set; }

        public string? VendorName { get; set; }

        // ===================== BASIC =====================

        [Required(ErrorMessage = "Alternate vendor name is required")]
        [StringLength(100, ErrorMessage = "Alternate vendor name cannot exceed 100 characters")]
        public string AltVendorName { get; set; }

        [StringLength(250, ErrorMessage = "Address line 1 cannot exceed 250 characters")]
        public string? AltVendorAddr1 { get; set; }

        [StringLength(250, ErrorMessage = "Address line 2 cannot exceed 250 characters")]
        public string? AltVendorAddr2 { get; set; }

        [StringLength(50, ErrorMessage = "City name cannot exceed 50 characters")]
        public string? City { get; set; }

        [StringLength(80, ErrorMessage = "State name cannot exceed 80 characters")]
        public string? StateName { get; set; }

        [RegularExpression(@"^[1-9][0-9]{5}$",
            ErrorMessage = "Enter a valid 6-digit PIN code")]
        public string? PinCode { get; set; }

        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters")]
        public string? Country { get; set; }

        // ===================== TAX =====================

        [StringLength(15, ErrorMessage = "GST number cannot exceed 15 characters")]
        public string? GSTNo { get; set; }

        [StringLength(10, ErrorMessage = "PAN number cannot exceed 10 characters")]
        public string? PANNo { get; set; }

        // ===================== CONTACT =====================

        [StringLength(50, ErrorMessage = "Contact person name cannot exceed 50 characters")]
        public string? ContactPerson { get; set; }

        [RegularExpression(@"^\d{10}$",
            ErrorMessage = "Contact number must be exactly 10 digits")]
        public string? ContactNo { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email address cannot exceed 100 characters")]
        public string? Email { get; set; }
    }
}
