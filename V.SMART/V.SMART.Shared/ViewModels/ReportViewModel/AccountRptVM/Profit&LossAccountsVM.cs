using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel
{
    public class Profit_LossAccountsVM
    {
        public string ParticularsLeft { get; set; } = string.Empty;
        public decimal AmountLeft { get; set; }

        public string ParticularsRight { get; set; } = string.Empty;
        public decimal AmountRight { get; set; }
    }
}
