using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MaintenanceViewModel.MaintenanceScheduleVM
{
    public class MaintenanceScheduleVM
    {
        public int ScheduleId { get; set; }
        [Required(ErrorMessage ="Machine is Required")]

        public int MachineId { get; set; }

        [Required]
        public int MaintenanceId { get; set; }

        [Required]
        [StringLength(500)]
        public String? Description { get; set; }

        [Required]
        public string? ScheduleDay { get; set; }

        [Required]
        public string? DocNo { get; set; }

        [Required]
        public int RevisionNo { get; set; }

        public DateTime RevisionDate { get; set; }

        public DateTime CreatedDate { get; set; } 
        public DateTime? ModifiedDate { get; set; } 
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; } = String.Empty;


        //Navigation Property for Display Purpose
        public string? MachineName { get; set; }

        public bool IsEditable { get; set; } = false;

        public string MaintenanceName { get; set; }

        public DateTime ScheduleDate { get; set; }
    }
}
