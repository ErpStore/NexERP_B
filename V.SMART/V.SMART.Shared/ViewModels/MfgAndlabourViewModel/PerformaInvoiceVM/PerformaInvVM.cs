using V.SMART.Shared.Utility_Constants;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.PerformaInvoiceVM
{
    public class PerformaInvVM : ICalculationDocument
    {
        public int InvId { get; set; }

        [Required(ErrorMessage = "Customer is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Customer.")]
        public int? CustId { get; set; }

        // 🔹 Customer
        public string? CustName { get; set; }
        public string? CustAddress { get; set; }
        public string? CustGst { get; set; }
        public string? CustContactNo { get; set; }
        public string? KindOfAttention { get; set; }

        public bool IsSameAsShipping { get; set; } = true;

        public int? ShipAddrsId { get; set; }
        public string? ShippingName { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingGstin { get; set; }
        public string? ShippingContactNo { get; set; }


        // 🔹 Currency
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Currency.")]
        public int? CurrId { get; set; }
        public string? CurrName { get; set; }
        public string? Symbol { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Today’s Value must be non-negative.")]
        public decimal TodayVal { get; set; }

        [StringLength(10, ErrorMessage = "Prefix cannot be Exceed 10 Characters")]
        public string? Prefix { get; set; }

        [Required(ErrorMessage ="Invoice Number is required")]
        [StringLength(10,ErrorMessage ="InvNo cannot be Exceed 10 Characters")]
        public string InvNo { get; set; }

        [Required(ErrorMessage = "Suffix is required")]
        public string Suffix { get; set; }

        [Required]
        public DateTime InvDate { get; set; } = DateTime.Now;
        public DateTime InvDateNow { get; set; } = DateTime.Now;

        [Range(1, int.MaxValue)]
        public int NoOfItems => PerformaInvSubVMs?.Count ?? 0;


        [StringLength(500)]
        public string? PayTerms { get; set; }

        [StringLength(500)]
        public string? DeliveryTerms { get; set; }

        [StringLength(500)]
        public string? MainRemark { get; set; }

        public bool InvCancl { get; set; } = false;
        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }


        [Precision(18, 4)]
        public decimal TotalGrossAmount { get; set; }

        public bool IsItemWiseDiscAmtOrPerReq { get; set; } = false;
        public bool DiscAmtOrPer { get; set; } = true;

        [Precision(18, 4)]
        public decimal DiscountPercent { get; set; }

        [Precision(18, 4)]
        public decimal DiscountAmount { get; set; }

        [Precision(18, 4)]
        public decimal TotalBasicAmount { get; set; }

        [Precision(18, 4)]
        public decimal FreightCharges { get; set; }

        public bool PackingAmtOrPer { get; set; } = true;
        [Precision(18, 4)]
        public decimal PackingPercent { get; set; }
        [Precision(18, 4)]
        public decimal PackingCharges { get; set; }

        public bool InsuranceAmtOrPer { get; set; } = true;
        [Precision(18, 4)]
        public decimal InsurancePercent { get; set; }
        [Precision(18, 4)]
        public decimal InsuranceCharges { get; set; }

        [Precision(18, 3)]
        public decimal CGstRate { get; set; }
        [Precision(18, 3)]
        public decimal SGstRate { get; set; }
        [Precision(18, 3)]
        public decimal IGstRate { get; set; }

        [Precision(18, 4)]
        public decimal TotalCGSTAmount { get; set; }
        [Precision(18, 4)]
        public decimal TotalSGSTAmount { get; set; }
        [Precision(18, 4)]
        public decimal TotalIGSTAmount { get; set; }

        [Precision(18, 4)]
        public decimal OtherCharges { get; set; }

        public bool TCSAmtOrPer { get; set; } = true;
        [Precision(18, 4)]
        public decimal TCSPercent { get; set; }
        [Precision(18, 4)]
        public decimal TCSAmount { get; set; }

        [Precision(18, 4)]
        public decimal TotalTaxable { get; set; }
        public bool IsRoundOffEnabled { get; set; } = true;

        [Precision(18, 4)]
        public decimal RoundOff { get; set; }

        public decimal GrandTotal { get; set; }

        public int? BankId { get; set; }
        public string? BankName { get; set; }

        // Audit
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Sub-items
        public List<PerformaInvSubVM> PerformaInvSubVMs { get; set; } = new();

        // 🔹 CalculationDocument interface
        public IEnumerable<ICalculationDocumentSubItem> CalculationDocumentSubItem => PerformaInvSubVMs;
        public bool HasItemWiseTax => CalculationDocumentSubItem?.Any(i => i.LineCGSTRate > 0 || i.LineSGSTRate > 0 || i.LineIGSTRate > 0) == true;

        // Computed totals
        public decimal TotalQty => PerformaInvSubVMs?.Sum(x => x.Qty) ?? 0;
        public decimal AvgUnitPrice => PerformaInvSubVMs != null && PerformaInvSubVMs.Any() ? PerformaInvSubVMs.Average(x => x.UnitPrice ?? 0m) : 0;
        public decimal TotalLineGross => PerformaInvSubVMs?.Sum(x => x.LineGross) ?? 0;
        public decimal TotalLineDiscountAmount => PerformaInvSubVMs?.Sum(x => x.LineDiscountAmount ?? 0) ?? 0;
        public decimal TotalLineBasicAmount => PerformaInvSubVMs?.Sum(x => x.LineBasicAmount) ?? 0m;
    }
}
