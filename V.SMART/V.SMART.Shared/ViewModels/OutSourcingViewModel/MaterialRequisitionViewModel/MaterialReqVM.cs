using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.MaterialRequisitionViewModel
{
    public class MaterialReqVM
    {
        public int MreqId { get; set; }

        public bool IsPurchOrSubCon { get; set; } = true;
        public bool IsManual { get; set; } = true;

        [Required(ErrorMessage = "Material Request No is required.")]
        [StringLength(20, ErrorMessage = "Material Request No must be between 3 and 20 characters.")]
        public string MReqNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(10, ErrorMessage = "Suffix must be between 1 and 10 characters.")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "Material Request Date is required.")]
        public DateTime MreqDate { get; set; } = DateTime.Now;

        public DateTime MreqDateNow { get; set; } = DateTime.Now;

        public int NoOfItems => MaterialReqSubVMs?.Count ?? 0;

        [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
        public string? Department { get; set; }

        public int? StoreId { get; set; }

        public string? StoreName { get; set; }

        public bool MreqCancel { get; set; }

        [StringLength(250, ErrorMessage = "Cancel reason cannot exceed 250 characters.")]
        public string? CancelReason { get; set; }

        public DateTime? CancelDate { get; set; }

        [StringLength(100, ErrorMessage = "Cancelled By cannot exceed 50 characters.")]
        public string? CancelBy { get; set; }

        public bool ShortClose { get; set; }
        public bool IsLevel1 { get; set; }
        public bool IsLevel2 { get; set; }
        public bool IsLevel3 { get; set; }

        public bool IsAuthorized { get; set; }
        public bool IsRejected { get; set; }

        public string? RejectReason { get; set; }

        public string? Level1Sign { get; set; }

        public string? Level2Sign { get; set; }

        public string? Level3Sign { get; set; }

        [StringLength(100, ErrorMessage = "Kind Attn cannot exceed 100 characters.")]
        public string? KindAttn { get; set; }

        [StringLength(100, ErrorMessage = "Last Approved By cannot exceed 100 characters.")]
        public string? LastApproved { get; set; }

        public DateTime? ApprovedDate { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? MainRemark { get; set; }

        public bool PRTally { get; set; }

        [StringLength(100, ErrorMessage = "Created By cannot exceed 50 characters.")]
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100, ErrorMessage = "Modified By cannot exceed 50 characters.")]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [ValidateComplexType]
        [MinLength(1, ErrorMessage = "At least one PR/Indent item is required.")]
        public List<MaterialReqSubVM> MaterialReqSubVMs { get; set; } = new();
    }


}
