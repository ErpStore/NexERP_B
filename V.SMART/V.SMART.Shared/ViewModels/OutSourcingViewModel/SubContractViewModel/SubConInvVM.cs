using V.SMART.Shared.Utility_Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel
{
    public class SubConInvVM : ICalculationDocument
    {
        public int InvId { get; set; }

        // ===== Header =====
        [Required(ErrorMessage = "Invoice number is required.")]
        [MaxLength(20)]
        public string InvNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(10)]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "Invoice date is required.")]
        public DateTime InvDate { get; set; } = DateTime.Now;

        public DateTime InvDateNow { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vendor is required.")]
        public int? VendorCode { get; set; }

        public string? VendorName { get; set; }
        public string? VendorAddress { get; set; }
        public string? VendorGSTNo { get; set; }
        public string? VendorContactNo { get; set; }

        [MaxLength(50)]
        public string? KindOfAttention { get; set; }

        public bool IsSameAsShipping { get; set; } = true;

        public int? ShipAddrsId { get; set; }
        public string? ShippingName { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingGSTNo { get; set; }
        public string? ShippingContactNo { get; set; }

        public int NoOfItems => SubConInvSubVMs?.Count ?? 0;

        [MaxLength(500)]
        public string? MainRemark { get; set; }

        // ===== Currency =====
        [Required(ErrorMessage = "Currency is required.")]
        public int CurrId { get; set; } = 1;

        public string? CurrName { get; set; }
        public string? Symbol { get; set; }

        public decimal TodayVal { get; set; }

        // ===== Status =====
        public bool InvTally { get; set; } = false;
        public bool IsAcptRejRewQtyRequired { get; set; } = false;

        public bool InvCancel { get; set; } = false;
        public DateTime? CancelDate { get; set; }

        [MaxLength(200)]
        public string? CancelReason { get; set; }

        public string? EWayNo { get; set; }
        public DateTime? EWayDate { get; set; }

        // ===== Calculation =====
        public decimal TotalGrossAmount { get; set; }

        public bool IsItemWiseDiscAmtOrPerReq { get; set; } = false;
        public bool DiscAmtOrPer { get; set; } = true;

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TotalBasicAmount { get; set; }

        public decimal FreightCharges { get; set; }

        public bool PackingAmtOrPer { get; set; } = true;
        public decimal PackingPercent { get; set; }
        public decimal PackingCharges { get; set; }

        public bool InsuranceAmtOrPer { get; set; } = true;
        public decimal InsurancePercent { get; set; }
        public decimal InsuranceCharges { get; set; }

        public decimal CGstRate { get; set; }
        public decimal SGstRate { get; set; }
        public decimal IGstRate { get; set; }

        public decimal TotalCGSTAmount { get; set; }
        public decimal TotalSGSTAmount { get; set; }
        public decimal TotalIGSTAmount { get; set; }

        public decimal OtherCharges { get; set; }

        public bool TCSAmtOrPer { get; set; } = true;
        public decimal TCSPercent { get; set; }
        public decimal TCSAmount { get; set; }

        public decimal TotalTaxable { get; set; }

        public bool IsRoundOffEnabled { get; set; } = true;
        public decimal RoundOff { get; set; }

        public decimal GrandTotal { get; set; }
        public decimal Balance { get; set; }

        public decimal TDSAmount { get; set; }

        // ===== Terms & Audit =====
        public int? TermsId { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // ===== Line Items =====
        [MinLength(1, ErrorMessage = "At least one invoice item is required.")]
        [ValidateComplexType]
        public List<SubConInvSubVM> SubConInvSubVMs { get; set; } = new();

        // ===== ICalculationDocument =====
        public IEnumerable<ICalculationDocumentSubItem> CalculationDocumentSubItem
            => SubConInvSubVMs;

        public bool HasItemWiseTax =>
            CalculationDocumentSubItem.Any(i =>
                i.LineCGSTRate > 0 || i.LineSGSTRate > 0 || i.LineIGSTRate > 0);

        // ===== Aggregates =====
        public decimal TotalQty => SubConInvSubVMs.Sum(x => x.Qty ?? 0);
        public decimal AvgUnitPrice =>
            SubConInvSubVMs.Any() ? SubConInvSubVMs.Average(x => x.UnitPrice ?? 0) : 0;

        public decimal TotalLineGross =>
            SubConInvSubVMs.Sum(x => x.LineGross);

        public decimal TotalLineDiscountAmount =>
            SubConInvSubVMs.Sum(x => x.LineDiscountAmount.GetValueOrDefault());

        public decimal TotalLineBasicAmount =>
            SubConInvSubVMs.Sum(x => x.LineBasicAmount);
    }

}
