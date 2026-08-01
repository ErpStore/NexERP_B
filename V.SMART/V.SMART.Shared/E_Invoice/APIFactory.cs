using V.SMART.Shared.E_Invoice.E_InvoiceHelper;
using V.SMART.Shared.E_Invoice.EwayHelper;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.E_Invoice
{
    public class APIFactory
    {
   
        public static IAPIService GetAPIService(string apiKey, string jsonData)
        {
            switch (apiKey)
            {
                case "EInvoice":
                    return new EinvoiceApiHelper(jsonData);

                case "EWay":
                    return new EwayAPIHelper(jsonData);
                default: return null;

            }
        }
    }
}
