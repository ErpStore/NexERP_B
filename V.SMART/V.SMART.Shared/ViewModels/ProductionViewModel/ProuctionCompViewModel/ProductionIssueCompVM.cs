using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel
{
    public class ProductionIssueCompVM
    {
        public int IssueId { get; set; }

        [Required, MaxLength(50)]
        public string IssueNo { get; set; } = string.Empty;

        [Required]
        public string Suffix { get; set; } = string.Empty;

        [Required]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime IssueDateNow { get; set; } = DateTime.Now;

        public int? CustId { get; set; }
        public string? CustName { get; set; }

        [Required, MaxLength(100)]
        public string IssueToWhom { get; set; } = string.Empty;

        [Required, Range(1, int.MaxValue)]
        public int? StoreIssId { get; set; }
        public string? StoreIssName { get; set; }

        public int NoOfItems => ProductionIssueCompSubVMs?.Count ?? 0;


        [MaxLength(300)]
        public string? MainRemark { get; set; }

        public bool IssueTally { get; set; }


        // Audit
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public bool IsWithoutPoDc { get; set; }

        public bool IsOutItemSameAsInItem { get; set; } = false;

        public bool Cancel { get; set; }

        public bool ShortClose { get; set; }

        [StringLength(250, ErrorMessage = "Cancel reason cannot exceed 250 characters.")]
        public string? CancelReason { get; set; }

        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }

        public bool IsSelected { get; set; }
        public bool IsManual { get; set; } = false;


        [MinLength(1)]
        [ValidateComplexType]
        public List<ProductionIssueCompSubVM> ProductionIssueCompSubVMs { get; set; } = new();
        
    }

}
