using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel
{
    public class RejectionVM
    {
        public long SlNo { get; set; }

        public string? PartyName { get; set; }

        public string? DocNo { get; set; }

        public string? PartNo { get; set; }

        public string? PartName { get; set; }

        public decimal TotalQty { get; set; }

        public decimal AcceptQty { get; set; }

        public decimal QuantityRejected { get; set; }

        public decimal QuantityReworked { get; set; }

        public decimal RejPer { get; set; }

        public decimal RewPer { get; set; }
    }
}
