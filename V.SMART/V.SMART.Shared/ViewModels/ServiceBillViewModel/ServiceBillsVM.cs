
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Utility_Constants;

namespace V.SMART.Shared.ViewModels.CashFlowViewModel.ServiceBillViewModel
{
    public class ServiceBillsVM : ICalculationDocument
    {
        [Key]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Please select whether it is Service Incoming or Outgoing.")]
        public bool IncomingServics { get; set; }

        [Required(ErrorMessage ="Please Enter the Invoice Number")]
        [StringLength(20,ErrorMessage="InvNo Cannot Exceed more than 20 Characters")]
        public string InvNo { get; set; } = null!;

        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(50, ErrorMessage = "Suffix cannot exceed 50 characters.")]
        public string Suffix { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please Enter the Invoice Date")]
        public DateTime InvDate { get; set; } = DateTime.Now;

        public DateTime InvDateNow { get; set; }= DateTime.Now;


        // -------------------- Customer --------------------
        [Required(ErrorMessage ="Please Select The Customer")]
        public int? CustVendorId { get; set; }

        public string? CustVendorName { get; set; }

        public int? NoOfItems => ServiceBillsSub?.Count ?? 0;

        public string? MainRemark { get; set; }


        // -------------------- Ref Details --------------------
        public string? DcNo { get; set; }
        public DateTime? DcDate { get; set; }=DateTime.Now;


        // -------------------- Transport --------------------
        public string? ModeofTrasnport { get; set; }
        [RegularExpression(@"^[A-Z]{2}[0-9]{1,2}[A-Z]{1,2}[0-9]{4}$", ErrorMessage = "Invalid Vehicle No (e.g., KA01AB1234).")]
        public string? VehicleNo { get; set; } = string.Empty;


        // -------------------- Currency --------------------
        [Required(ErrorMessage ="Please Select The Currency")]
        public int? CurrId { get; set; }
        public string? CurrName { get; set; }
        public string? Symbol { get; set; }


        [Range(0,double.MaxValue, ErrorMessage = "Today's value must be non-negative.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TodayVal { get; set; }
 

        // -------------------- Cancel --------------------
        public bool IsCancel { get; set; }
        public string? CancelReason { get; set; }
        public bool ShortClose { get; set; } = false;

        [StringLength(300, ErrorMessage = "Payment Terms cannot exceed 300 characters.")]
        public string? PayTerms { get; set; }
        public string? Remarks { get; set; }


        // -------------------- Amounts --------------------
        [Range(0, double.MaxValue, ErrorMessage = "Total Gross Amount must be non-negative.")]
        public decimal TotalGrossAmount { get; set; }

        public bool IsItemWiseDiscAmtOrPerReq { get; set; }
        public bool DiscAmtOrPer { get; set; }


        [Range(0, 100, ErrorMessage = "Discount percent must be between 0 and 100.")]
        public decimal DiscountPercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount amount must be non-negative.")]
        public decimal DiscountAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total Basic Amount must be non-negative.")]
        public decimal TotalBasicAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Freight charges must be non-negative.")]
        public decimal FreightCharges { get; set; }

        // -------------------- Packing --------------------
        public bool PackingAmtOrPer { get; set; }


        [Range(0, 100, ErrorMessage = "Packing percent must be between 0 and 100.")]
        public decimal PackingPercent { get; set; }


        [Range(0, double.MaxValue, ErrorMessage = "Packing charges must be non-negative.")]
        public decimal PackingCharges { get; set; }


        // -------------------- Insurance --------------------
        public bool InsuranceAmtOrPer { get; set; }

        [Range(0, 100, ErrorMessage = "Insurance percent must be between 0 and 100.")]
        public decimal InsurancePercent { get; set; }


        [Range(0, double.MaxValue, ErrorMessage = "Insurance charges must be non-negative.")]
        public decimal InsuranceCharges { get; set; }

        // -------------------- GST --------------------
        [Range(0, 100, ErrorMessage = "CGST rate must be between 0 and 100.")]
        public decimal CGstRate { get; set; }

        [Range(0, 100, ErrorMessage = "SGST rate must be between 0 and 100.")]
        public decimal SGstRate { get; set; }

        [Range(0, 100, ErrorMessage = "IGST rate must be between 0 and 100.")]
        public decimal IGstRate { get; set; }


        [Range(0, double.MaxValue, ErrorMessage = "Total CGST amount must be non-negative.")]
        public decimal TotalCGSTAmount { get; set; }


        [Range(0, double.MaxValue, ErrorMessage = "Total SGST amount must be non-negative.")]
        public decimal TotalSGSTAmount { get; set; }


        [Range(0, double.MaxValue, ErrorMessage = "Total IGST amount must be non-negative.")]
        public decimal TotalIGSTAmount { get; set; }


        [Range(0, double.MaxValue, ErrorMessage = "Other charges must be non-negative.")]
        public decimal OtherCharges { get; set; }

        // -------------------- TCS --------------------
        public bool TCSAmtOrPer { get; set; }

        [Range(0, 100, ErrorMessage = "TCS percent must be between 0 and 100.")]
        public decimal TCSPercent { get; set; }


        [Range(0, double.MaxValue, ErrorMessage = "TCS amount must be non-negative.")]
        public decimal TCSAmount { get; set; }


        [Range(0, double.MaxValue, ErrorMessage = "Total taxable amount must be non-negative.")]
        public decimal TotalTaxable { get; set; }


        //Debit Note / Credit Note Detais

        [Column(TypeName = "decimal(18,4)")]
        public decimal DebitAmount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal DebitRem { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Debit { get; set; }


        // -------------------- Round Off --------------------
        public bool IsRoundOffEnabled { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal RoundOff { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Grand total must be non-negative.")]
        public decimal GrandTotal { get; set; }

        public decimal Balance { get; set; }

        public bool ServiceTally { get; set; } = false;

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [MinLength(1, ErrorMessage = "At least one Service Bill item is required.")]
        [ValidateComplexType]
        public List<ServiceBillsSubVM> ServiceBillsSub { get; set; } = new();

        // ICalculationDocument implementation
        // Use this attribute to tell the validator to check the collection.
        public IEnumerable<ICalculationDocumentSubItem> CalculationDocumentSubItem => ServiceBillsSub;
        public bool HasItemWiseTax =>
            CalculationDocumentSubItem?.Any(i => i.LineCGSTRate > 0 || i.LineSGSTRate > 0 || i.LineIGSTRate > 0) == true;


        public decimal TotalQty => ServiceBillsSub?.Sum(x => x.Qty) ?? 0;
        public decimal AvgUnitPrice => (ServiceBillsSub?.Any() == true ? ServiceBillsSub.Average(x => x.UnitPrice) : 0)??0;
        public decimal TotalLineGross => ServiceBillsSub?.Sum(x => x.LineGross) ?? 0;
        public decimal TotalLineDiscountAmount => ServiceBillsSub?.Sum(x => x.LineDiscountAmount) ?? 0;
        public decimal TotalLineBasicAmount => ServiceBillsSub?.Sum(x => x.LineBasicAmount) ?? 0;
    }
}
