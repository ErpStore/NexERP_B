using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ContractReviewVM
{
    public class ContractReviewVM
    {
        
        public int Id { get; set; }

        [StringLength(10)]
        public string? Prefix { get; set; } = string.Empty;

        [Required(ErrorMessage = "ContractReview Number and ID are required.")]
        [StringLength(25)]
        public string ContractNo { get; set; } = string.Empty;

        [Required]
        [StringLength(12)]
        public string Suffix { get; set; }

        [Required]
        public DateTime ContractDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime ContractDateNow { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "PO Number and PO ID are required.")]
        public int? PoId { get; set; }
        public string PoNo { get; set; }
        public string? CustName { get; set; }
        public string? CustAddress { get; set; }
        public string? CustGst { get; set; }
        public string? CustContactNo { get; set; }

        [Required(ErrorMessage = "ContractReview Number and ID are required.")]

        public string RevisedBy { get; set; }

        [Required(ErrorMessage = "Approval is needed for this record to be accepted!")]
        public string ApprovedBy { get; set; }


        public bool? ReviewResult { get; set; }

        public string ReviewJson { get; set; }

    
        public string? OANo { get; set; }

        public DateTime? OADate { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ValidateComplexType]

        public List<ContractReviewSubVM> ContractReviewSubVMS { get; set; } = new List<ContractReviewSubVM>();

    }
}
