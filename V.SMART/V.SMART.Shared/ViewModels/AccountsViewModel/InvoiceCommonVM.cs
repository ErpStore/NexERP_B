using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.AccountsViewModel
{
    public class InvoiceCommonVM
    {
        public int? VendorCode { get; set; }
        public string VendorName { get; set; }
        public decimal? OpenBal { get; set; }
        public decimal? OpenBalPndg { get; set; }
        public decimal Balance { get; set; }
    }
}
