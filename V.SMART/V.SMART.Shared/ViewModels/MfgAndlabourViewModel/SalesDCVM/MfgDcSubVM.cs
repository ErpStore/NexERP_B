using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM
{
    public class MfgDcSubVM : IValidatableObject
    {
        public int DcSubId { get; set; }

        [Required(ErrorMessage = "DcId is required.")]
        public int DcId { get; set; }

        [Required(ErrorMessage = "Sl No is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sl No must be greater than 0.")]
        public int SlNo { get; set; }

        public string? LineId { get; set; }

        [Required(ErrorMessage = "Please select ItemCode.")]
        public int? ItemId { get; set; }
        public ItemVM? SelectedItem { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }


        [Required(ErrorMessage = "Qty is Required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal? Qty { get; set; }

        [Required(ErrorMessage = "UnitPrice is Required")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0.")]
        public decimal? UnitPrice { get; set; }

        public decimal? LabourCost { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "BalQty cannot be negative.")]
        public decimal BalQty { get; set; }

        public int? RefPoSubId { get; set; }
        public string? RefPoNo { get; set; }
        public DateTime? RefPoDate { get; set; }
        public DateTime? PoDueDate { get; set; }
        public decimal PoBalQty { get; set; }
        

        public string? Remark { get; set; }
        public bool ItemCancel { get; set; }= false;
       
        public string? ItemCancelReason { get; set; }

      //  [Required(ErrorMessage = "Type Of PO is required.")]
        public string TypeOfPo { get; set; } = string.Empty;


        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        public bool IsEditable { get; set; }= false;

        public decimal? StockQty { get; set; }

        public string? RcSubIds { get; set; }

        public int? RefSCNSubId { get; set; }

        public string? RefGRNNo { get; set; }
        public DateTime? RefGRNDate { get; set; }

        public decimal RejectQty { get; set; }
        public decimal ReworkQty { get; set; }

        public int? InspectId { get; set; }
        public string? InspectNo { get; set; }
        public DateTime? InspectDate { get;set; }


        [MaxLength(200, ErrorMessage = "Rejection RoRemark Remark Length cannot be exceed 200 Characters")]
        public string? RejRowRemark { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Qty vs BalQty
            if (BalQty < 0)
                yield return new ValidationResult("BalQty cannot be negative.", new[] { nameof(BalQty) });

            if (BalQty > Qty)
                yield return new ValidationResult("BalQty cannot exceed Qty.", new[] { nameof(BalQty) });

            if (string.IsNullOrWhiteSpace(TypeOfPo) && string.IsNullOrEmpty(RefPoNo)==false)
                yield return new ValidationResult("Type Of PO is required.", new[] { nameof(TypeOfPo) });

            if ((RejectQty+ ReworkQty) > Qty || (RejectQty + ReworkQty) > BalQty)
                yield return new ValidationResult("RejectQty + ReworkQty cannot exceed Qty or BalQty.", new[] { nameof(BalQty) });
        }

    }
}
