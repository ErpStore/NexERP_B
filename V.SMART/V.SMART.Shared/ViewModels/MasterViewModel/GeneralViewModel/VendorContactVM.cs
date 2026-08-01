using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel
{
    public class VendorContactVM
    {
        public int Id { get; set; }

        public int? VendorCode { get; set; }
        public string? VendorName { get; set; }


        [StringLength(100, ErrorMessage = "Contact Person Name cannot exceed 100 characters.")]
        public string? ContactPerson { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Contact Number must contain digits only.")]
        [StringLength(15, ErrorMessage = "Contact Number cannot exceed 15 digits.")]
        public string? ContactNo { get; set; }

        [StringLength(100, ErrorMessage = "Email address cannot exceed 100 characters.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string? Email { get; set; }
    }
}
