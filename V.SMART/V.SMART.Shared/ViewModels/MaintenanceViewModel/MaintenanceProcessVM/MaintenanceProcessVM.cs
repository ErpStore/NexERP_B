using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MaintenanceViewModel.MaintenanceProcessVM
{
    public class MaintenanceProcessVM
    {
        
        public int Id { get; set; }   // optional surrogate key if needed later

        public int ScheduleId { get; set; }
        public DateTime? MainDate { get; set; }

        public int? MachId { get; set; } = 0;

        public int? MainId { get; set; } = 0;

        public bool? StatusC { get; set; } = false;

        public string? Descr { get; set; }

        public string? Prepby { get; set; }

        public string? Remarks { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; } = DateTime.Now;
    }
}
