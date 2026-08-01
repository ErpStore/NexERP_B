using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM
{
    public class PurchaseSCNSubVM : IValidatableObject
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
        public decimal? AcceptQty { get; set; }
        public decimal? RejectQty { get; set; }

        public decimal? RejectBalQty { get; set; }

        public decimal? StockQty { get; set; }
        public decimal? ReworkQty { get; set; }

        [Required(ErrorMessage = "UnitPrice is Required")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0.")]
        public decimal? UnitPrice { get; set; }

        public int? RefPoSubId { get; set; }
        public int? RefGRNSubId { get; set; }
        public string? RefGRNNo { get; set; }
        public DateTime? RefGRNDate { get; set; }

        public string? RefDcNo { get; set; }
        public DateTime? RefDcDate { get; set; }

        public string? RefInvNo { get; set; }
        public DateTime? RefInvDate { get; set; }

        public DateTime? DueDate { get; set; }

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        public bool ItemCancel { get; set; } = false;
        public string? ItemCancelReason { get; set; }

        public string? Remark { get; set; }
        public string? InspectionNo { get; set; }

        public string? NCRNo { get; set; }

        public string? BatchNo { get; set; }
        public string? HeatNo { get; set; }

        public int? RejectionId { get; set; }

        public string? RejectionCode { get; set; }

        public string? RejectionDescription { get; set; }

        public decimal? BalQty { get; set; }
        [Precision(18, 3)]

        public decimal QtyConvert => Math.Round(UnitConvert.GetValueOrDefault() > 0
                                               ? AcceptQty.GetValueOrDefault() * UnitConvert.GetValueOrDefault()
                                               : AcceptQty.GetValueOrDefault(),
                                               3,
                                               MidpointRounding.AwayFromZero);

        public bool IsEditable { get; set; } = false;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var acc = AcceptQty.GetValueOrDefault();
            var rej = RejectQty.GetValueOrDefault();
            var rew = ReworkQty.GetValueOrDefault();

            if (acc + rej + rew <= 0)
            {
                yield return new ValidationResult(
                    "Please enter Accepted, Rejected or Rework quantity. At least one quantity must be greater than 0.",
                    new[] { nameof(AcceptQty), nameof(RejectQty), nameof(ReworkQty) }
                );
            }
        }

    }
}
