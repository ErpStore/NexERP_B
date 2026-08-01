using V.SMART.Shared.Data.SalesAndLabour.Leads;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.General
{
    public class ContactPerson
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Customer")]
        public int? CustId { get; set; }

        public Customer? Customer { get; set; }

        [StringLength(50, ErrorMessage = "Contact person name cannot exceed 50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Only letters, numbers, and spaces are allowed.")]

        public string? ContactPersonName { get; set; }

        [StringLength(10, ErrorMessage = "PhoneNo cannot exceed 10 characters.")]
        public string? PhoneNo { get; set; }

        [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters.")]

        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "Designation cannot exceed 50 characters.")]
        public string? Designation { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
        public string? Category { get; set; }

        [ForeignKey(nameof(LeadId))]
        public int? LeadId { get; set; }
        public Leads? Lead { get; set; }
    }
}
