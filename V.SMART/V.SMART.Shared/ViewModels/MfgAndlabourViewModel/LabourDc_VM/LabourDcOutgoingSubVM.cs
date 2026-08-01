using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.LabourDC;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FastReport.Barcode.Iban;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourDc_VM
{
    public class LabourDcOutgoingSubVM
    {

        public int DcSubId { get; set; }

        [Required(ErrorMessage = "DcId is required.")]
        public int DcId { get; set; }

        [Required(ErrorMessage = "Sl No is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sl No must be greater than 0.")]
        public int SlNo { get; set; }

        [Required(ErrorMessage = "Please select ItemCode.")]
        public int ItemId { get; set; }
        public ItemVM? SelectedItemOut { get; set; }

        public string TransType { get; set; } = "In";

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Specification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }
        public decimal? Weight { get; set; }
        public decimal? UnitConvert { get; set; }
        public decimal? AltRate { get; set; }

        public string? Category { get; set; }
        public int CategoryCode { get; set; }

        [Required(ErrorMessage = "Qty is Required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal? Qty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "BalQty cannot be negative.")]
        public decimal BalQty { get; set; }

        //[Required(ErrorMessage = "UnitPrice is Required")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0.")]
        public decimal? UnitPrice { get; set; }

        public decimal? RejectQty { get; set; }
        public decimal? ReworkQty { get; set; }
        public string? RejectRowRem { get; set; }

        public decimal? AltQty { get; set; }
        public decimal? StockQty { get; set; }


        [StringLength(250, ErrorMessage = "Dc Row Remarks Length cannot be exceed 250 Characters")]
        public string? DcRowRem { get; set; }

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        public bool ItemCancel { get; set; }

        public string? CancelItemReason { get; set; }

        public int? RefPoSubId { get; set; }
        public string? RcSubIds { get; set; }
        public int? PoId { get; set; }


        public string? RefPoNo { get; set; }
        public DateTime? RefPoDate { get; set; }
        public DateTime? PoDueDate { get; set; }


        public int? RefSCNSubId { get; set; }
        public string? RefSCNNo { get; set; }
        public DateTime? RefSCNDate { get; set; }

        public int GRNSubIdIn { get; set; }
        public int? RefGRNSubId { get; set; }
        public string? RefGRNNo { get; set; }
        public DateTime? RefGRNDate { get; set; }
        public string? RefDcNo { get; set; }
        public DateTime? RefDcDate { get; set; }

        public List<string> RefDcNos { get; set; }
        public List<string> RefDCDates { get; set; }


        public string? BatchNo { get; set; }
        public string? WONo { get; set; }
        public string? HeatNo { get; set; }

        public string? RCNos { get; set; }

        public int? LineNo { get; set; }
        public decimal? PoBalQty { get; set; }

        //public string? TypeOfPo { get; set; }

        public decimal? PossibleQty { get; set; }

        public bool IsEditable { get; set; } = false;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Qty vs BalQty
            if (BalQty < 0)
                yield return new ValidationResult("BalQty cannot be negative.", new[] { nameof(BalQty) });

            if (BalQty > Qty)
                yield return new ValidationResult("BalQty cannot exceed Qty.", new[] { nameof(BalQty) });

            if (BalQty > StockQty)
                yield return new ValidationResult("BalQty cannot exceed Stock Qty.", new[] { nameof(StockQty) });

            // OUT → UnitPrice mandatory
            if (TransType == "Out")
            {
                if (!UnitPrice.HasValue || UnitPrice <= 0)
                {
                    yield return new ValidationResult(
                        "Unit Price is required and must be greater than 0 for OUT transactions.",
                        new[] { nameof(UnitPrice) });
                }

            }
        }

    }
}
