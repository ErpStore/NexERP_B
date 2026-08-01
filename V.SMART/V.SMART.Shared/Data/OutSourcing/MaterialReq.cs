using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Pages.Master_Module_pages.Store_Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.OutSourcing
{
    public class MaterialReq
    {
        [Key]
        public int MreqId { get; set; }

        public bool IsPurchOrSubCon { get; set; } = true;
        public bool IsManual { get; set; } = true;

        [Required, StringLength(20)]
        public string MReqNo { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string Suffix { get; set; } = string.Empty;

        public DateTime MreqDate { get; set; } = DateTime.Now;
        public DateTime MreqDateNow { get; set; } = DateTime.Now;

        public int NoOfItems { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        public bool MreqCancel { get; set; } = false;
        [StringLength(250)]
        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }
        public bool ShortClose { get; set; } = false;

        public bool IsLevel1 { get; set; } = false;
        public bool IsLevel2 { get; set; } = false;
        public bool IsLevel3 { get; set; } = false;

        public bool IsAuthorized { get; set; } = false;

        public bool IsRejected { get; set; } = false;
        [StringLength(250)]
        public string? RejectReason { get; set; }

        [StringLength(100)]
        public string? Level1Sign { get; set; }
        [StringLength(100)]
        public string? Level2Sign { get; set; }
        [StringLength(100)]
        public string? Level3Sign { get; set; }

        [StringLength(150)]
        public string? KindAttn { get; set; }

        [StringLength(100)]
        public string? LastApproved { get; set; }
        public DateTime? ApprovedDate { get; set; }

        [StringLength(500)]
        public string? MainRemark { get; set; }

        public bool PRTally { get; set; } = false;

        // Audit Fields
        [StringLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Property
        public virtual ICollection<MaterialReqSub> MaterialReqSubs { get; set; } = new List<MaterialReqSub>();
    }


}
