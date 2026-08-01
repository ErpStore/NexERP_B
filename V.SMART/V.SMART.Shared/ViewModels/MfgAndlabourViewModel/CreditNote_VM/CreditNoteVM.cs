using DocumentFormat.OpenXml.Spreadsheet;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.SalesAndLabour.Credit_Note;
using V.SMART.Shared.Utility_Constants;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.MfgInvVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.CreditNote_VM
{
    public class CreditNoteVM: ICalculationDocument
    {
      
        public int? CrId { get; set; }

        [StringLength(10, ErrorMessage = "Prefix should be 10 characters")]
        public string? Prefix { get; set; }

        [Required(ErrorMessage = "Credit Number is required")]
        public string? CreditNo { get; set; }

        [Required]
        public string? Suffix { get; set; }

        [Required]
        public DateTime CreditDate { get; set; } = DateTime.Now;
        public DateTime CreditDateNow { get; set; } = DateTime.Now;

        public int? CustId { get; set; }
        public CustomerVM? SelectedCustomer { get; set; }
        public string? CustName { get; set; }
        public string? CustAddress { get; set; }

        public string? BusiType { get; set; }

        [Phone(ErrorMessage = "Invalid contact number.")]
        public string? ContactNo { get; set; }

        public string? GSTNo { get; set; }
        public string? PANNo { get; set; }
        public string? KindOfAttention { get; set; }
        public bool IsSameAsShipping { get; set; } = true;
        
        public int? ShipAddrsId { get; set; }
        public string? ShippingName { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingGstin { get; set; }
        public string? ShippingContactNo { get; set; }

        public int? NoOfItems { get; set; }

        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? MainRemark { get; set; }

        [Required(ErrorMessage = "Please Select the Currency!..")]
        public int CurrId { get; set; } = 1;
        public string? CurrName { get; set; }
        public string? Symbol { get; set; }

        public decimal TodayVal { get; set; }

        public bool? IsManualInvNo { get; set; } = false;

        public bool? Rejection { get; set; } = false;

        public bool? RateDifference { get; set; } = false;

        public bool? Sales { get; set; } = true;

        public bool? Labour { get; set; } = false;

        public bool? Export { get; set; } = false;

        //For E-Invoice

        public string? ACKNO { get; set; }

        public string? IRNNo { get; set; }

        public string? SignedInvoiceQrCode { get; set; }

        public DateTime? ACKNODate { get; set; }

        public string? InvType { get; set; } = "CRN";
        //--------------------------------------------

        // Calculation fields
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
        [Precision(18, 4)]
        public decimal GrandTotal { get; set; }
        [Precision(18, 4)]
        public decimal Balance { get; set; }

        [Precision(18, 4)]
        public decimal RoundOff { get; set; }

        public bool IsRoundOffEnabled { get; set; } = true;

        public bool Cancel { get; set; } = false;
        [StringLength(120, ErrorMessage = "Cancel Reason cannot exceed 120 characters.")]
        public string? CancelReason { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [MinLength(1, ErrorMessage = "At least one Invoice item is required.")]
        [ValidateComplexType]
        public List<CreditNoteSubVM> CreditNoteSubVMs { get; set; } = new();



        // ICalculationDocument implementation
        // Use this attribute to tell the validator to check the collection.
        public IEnumerable<ICalculationDocumentSubItem> CalculationDocumentSubItem => CreditNoteSubVMs;
        public bool HasItemWiseTax =>
            CalculationDocumentSubItem?.Any(i => i.LineCGSTRate > 0 || i.LineSGSTRate > 0 || i.LineIGSTRate > 0) == true;


        public decimal TotalQty => CreditNoteSubVMs?.Sum(x => x.Qty) ?? 0;
        public decimal TotalRejQty => CreditNoteSubVMs?.Sum(x => x.RejQty) ?? 0;
        public decimal AvgUnitPrice => (CreditNoteSubVMs?.Any() == true ? CreditNoteSubVMs.Average(x => x.UnitPrice) : 0) ?? 0;
        public decimal TotalLineGross => CreditNoteSubVMs?.Sum(x => x.LineGross) ?? 0;
        public decimal TotalLineDiscountAmount => CreditNoteSubVMs?.Sum(x => x.LineDiscountAmount) ?? 0;
        public decimal TotalLineBasicAmount => CreditNoteSubVMs?.Sum(x => x.LineBasicAmount) ?? 0;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Customer validation
            if (!CustId.HasValue || CustId.Value == 0)
            {
                yield return new ValidationResult(
                    "Customer is required",
                    new[] { nameof(CustId) });
            }

            // Sub-items validation
            if (CreditNoteSubVMs == null || !CreditNoteSubVMs.Any())
            {
                yield return new ValidationResult(
                    "At least one line item is required.",
                    new[] { nameof(CreditNoteSubVMs) });
            }
            else
            {
                foreach (var item in CreditNoteSubVMs)
                {
                    string code = item.ItemCode ?? "(No Code)";

                    // You can add conditional check based on rejection if needed
                    if ((item.RejQty ) <= 0 && (item.CrDrQty ) <= 0)
                    {
                        yield return new ValidationResult(
                            $"Item {code} must have reject quantity greater than 0.",
                            new[] { nameof(CreditNoteSubVMs) });
                    }

                    if ((item.CrDrUnitPrice) <= 0)
                    {
                        yield return new ValidationResult(
                            $"Item {code} must have rate diffrence greater than 0.",
                            new[] { nameof(CreditNoteSubVMs) });
                    }
                }
            }
        }
    }
}
