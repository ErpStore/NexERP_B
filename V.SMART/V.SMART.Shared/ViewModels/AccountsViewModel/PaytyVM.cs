using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.AccountsViewModel
{
    public class PartyVM
    {
        public int PartyCode { get; set; }
        public string PartyType { get; set; } = ""; // Vendor, Customer, Supplier, Staff, etc.
        public string PartyName { get; set; } = "";
        public decimal OpeningBalance { get; set; }
        public decimal OpeningBalancePending { get; set; }
        public decimal AdvancePaid { get; set; }
        public decimal PartyPending { get; set; }
        public decimal PartyAvailableCredit { get; set; }

    }
}
