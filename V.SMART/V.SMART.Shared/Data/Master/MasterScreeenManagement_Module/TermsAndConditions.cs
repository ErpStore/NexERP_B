using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Master.MasterScreeenManagement_Module
{
    public class TermsAndConditions
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage="Title is Required!..")]
        [StringLength (50, ErrorMessage = "Title cannot be exceed 50 Characters")]
        public string? Title { get; set; }
        [Required(ErrorMessage = "Details is Required!..")]
        public string? Details { get; set; }
        public bool IsActive { get; set; } = true;
        public string ? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; } = null;

    }
}
