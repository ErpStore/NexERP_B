using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Production.ProductionIssueWOAssy
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    public class ProductionIssueAssy
    {
        [Key]
        public int IssueId { get; set; }

        [Required, MaxLength(50)]
        public string IssueNo { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Suffix { get; set; }

        [MaxLength(10)]
        public string? DepartmentCode { get; set; }

        [MaxLength(5)]
        public string? MonthCode { get; set; }

        [Required]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime IssueDateNow { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? IssueToWhom { get; set; }

        [Required]
        public int StoreIssId { get; set; }  
        [ForeignKey(nameof(StoreIssId))]
        public virtual Store StoreIssue { get; set; }

        [Required]
        public int NoOfItems { get; set; }

        [MaxLength(300)]
        public string? MainRemark { get; set; }

        public bool IssueTally { get; set; } = false;


        [Precision(18, 3)]
        public decimal IssueQty { get; set; }

        [Required, MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<ProductionIssueAssySub> ProductionIssueAssySubs { get; set; }

    }

}
