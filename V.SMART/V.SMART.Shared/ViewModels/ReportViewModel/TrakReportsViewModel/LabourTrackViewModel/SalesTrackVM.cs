using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel
{
    public class SalesTrackVM
    {
        public int? PoId { get; set; }

        public int? DcId { get; set; }

        public int? InvId { get; set; }

        public int? EnquiryId { get; set; }

        public int? QuoteId { get; set; }

        public int? JobId { get; set; }

        public int? RCId { get; set; }

        public int? MreqId { get; set; }



        public string? PONo { get; set; }

        public DateTime? PODate { get; set; }

        public int? CustId { get; set; }

        public string? CustName { get; set; }

        public string? CustomerGstNo { get; set; }

        public bool? PoTally { get; set; } = false;


        public bool? ShortClose { get; set; }

        public bool? PoShortClose { get; set; }

        public bool? DcShortClose { get; set; }

        public bool? QtShortClose { get; set; }

        public bool? EnqShortClose { get; set; }

        public bool? PoCancl { get; set; } = false;

        public int? ItemId { get; set; }

        public string? Itemcode { get; set; }

        public string? ItemName { get; set; }

        public int? CategoryCode { get; set; }

        public decimal? Qty { get; set; }

        public decimal? BalQty { get; set; }



        public string? EnqNo { get; set; }

        public DateTime? EnqDate { get; set; }

        public decimal? EnqQty { get; set; }

        public decimal? EnqBalQty { get; set; }

        public bool? EnqTally { get; set; }

        public bool? IsCancel { get; set; }

        public string? QtNo { get; set; }

        public DateTime? QtDate { get; set; }

        public decimal? QtQty { get; set; }

        public decimal? QtBalQty { get; set; }

        public bool? QuotationTally { get; set; }

        public string? JobOrderNo { get; set; }

        public DateTime? JobOrderDate { get; set; }

        public decimal? JobOrderQty { get; set; }

        public decimal? JobBalQty { get; set; }



        public bool? JobTally { get; set; }

        public bool? Cancel { get; set; }

        public string? RouteCardNo { get; set; }

        public DateTime? RouteCardDate { get; set; }

        public decimal? RcQty { get; set; }

        public decimal? RcBalQty { get; set; }


        public bool? RcStatus { get; set; }


        public string? MReqNo { get; set; }

        public DateTime? MreqDate { get; set; }

        public decimal? PurchQty { get; set; }

        public decimal? MrBalQty { get; set; }




        public bool? MrTally { get; set; }


        public bool? MreqCancel { get; set; }

        public bool? MreqShortClose { get; set; }



        public string? DcNo { get; set; }

        public DateTime? DcDate { get; set; }

        public bool? DcTally { get; set; }

        public bool? DcCancel { get; set; }

        public decimal? DcQty { get; set; }

        public decimal? DcBalQty { get; set; }


        public string? InvNo { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public bool? InvTally { get; set; }

        public bool? InvCancel { get; set; }

        public decimal? InvQty { get; set; }

        public decimal? InvBQty { get; set; }





        public decimal? GrandTotal { get; set; }

        public string? DocType { get; set; }
    }
}
