using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Maintenance.CalibrationHistoryAndMaintenance
{
    public class CalibrationHistory
    {
        [Key]
        public int? CalibrateId { get; set; }

        public int InstrumentId { get; set; }    
        [ForeignKey(nameof(InstrumentId))]
        public virtual InstrumentDetails? Instrument{ get; set; }

        public DateTime? PrevDate { get; set; }

        [Required]
        [StringLength(250)]
        public string? CalibrateNo { get; set; }

        [StringLength(250)]
        public string? CalibrateStatus { get; set; }

        public bool CalibrateCancl { get; set; } = false;
        public DateTime? CancelDate { get; set; }
        public string? CancelReason { get; set; }

        [StringLength(250)]
        public string? Authorised { get; set; }

        [StringLength(250)]
        public string? CalibrateDetails { get; set; }

        public DateTime? ExpectedCalibrate { get; set; }

        [StringLength(40)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(40)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
