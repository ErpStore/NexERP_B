using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.FeasibilityVM
{
    public class EnquiryFeasibilityVM
    {
        public int FesId { get; set; }

        [Required(ErrorMessage = "Feasibility No. is required.")]
        [StringLength(20, ErrorMessage = "Feasibility No. cannot exceed 20 characters.")]
        public string FesNo { get; set; }
        public string? Suffix { get; set; }

        [Required(ErrorMessage = "Feasibility Date is required.")]
        public DateTime FesDate { get; set; } = DateTime.Now;
        public DateTime FesDateNow { get; set; } = DateTime.Now;

        public int RefEnqId { get; set; }
        [Required(ErrorMessage = "RefEnquiry No. is required.")]
        public string RefEnqNo { get; set; }

        public DateTime RefEnqDate { get; set; }


        [Required(ErrorMessage = "Customer is required.")]
        public int CustId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerAddress { get; set; }

        public string CustomerPhoneNo { get; set; }
        public string CustomerGSTNO { get; set; }


        public string? Remark { get; set; }

        public bool CompatibilityConfm { get; set; }


        public string? QuotRefNo { get; set; }

        [Required(ErrorMessage = "please select PreparedBy? PreparedBy is required.")]
        [MaxLength(50)]
        public string PreparedBy { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;


        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [MinLength(1, ErrorMessage = "At least one EnquiryFeasibility item is required.")]
        [ValidateComplexType]
        public List<EnquiryFeasibilitySubVM> EnquiryFeasibilitySubVM { get; set; } = new();
    }

}
