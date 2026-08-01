using V.SMART.Shared.ViewModels.InventoryViewModel.SCNGenViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InventoryViewModel.MaterialIssueNoteVM
{
    public class MaterialIssNoteVM : IValidatableObject
    {
        public int MINId { get; set; }

        [Required(ErrorMessage = "MIN Number is required.")]
        [StringLength(20, ErrorMessage = "MIN Number cannot exceed 20 characters.")]
        public string IssueNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is Required")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "MIN Generation Date is required.")]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Material Issue To Whom is required.")]
        [StringLength(100,ErrorMessage ="Material Issue To Whom is Cannot exceed 100 Characters")]
        public string? ToWhom { get; set; }

        [Required(ErrorMessage = "Store is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Store.")]
        public int? StoreIssId { get; set; }
        public string? StoreIssName { get; set; }

        public int NoOfItems => MaterialIssNoteSubVMs?.Count ?? 0;

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
        public decimal TotalQty => MaterialIssNoteSubVMs?.Sum(x => x.IssueQty ?? 0) ?? 0;
        public decimal AverageUnitPrice => MaterialIssNoteSubVMs != null && MaterialIssNoteSubVMs.Count > 0
            ? MaterialIssNoteSubVMs.Average(x => x.UnitPrice ?? 0)
            : 0;
        public decimal TotalLineGross => MaterialIssNoteSubVMs?.Sum(x => x.LineGross) ?? 0;



        [MinLength(1, ErrorMessage = "At least one Row item is required.")]
        [ValidateComplexType]
        public List<MaterialIssNoteSubVM> MaterialIssNoteSubVMs { get; set; } = new();




        // ✅ Custom validation logic
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // 🔹 When "With BOM" is checked
            if (IsWithBom)
            {
                if (!AssyId.HasValue || AssyId.Value <= 0)
                {
                    yield return new ValidationResult(
                        "Assembly selection is required when 'Is With BOM' is checked.",
                        new[] { nameof(AssyId) });
                }

                if (ReqQty <= 0)
                {
                    yield return new ValidationResult(
                        "Required Qty must be greater than zero when 'Is With BOM' is checked.",
                        new[] { nameof(ReqQty) });
                }
            }
        }

    }

}
