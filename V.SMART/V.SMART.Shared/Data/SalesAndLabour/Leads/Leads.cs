using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.Leads
{
    public class Leads
    {
        [Key]
        public int LeadId { get; set; }

        [Required(ErrorMessage = "Please enter Company Name.")]
        [StringLength(150, ErrorMessage = "LeadName cannot exceed 150 characters.")]
        public string? LeadName { get; set; }

        [StringLength(150, ErrorMessage = "ContactPerson cannot exceed 150 characters.")]
        public string? ContactPerson { get; set; }

        [StringLength(150, ErrorMessage = "Designation cannot exceed 150 characters.")]
        public string? Designation { get; set; }

        [StringLength(13)]
        [RegularExpression(@"^(\+?\d{1,4}[- ]?)?\d{6,14}$", ErrorMessage = "Enter a valid mobile number.")]
        public string? PhoneNo { get; set; }

        [StringLength(150, ErrorMessage = "LeadOwner cannot exceed 150 characters.")]
        public string? LeadOwner { get; set; }
        [StringLength(150, ErrorMessage = "LeadSource cannot exceed 150 characters.")]
        public string? LeadSource { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@(gmail\.(com|co\.in)|[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})$",
         ErrorMessage = "Enter a valid Gmail or company email address")]
        public string? Email { get; set; }

        [StringLength(150, ErrorMessage = "LeadStatus cannot exceed 150 characters.")]
        public string? LeadStatus { get; set; }

        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        public string? Address { get; set; }
        public int StateId { get; set; }

        [StringLength(50, ErrorMessage = "State cannot exceed 50 characters.")]
        public string? StateName { get; set; }

        [ForeignKey("StateId")]
        public State? State { get; set; }

        [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters.")]
        public string? Country { get; set; }

        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
        public string? City { get; set; }

        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "PIN code must be exactly 6 digits and cannot start with 0")]

        [StringLength(6)]
        public string? PinCode { get; set; }

        [StringLength(50, ErrorMessage = "WebSite cannot exceed 50 characters.")]
        public string? WebSite { get; set; }

        [StringLength(25, ErrorMessage = "BusiType cannot exceed 25 characters.")]
        public string? BusiType { get; set; }

        [StringLength(25, ErrorMessage = "SupType cannot exceed 25 characters.")]
        public string? SupType { get; set; }
        public int? CurrId { get; set; }

        [ForeignKey("CurrId")]
        public Currency Currency { get; set; }

        [StringLength(15)]
        public string? GSTNo { get; set; }

        [StringLength(10, ErrorMessage = "PAN No must be 10 characters")]
        public string? PANNo { get; set; }

        public string? Remarks { get; set; }
        public bool? LeadTally { get; set; }

        [StringLength(40)]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        [StringLength(40)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }
}
