using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel
{
    public class RouteCardSubVM : IValidatableObject
    {
        public int RCSubId { get; set; }

        [Required(ErrorMessage = "RcId is No Binding the Sub Items")]
        public int? RCId { get; set; }

        [Required(ErrorMessage = "SlNo is Required")]
        [Range(1, int.MaxValue, ErrorMessage = "SlNo must be 1 or greater.")]
        public int SlNo { get; set; }

        public bool UseSameSeq { get; set; } = false;
        public string? GroupKey { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "SeqNo must be 1 or greater.")]
        [Required(ErrorMessage = "SeqNo is Required")]
        public int? SeqNo { get; set; }

        [Required(ErrorMessage = "Valid Process Selection is Required.")]
        public int? ProcessId { get; set; }
        public string? ProcessName { get; set; }

        [Required(ErrorMessage = "Incoming Item is Not Mapping the Sub Table")]
        public int? ItemIdIn { get; set; }
        public string? ItemCodeIn { get; set; }

        [Required(ErrorMessage = "Outgoing Item is Not Mapping the Sub Table")]
        public int? ItemIdOut { get; set; }
        public string? ItemCodeOut { get; set; }

        [Precision(18, 3)]
        public decimal? TotalQty { get; set; }

        [Precision(18, 3)]
        public decimal? BalQty { get; set; }


        [Precision(18, 3)]
        public decimal IssuedQty { get; set; }

        [Precision(18, 3)]
        public decimal WipQty { get; set; }

        [Precision(18, 3)]
        public decimal? AccQty { get; set; }

        [Precision(18, 3)]
        public decimal? RejQty { get; set; }

        [Precision(18, 3)]
        public decimal? RewQty { get; set; }

        [Precision(18, 3)]
        public decimal? NextProcessQty { get; set; }


        public int? MachineId { get; set; }
        public string? MachineName { get; set; }


        public int? VendorId { get; set; }
        public string? VendorName { get; set; }

        [Required(ErrorMessage = "Process Cost is Required")]
        public decimal? ProcessCost { get; set; }

        [Required(ErrorMessage = "Cycle Time is Required")]
        public TimeSpan? CycleTime { get; set; }
        public TimeSpan? SettingTime { get; set; }


        public string CycleTimeText
        {
            get => CycleTime.HasValue ? CycleTime.Value.ToString(@"hh\:mm") : "";
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    CycleTime = null;
                    return;
                }

                if (int.TryParse(value, out int minutes))
                {
                    CycleTime = TimeSpan.FromMinutes(minutes);
                    return;
                }

                if (TimeSpan.TryParse(value, out TimeSpan ts))
                {
                    CycleTime = ts;
                    return;
                }

                CycleTime = null;
            }
        }

        public string SettingTimeText
        {
            get => SettingTime.HasValue ? SettingTime.Value.ToString(@"hh\:mm") : "";
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    SettingTime = null;
                    return;
                }

                if (int.TryParse(value, out int minutes))
                {
                    SettingTime = TimeSpan.FromMinutes(minutes);
                    return;
                }

                if (TimeSpan.TryParse(value, out TimeSpan ts))
                {
                    SettingTime = ts;
                    return;
                }

                SettingTime = null;
            }
        }

        public bool IsBOM { get; set; }
        public bool IsInspection { get; set; }
        public bool IsProcessSkip { get; set; }
        public bool IsFinalProcess { get; set; }

        [StringLength(250, ErrorMessage = "The reason provided is too long. Maximum allowed length is 250 characters.")]
        public string? ProcessSkipReason { get; set; }


        public byte ProcessStatus { get; set; }

        public bool IsReworkProcess { get; set; } = false;
        public bool IsEditable { get; set; } = false;


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Either Machine or Vendor must be selected
            if (MachineId == null && VendorId == null)
            {
                yield return new ValidationResult(
                    "Either Machine or Vendor selection is required.",
                    new[] { nameof(MachineId), nameof(VendorId) }
                );
            }
        }

    }
}
