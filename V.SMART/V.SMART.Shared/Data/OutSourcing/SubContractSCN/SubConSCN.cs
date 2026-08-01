using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.OutSourcing.SubContractSCN
{
    public class SubConSCN
    {
        [Key]
        public int SCNId { get; set; }

        [Required, MaxLength(50)]
        public string SCNNo { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Suffix { get; set; } = string.Empty;

        [Required]
        public DateTime SCNDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime SCNDateNow { get; set; } = DateTime.Now;

        [MaxLength(250)]
        public string? MainRemark { get; set; }


        [Required]
        public int? VendorCode { get; set; }

        [ForeignKey(nameof(VendorCode))]
        public virtual Vendor? vendor { get; set; }
        public int? AddStoreId { get; set; }
        [ForeignKey(nameof(AddStoreId))]
        public virtual Store? AddStore { get; set; }
        public int? IssueStoreId { get; set; }
        [ForeignKey(nameof(IssueStoreId))]
        public virtual Store? IssueStore { get; set; }

        public bool SCNCancel { get; set; } = false;
        public DateTime? CancelDate { get; set; }
        public string? CanceledBy { get; set; } = string.Empty;
        public bool ShortClose { get; set; } = false;

        public bool SCNTally { get; set; } = false;

        [MaxLength(500)]
        public string? MainRemarks { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        [MaxLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }


        [MaxLength(50)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<SubConSCNSub>? SubConSCNSubs { get; set; }
    }
}
