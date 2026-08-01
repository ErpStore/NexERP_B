using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.GSTITCVM
{
    public class ITC04OutwardVM
    {
        public string GSTNo { get; set; }
        public string VendorName { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
        public string DcNo { get; set; }
        public string DcDate { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string MeasureUnit { get; set; }
        public decimal Qty { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal IGST { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
    }
}
