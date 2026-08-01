using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.Fesibility
{
    public class EnquiryFeasibility
    {
        [Key]
        public int FesId { get; set; }

        [Required(ErrorMessage = "Feasibility No. is required.")]
        [MaxLength(20)]
        [StringLength(20, ErrorMessage = "Feasibility No. cannot exceed 20 characters.")]
        public string FesNo { get; set; }
        public string? Suffix { get; set; }

        [Required(ErrorMessage = "Feasibility Date is required.")]
        public DateTime FesDate { get; set; } = DateTime.Now;
        public DateTime FesDateNow { get; set; } = DateTime.Now;


        public int RefEnqId { get; set; }
        [ForeignKey(nameof(RefEnqId))]
        public EnquirySales EnquirySales { get; set; }


        [MaxLength(20)]
        [Required(ErrorMessage = "RefEnquiry No. is required.")]
        public string RefEnqNo { get; set; }

        public DateTime RefEnqDate { get; set; }


        [Required(ErrorMessage = "Customer is required.")]
        public int CustId { get; set; }

        [ForeignKey(nameof(CustId))]
        public Customer Customer { get; set; }

        [MaxLength(500)]
        public string? Remark { get; set; }

        public bool CompatibilityConfm { get; set; }

        [MaxLength(50)]
        public string? QuotRefNo { get; set; }

        [Required(ErrorMessage = "please select PreparedBy? PreparedBy is required.")]
        [MaxLength(50)]
        public string PreparedBy { get; set; }

        [MaxLength(50)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<EnquiryFeasibilitySub> EnquiryFeasibilitySub { get; set; } = new List<EnquiryFeasibilitySub>();
    }


}
