using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MaintenanceViewModel.BreakdownMaintenanceVM
{
    public class BreakdownMaintenanceVM
    {
        //Id
        public int BreakdownId { get; set; }

        //Machine
        [Required(ErrorMessage = "Machine is Required")]
        public int MachineId { get; set; }

        //BreakDown Date
        public DateTime BreakdownDate { get; set; } = DateTime.Now;

        //BreakDown Description
        [Required(ErrorMessage="BreakDown Description is Required")]
        [StringLength(500,ErrorMessage = "BreakDown Description cannot Exceed 500 characters.")]
        public string? BreakdownDescription { get; set; }

        //Resolution Date
        public DateTime ResolutionDate { get; set; } = DateTime.Now;

        //Corrective Action Taken
        [StringLength(500,ErrorMessage="Corrective Action Taken cannot Exceed 500 characters.")]
        public string? ResolutionDescription { get; set; }

        //Amount Spent
        public decimal? Amount { get; set; }

        //Idle Cost
        public decimal? Idlecost { get; set; }

        //Remarks
        [StringLength(500,ErrorMessage ="Remarks cannot Exceed 500 characters")]
        public string? Remarks { get; set; }

        //Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; } = String.Empty;
        public DateTime? ModifiedDate { get; set; } = null;

        public string ModifiedBy { get; set; } = String.Empty;

        //Navigation Property
        public string?  MachineName { get; set; }
    }
}
