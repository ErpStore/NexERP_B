using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.HumanResourceViewModel.OfferLetter
{
    public class OfferLetterSubVM
    {
        
        public int OfferSubId { get; set; }

        public int OfferId { get; set; }

        public int SlNo { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public bool IsEditable { get; set; } = false;
    }
}
