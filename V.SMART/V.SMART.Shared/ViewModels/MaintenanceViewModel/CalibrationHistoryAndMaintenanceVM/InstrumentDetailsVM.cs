using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Maintenance.CalibrationHistoryAndMaintenance
{
    public class InstrumentDetailsVM
    {
        [Key]
        public int Id { get; set; }

        public int ItemId { get; set; }

        [Required(ErrorMessage = "Instrument Unique Identification No. is required")]
        [StringLength(250)]
        public string? UniqueId { get; set; }

        [StringLength(250)]
        public string? Frequency { get; set; }

        [StringLength(250)]
        public string? Range { get; set; }

        [StringLength(250)]
        public string? LeastCount { get; set; }

        [StringLength(250)]
        public string? OtherDetails { get; set; }

        public DateTime? NextCalibrate { get; set; }

        [StringLength(40)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(40)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        //For Display Purpose
        public string? ItemCode { get; set; } = string.Empty;

        public string? ItemName { get; set; } = string.Empty;
    }
}
