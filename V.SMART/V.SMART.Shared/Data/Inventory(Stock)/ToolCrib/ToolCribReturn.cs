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
    public class ToolCribReturn
    {
        [Key]
        public int TCReturnId { get; set; }

        [Required]
        public string TCReturnNo { get; set; }

        [Required]
        public DateTime TCReturnDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime TCReturnDateNow { get; set; } = DateTime.Now;

        [Required]
        public string Suffix { get; set; }

        [Required]
        [StringLength(100)]
        public String FromWhom { get; set; }

        [Required]
        public int NoOfItems { get; set; }

        [StringLength(300)]
        public string? MainRemarks { get; set; }

        [StringLength(300)]
        public string? BreakageRemarks { get; set; }

        public int? StoreId { get; set; }
        [ForeignKey(nameof(StoreId))]
        public Store? Store { get; set; }


        [StringLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }


        [StringLength(100)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }


        [MinLength(1, ErrorMessage = "At least one toolcribreturn item is required.")]
        [ValidateComplexType]
        public virtual ICollection<ToolCribReturnSub> ToolCribReturnSubs { get; set; } = new List<ToolCribReturnSub>();
    }
}
