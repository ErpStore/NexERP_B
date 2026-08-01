using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.Export
{
    public class ExpInv
    {
        [Key]
        public int ExpInvId { get; set; }

        [StringLength(12)]
        public string? Prefix { get; set; }

        [Required]
        [StringLength(15)]
        public string ExpInvNo { get; set; }

        [Required]
        [StringLength(12)]
        public string Suffix { get; set; }

        [Required]
        public DateTime ExpInvDate { get; set; } = DateTime.Now;
        public DateTime ExpInvDateNow { get; set; } = DateTime.Now;

        [Required]
        public int? CustId { get; set; }

        [ForeignKey(nameof(CustId))]
        public virtual Customer? Customer { get; set; }

        [StringLength(100)]
        public string? KindOfAttention { get; set; }

        public bool IsSameAsShipping { get; set; } = true;

        public int? ShipAddrsId { get; set; }

        [ForeignKey(nameof(ShipAddrsId))]
        public CustomerIndirect? Consinee { get; set; }

        public int? NoOfItems { get; set; }

        [StringLength(250)]
        public string? MainRemark { get; set; }

        [Required(ErrorMessage = "Please Select the Currency!..")]
        public int CurrId { get; set; } = 1;

        [ForeignKey(nameof(CurrId))]
        public virtual Currency Currency { get; set; }

        public decimal TodayVal { get; set; }
        public bool IsCancel { get; set; } = false;

        [StringLength(120)]
        public string? CancelReason { get; set; }


        public string? DeliveryTems { get; set; }

        
        public string? PayTerms { get; set; }

        public bool InvTally { get; set; } = false;


        public int? StoreIssId { get; set; }
        [ForeignKey(nameof(StoreIssId))]
        public virtual Store? StoreIssue { get; set; }

        public bool DcCumInv { get; set; } = false;

        [StringLength(15)]
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

        [StringLength(20)]
        public string? TransId { get; set; }

        [StringLength(100)]
        public string? TransName { get; set; }

        [StringLength(15)]
        public string? TransDocNo { get; set; }

        [StringLength(13)]
        public string? DriverNo { get; set; }

        public bool InvShortClose { get; set; } = false;

        public bool IsTaxRequired { get; set; } = false;


        public bool? IsManualInvNo { get; set; } = false;


        //For E-Invoice
        [StringLength(53)]
        public string? ACKNO { get; set; }

        [StringLength(160)]
        public string? IRNNo { get; set; }

        public string? SignedInvoiceQrCode { get; set; }

        public DateTime? ACKNODate { get; set; }

        [StringLength(5)]
        public string? InvType { get; set; }

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

        // -------- TDS --------
        [Precision(18, 4)]
        public decimal TDSAmount { get; set; }

        public DateTime? InvoiceRemovalDate { get; set; }
        public DateTime? GoodsRemovalDate { get; set; }

        public string? CountryOfOrigin { get; set; }
        public string? CountryOfDestination { get; set; }
        public string? PreCarriage { get; set; }
        public string? PlaceOfReceipt { get; set; }
        public string? FlightOrVesselNo { get; set; }
        public string? PortOfLoading { get; set; }
        public string? PortOfDischarge { get; set; }
        public string? FinalDestination { get; set; }

        public decimal? GrossWeight { get; set; }
        public decimal? NetWeight { get; set; }

        public string? PackageDetails { get; set; }
        public string? TransporterAddress { get; set; }
        public string? TransporterName { get; set; }
        public string? DocketNumber { get; set; }

        public string? CompanyType { get; set; }
        public string? Incoterm { get; set; }


        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<ExpInvSub> ExpInvSubs { get; set; } = new List<ExpInvSub>();
    }
}
