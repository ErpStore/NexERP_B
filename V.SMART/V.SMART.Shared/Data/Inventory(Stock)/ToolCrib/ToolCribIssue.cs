using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Inventory_Stock_.ToolCrib
{
    public class ToolCribIssue
    {
        [Key]
        public int TCIssueId { get; set; }

        [Required(ErrorMessage = "ToolCrib Issue Number is Required!..")]
        [StringLength(20)]
        public string TCIssueNo { get; set; }

        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(20)]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "ToolCrib Issue Date is Required!..")]
        public DateTime TCIssueDate { get; set; } = DateTime.Now;
        public DateTime TCIssueDateNow { get; set; } = DateTime.Now;

        public bool TCIssueTally { get; set; } = false;


        public int? CategoryCode { get; set; }
        [ForeignKey(nameof(CategoryCode))]
        public Category? Category { get; set; }

        public int? StoreId { get; set; }
        [ForeignKey(nameof(StoreId))]
        public Store? Store { get; set; }


        [StringLength(100)]
        public string? IssuedToWhom { get; set; }
        public bool IsReturn { get; set; }

        [StringLength(300)]
        public string? MainRemarks { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }

        [Required]
        public int NoOfItems { get; set; }


        [StringLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        [StringLength(100)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<ToolCribIssueSub> ToolCribIssueSubs { get; set; } = new List<ToolCribIssueSub>();
    }
}
