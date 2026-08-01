using V.SMART.Shared.Data.Master.General;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry
{
    public class EnquirySales
    {
        [Key]
        public int EnquiryId { get; set; }

        [StringLength(10, ErrorMessage = "Prefix cannot exceed 10 characters.")]
        public string? Prefix { get; set; }

        [Required(ErrorMessage = "Enquiry No. is required.")]
        [StringLength(30, ErrorMessage = "Enquiry No. cannot exceed 30 characters.")]
        public string EnquiryNo { get; set; }

        [Required(ErrorMessage = "Suffix is required.")]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "Enquiry Date is required.")]
        public DateTime EnquiryDate { get; set; } = DateTime.Now;
        public DateTime EnquiryDateNow { get; set; } = DateTime.Now;


        [Required(ErrorMessage = "Customer is required.")]
        public int CustId { get; set; }

        [ForeignKey(nameof(CustId))]
        public Customer Customer { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "No of items must be a valid number.")]
        public int NoOfItems { get; set; }

        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? MainRemarks { get; set; }

        [StringLength(80, ErrorMessage = "Kind Of Attention cannot exceed 80 characters.")]
        public string? KindOfAttention { get; set; }

        [Precision(18, 4)]
        [Range(0, double.MaxValue, ErrorMessage = "Total value must be a positive number.")]
        public decimal TotalValue { get; set; }

        public bool EnquiryTally { get; set; } = false;

        [Required(ErrorMessage ="please select either Manufacturing or Labour")]
        public bool MfgORLab { get; set; } = true;


        public bool Cancel { get; set; }
        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }


        public bool ShortClose { get; set; }


        [StringLength(50)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [StringLength(80, ErrorMessage = "POC Name cannot exceed 80 characters.")]
        public string? POCName { get; set; }

        [Phone(ErrorMessage = "Invalid contact number.")]
        public string? POCContactNo { get; set; }

        public DateTime? ExpectedReplyDate { get; set; }

        public virtual ICollection<EnquirySalesSub> EnquirySalesSub { get; set; } = new List<EnquirySalesSub>();
    }



}
