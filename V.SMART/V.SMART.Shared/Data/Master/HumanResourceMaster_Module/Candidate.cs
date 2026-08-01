using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Master.HumanResourceMaster_Module
{
    public class Candidate
    {
        [Key]
        public int CandidateID { get; set; }

        [Required, StringLength(150)]
        public string? CandidateName { get; set; }


        [Required]
        public DateTime? DOB { get; set; }

        [Required, StringLength(25)]
        public string? Gender { get; set; }

        [Required, StringLength(13)]
        public string? PhoneNo { get; set; }

        [StringLength(50)]
        public string? Gmail { get; set; }

        [Required, StringLength(250)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? Qualification { get; set; }

        public float Experience { get; set; }

        [StringLength(500)]
        public string? Skills { get; set; }

        [Required, StringLength(20)]
        public string? Source { get; set; }

        [StringLength(500)]
        public string? Notice_Period { get; set; }

        [Precision(18, 4)]
        [Range(0, double.MaxValue)]
        public decimal Current_Salary { get; set; }

        [Precision(18, 4)]
        [Range(0, double.MaxValue)]
        public decimal Expected_Salary { get; set; }

        [Required, StringLength(150)]
        public string? Position { get; set; }

        [Required, StringLength(150)]
        public string? Department { get; set; }


        [StringLength(20)]
        public string? Status { get; set; }

        [StringLength(40)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(40)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
