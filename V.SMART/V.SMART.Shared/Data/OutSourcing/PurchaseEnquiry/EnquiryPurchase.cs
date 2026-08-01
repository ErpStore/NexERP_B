using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry
{
    public class EnquiryPurchase
    {
        [Key]
        public int EnquiryId { get; set; }

        [Required]
        public bool PurchORSubCon { get; set; } = true;

        [StringLength(10)]
        public string? Prefix { get; set; }

        [Required]
        [StringLength(30)]
        [RegularExpression(@"^(?![\/-]+$)[0-9\/-]+$")]
        public string EnquiryNo { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Suffix { get; set; } = string.Empty;

        [Required]
        public DateTime EnquiryDate { get; set; } = DateTime.Now;

        public DateTime EnquiryDateNow { get; set; } = DateTime.Now;

        public bool EnquiryTally { get; set; } = false;

        [Required]
        public int? NoOfItems { get; set; }

        [StringLength(250)]
        public string? MainRemarks { get; set; }

        [StringLength(80)]
        public string? KindOfAttention { get; set; }

        public bool EnquiryShortClose { get; set; } = false;

        public bool Cancel { get; set; } = false;

        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }


        [StringLength(500)]
        public string? CancellationRemarks { get; set; }

        [StringLength(100)]
        public string? POCName { get; set; }

        [Phone]
        [StringLength(15)]
        public string? POCContactNo { get; set; }

        public DateTime? ExpactedReplayDate { get; set; }

        // Audit Fields
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<EnquiryPurchaseSub> EnquiryPurchaseSub { get; set; } = new List<EnquiryPurchaseSub>();

        public virtual ICollection<EnquiryPurchaseVendorAssign> EnquiryPurchaseVendorAssigns { get; set; } = new List<EnquiryPurchaseVendorAssign>();

    }
}
