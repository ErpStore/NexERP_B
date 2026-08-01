using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.HumanResource.OfferLetter;

namespace V.SMART.Shared.Data.HumanResource.AppointmentLetter
{
    public class AppointmentLetter
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public string AppointmentNo { get; set; } = string.Empty;

        [Required]
        public string Suffix { get; set; }

        public DateTime AppointmentDate { get; set; }

        [Required]
        public int? CandidateID { get; set; }

        [ForeignKey(nameof(CandidateID))]
        public virtual Candidate? Candidate { get; set; }


        [StringLength(50)]
        public string? Position { get; set; }
        [MaxLength]
        public string? Introduction { get; set; }

        [MaxLength]
        public string? ClosingNotes { get; set; }

        [Required]
        [StringLength(50)]
        public string? CreatedBy { get; set; }

        [Required]
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<AppointmetLetterSub> AppointmetLetterSubs { get; set; } = new List<AppointmetLetterSub>();

    }
}
