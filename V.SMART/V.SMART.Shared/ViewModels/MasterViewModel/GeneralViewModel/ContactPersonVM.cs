using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel
{
    public class ContactPersonVM
    {
        public int Id { get; set; }

        public int? CustId { get; set; }
        public string? CustomerName { get; set; }

        public int? LeadId { get; set; }
        public string? LeadName { get; set; }

        [StringLength(50, ErrorMessage = "Contact person name cannot exceed 50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Only letters, numbers, and spaces are allowed.")]
        public string? ContactPersonName { get; set; }

        [StringLength(10, ErrorMessage = "PhoneNo cannot exceed 10 characters.")]
        public string? PhoneNo { get; set; }

        [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "Designation cannot exceed 50 characters.")]
        public string? Designation { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
        public string? Category { get; set; }
    }
}
