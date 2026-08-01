using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.Data.Production.ProductionComponent
{
    public class ProductionReturnComp
    {

        [Key]
        public int ReturnId { get; set; }

        public bool IsNoInspection { get; set; } = false;
        public bool Rejection { get; set; } = false;
        public bool Return { get; set; } = false;

        [Required, MaxLength(50)]
        public string ReturnNo { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Suffix { get; set; } = string.Empty;

        [Required]
        public DateTime ReturnDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime ReturnDateNow { get; set; } = DateTime.Now;


        public int? CustId { get; set; }
        [ForeignKey(nameof(CustId))]
        public virtual Customer? Customer { get; set; }


        [MaxLength(250)]
        public string? MainRemark { get; set; }

        public string ReturnBy { get; set; } = string.Empty;
        public int? AddStoreId { get; set; }
        [ForeignKey(nameof(AddStoreId))]
        public virtual Store? AddStore { get; set; }

        public bool ReturnTally { get; set; } = false;

        [MaxLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        [MaxLength(50)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public bool Cancel { get; set; } = false;

        [StringLength(250)]
        public string? CancelBy { get; set; }

        public DateTime? CancelDate { get; set; }

        [StringLength(500)]
        public string? CancelReason { get; set; }

        public bool ShortClose { get; set; } = false;
        public bool IsWithoutPoDc { get; set; } = false;

        public bool IsManual { get; set; }

        public int? ShiftId { get; set; }
        [ForeignKey(nameof(ShiftId))]
        public virtual ShiftAllocation? Shift { get; set; }

        public int? ProcessId { get; set; }
        [ForeignKey(nameof(ProcessId))]
        public virtual Process? Process { get; set; }


        public virtual ICollection<ProductionReturnCompSub>? ProductionReturnCompSubs { get; set; }
        public virtual ICollection<ProductionReturnCompTrack>? ProductionReturnCompTracks { get; set; }

    }
}
