using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.AccountsModule
{
    public class AdvaceadjustmentSub
    {
        [Key]
        public int AdjustSubId { get; set; }
        public int AdjumentId { get; set; }

        [ForeignKey(nameof(AdjumentId))]
        public Advaceadjustment? Advaceadjustments { get; set; }
        public string BillNo { get; set; }

        public DateTime BillDate = DateTime.Now;
        public int RefId { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal AdjustAmount { get; set; }

        public decimal SavedAdjustAmount { get; set; } // from DB
    }
}
