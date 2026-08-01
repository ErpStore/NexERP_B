using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.Data.Planning.AssyJobOrder
{
    public class JobOrder
    {
        [Key]
        public int JobId { get; set; }

        public bool MfgORLab { get; set; } = true;  // Mfg = true, Lab = false
        public bool IsManual { get; set; }

        [Required]
        [StringLength(10)]
        public string JobNo { get; set; }

        [Required]
        [StringLength(10)]
        public string Suffix { get; set; }

        [Required]
        public DateTime JobDate { get; set; } = DateTime.Now;

        public int? CustId { get; set; }
        [ForeignKey(nameof(CustId))]
        public Customer? Customer { get; set; }
        public int? RefPoId { get; set; }

        [ForeignKey (nameof(RefPoId))]
        public MfgPo? MfgPo { get; set; }

        public int? RefPoSubId { get; set; }
        [ForeignKey(nameof(RefPoSubId))]
        public MfgPoSub? MfgPoSub { get; set; }

        [Precision(18, 3)]
        public decimal POQty { get; set; }

        [Precision(18, 3)]
        public decimal JobOrderQty { get; set; }

        [Precision(18, 3)]
        public decimal JobBalQty { get; set; }

        public bool JobTally { get; set; } = false;

        [MaxLength(300)]
        public string? Remark { get; set; }

        public int? AssyId { get; set; }
        [ForeignKey (nameof(AssyId))]
        public Item? AssyItem { get; set; }

        [Precision(18,3)]
        public decimal UtilQty { get; set; }


        public bool Cancel { get; set; } = false;
        [StringLength(300)]
        public string? CancelReason { get; set; }


        public int? ParentJobId { get; set; }

        [ForeignKey(nameof(ParentJobId))]
        public JobOrder? ParentJobOrder { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int? StaffID { get; set; }

        [ForeignKey(nameof(StaffID))]
        public Staff? Staff { get; set; }
        public ICollection<JobOrderSub> JobOrderSubs { get; set; }
    }

}
