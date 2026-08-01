using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.DebitNoteListViewModel
{
    public class DebitNoteListVM
    {
        public long SlNo { get; set; }
        public string? DebitNo { get; set; }
        public DateTime DebitDate { get; set; }
        public string? VendorSupplier { get; set; }
        public string? DebitNoteStatus { get; set; }

        public string? Remarks { get; set; }
        public string? Type { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSpecification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }
        public decimal Qty { get; set; }
        public decimal RejectQty { get; set; }
        public decimal CDQty { get; set; }
        public decimal ReworkQty { get; set; }
        public decimal CrDrUnitPrice { get; set; }
        public string? ItemRemarks { get; set; }
        public string? RefPoNo { get; set; }
        public string? RefPoDt { get; set; }
        public string? RefInvNo { get; set; }
        public string? RefInvDt { get; set; }
        public string? RefSubConInvNo { get; set; }
        public string? RefSubConInvDt { get; set; }
    }
}
