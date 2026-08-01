using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.EWayModel
{
    public class EWayDocument
    {
        public int DcId { get; set; }

        public int? SubSupplyType { get; set; }
        public string? Prefix { get; set; }
        public string? SubSupplyDesc { get; set; }
        public string? DocType { get; set; } 
        public string? ModeofTrasnport { get; set; } 
        public string? Suffix { get; set; } 
        public string? SubSuppType { get; set; } 

       
   

        public string? CustGstNo { get; set; }
        public string? CustName { get; set; }
        public string? CustAddress { get; set; }
        public string? Location { get; set; }
        public string? PinCode { get; set; }
        public int? StateCode { get; set; }

        public decimal? TotVal { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? ItemDiscAmt { get; set; }

        public decimal? MainDiscount { get; set; }
        public bool? MainDiscAmtOrPer { get; set; }

        public decimal? FreightCharges { get; set; }
        public decimal? PandF { get; set; }
        public bool? PandFAmtOrPer { get; set; }

        public decimal? Insurance { get; set; }
        public bool? InsuranceAmtOrPer { get; set; }

        public decimal? Cgst { get; set; }
        public decimal? Sgst { get; set; }
        public decimal? Igst { get; set; }

        public decimal? ItemwiseCgst { get; set; }
        public decimal? ItemwiseSgst { get; set; }
        public decimal? ItemwiseIgst { get; set; }

        public decimal? TCS { get; set; }
        public bool TCSAmtOrPer { get; set; }

        public decimal? OtherCharges { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? HSNCode { get; set; }

        public decimal? Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? MeasureUnit { get; set; }

        public string? EwayTransId { get; set; }
        public string? EwayTransName { get; set; }
        public string? EwayTransDocNo { get; set; }

        public string? TransportMode { get; set; }
        public decimal? Distance { get; set; }
        public string? VehicleNo { get; set; }

        public int? CustId { get; set; }
        public int DcSubId { get; set; }
        public int? ItemId { get; set; }
        public bool RoffSales { get; set; }

        //For Invoice

        public int? Id { get; set; }
        public int SubId { get; set; }
        public string? DocNo { get; set; }
        public DateTime DocDate { get; set; }

        public bool Selected { get; set; } = false;

    }
}
