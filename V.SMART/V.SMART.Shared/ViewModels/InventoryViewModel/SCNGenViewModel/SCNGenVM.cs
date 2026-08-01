using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InventoryViewModel.SCNGenViewModel
{
    public class SCNGenVM : IValidatableObject
    {
        public int SCNGenId { get; set; }

        [Required(ErrorMessage = "SCN Generation Number is required.")]
        [StringLength(10, ErrorMessage = "SCN Generation Number cannot exceed 10 characters.")]
        public string SCNGenNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is Required")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "SCN Generation Date is required.")]
        public DateTime SCNGenDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Store is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Store.")]
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }

        public int NoOfItems => SCNGenSubVMs?.Count ?? 0;

        public bool IsWithBom { get; set; } = false;

        public int? AssyId { get; set; }
        public string? AssyItemCode { get; set; }
        public string? AssyItemName { get; set; }

        public int? SubAssyId { get; set; }
        public string? SubAssyItemCode { get; set; }
        public string? SubAssyItemName { get; set; }


        public int? SubAssyId2 { get; set; }
        public string? SubAssyItemCode2 { get; set; }
        public string? SubAssyItemName2 { get; set; }


        public int? SubAssyId3 { get; set; }
        public string? SubAssyItemCode3 { get; set; }
        public string? SubAssyItemName3 { get; set; }

        public decimal ReqQty { get; set; }

        [StringLength(500, ErrorMessage = "Main Remark cannot exceed 500 characters.")]
        public string? MainRemark { get; set; }

        [StringLength(100, ErrorMessage = "Created By cannot exceed 100 characters.")]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100, ErrorMessage = "Modified By cannot exceed 100 characters.")]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // ---------------- Calculated Totals ----------------
        public decimal TotalQty => SCNGenSubVMs?.Sum(x => x.Qty ?? 0) ?? 0;
        public decimal AverageUnitPrice => SCNGenSubVMs != null && SCNGenSubVMs.Count > 0
            ? SCNGenSubVMs.Average(x => x.UnitPrice ?? 0)
            : 0;
        public decimal TotalLineGross => SCNGenSubVMs?.Sum(x => x.LineGross) ?? 0;

        [MinLength(1, ErrorMessage = "At least one Row item is required.")]
        [ValidateComplexType]
        public List<SCNGenSubVM> SCNGenSubVMs { get; set; } = new();

        // ✅ Custom validation logic
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsWithBom && (!AssyId.HasValue || AssyId.Value <= 0))
            {
                yield return new ValidationResult("Assembly selection is required when 'Is With BOM' is checked.",
                    new[] { nameof(AssyId) });
            }

            if (IsWithBom && AssyId.HasValue && AssyId.Value > 0 && ReqQty <= 0)
            {
                yield return new ValidationResult(
                    "Required Qty is required when 'Is With BOM' is checked.",
                    new[] { nameof(ReqQty) });
            }
        }
    }
}
