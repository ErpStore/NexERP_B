using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry
{
    public class EnquirySalesSub
    {
        [Key]
        public int EnquirySubId { get; set; }

        [Required]
        public int EnquiryId { get; set; }

        [ForeignKey(nameof(EnquiryId))]
        public EnquirySales EnquirySales { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Serial number must be greater than zero.")]
        public int SlNo { get; set; }

        [Required(ErrorMessage = "Please select an Item. Item code should not be empty.")]
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; }

        [Required(ErrorMessage = "Unit of Measurement is Required")]
        [StringLength(20, ErrorMessage = "UOM cannot exceed 20 characters.")]
        public string UOM { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        [Precision(18, 3)]
        public decimal Qty { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "Balance quantity cannot be negative.")]
        public decimal BalQty { get; set; }

        public bool ItemCancel { get; set; }
        public string? ItemCancelReason { get; set; }

        public decimal UnitPrice { get; set; }

        [StringLength(250, ErrorMessage = "Remark should not exceed 250 characters.")]
        public string? Remark { get; set; }

        public int? CostCenterId { get; set; }

        [ForeignKey(nameof(CostCenterId))]
        public CostCenter? CostCenter { get; set; }

        public bool IsEstiItemTally { get; set; }
    }


}
