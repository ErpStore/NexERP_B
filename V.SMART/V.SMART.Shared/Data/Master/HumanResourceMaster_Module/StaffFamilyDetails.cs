using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.HumanResourceMaster_Module
{
    public class StaffFamilyDetails
    {
        [Key]
        public int Slno { get; set; }

        [Required]
        [ForeignKey("Staff")]
        public int StaffID { get; set; }

        public Staff? Staff { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(50)]
        public string? Relation { get; set; }

        [StringLength(20)]
        public string? Landline { get; set; }

        [StringLength(20)]
        public string? Mobno { get; set; }
    }
}
