using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Inspection.MasterInspection
{
    public class MasterInspection
    {
        [Key]
        public int Id { get; set; } = 0;

        [Required]
        [StringLength(250)]
        public string? InspectNo { get; set; }

        [Required]
        public string Suffix { get; set; } = string.Empty;
        public DateTime? InspectDate { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Quantity must be non-negative.")]
        public float? Qty { get; set; } = 0;

        
        public int? ProcessId { get; set; }
        [ForeignKey("ProcessId")]
        public virtual Process? Process { get; set; }

        [Required]
        public int? ItemId { get; set; } = 0;
        [ForeignKey("ItemId")]
        public virtual Item? Item { get; set; }

        [Required]
        public string? StaffName { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }

        [Column(TypeName = "NVARCHAR(MAX)")]
        public string? InspectData { get; set; }

        public int TotRows { get; set; } = 0;

        [StringLength(40)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(40)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

    }
}
