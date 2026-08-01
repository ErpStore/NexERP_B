using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Maintenance.MaintenanceSchedule
{
    public class MaintenanceSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [Required]
        public int MachineId { get; set; }
        [ForeignKey(nameof(MachineId))]
        public virtual Machine? Machine { get; set; }

        [Required]
        public int MaintenanceId { get; set; }

        [Required]
        [StringLength(500)]
        public String Description  { get;    set; }

        [Required]
        public string ScheduleDay{ get; set; }

        [Required]
        public string DocNo { get; set; }

        [Required]
        public int RevisionNo { get; set; }

        public DateTime RevisionDate { get; set; }

        // Audit
        public DateTime CreatedDate { get; set; }=DateTime.Now;
        public DateTime? ModifiedDate { get; set; }= null;
        public string CreatedBy { get; set; } = String.Empty;
        public string ModifiedBy { get; set; } = String.Empty;
    }
  
}
