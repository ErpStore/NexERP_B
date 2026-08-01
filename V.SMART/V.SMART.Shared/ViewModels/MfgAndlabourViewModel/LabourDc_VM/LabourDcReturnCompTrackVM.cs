using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourDc_VM
{
    public class LabourDcReturnCompTrackVM
    {
        public int TrackId { get; set; }

        [Required]
        public int? RefDcSubId { get; set; }

        public int? RefSCNSubId { get; set; }
        public string? SCNNo { get; set; }

        public int? RefGRNSubId { get; set; }
        public string? GRNNo { get; set; }

        public int? RefPoSubId { get; set; }
        public string? PoNo { get; set; }

        public int? ItemIdIn { get; set; }

        [Precision(18, 3)]
        public decimal? QtyIn { get; set; }

        public int? ItemIdOut { get; set; }

        [Precision(18, 3)]
        public decimal? QtyOut { get; set; }

        [MaxLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
