using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM
{
    public class PurchaseSCNVM
    {
        public int SCNId { get; set; }

        public int? VendorCode { get; set; }
        public string? VendorName { get; set; }
        public string? VendorAddress { get; set; }
        public string? VendorGstNo { get; set; }
        public string? VendorContactNo { get; set; }
        public string? KindofAttention { get; set; }

        [Required(ErrorMessage = "SCN Number is required.")]
        [StringLength(15, ErrorMessage = "SCN No Length cannot be exceed 15 Characters")]
        public string SCNNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required.")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "SCN Date is required.")]
        public DateTime SCNDate { get; set; } = DateTime.Now;
        public DateTime SCNDateNow { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Please select  StoreAdd")]
        public int? AddStoreId { get; set; }
        public string? AddStoreName { get; set; }

        [Required(ErrorMessage = "Please select StoreIssue")]
        public int? StoreIssId { get; set; }
        public string? IssueStoreName { get; set; }

        [MaxLength(250, ErrorMessage = "Remark Length cannot be exceed 250 Characters")]
        public string? MainRemarks { get; set; }

        public int NoOfItems => PurchaseSCNSubVMs.Count;

        public bool SCNCancel { get; set; } = false;
        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public  string? CancelBy { get; set; }


        public bool SCNTally { get; set; } = false;
        public bool ShortClose { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }


        public bool IsLevel1 { get; set; } = false;
        public bool IsLevel2 { get; set; } = false;
        public bool IsLevel3 { get; set; } = false;
        public bool IsLevel4 { get; set; } = false;

        public string? Level1Sign { get; set; }
        public string? Level2Sign { get; set; }
        public string? Level3Sign { get; set; }
        public string? Level4Sign { get; set; }


        public bool Authorized { get; set; } = false;
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }


        public bool IsRejected { get; set; } = false;
        public string? RejectReason { get; set; }

        // ---------------- Calculated Totals ----------------
        public decimal TotalQty => PurchaseSCNSubVMs?.Sum(x => x.AcceptQty ?? 0m) ?? 0m;

        public decimal AverageUnitPrice => PurchaseSCNSubVMs != null && PurchaseSCNSubVMs.Count > 0
            ? PurchaseSCNSubVMs.Average(x => x.UnitPrice ?? 0m)
            : 0m;

        [MinLength(1, ErrorMessage = "At least one SCN item is required.")]
        [ValidateComplexType]
        public List<PurchaseSCNSubVM> PurchaseSCNSubVMs { get; set; } = new List<PurchaseSCNSubVM>();

    }
}
