using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel
{
    public class MachineVM
    {
        public int MachineId { get; set; }

        [Required(ErrorMessage = "Machine Name is required.")]
        [StringLength(250, ErrorMessage = "Machine Name cannot exceed 250 characters.")]
        public string MachineName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Serial No is required.")]
        [StringLength(50, ErrorMessage = "Serial No cannot exceed 50 characters.")]
        public string SerialNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Identification is required.")]
        [StringLength(250, ErrorMessage = "Identification cannot exceed 250 characters.")]
        public string Identification { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(50, ErrorMessage = "Location cannot exceed 50 characters.")]
        public string Location { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Make cannot exceed 50 characters.")]
        public string? Make { get; set; }

        [StringLength(50, ErrorMessage = "Machine Capacity cannot exceed 50 characters.")]
        public string? MachineCapacity { get; set; }

        [StringLength(50, ErrorMessage = "Machine Model cannot exceed 50 characters.")]
        public string? MachineModel { get; set; }

        [StringLength(50, ErrorMessage = "Machine Accuracy cannot exceed 50 characters.")]
        public string? MachineAccuracy { get; set; }

        [StringLength(500, ErrorMessage = "Other Details cannot exceed 500 characters.")]
        public string? OtherDetails { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Machine Hour Rate must be a positive number.")]
        public decimal MachineHrRate { get; set; }

        // Audit fields (optional in UI)
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
