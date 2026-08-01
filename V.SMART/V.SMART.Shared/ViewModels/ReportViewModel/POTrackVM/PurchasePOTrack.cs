using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.POTrackVM
{
    public class PurchasePOTrack
    {
        public long? SlNo { get; set; }
        public int? VendorCode { get; set; }
        public string? Supplier { get; set; }
        public string? Type { get; set; }
        public string? PoNo { get; set; }
        public DateTime? PODate { get; set; }
        public string PoCancel { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Specification { get; set; }
        public string? UOM { get; set; }
        public string? HSNCode { get; set; }
        public string ItemCancel { get; set; }
        public decimal? Qty { get; set; }
        public decimal? BalQty { get; set; }
        public decimal? UnitPrice { get; set; }

        public string? DcNo { get; set; }
        public DateTime? DcDate { get; set; }
        public decimal? DcQty { get; set; }
        public decimal? DcBalQty { get; set; }

        public string? GRNNo { get; set; }
        public DateTime? GRNDate { get; set; }
        public decimal? GRNQty { get; set; }
        public decimal? GRNBalQty { get; set; }

        public string RefDcNo { get; set; }
        public DateTime? RefDcDate { get; set; }

        public string? SCNNo { get; set; }
        public DateTime? SCNDate { get; set; }
        public decimal? AcceptQty { get; set; }
        public decimal? RejectQty { get; set; }
        public decimal? ReworkQty { get; set; }

        public string? InvNo { get; set; }
        public DateTime? InvDate { get; set; }
        public decimal? InvQty { get; set; }

    }
}
