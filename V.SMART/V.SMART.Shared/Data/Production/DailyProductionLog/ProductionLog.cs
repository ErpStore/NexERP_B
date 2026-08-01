

using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Production.DailyProductionLog
{
    using V.SMART.Shared.Data.Inspection.FinalInspection;
    using V.SMART.Shared.Data.Inspection.IncomingInspection;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ProductionLog
    {
        // =========================
        // PRIMARY
        // =========================
        [Key]
        public long LogId { get; set; }

        [Required, StringLength(50)]
        public string LogNo { get; set; } = "";

        [Required, StringLength(50)]
        public string Suffix { get; set; } = "";

        [Required]
        public DateTime LogDate { get; set; } = DateTime.Now;
        public DateTime LogDateNow { get; set; } = DateTime.Now;


        // =========================
        // ROUTE CARD & PROCESS
        // =========================
        public int? RCId { get; set; }
        [ForeignKey(nameof(RCId))]
        public virtual RouteCard? RouteCard { get; set; }

        [Precision(18, 3)]
        public decimal RCQty { get; set; }

        public int? RCProcessId { get; set; }
        [ForeignKey(nameof(RCProcessId))]
        public virtual RouteCardSub? RouteCardProcess { get; set; }

        public int? RCPSubId { get; set; }
        [ForeignKey(nameof(RCPSubId))]
        public virtual RouteCardSub? RouteCardRcSub { get; set; }

        public int? ProcessId { get; set; }
        [ForeignKey(nameof(ProcessId))]
        public virtual Process? Process { get; set; }

        public decimal ProcessCost { get; set; }
        public TimeSpan? CycleTime { get; set; }
        public TimeSpan? SettingTime { get; set; }

        // =========================
        // SHIFT & TIME
        // =========================
        public int? ShiftId { get; set; }
        [ForeignKey(nameof(ShiftId))]
        public virtual ShiftAllocation? Shift { get; set; }
        public TimeOnly? ShiftStartTime { get; set; }
        public TimeOnly? ShiftEndTime { get; set; }


        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        [Precision(18, 2)]
        public decimal TotalHours { get; set; }

        [Precision(18, 2)]
        public decimal MachineIdleTime { get; set; }


        // =========================
        // MACHINE & OPERATOR
        // =========================
        public int? MachineId { get; set; }
        [ForeignKey(nameof(MachineId))]
        public virtual Machine? Machine { get; set; }

        [Required]
        public string Operator { get; set; }


        // =========================
        // CUSTOMER
        // =========================
        public int? CustId { get; set; }
        [ForeignKey(nameof(CustId))]
        public virtual Customer? Customer { get; set; }


        // =========================
        // ITEMS (INPUT / OUTPUT)
        // =========================
        public int? ItemIdOut { get; set; }
        [ForeignKey(nameof(ItemIdOut))]
        public virtual Item? ItemOut { get; set; }

        [Precision(18, 3)]
        public decimal QtyOut { get; set; }

        [Precision(18, 3)]
        public decimal QtyOutPerUnit { get; set; }

        public int? ItemIdIn { get; set; }
        [ForeignKey(nameof(ItemIdIn))]
        public virtual Item? ItemIn { get; set; }




        [Precision(18, 3)]
        public decimal InputQty { get; set; }

        [StringLength(50)]
        public string? BatchNo { get; set; }



        // =========================
        // RESULT QUANTITIES
        // =========================
        [Precision(18, 3)]
        public decimal AccQty { get; set; }

        [Precision(18, 3)]
        public decimal RejQty { get; set; }
        public string? RejReason { get; set; }

        [Precision(18, 3)]
        public decimal RewQty { get; set; }
        public string? RewReason { get; set; }

        public int? RewProcessId { get; set; }
        [ForeignKey(nameof(RewProcessId))]
        public virtual Process? ReworkProcess { get; set; }


        // =========================
        // STORE MOVEMENTS
        // =========================
        public int? IssueStoreId { get; set; }
        [ForeignKey(nameof(IssueStoreId))]
        public virtual Store? IssueStore { get; set; }

        public int? AddStoreId { get; set; }
        [ForeignKey(nameof(AddStoreId))]
        public virtual Store? AddStore { get; set; }


        // =========================
        // RETURN (WITHOUT PROCESS)
        // =========================
        [Precision(18, 3)]
        public decimal ReturnQty { get; set; }

        public string? ReturnReason { get; set; }


        // =========================
        // COST CENTER
        // =========================
        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public virtual CostCenter? CostCenter { get; set; }


        // =========================
        // QUALITY CONTROL
        // =========================
        public bool IsQcRequired { get; set; }

        public int? InspectId { get; set; }
        [ForeignKey(nameof(InspectId))]
        public virtual IncomingInspection? IncomingInspection { get; set; }

        // =========================
        // PRODUCTION PERFORMANCE
        // =========================
        public decimal Possible { get; set; }
        public decimal ExpectedQty { get; set; }
        public decimal TargetQtyForTotalHours { get; set; }

        public decimal EfficiencyPercent { get; set; }

        public decimal BonusEfficiencyPercent { get; set; }
        public decimal EffectiveWorkingMinutes { get; set; }


        // =========================
        // STATUS & TRACKING
        // =========================
        public bool LogStatus { get; set; } = false;


        [StringLength(300, ErrorMessage = "Remark cannot exceed 300 characters")]
        public string Remark { get; set; } = string.Empty;


        public virtual ICollection<ProductionLogSub> ProductionLogSubs { get; set; }= new List<ProductionLogSub>();


        // =========================
        // AUDIT
        // =========================
        public string CreatedBy { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

}
