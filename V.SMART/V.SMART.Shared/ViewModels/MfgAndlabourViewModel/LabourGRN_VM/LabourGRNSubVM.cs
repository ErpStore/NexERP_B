using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourGRN_VM
{
    public class LabourGRNSubVM : IValidatableObject
    {
        public int GRNSubId { get; set; }
        public int? GroupId { get; set; }

        [Required(ErrorMessage = "GRNId is required.")]
        public int GRNId { get; set; }

        public bool isOpenPo { get; set; } = false;

        [Required(ErrorMessage = "Sl No is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sl No must be greater than 0.")]
        public int SlNo { get; set; }

        //-----------------ItemOut-------------------------------
        [Required(ErrorMessage = "Outgoing Item is required.")]
        public int? ItemId { get; set; }

        public ItemVM? SelectedItemOut { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }
        public decimal? Weight { get; set; }
        public decimal? UnitConvert { get; set; }
        public decimal? AltRate { get; set; }
        public decimal? LabourCost { get; set; }
        public string? Category { get; set; }
        public int CategoryCode { get; set; }


        [Precision(18, 3)]
        [Range(0.001, double.MaxValue, ErrorMessage = "DC Qty must be greater than zero.")]
        public decimal? DCQty { get; set; }

        [Required(ErrorMessage = "Qty is required.")]
        [Precision(18, 3)]
        [Range(0.001, double.MaxValue, ErrorMessage = "Qty-Out must be greater than zero.")]
        public decimal? Qty { get; set; }


        [Precision(18, 3)]
        public decimal? BalQty { get; set; }

        public decimal? ProdCompBalQty { get; set; }
        //----------------------------------------------------------

        //------------------------------------------------------------------
        public decimal? StockQty { get; set; }

        public string TransType { get; set; } = "Out";

        [Range(0.0001, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0.")]
        public decimal? UnitPrice { get; set; } = 1;

        public string? WONo { get; set; }

        public string? RowRemarks { get; set; }

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        public string? HeatNo { get; set; }

        public string? BatchNo { get; set; }

        public bool ItemCancel { get; set; }

        public string? CancelItemReason { get; set; }//CostId

        public int? RefPoSubId { get; set; }
        public string? RefPoNo { get; set; }
        public decimal? POBalQty { get; set; }
        public DateTime? RefPoDate { get; set; }
        public DateTime? PoDueDate { get; set; }

        public int? LineNo { get; set; }

        public bool IsEditable { get; set; } = false;
        public bool CanEditItem => (Qty ?? 0) == (BalQty ?? 0) && !ItemCancel;



        // Validation properties
        public bool HasValidQty => Qty > 0;
        public bool HasAnyValidQty => HasValidQty;
        public bool HasAnyData => ItemId > 0 || ItemId.HasValue || Qty > 0;
        public bool HasValidINData => ItemId > 0 && Qty > 0;
        public bool HasValidData => (HasValidINData) && UnitPrice > 0;
        public bool IsEmptyRow => !HasAnyData;
        public bool HasPartialData => HasAnyData && !HasValidData;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Qty vs BalQty
            if (BalQty < 0)
                yield return new ValidationResult("BalQty cannot be negative.", new[] { nameof(BalQty) });

            if (BalQty > Qty) 
                yield return new ValidationResult("BalQty cannot exceed Qty.", new[] { nameof(BalQty) });

            // Conditional DCQty validation
            bool skipDcQtyValidation = isOpenPo && TransType == "Out";

            if (!skipDcQtyValidation)
            {
                if (DCQty == null || DCQty <= 0)
                {
                    yield return new ValidationResult(
                        "DC Qty must be greater than zero.",
                        new[] { nameof(DCQty) });
                }
            }
        }

    }

}
