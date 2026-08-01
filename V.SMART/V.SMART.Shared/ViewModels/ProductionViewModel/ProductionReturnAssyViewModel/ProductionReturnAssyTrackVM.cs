using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel
{
    public class ProductionReturnAssyTrackVM
    {
        public int TrackId { get; set; }

        [Required]
        public int RefReturnId { get; set; }

        public int? RefIssueId { get; set; }
        public string? RefIssueNo { get; set; }

        public int? RefJobOrderId { get; set; }
        public string? RefJobNo { get; set; }

        public int? RefIssueSubId { get; set; }

        public int? ReturnItemId { get; set; }

        [Precision(18, 3)]
        public decimal? ReturnQty { get; set; }

        public int? IssueItemId { get; set; }

        [Precision(18, 3)]
        public decimal? IssueQty { get; set; }

        [MaxLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

}
