using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel
{
    public class LabourTrackSummaryVM
    {
       
        //Customer
        public int? CustId { get; set; }

        public string? CustName { get; set; }

        //PO Details
        public int? PoId { get; set; }
        public string? PONo { get; set; }
        public DateTime? PODate { get; set; }
        public string? POStatus { get; set; }

        public int? PoSubId { get; set; }
        public decimal? PoQty { get; set; }
        public decimal? PoBalQty { get; set; }



        //Labour GRN
        public int? GRNId { get; set; }
        public string? GRNNo { get; set; }

        public string? RefDcNo { get; set; }
        public DateTime? GRNDate { get; set; }
        public string? GRNStatus { get; set; }


        public int? GRNSubId { get; set; }
        public decimal? GrnQty { get; set; }
        public decimal? GrnBqty { get; set; }

        public string? TransType { get; set; }


        //Labour SCN
        public int? SCNId { get; set; }
        public string? SCNNo { get; set; }
        public DateTime? SCNDate { get; set; }
        public string? SCNStatus { get; set; }

        public int? SCNSubId { get; set; }
        public decimal? ScnQty { get; set; }
        public decimal? ScnBqty { get; set; }


        //Labour DC
        public int? DCId { get; set; }
        public string? DCNo { get; set; }


        public DateTime? DCDate { get; set; }
        public string? DCStatus { get; set; }


        public int? DcSubId { get; set; }
        public decimal? DcQty { get; set; }
        public decimal? DCBQty { get; set; }


        // Labour Invoice
        public int? InvId { get; set; }
        public string? InvNo { get; set; }
        public DateTime? InvDate { get; set; }
        public string? InvStatus { get; set; }



        public int? LabInvSubId { get; set; }
        public decimal? Invqty { get; set; }
        public decimal? InvBQty { get; set; }

        [NotMapped]
        public string? DocType { get; set; }


        public int? ItemId { get; set; }

        public string? ItemCode { get; set; }

        public string? ItemName { get; set; }

        public int? CategoryCode { get; set; }


        public decimal? GrandTotal { get; set; }
    

    }
}
