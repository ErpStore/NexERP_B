using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel
{
    public class AssemblyDefLabourVM : IValidatableObject
    {
        
            public int Id { get; set; }

            public bool IsSelected { get; set; }

            // ================= Assembly =================
            [Required(ErrorMessage = "Assembly selection is required")]
            public int? AssmblyID { get; set; }

            public string? AssemblyItemCode { get; set; }
            public string? AssemblyItemName { get; set; }
             public decimal? AssemblyPrice { get; set; }
             public decimal? LabourPrice { get; set; }
            public ItemVM? SelectedAssyItemVMs { get; set; }
            public string? AssemblyUOM { get; set; }
            public decimal? AssemblyUnitPrice { get; set; }
            public string? AssemblyCategoryName { get; set; }


        // ================= Child / Part =================
        [Required(ErrorMessage = "Part Item is required")]
            public int? ItemId { get; set; }

            public string? PartItemCode { get; set; }
            public string? PartItemName { get; set; }
            public ItemVM? SelectedItemVMs { get; set; }

            public string? UOM { get; set; }
            public string? PartSpecification { get; set; }
            public string? HSNCode { get; set; }

            public string? ItemType { get; set; }
            public string? CategoryName { get; set; }


            // ================= Quantities =================
            [Range(1, int.MaxValue)]
            public int SlNo { get; set; }

            [Required]
            [Range(0.001, 999999999999.999)]
            public decimal? UtilQty { get; set; }

            [Required]
            [Range(0, 999999999999.999)]
            public decimal? BalQty { get; set; }

            
            [Range(0, 999999999999.999)]
             public decimal? ReqQty { get; set; }

            // ================= Flags =================
            public bool IsComponent { get; set; }
            public bool Completed { get; set; }
            public bool Cancel { get; set; }

            // ================= Cost & Process =================
            public int? CostId { get; set; }
            public string? CostCenterName { get; set; }

            public int? ProcessId { get; set; }
            public string? ProcessName { get; set; }

            public decimal StockConvertedQty { get; set; }
            public decimal PRConvertedQty { get; set; }

            // ================= Audit =================
            public string? Remarks { get; set; }
            public string? CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public string? ModifiedBy { get; set; }
            public DateTime? ModifiedDate { get; set; }

            public bool IsEditable { get; set; }
            public bool? IsBOM { get; set; }
            public decimal? BomStockQty { get; set; }

            public string? RackNo { get; set; }
            //sns

            [Precision(18, 3)]
            public decimal? UnitPrice { get; set; }

            [Precision(18, 3)]
            public decimal? MaterialCost { get; set; }

            [Precision(18, 3)]
            public decimal? LaborCost { get; set; }

            [Precision(18, 3)]
            public decimal? TotalCost { get; set; }

            [Precision(18, 3)]
            public decimal? AssemblyCost { get; set; }

            //This will for labour costcalculation
            public bool IsLabourMaterial { get; set; } = false;

            // ================= Validation =================
            public IEnumerable<ValidationResult> Validate(ValidationContext context)
            {
                if (IsComponent && (!ProcessId.HasValue || ProcessId <= 0))
                {
                    yield return new ValidationResult(
                        "Process is mandatory for components.",
                        new[] { nameof(ProcessId) });
                }

                if (IsSelected && (!ReqQty.HasValue || ReqQty <= 0.001m))
                {
                    yield return new ValidationResult(
                        "Required Quantity must be greater than 0.001.",
                        new[] { nameof(ReqQty) });
                }
            }
    }
    
}
