using V.SMART.Shared.Data.SalesAndLabour.ContractReview;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ContractReviewVM
{
    public class ContractReviewSubVM
    {
       
        public int SubId { get; set; }
        public int Id { get; set; }
        
        public int SlNo { get; set; }

        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Specification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }
        public decimal? Weight { get; set; }

        public string? Category { get; set; }
        public string? CategoryName { get; set; }

        public string? Issue { get; set; } = string.Empty;

        public decimal Qty { get; set; }

        public decimal UnitPrice { get; set; }
       
        public decimal? LineCGSTRate { get; set; }

        public decimal? LineSGSTRate { get; set; }

        public decimal? LineIGSTRate { get; set; }

        public string? Remarks { get; set; }

        public string? FurtherRevision { get; set; }

        public string? DeliveryTerms { get; set; }

        public string? LeadTime { get; set; }

        public string? ModeOfTransport { get; set; }

        public string Type { get; set; } = string.Empty;

        public string? CustomerSplReq { get; set; }

        public string? RiskOrderAccept { get; set; }
    }
}
