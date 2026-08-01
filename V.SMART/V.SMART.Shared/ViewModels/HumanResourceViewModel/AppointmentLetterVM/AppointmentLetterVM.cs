using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.HumanResourceViewModel.AppointmentLetterVM
{
    public class AppointmentLetterVM
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = "Offer Number is required.")]
        [StringLength(50, ErrorMessage = "Offer Number cannot exceed 50 characters.")]
        public string AppointmentNo { get; set; } = string.Empty;


        [Required(ErrorMessage = "Suffix is required.")]
        public string Suffix { get; set; }

        public DateTime AppointmentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Candidate is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Candidate.")]
        public int CandidateID { get; set; }

        public bool IsSelected { get; set; } = false;
        public string? Candidatename { get; set; }
        public string? Address { get; set; }
        public string? Position { get; set; }

        [MaxLength]
        public string? Introduction { get; set; }

        [MaxLength]
        public string? ClosingNotes { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int OfferId { get; set; }
        public int OfferNo { get; set; }


        public List<AppointmentLetterSubVM> AppointmentLetterSubVM { get; set; } = new();
    }
}
