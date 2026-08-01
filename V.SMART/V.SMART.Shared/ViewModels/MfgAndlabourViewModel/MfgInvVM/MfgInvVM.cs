using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Utility_Constants;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.MfgInvVM
{
    public class MfgInvVM : ICalculationDocument
    {
        public int InvId { get; set; }

        [StringLength(10,ErrorMessage="Prefix should be 10 characters")]
        public string? Prefix { get; set; }

        [Required(ErrorMessage="Invoice Number is required")]
        public string InvNo { get; set; }

        [Required]
        
        public string Suffix { get; set; }

        [Required]
        public DateTime InvDate { get; set; } = DateTime.Now;
        public DateTime InvDateNow { get; set; } = DateTime.Now;

        [Required]
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
        public bool QuotationTally { get; set; } = false;
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
        public bool IsCancel { get; set; } = false;

        [StringLength(120, ErrorMessage = "Cancel Reason cannot exceed 120 characters.")]
        public string? CancelReason { get; set; }

        [StringLength(50, ErrorMessage = "Delivery Tems cannot exceed 50 characters.")]
        public string? DeliveryTems { get; set; }

        [StringLength(50, ErrorMessage = "Pay Terms cannot exceed 50 characters.")]
        public string? PayTerms { get; set; }

        public bool InvTally { get; set; } = false;

        
        public int? StoreIssId { get; set; }
        public string? StoreIssueName { get; set; }


        public bool DcCumInv { get; set; } = false;

        [StringLength(15, ErrorMessage = "E-Way bill number cannot exceed 15 characters.")]
        public string? EWayNo { get; set; }

        public DateTime? EWayDate { get; set; }

        [StringLength(13, ErrorMessage = "Invalid vehicle number length")]
        [RegularExpression(@"^$|^[A-Z]{2}[ -]?\d{1,2}[ -]?[A-Z]{1,2}[ -]?\d{4}$", ErrorMessage = "Invalid vehicle number format (e.g., KA01AB1234)")]
        public string? VehicleNo { get; set; }

        public int? TransportMode { get; set; }

        [StringLength(80, ErrorMessage = "From location cannot exceed 80 characters.")]
        public string? TransFrom { get; set; }

        [StringLength(80, ErrorMessage = "To location cannot exceed 80 characters.")]
        public string? TransTo { get; set; }


        public int? SubSupplyType { get; set; }

        [StringLength(200, ErrorMessage = "SubSupplyDesc cannot exceed 200 characters.")]
        public string? SubSupplyDesc { get; set; }


        [StringLength(20, ErrorMessage ="TransactionId cannot exceed 20 characters.")]
        public string? TransId { get; set; }

        [StringLength(100,ErrorMessage = "Transaction Name cannot exceed 100 characters.")]
        public string? TransName { get; set; }

        
        [StringLength(15, ErrorMessage = "Transaction Doc No. cannot exceed 15 characters.")]
        public string? TransDocNo { get; set; }

        [RegularExpression(@"^\+?\d{1,10}$", ErrorMessage = "Driver No must contain only numbers, optional +, and max 10 digits")]
        public string? DriverNo { get; set; }

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
        public bool IsRoundOffEnabled { get; set; } = true;

        [Precision(18, 4)]
        public decimal RoundOff { get; set; }
        public decimal GrandTotal { get; set; }

        [Precision(18, 4)]
        public decimal Balance { get; set; }

        public string? LcNo { get; set; }

        public DateTime? LcDate { get; set; }

        public DateTime? LcExpiryDate { get; set; }

        public bool PoShortClose { get; set; } = false;

        // -------- TDS --------
        [Precision(18, 4)]
        public decimal TDSAmount { get; set; }

        //For E-Invoice
        public bool? IsManualInvNo { get; set; } = false;

        public string? ACKNO { get; set; }
        public string? IRNNo { get; set; }
        public string? SignedInvoiceQrCode { get; set; }
        public DateTime? ACKNODate { get; set; }
        public string? InvType { get; set; } = "INV";
        //--------------------------------------------




        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [MinLength(1, ErrorMessage = "At least one Invoice item is required.")]
        [ValidateComplexType]
        public List<MfgInvSubVM> MfgSubInvVMs { get; set; } = new();



        // ICalculationDocument implementation
        // Use this attribute to tell the validator to check the collection.
        public IEnumerable<ICalculationDocumentSubItem> CalculationDocumentSubItem => MfgSubInvVMs;
        public bool HasItemWiseTax =>
            CalculationDocumentSubItem?.Any(i => i.LineCGSTRate > 0 || i.LineSGSTRate > 0 || i.LineIGSTRate > 0) == true;


        public decimal TotalQty => MfgSubInvVMs?.Sum(x => x.Qty) ?? 0;
        public decimal AvgUnitPrice => (MfgSubInvVMs?.Any() == true ? MfgSubInvVMs.Average(x => x.UnitPrice) : 0) ?? 0;
        public decimal TotalLineGross => MfgSubInvVMs?.Sum(x => x.LineGross) ?? 0;
        public decimal TotalLineDiscountAmount => MfgSubInvVMs?.Sum(x => x.LineDiscountAmount) ?? 0;
        public decimal TotalLineBasicAmount => MfgSubInvVMs?.Sum(x => x.LineBasicAmount) ?? 0;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Purchase Return validations
            if (!CustId.HasValue || CustId.Value == 0)
            {
                yield return new ValidationResult(
                    "Customer is required",
                    new[] { nameof(CustId) });
            }

            if (DcCumInv)
            {
                if (!StoreIssId.HasValue || StoreIssId.Value == 0)
                {
                    yield return new ValidationResult(
                        "Store Issue is required when DC Cum Invoice is enabled.",
                        new[] { nameof(StoreIssId) });
                }

                if (TransportMode <= 0)
                {
                    yield return new ValidationResult(
                        "Please select Mode Of Transport.",
                        new[] { nameof(TransportMode) });
                } 
                else if (SubSupplyType<=0) 
                {
                    yield return new ValidationResult(
                        "Please select Sub Supply Type.",
                        new[] { nameof(SubSupplyType) });
                }
                else if (string.IsNullOrEmpty(VehicleNo))
                {
                    yield return new ValidationResult(
                        "Please enter valid Vehicle No.",
                        new[] { nameof(VehicleNo) });
                }
                else if (string.IsNullOrEmpty(TransFrom))
                {
                    yield return new ValidationResult(
                        "Please enter valid Transport From",
                        new[] { nameof(TransFrom) });
                }
                else if (string.IsNullOrEmpty(TransTo))
                {
                    yield return new ValidationResult(
                        "Please enter valid Transport To.",
                        new[] { nameof(TransTo) });
                }
            }



        }
    }
}
