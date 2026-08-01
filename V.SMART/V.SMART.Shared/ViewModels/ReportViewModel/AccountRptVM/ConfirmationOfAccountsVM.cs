using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.AccountsVM
{
    public class ConfirmationOfAccountsVM
    {
        public string Date { get; set; }= DateTime.Now.ToString("dd/MM/yyyy");

        public string? Particulars { get; set; }

        public string? Type { get; set; }

        public string? RefNo { get; set; }

        public decimal? Inflow { get; set; }

        public decimal? Outflow { get; set; }

    }
}
