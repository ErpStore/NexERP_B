namespace V.SMART.Shared.Data.Master.Inventory
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Machine
    {
        [Key]
        public int MachineId { get; set; }

        [Required(ErrorMessage = "Machine Name is required.")]
        [StringLength(250, ErrorMessage = "Machine Name cannot exceed 250 characters.")]
        public string MachineName { get; set; }

        [Required(ErrorMessage = "Serial No is required.")]
        [StringLength(50, ErrorMessage = "Serial No cannot exceed 50 characters.")]
        public string SerialNo { get; set; }

        [Required(ErrorMessage = "Identification is required.")]
        [StringLength(250, ErrorMessage = "Identification cannot exceed 250 characters.")]
        public string Identification { get; set; }  // e.g., "Main Plant Lathe #2"

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(50, ErrorMessage = "Location cannot exceed 50 characters.")]
        public string Location { get; set; }  // e.g., "Floor 1", "Bay A"

        [StringLength(50, ErrorMessage = "Make cannot exceed 50 characters.")]
        public string? Make { get; set; }  // e.g., "Siemens", "Bosch"

        [StringLength(50, ErrorMessage = "MachineCapacity cannot exceed 50 characters.")]
        public string? MachineCapacity { get; set; }  // Example: "100 Tons", "5 kW"

        [StringLength(50, ErrorMessage = "MachineModel cannot exceed 50 characters.")]
        public string? MachineModel { get; set; }  // Example: "MX-4500"

        [StringLength(50, ErrorMessage = "MachineAccuracy cannot exceed 50 characters.")]
        public string? MachineAccuracy { get; set; }  // Example: "±0.01 mm"

        [StringLength(500, ErrorMessage = "OtherDetails cannot exceed 500 characters.")]
        public string? OtherDetails { get; set; }

        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Machine Hour Rate must be a positive number.")]
        public decimal MachineHrRate { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }


    }

}
