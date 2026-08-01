using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Master.Rejection_Module
{
    public class RejectionMaster
    {
        [Key]
        public int RejectionId { get; set; }

        [Required]
        [StringLength(250)]
        public string? RejectionCode { get; set; }

   
        [StringLength(250)]
        public string? RejectionDescription { get; set; }

        public bool IsActive { get; set; } = true;

        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
