
using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.SalesAndLabour.ContractReview
{
    public class ContractReviewSub
    {
        [Key]
        public int SubId { get; set; }

        [Required]
        public int Id { get; set; }   
        [ForeignKey(nameof(Id))]
        public virtual ContractReview ContractReview { get; set; }

        [Required]
        public int SlNo { get; set; }

        [Required]
        public int ItemId { get; set; }   
        [ForeignKey(nameof(ItemId))]
        public virtual Item Item { get; set; }

        [StringLength(50)]
        public string? Issue { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999, ErrorMessage = "Qty must be greater than 0")]
        public decimal Qty { get; set; }

        [Required]
        [Precision(18, 4)]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Precision(18, 4)]
        public decimal? LineCGSTRate { get; set; }

        [Precision(18, 4)]
        public decimal? LineSGSTRate { get; set; }

        [Precision(18, 4)]
        public decimal? LineIGSTRate { get; set; }

        [StringLength(200)]
        public string? Remarks { get; set; }

        [StringLength(100)]
        public string? FurtherRevision { get; set; }

        [StringLength(100)]
        public string? DeliveryTerms { get; set; }

        [StringLength(100)]
        public string? LeadTime { get; set; }

        [StringLength(100)]
        public string? ModeOfTransport { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? CustomerSplReq { get; set; }

        [StringLength(100)]
        public string? RiskOrderAccept { get; set; }
    }
}
