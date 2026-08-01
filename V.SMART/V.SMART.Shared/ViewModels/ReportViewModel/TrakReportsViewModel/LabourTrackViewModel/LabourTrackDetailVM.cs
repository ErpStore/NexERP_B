using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel
{
    public class LabourTrackDetailVM
    {
        //Customer
        public int CustId { get; set; }
        public string CustName { get; set; }


        //Item Details
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemSpecification {  get; set; }
        public string MeasureUnit { get; set; }


        //PO Details
        public int PoId { get; set; }
        public string PONo { get; set; }
        public DateTime PODate { get; set; }
        public string POStatus { get; set; }

        public decimal PoQty { get; set; }
        public decimal PoBalQty { get; set; }
        public string PoItemStatus { get; set; }



        //Labour GRN
        public int GRNId { get; set; }
        public string GRNNo { get; set; }
        public DateTime? GRNDate { get; set; }
        public string GRNStatus { get; set; }
        public decimal GRNQty { get; set; }
        public decimal GRNBalQty { get; set; }
        public string GRNItemStatus { get; set; }


        //Labour SCN
        public int SCNId { get; set; }
        public string SCNNo { get; set; }
        public DateTime? SCNDate { get; set; }
        public string SCNStatus { get; set; }
        public decimal SCNQty { get; set; }
        public decimal SCNBalQty { get; set; }
        public string SCNItemStatus { get; set; }


        //Labour DC
        public int DCId { get; set; }
        public string DCNo { get; set; }
        public DateTime? DCDate { get; set; }
        public string DCStatus { get; set; }
        public decimal DCQty { get; set; }
        public decimal DcBalQty { get; set; }
        public string DcItemStatus { get; set; }



        // Labour Invoice
        public int InvId { get; set; }
        public string InvNo { get; set; }
        public DateTime? InvDate { get; set; }
        public string InvStatus { get; set; }
        public decimal InvQty { get; set; }
        public decimal InvBalQty { get; set; }
        public string InvItemStatus { get; set; }
    }
}
