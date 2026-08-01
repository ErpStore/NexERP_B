using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Maintenance.Maintenance_Process
{
    public class MaintenanceProcess
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }   // optional surrogate key if needed later
        public int ScheduleId { get; set; }
        [ForeignKey(nameof(ScheduleId))]

        public DateTime? MaintenanceDate { get; set; }

        public int? MachineId { get; set; } = 0;

        public int? MaintenanceId { get; set; } = 0;

        public bool? MaintenanceStatus { get; set; } = false;

        [StringLength(250)]
        public string? Description { get; set; }

        [StringLength(250)]
        public string? Preparedby { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }

        [StringLength(40)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }=DateTime.Now;

        [StringLength(40)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }= DateTime.Now;

        // Local use only (not mapped to DB)
        [NotMapped]
        public bool IsEditable { get; set; } = false;
    }
}
