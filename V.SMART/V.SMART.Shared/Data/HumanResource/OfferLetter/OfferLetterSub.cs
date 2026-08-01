using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.HumanResource.OfferLetter
{
    public class OfferLetterSub
    {
        [Key]
        public int OfferSubId { get; set; }

        [Required]
        public int OfferId { get; set; }
        [ForeignKey(nameof(OfferId))]
        public virtual OfferLetter OfferLetter { get; set; }

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
