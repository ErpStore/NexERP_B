using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.HumanResourceMaster_Module
{
    public class StaffEducation
    {
        [Key]
        public int Slno { get; set; }

        [Required]
        public int StaffID { get; set; }
        [ForeignKey(nameof(StaffID))]
        public Staff? Staff { get; set; }

        [StringLength(50)]
        public string? Degree { get; set; }

        [StringLength(150)]
        public string? Institution { get; set; }

        [StringLength(4)]
        public string? FromYear { get; set; }

        [StringLength(4)]
        public string? ToYear { get; set; }
    }
}
