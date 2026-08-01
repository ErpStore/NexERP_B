
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.AccountsViewModel
{
    public class ReceiptsSubVM
    {
        public int ReceiptSubId { get; set; }
        public int ReceiptId { get; set; }

        public string BillNo { get; set; }

        public DateTime BillDate = DateTime.Now;
        public int RefId { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal AdjustAmount { get; set; }

        public decimal SavedAdjustAmount { get; set; } // from DB

    }
}
