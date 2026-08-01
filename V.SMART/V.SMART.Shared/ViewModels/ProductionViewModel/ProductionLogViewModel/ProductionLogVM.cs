using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ProductionViewModel.ProductionLogViewModel
{
    public class ProductionLogVM : IValidatableObject
    {
        // =========================
        // PRIMARY
        // =========================
        public long LogId { get; set; }

        [Required(ErrorMessage = "Log No. is required")]
        public string LogNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "Log Date is required")]
        public DateTime LogDate { get; set; } = DateTime.Now;
        public DateTime LogDateNow { get; set; } = DateTime.Now;


        // =========================
        // ROUTE CARD & PROCESS
        // =========================
        [Required(ErrorMessage = "Route Card selection is required")]
        public int? RCId { get; set; }

        public int? RCPSubId { get; set; }
        public string? RCNo { get; set; }

        [Required(ErrorMessage = "Route Card Qty is required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Route Card Qty must be greater than zero")]
        public decimal RCQty { get; set; }

        public decimal Possible { get; set; }    


        [Required(ErrorMessage = "Route Card sub process is not mapped")]
        public int? RCProcessId { get; set; }

        [Required(ErrorMessage = "Process selection is required")]
        public int? ProcessId { get; set; }
        public string? ProcessName { get; set; }
        public decimal ProcessCost { get; set; }

        [Required(ErrorMessage = "Cycle Time is required")]
        public TimeSpan? CycleTime { get; set; }

        public TimeSpan? SettingTime { get; set; }

        // =========================
        // SHIFT & TIME
        // =========================
        [Required(ErrorMessage = "Shift selection is required")]
        public int? ShiftId { get; set; }
        public string? ShiftName { get; set; }

        public TimeOnly? ShiftStartTime { get; set; }
        public TimeOnly? ShiftEndTime { get; set; }

        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public TimeSpan TotalTime
        {
            get
            {
                if (!StartTime.HasValue || !EndTime.HasValue)
                    return TimeSpan.Zero;

                return EndTime >= StartTime
                    ? EndTime.Value - StartTime.Value
                    : (TimeOnly.MaxValue - StartTime.Value)
                        + EndTime.Value.ToTimeSpan()
                        + TimeSpan.FromMinutes(1);
            }
        }

        // =========================
        // MACHINE & OPERATOR
        // =========================
        [Required(ErrorMessage = "Machine selection is required")]
        public int? MachineId { get; set; }
        public string? MachineName { get; set; }

        [Required(ErrorMessage = "Operator name is required")]
        public string? Operator { get; set; }

        // =========================
        // CUSTOMER
        // =========================
        public int? CustId { get; set; }
        public string? CustomerName { get; set; }

        // =========================
        // ITEMS (INPUT / OUTPUT)
        // =========================
        [Required(ErrorMessage = "Outgoing Item is not mapped")]
        public int? ItemIdOut { get; set; }
        public string? ItemOutCode { get; set; }
        public string? ItemOutName { get; set; }

        [Required(ErrorMessage = "Outgoing Quantity is required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Outgoing Quantity must be greater than zero")]
        public decimal QtyOut { get; set; }
        
        public decimal QtyOutPerUnit { get; set; }

        [Required(ErrorMessage = "Incoming Item is not mapped")]
        public int? ItemIdIn { get; set; }
        public string? ItemInCode { get; set; }
        public string? ItemInName { get; set; }


        //Important: MaxAllowedQty is used to restrict the maximum quantity that can be logged for input.
        public decimal MaxAllowedQty { get; set; } = 0;



        [Required(ErrorMessage = "Input Quantity is required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Input Quantity must be greater than zero")]
        public decimal InputQty { get; set; }
        public decimal RMStock { get; set; }

        [StringLength(50)]
        public string? BatchNo { get; set; }

        [Required(ErrorMessage = "Add Store selection is required")]
        public int? AddStoreId { get; set; }
        public string? AddStoreName { get; set; }


        [Required(ErrorMessage = "Issue Store selection is required")]
        public int? IssueStoreId { get; set; }
        public string? IssueStoreName { get; set; }


        // =========================
        // RESULT QUANTITIES
        // =========================
        public decimal AccQty { get; set; }
        public decimal RejQty { get; set; }
        public string? RejReason { get; set; }

        public decimal RewQty { get; set; }
        public string? RewReason { get; set; }

        public int? RewProcessId { get; set; }
        public string? RewProcessName { get; set; }

        // =========================
        // RETURN
        // =========================
        public decimal ReturnQty { get; set; }
        public string? ReturnReason { get; set; }

        // =========================
        // QUALITY CONTROL
        // =========================

        public bool IsBOM { get; set; }
        public bool IsQcRequired { get; set; }

        public int? InspectId { get; set; }
        public string? InspectNo { get; set; }
        public DateTime? InspectDate { get; set; }


        // =========================
        // PERFORMANCE (Calculated)
        // =========================
        public decimal ExpectedQty { get; set; }
        public decimal TargetQtyForTotalHours { get; set; }
        public decimal EfficiencyPercent { get; set; }
        public decimal BonusEfficiencyPercent { get; set; }
        public decimal EffectiveWorkingMinutes { get; set; }


        public int? CostId { get; set; }
        public string? CostCenterName { get; set; }
        // =========================
        // STOPPAGE
        // =========================
        public bool IsStoppageLoaded { get; set; }

        [StringLength(300, ErrorMessage = "Remark cannot exceed 300 characters")]
        public string Remark { get; set; } = string.Empty;

        [ValidateComplexType]
        public List<ProductionLogSubVM> ProductionLogSubVMs { get; set; } = new();

        public decimal MachineIdleTime =>
            ProductionLogSubVMs.Sum(x => x.StopageTime ?? 0);

        // =========================
        // AUDIT
        // =========================
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // =========================
        // OBJECT LEVEL VALIDATIONS
        // =========================
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (CycleTime.HasValue && CycleTime <= TimeSpan.FromMinutes(1))
                yield return new ValidationResult(
                    "Cycle Time must be greater than 1 minute",
                    new[] { nameof(CycleTime) });

            if (SettingTime.HasValue && TotalTime > TimeSpan.Zero &&
                SettingTime >= TotalTime)
                yield return new ValidationResult(
                    "Setting Time must be less than total working time",
                    new[] { nameof(SettingTime) });

            if (!StartTime.HasValue)
                yield return new ValidationResult(
                    "Start Time is required",
                    new[] { nameof(StartTime) });

            if (!EndTime.HasValue)
                yield return new ValidationResult(
                    "End Time is required",
                    new[] { nameof(EndTime) });

            if (AccQty < 0 || RejQty < 0)
                yield return new ValidationResult(
                    "Quantities cannot be negative",
                    new[] { nameof(AccQty), nameof(RejQty) });

            if (RejQty > 0 && string.IsNullOrWhiteSpace(RejReason))
                yield return new ValidationResult(
                    "Rejection reason is required",
                    new[] { nameof(RejReason) });

            if (RewQty > 0 && string.IsNullOrWhiteSpace(RewReason))
                yield return new ValidationResult(
                    "RewReason reason is required",
                    new[] { nameof(RewReason) });

            if (ReturnQty > 0 && string.IsNullOrWhiteSpace(ReturnReason))
                yield return new ValidationResult(
                    "Return reason is required",
                    new[] { nameof(ReturnReason) });
        }
    }

}
