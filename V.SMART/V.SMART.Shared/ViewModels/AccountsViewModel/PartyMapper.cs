
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;

namespace V.SMART.Shared.ViewModels.AccountsViewModel
{
    public static class PartyMapper
    {
        public static PartyVM FromVendor(VendorVM v)
        {
            return new PartyVM
            {
                PartyCode = v.VendorCode,
                PartyName = v.VendorName,
                OpeningBalance = v.OpeningBalance ?? 0m,
                OpeningBalancePending = v.OpeningBalancePending ?? 0m,
                PartyType = "VENDOR"
            };
        }

        public static PartyVM FromCustomer(CustomerVM c)
        {
            return new PartyVM
            {
                PartyCode = c.CustId,
                PartyName = c.CustName,
                OpeningBalance =c.OpeningBalance??0m,
                OpeningBalancePending = c.OpeningBalancePending ?? 0m,
                PartyType = "CUSTOMER"
            };
        }
    }
}
