using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Maintenance.BreakdownMaintenance
{
    public class BreakdownMaintenance
    {
        [Key]
        public int BreakdownId { get; set; }

        [Required(ErrorMessage ="Please Select The Machine")]
        public int? MachineId { get; set; }

        [ForeignKey(nameof(MachineId))]
        public virtual Machine? Machine { get; set; }

        public DateTime BreakdownDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(500)]
        public string? BreakdownDescription { get; set; }

        public DateTime ResolutionDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Corrective Action Taken Field is Required")]
        [StringLength(500)]
        public string? ResolutionDescription { get; set; }

        [Required(ErrorMessage = "Please Enter The Amount Spent")]
        public decimal? Amount { get; set; }

        [Required(ErrorMessage = "Please Enter The Idle Cost")]
        public decimal? Idlecost { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; } = null;
         
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
