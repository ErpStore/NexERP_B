using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.CreditNoteListViewModel
{
    public class CreditNoteListVM
    {
        public long SlNo { get; set; }
        public string? CreditNo { get; set; }
        public DateTime CreditDate { get; set; }
        public string? Customer { get; set; }
        public string? CreditNoteStatus { get; set; }
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
        public string? RefLabInvNo { get; set; }
        public string? RefLabInvDt { get; set; }
    }
}
