using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InventoryViewModel.StockIssueRequestVM
{
    public class StockIssueRequestVM
    {
            public int RequestId { get; set; }

            [Required(ErrorMessage = "Request Number is required.")]
            [StringLength(20, ErrorMessage = "Request Number cannot exceed 20 characters.")]
            public string RequestNo { get; set; } = string.Empty;

            [Required(ErrorMessage = "Suffix is Required")]
            public string Suffix { get; set; } = string.Empty;

            [Required(ErrorMessage = "MIN Generation Date is required.")]
            public DateTime RequestDate { get; set; } = DateTime.Now;

            [Required(ErrorMessage = "Material Issue To Whom is required.")]
            [StringLength(100, ErrorMessage = "Material Issue To Whom is Cannot exceed 100 Characters")]
            public string? ToWhom { get; set; }

            [Required(ErrorMessage = "Store is required.")]
            [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Store.")]
            public int? StoreIssId { get; set; }
            public string? StoreIssName { get; set; }

            public int NoOfItems => StockIssueRequestSubVMs?.Count ?? 0;

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

            public bool ReqTally { get; set; } = false;
            public bool IsLevel1 { get; set; } = false;
            public bool IsLevel2 { get; set; } = false;
            public bool IsLevel3 { get; set; } = false;
            public bool IsAuthorized { get; set; } = false;

            [StringLength(100)]
            public string? Level1Sign { get; set; }
            [StringLength(100)]
            public string? Level2Sign { get; set; }
            [StringLength(100)]
            public string? Level3Sign { get; set; }
            public bool IsRejected { get; set; } = false;
            [StringLength(250)]
            public string? RejectReason { get; set; }

            [StringLength(100)]
            public string? LastApproved { get; set; }
            public DateTime? ApprovedDate { get; set; }

            public string? ApprovedBy { get; set; }
            public DateTime? ApprovalDate { get; set; }

           [StringLength(100, ErrorMessage = "Created By cannot exceed 100 characters.")]
            public string CreatedBy { get; set; } = string.Empty;

            public DateTime CreatedDate { get; set; } = DateTime.Now;

            [StringLength(100, ErrorMessage = "Modified By cannot exceed 100 characters.")]
            public string? ModifiedBy { get; set; }

            public DateTime? ModifiedDate { get; set; }

            // ---------------- Calculated Totals ----------------
            public decimal TotalQty => StockIssueRequestSubVMs?.Sum(x => x.ReqQty ?? 0) ?? 0;
            public decimal AverageUnitPrice => StockIssueRequestSubVMs != null && StockIssueRequestSubVMs.Count > 0
                ? StockIssueRequestSubVMs.Average(x => x.UnitPrice ?? 0)
                : 0;
            public decimal TotalLineGross => StockIssueRequestSubVMs?.Sum(x => x.LineGross) ?? 0;



            [MinLength(1, ErrorMessage = "At least one Row item is required.")]
            [ValidateComplexType]
            public List<StockIssueRequestSubVM> StockIssueRequestSubVMs { get; set; } = new();




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
