using V.SMART.Shared.Utility_Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourInvoiceVM
{
    public class PoTaxDetailVM
    {
        public decimal? Rate { get; set; }
        public decimal? LineDiscountPercent { get; set; }

        public decimal? LineDiscountAmount { get; set; }

        public decimal? LineCGSTRate { get; set; }

        public decimal? LineSGSTRate { get; set; }

        public decimal? LineIGSTRate { get; set; }

        public bool IsItemWiseDiscAmtOrPerReq { get; set; } = false;  // true for amount, false for percent
        public bool DiscAmtOrPer { get; set; } = true;

      
        public decimal DiscountPercent { get; set; }

      
        public decimal DiscountAmount { get; set; }

        public decimal TotalBasicAmount { get; set; }

      
        public decimal FreightCharges { get; set; }

        public bool PackingAmtOrPer { get; set; } = true;

      
        public decimal PackingPercent { get; set; }

      
        public decimal PackingCharges { get; set; }

        public bool InsuranceAmtOrPer { get; set; } = true;

       
        public decimal InsurancePercent { get; set; }

      
        public decimal InsuranceCharges { get; set; }

        public decimal CGstRate { get; set; }

      
        public decimal SGstRate { get; set; }

        public decimal IGstRate { get; set; }

     
        public decimal TotalCGSTAmount { get; set; }

     
        public decimal TotalSGSTAmount { get; set; }

      
        public decimal TotalIGSTAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Other charges must be non-negative.")]
        public decimal OtherCharges { get; set; }

        public bool TCSAmtOrPer { get; set; } = true;

        [Range(0, 100, ErrorMessage = "TCS percent must be between 0 and 100.")]
        public decimal TCSPercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "TCS amount must be non-negative.")]
        public decimal TCSAmount { get; set; }

    }
}
