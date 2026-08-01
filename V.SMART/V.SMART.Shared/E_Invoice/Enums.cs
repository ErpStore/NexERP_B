using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.E_Invoice
{
    public static class Enums
    {
        public enum ServiceType
        {
            EInvoice = 0,
            Eway = 1
        }

        public enum InvType
        {
            Sales = 0,
            Labour = 1,
            Export = 2,
            CreditNote = 3,
            DebitNote = 4
        }

        public enum EwayType
        {
            MfgDc = 0,
            MfgInv = 1,
            LabourDc = 2,
            LabInv = 3,
            SubConDc = 4
        }
    }

}
