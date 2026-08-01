using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.Fesibility
{
    public class EnquiryFeasibilitySub
    {
        [Key]
        public int FesSubId { get; set; }

        [Required]
        public int FesId { get; set; }
        [ForeignKey(nameof(FesId))]
        public EnquiryFeasibility EnquiryFeasibility { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Serial number must be greater than zero.")]
        public int SlNo { get; set; }

        [Required(ErrorMessage = "Item code/Part No. should not be empty.")]
        public int? ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        public string? TypeOfProject { get; set; }

        public string? BaseMaterial { get; set; }

        public string? ToolRequired { get; set; }

        public string? SpclReqOutsource { get; set; }

        public string? TechCapaBilities { get; set; }

        public string? DelvryTrms { get; set; }

        public decimal ManHrReq { get; set; }

        public string? RiskIfAny { get; set; }

        public string? SpclReqCustomer { get; set; }

        public string? Remarks { get; set; }

        public string? CompbltyConfm { get; set; }

        public decimal StockAvail { get; set; }
    }

}
