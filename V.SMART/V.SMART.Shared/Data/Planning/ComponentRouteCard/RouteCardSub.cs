using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Planning.ComponentRouteCard
{
    public class RouteCardSub
    {
        [Key]
        public int RCSubId { get; set; }
        public int RCId { get; set; }

        [ForeignKey(nameof(RCId))]
        public virtual RouteCard? RouteCard { get; set; }

        public int SlNo { get; set; }

        public bool UseSameSeq { get; set; } = false;
        public string? GroupKey { get; set; }

        public int? SeqNo { get; set; }// For Sub-Process Mode
        public int ProcessId { get; set; }
        [ForeignKey(nameof(ProcessId))]
        public virtual Process? Process { get; set; }


        // Input / Output Item
        public int? ItemIdIn { get; set; }

        [ForeignKey(nameof(ItemIdIn))]
        public virtual Item? IncomingItem { get; set; }

        public int? ItemIdOut { get; set; }

        [ForeignKey(nameof(ItemIdOut))]
        public virtual Item? OutgoingItem { get; set; }


        // Quantities
        [Precision(18, 3)] 
        public decimal TotalQty { get; set; }

        [Precision(18, 3)] 
        public decimal BalQty { get; set; }


        [Precision(18, 3)]
        public decimal IssuedQty { get; set; }

        [Precision(18, 3)]
        public decimal WipQty { get; set; }

        [Precision(18, 3)] 
        public decimal AccQty { get; set; }

        [Precision(18, 3)] 
        public decimal RejQty { get; set; }

        [Precision(18, 3)] 
        public decimal RewQty { get; set; }


        [Precision(18, 3)] 
        public decimal NextProcessQty { get; set; }


        // Machine / Vendor
        public int? MachineId { get; set; }

        [ForeignKey(nameof(MachineId))]
        public virtual Machine? Machine { get; set; }


        public int? VendorId { get; set; }
        [ForeignKey(nameof(VendorId))]
        public virtual Vendor? Vendor { get; set; }

        public decimal ProcessCost { get; set; }

        // Time
        public TimeSpan? CycleTime { get; set; }
        public TimeSpan? SettingTime { get; set; }


        // Flags
        public bool IsBOM { get; set; }
        public bool IsInspection { get; set; }
        public bool IsProcessSkip { get; set; }

        [StringLength(250, ErrorMessage = "The reason provided is too long. Maximum allowed length is 250 characters.")]
        public string? ProcessSkipReason { get; set; }


        public bool IsFinalProcess { get; set; }

        public bool IsReworkProcess { get; set; } = false;

        // Process Status
        public byte ProcessStatus { get; set; }
        // 0 = Pending
        // 1 = InProgress
        // 2 = Partially Completed
        // 3 = Completed



    }

}
