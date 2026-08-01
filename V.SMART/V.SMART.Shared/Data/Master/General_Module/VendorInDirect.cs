using V.SMART.Shared.Data.Master.General;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.General_Module
{
    public class VendorInDirect
    {
        [Key]
        public int AltVendorCode { get; set; }

        public int? VendorCode { get; set; }

        [ForeignKey(nameof(VendorCode))]
        public Vendor? Vendor { get; set; }
        [Required(ErrorMessage = "Alt Vender Name required")]
        [StringLength(100, ErrorMessage = "Alternate vendor name should not be exceed 100 characters.")]
        public string AltVendorName { get; set; }
        [StringLength(250, ErrorMessage = "Alternate vendor address1 should not be exceed 250 characters.")]
        public string? AltVendorAddr1 { get; set; }
        [StringLength(250, ErrorMessage = "Alternate vendor address2 should not be exceed 250 characters.")]
        public string? AltVendorAddr2 { get; set; }

        [StringLength(50, ErrorMessage = "City should not be exceed 50 characters.")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "Country should not be exceed 100 characters.")]
        public string? Country { get; set; }

        [StringLength(100, ErrorMessage = "Email should not be exceed 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        [StringLength(15, ErrorMessage = "GST No. should not be exceed 15 characters.")]
        public string? GSTNo { get; set; }

        [StringLength(80)]
        public string? StateName { get; set; }

        [StringLength(6)]
        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "PIN code must be exactly 6 digits and cannot start with 0")]
        public string? PinCode { get; set; }

        [StringLength(10, ErrorMessage = "PAN No should not be exceed 10 characters.")]
        public string? PANNo { get; set; }

        [StringLength(50, ErrorMessage = "Contact Person should not be exceed 50 characters.")]
        public string? ContactPerson { get; set; }

        [StringLength(10, ErrorMessage = "Contact no. should not be exceed 10 characters. ")]
        public string? ContactNo { get; set; }

    }

}
