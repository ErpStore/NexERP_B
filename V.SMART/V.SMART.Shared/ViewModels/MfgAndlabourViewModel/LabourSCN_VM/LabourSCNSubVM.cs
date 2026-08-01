using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.SalesAndLabour.Labour_SCN;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourSCN_VM
{
    public class LabourSCNSubVM
    {
        
        public int SCNSubId { get; set; }

        [Required(ErrorMessage = "SCNId is required.")]
        public int SCNId { get; set; }

        [Required(ErrorMessage = "Sl No is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sl No must be greater than 0.")]
        public int SlNo { get; set; }

        [Required(ErrorMessage = "Please select ItemCode.")]
        public int? ItemId { get; set; }
        public ItemVM? SelectedItem { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Specification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }
        public decimal? Weight { get; set; }
        public decimal? UnitConvert { get; set; }

        public string? Category { get; set; }
        public string? CategoryName { get; set; }

        //[Required(ErrorMessage = "Accept Qty is Required")]
        [Range(0.000, double.MaxValue, ErrorMessage = "Accept Qty must be greater than 0.")]
        public decimal? AcceptQty { get; set; }

        public decimal? BalQty { get; set; }
        public decimal? RejectQty { get; set; }
        public decimal? ReworkQty { get; set; }
        public decimal? SCNBalQty { get; set; }

        public decimal? StockQty { get; set; }

        [Required(ErrorMessage = "UnitPrice is Required")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0.")]
        public decimal? UnitPrice { get; set; }

        public int? RefGRNSubId { get; set; }

        public string? RefGRNNo { get; set; }
        public DateTime? RefGRNDate { get; set; }

        public string? RefDcNo { get; set; }
        public DateTime? RefDcDate { get; set; }

        public DateTime? DueDate { get; set; }

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }
        
        public bool ItemCancel { get; set; } = false;

        public string? Remark { get; set; }
        public string? InspectionNo { get; set; }
        public string? NCRNo { get; set; }
        public string? CancelItemReason { get; set; }

        public string? HeatNo { get; set; }
        public string? BatchNo { get; set; }

        public bool IsEditable { get; set; } = false;
        public decimal? QtyReceived { get; set; }
        public decimal? UtilQty { get; set; }
        public int? RejectionId { get; set; }

        public string? RejectionCode { get; set; }

        public string? RejectionDescription { get; set; }


        //UI Ony isplay 
        public decimal? RecievedQty => (AcceptQty ?? 0) + (RejectQty ?? 0) + (ReworkQty ?? 0);
        public decimal? QtyConvert => Math.Round(UnitConvert.GetValueOrDefault() > 0
                                       ? AcceptQty.GetValueOrDefault() * UnitConvert.GetValueOrDefault()
                                       : AcceptQty.GetValueOrDefault(),
                                       3,
                                       MidpointRounding.AwayFromZero);


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Qty vs BalQty
            if (AcceptQty < 0)
                yield return new ValidationResult("BalQty cannot be negative.", new[] { nameof(AcceptQty) });

            if ((AcceptQty + RejectQty + ReworkQty) > QtyReceived)
                yield return new ValidationResult("AcceptQty+RejectQty+ReworkQty cannot exceed QtyReceived.", new[] { nameof(QtyReceived) });

        }
    }
}
