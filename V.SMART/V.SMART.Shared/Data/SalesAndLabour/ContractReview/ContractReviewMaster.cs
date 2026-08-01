using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.ContractReview
{
    public class ContractReviewMaster
    {
        [Key]
        public int SlNo { get; set; }

        [Required, StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }
}
