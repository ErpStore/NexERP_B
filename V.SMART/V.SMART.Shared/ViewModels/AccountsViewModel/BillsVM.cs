using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.AccountsViewModel
{
    public class BillsVM
    {
        public int Id;
        public int PartyCode;
        public string BillNo = "";
        public DateTime BillDate = DateTime.Now;
        public decimal Balance;
        public string BillType = "";
        public decimal TDSAmount;
        public string Suffix = "";

    }
}
