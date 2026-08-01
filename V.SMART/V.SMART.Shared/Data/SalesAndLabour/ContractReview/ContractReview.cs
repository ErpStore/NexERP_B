
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.SalesAndLabour.ContractReview
{
    public class ContractReview
    {
        [Key]
        public int Id { get; set; }

        [StringLength(10)]
        public string? Prefix { get; set; } = string.Empty;

        [Required(ErrorMessage = "ContractReview Number is required.")]
        [StringLength(25)]
        public string ContractNo { get; set; } = string.Empty;

        [Required]
        [StringLength(12)]
        public string Suffix { get; set; }

        [Required()]
        public DateTime ContractDate { get; set; }= DateTime.Now;

        [Required]
        public DateTime ContractDateNow { get; set; }=DateTime.Now;

        [Required(ErrorMessage = "PO Number and PO ID are required.")]
        public int? PoId { get; set; }
        [ForeignKey(nameof(PoId))]
        public virtual MfgPo? MfgPo { get; set; }

        [Required(ErrorMessage = "Please select Revised By")]
        [StringLength(100)]
        public string RevisedBy { get; set; }

        [Required(ErrorMessage = "Approval is needed for this record to be accepted!")]
        [StringLength(100)]
        public string ApprovedBy { get; set; }
       

        public bool? ReviewResult { get; set; }

        public string ReviewJson { get; set; }

        [StringLength(30)]
        public string? OANo { get; set; }

        public DateTime? OADate { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public ICollection<ContractReviewSub> ContractReviewSubs { get; set; } = new List<ContractReviewSub>();
       
    }
}
