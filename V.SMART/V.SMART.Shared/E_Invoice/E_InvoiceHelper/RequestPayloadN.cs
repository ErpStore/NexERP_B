using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.E_Invoice.E_InvoiceHelper
{
    public class RequestPayloadN
    {
        public string Data { get; set; }

        public string action { get; set; }
    }

    public class Jsonpayload
    {
        public string AckNo { get; set; }
        public string AckDt { get; set; }
        public string Irn { get; set; }
        public string SignedInvoice { get; set; }
        public string SignedQRCode { get; set; }
        public string EwbNo { get; set; }
        public string EwbDt { get; set; }
        public string ewayBillNo { get; set; }
        public string ewayBillDate { get; set; }
        public string validUpto { get; set; }


    }
}
