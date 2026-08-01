using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.HumanResource.AppointmentLetter
{
    public class AppointmetLetterSub
    {
        [Key]
        public int AppointmentSubId { get; set; }

        [Required]
        public int AppointmentId { get; set; }
        [ForeignKey(nameof(AppointmentId))]
        public virtual AppointmentLetter AppointmentLetter { get; set; }

        [Required]
        public int SlNo { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength]
        public string Description { get; set; }
    }
}
