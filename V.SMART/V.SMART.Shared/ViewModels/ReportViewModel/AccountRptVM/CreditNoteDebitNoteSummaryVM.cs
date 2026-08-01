using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.AccountsReportViewModel
{
    public class CreditNoteDebitNoteSummaryVM
    {
        public int? SlNo { get; set; }

        public string? Customer { get; set; }

        public string? Type { get; set; }

        public string? InvoiceNo { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public decimal? TotalBasicAmount { get; set; }

        public decimal? DiscountPercent { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? TotalGrossAmount { get; set; }

        public decimal? FreightCharges { get; set; }

        public decimal? PackingPercent { get; set; }

        public decimal? PackingCharges { get; set; }

        public decimal? InsurancePercent { get; set; }

        public decimal? InsuranceCharges { get; set; }

        public decimal? TotalTaxable { get; set; }

        public decimal? CGstRate { get; set; }

        public decimal? TotalCGSTAmount { get; set; }

        public decimal? SGstRate { get; set; }

        public decimal? TotalSGSTAmount { get; set; }

        public decimal? IGstRate { get; set; }

        public decimal? TotalIGSTAmount { get; set; }

        public decimal? TCSPercent { get; set; }

        public decimal? TCSAmount { get; set; }

        public decimal? OtherCharges { get; set; }

        public decimal? GrandTotal { get; set; }

        public decimal? CreditAmt { get; set; }

        public string? MainRemark { get; set; }
    }
}
