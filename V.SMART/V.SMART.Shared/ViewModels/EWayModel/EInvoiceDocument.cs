using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.EWayModel
{
    public class EInvoiceDocument
    {
       
        public string? InvType { get; set; }
        public string? Prefix { get; set; }
        public int? InvId { get; set; }
        public int? InvSubId { get; set; }
        public string? DocNo { get; set; }
        public string? Suffix { get; set; }
        public DateTime? DocDate { get; set; }
        public decimal? TodayVal { get; set; }


        public int? CustId { get; set; }
        public string? CustName { get; set; }
        public string? CustAddress { get; set; }
        public string? CustGstNo { get; set; }
        public string? Location { get; set; }
        public string? PinCode { get; set; }
        public int? StateCode { get; set; }
        public string? Email { get; set; }
        public string? CustSupType { get; set; }


        public decimal TotVal { get; set; }
        public decimal MainDiscount { get; set; }
        public bool MainDiscAmtOrPer { get; set; }

        public decimal FreightCharges { get; set; }

       
        public bool PandFAmtOrPer { get; set; }
        public decimal PandF { get; set; }
        public decimal PackingCharges { get; set; }


        public decimal Insurance { get; set; }
        public bool InsuranceAmtOrPer { get; set; }
        public decimal Insurancecharges { get; set; }

        public decimal OtherCharges { get; set; }

    
        public decimal Cgst { get; set; }
        public decimal Sgst { get; set; }
        public decimal Igst { get; set; }
        
        public bool TCSAmtOrPer { get; set; }
        public decimal TCS { get; set; }
        public decimal TCSCharges { get; set; }

        public decimal? Distance { get; set; }
        public string? TransportMode { get; set; }
        public string? VehicleNo { get; set; }
        public string? EwayTransId { get; set; }
        public string? EwayTransName { get; set; }
        public string? EwayTransDocNo { get; set; }


        public decimal? TotalAmount { get; set; }
        public decimal? ItemDiscAmt { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? HSNCode { get; set; }
        public string? MeasureUnit { get; set; }
        public string? IsService { get; set; }



        public decimal? Qty { get; set; }
        public decimal? UnitPrice { get; set; }

        public decimal? ItemWiseIgst { get; set; }
        public decimal? ItemWiseCgst { get; set; }
        public decimal? ItemWiseSgst { get; set; }
       

        public bool RoffSales { get; set; }

        public string? AltPincode { get; set; }
        public string? AltCustAddr1 { get; set; }
        public string? AltCustAddr2 { get; set; }
        public string? AltGSTNo { get; set; }
        public string? AltCustName { get; set; }
        public string? AltLocation { get; set; }
        public int AltStateCode { get; set; }

    }
}
