using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.PurchaseAndSubcontract.Purchase_Quote
{
    public class PurchaseQuoteSub
    {
        [Key]
        public int QuoteSubId { get; set; }     
        
        public int QuoteId { get; set; }
        [ForeignKey(nameof(QuoteId))]
        public PurchaseQuote PurchaseQuote { get; set; }

        public int SlNo { get; set; }

        [Required(ErrorMessage = "Please select ItemCode")]
        public int? ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public CostCenter? CostCenter { get; set; }

        [Required(ErrorMessage = "Please Enter the Quantity")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        [Precision(18, 3)]
        public decimal Qty { get; set; } 
        public decimal BalQty { get; set; }

        [Required(ErrorMessage = "Please Enter the Price")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }


        public int? RefEnqVendorAssignId { get; set; }
        [ForeignKey(nameof(RefEnqVendorAssignId))]
        public EnquiryPurchaseVendorAssign? EnquiryPurchaseVendorAssign { get; set; }

        public bool QuoteTally { get; set; } = false;
        public bool ItemCancel { get; set; } = false;
        public bool ItemShortClose { get; set; } = false;

        public string? ItemCancelReason { get; set; }
        public decimal? LineDiscountPercent { get; set; } 
        public decimal? LineDiscountAmount { get; set; }  
        public decimal LineBasicAmount { get; set; }     

        public decimal? LineCGSTRate { get; set; }        
        public decimal? LineSGSTRate { get; set; }        
        public decimal? LineIGSTRate { get; set; }        

        public string? Remarks { get; set; }

    }
}
