using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels
{
    public class ProcessFlowChartVM : IValidatableObject
    {
        // Primary Key
        public int SubId { get; set; }
        public int Id { get; set; }
        public int? CompItemId { get; set; }
        public string? CompItemCode { get; set; }
        public string? CompItemName { get; set; }

        [Required(ErrorMessage = "Raw Material selection is Required")]
        public int? RMId { get; set; }
        public string? RMCode { get; set; }
        public string? RMName { get; set; }

        // Serial Number
        [Required(ErrorMessage = "SlNo is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "SlNo must be 1 or greater.")]
        public int SlNo { get; set; }

        // Process
        [Required(ErrorMessage = "Valid Process Selection is Required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Process.")]
        public int? ProcId { get; set; }
        public string? ProcessName { get; set; }

        // SubProcess
        [Required(ErrorMessage = "Please select Sub Process.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sub Process must start from 1.")]
        public int SubProcess { get; set; }

        public bool UseSameSubProcess { get; set; } = false;

        // Weights
        [Required(ErrorMessage = "RM Weight is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "RM Weight must be a positive number.")]
        public decimal? RmWt { get; set; }

        [Required(ErrorMessage = "Component Weight is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Component Weight must be a positive number.")]
        public decimal? CompWt { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Scrap Weight must be a non-negative number.")]
        public decimal? ScrapWt { get; set; }

        // Scrap
        public int? ScrapEndBit { get; set; }
        public string? ScrapMaterialName { get; set; }

        // Machine & Vendor
        // Required handled inside Validate()
        public int? MachId { get; set; }
        public string? MachineName { get; set; }

        public int? VendorCode { get; set; }
        public string? VendorName { get; set; }

        // Remarks
        [StringLength(300, ErrorMessage = "Remarks cannot exceed 300 characters.")]
        public string? Remarks { get; set; }

        // Pricing
        public decimal? UnitPrice { get; set; }

        public string? Program { get; set; }

        // Cycle Time
        [Required(ErrorMessage = "Cycle Time is Required.")]
        public TimeSpan? CycleTime { get; set; }

        // Setting Time
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

        // Flags
        public bool BOM { get; set; } = false;
        public bool Inspection { get; set; } = false;
        public bool ProcessSkip { get; set; } = false;

        // UI Editable flag
        public bool IsEditable { get; set; } = false;

        public string? GroupKey { get; set; }

        // Custom Validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Machine or Vendor required
            if ((!MachId.HasValue || MachId.Value == 0) &&
                (!VendorCode.HasValue || VendorCode.Value == 0))
            {
                yield return new ValidationResult("Please select either a Machine or a Vendor.",
                    new[] { nameof(MachId), nameof(VendorCode) }
                );
            }

            // Component cannot be heavier than RM
            if (RmWt.HasValue && CompWt.HasValue && RmWt < CompWt)
            {
                yield return new ValidationResult("Component Weight cannot be greater than Raw Material Weight.",
                    new[] { nameof(CompWt) }
                );
            }
        }
    }

}
