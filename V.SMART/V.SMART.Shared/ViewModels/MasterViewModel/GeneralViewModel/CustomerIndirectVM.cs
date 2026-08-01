using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel
{
    public class CustomerIndirectVM
    {
        public int AltCustId { get; set; }

        public int CustId { get; set; }
        public string? CustomerName { get; set; }

        [StringLength(150, ErrorMessage = "Alternate Customer cannot exceed 150 characters.")]
        public string? AltCustName { get; set; }

        [StringLength(250, ErrorMessage = "Alternate Customer Address cannot exceed 250 characters.")]
        public string? AltCustAddr1 { get; set; }

        [StringLength(250, ErrorMessage = "Alternate Customer Address cannot exceed 250 characters.")]
        public string? AltCustAddr2 { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
        public string? State { get; set; }

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
        public string? Country { get; set; }

        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [EmailAddress(ErrorMessage = "Enter valid Email Address")]
        public string? Email { get; set; }

        [StringLength(15, ErrorMessage = "GST No. must be 15 characters")]
        [RegularExpression(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$", ErrorMessage = "Invalid GST No format")]
        public string? GSTNo { get; set; }

        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "PIN code must be exactly 6 digits and cannot start with 0")]
        [StringLength(6, ErrorMessage = "PIN code cannot exceed 6 characters.")]
        public string? Pincode { get; set; }

        [StringLength(100, ErrorMessage = "Contact Person cannot exceed 100 characters.")]
        public string? ContactPerson { get; set; }

        [StringLength(10, ErrorMessage = "Contact No. cannot exceed 10 characters.")]
        public string? ContactNo { get; set; }

        [StringLength(10, ErrorMessage = "PAN No must be 10 characters")]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN No format")]
        public string? PANNo { get; set; }
    }
}
