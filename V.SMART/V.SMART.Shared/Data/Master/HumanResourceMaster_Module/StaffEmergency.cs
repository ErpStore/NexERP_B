using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.HumanResourceMaster_Module
{
    public class StaffEmergency
    {
        [Key]
        public int SlNo { get; set; }

        [Required]
        [ForeignKey(nameof(Staff))]
        public int StaffID { get; set; }

        public Staff? Staff { get; set; }

        [StringLength(150)]
        public string? Name { get; set; }

        [StringLength(50)]
        public string? Relation { get; set; }

        [StringLength(50)]
        public string? Landline { get; set; }

        [StringLength(50)]
        public string? MobileNo { get; set; }

        // 👇 Example NEW FIELD (you said added one more)
        [StringLength(250)]
        public string? Address { get; set; }
    }
}
