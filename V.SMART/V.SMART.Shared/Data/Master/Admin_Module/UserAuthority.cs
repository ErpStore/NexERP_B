using V.SMART.Shared.Data.Master.Admin;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.Admin_Module
{
    public class UserAuthority
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "User Id is required")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
         public virtual User? User { get; set; }


        // Sales Quotation
        public bool IsMfgQuote { get; set; } = false;
        public string? IsMfgQuoteLevel { get; set; }

        //Purchase Requisition 
        public bool IsPR { get; set; } = false;
        public string? IsPRLevel { get; set; }

        // Purchase PO
        public bool IsPO { get; set; } = false;
        public string? IsPOLevel { get; set; }

        //Purchase SCN
        public bool IsPurchSCN { get; set; } = false;
        public string? IsPurchSCNLevel { get; set; }

        //Sub-Con SCN
        public bool IsSubConSCN { get; set; } = false;
        public string? IsSubConSCNLevel { get; set; }

        //Production Assembly SCN
        public bool IsProdAssySCN { get; set; } = false;
        public string? IsProdAssySCNLevel { get; set; }

        //Production Component SCN
        public bool IsProdCompSCN { get; set; } = false;
        public string? IsProdCompSCNLevel { get; set; }

        //Labour SCN
        public bool IsLabourSCN { get; set; } = false;
        public string? IsLabourSCNLevel { get; set; }

        //Leave Application
        public bool IsLeave { get; set; } = false;
        public string? IsLeaveLevel { get; set; }

        // Route-Card
        public bool IsRc { get; set; } = false;
        public string? IsRcLevel { get; set; }

        //Sales Po
        public bool IsSalesPo { get; set; } = false;
        public string? IsSalesPoLevel { get; set; }


        // ===== Material Issue Request  =====
        public bool IsStockReq { get; set; } = false;
        public string? IsStockReqLevel { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; } = null;

    }
}
