using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.AccountsModule
{
    public class FundTransResult
    {
        public List<FundTrans> Items { get; set; } = new();
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal NetBalance { get; set; }
    }
}
