using V.SMART.Shared.Data.Master.General;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.General_Module
{

    public class VendorContact
    {
        [Key]
        public int Id { get; set; }  // Primary Key

        public int? VendorCode { get; set; }  // Foreign Key

        [ForeignKey(nameof(VendorCode))]
        public Vendor? Vendor { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(15)]
        public string? ContactNo { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
