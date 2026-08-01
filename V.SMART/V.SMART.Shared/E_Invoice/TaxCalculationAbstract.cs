using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.E_Invoice
{
    interface ITaxCalculation
    {
        decimal GetTotalTaxAmt(decimal totBasicAmt, decimal igstAmt, decimal sgstAmt, decimal cgstAmt);

    }

    interface IMainTaxCalculation : ITaxCalculation
    {
        decimal GetTaxAmt(decimal totBasicAmt, decimal taxType);
    }


    interface IItemTaxCalculation : ITaxCalculation
    {
        decimal GetTaxAmt(decimal qty, decimal unitRate, decimal taxType);
    }

    public class MainTaxCalculation : IMainTaxCalculation
    {
        public decimal GetTaxAmt(decimal totBasicAmt, decimal taxType)
        {
            return Math.Round(totBasicAmt * taxType / 100, 2, MidpointRounding.AwayFromZero);
        }

        public decimal GetTotalTaxAmt(decimal totBasicAmt, decimal igstAmt, decimal sgstAmt, decimal cgstAmt)
        {
            return Math.Round(((totBasicAmt) + igstAmt + sgstAmt + cgstAmt), 2);
        }
    }
}
