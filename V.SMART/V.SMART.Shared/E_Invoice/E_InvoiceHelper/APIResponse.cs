using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.E_Invoice.E_InvoiceHelper
{
    public class APIResponse
    {
        public string Status { get; set; }
        public string error { get; set; }
        public List<ErrorDetail> ErrorDetails { get; set; }
        public List<InfoDtl> InfoDtls { get; set; }



        public class ErrorDetail
        {
            public string ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
        }
        public class InfoDtl
        {
            public string InfCd { get; set; }
            public List<Infodata> Desc { get; set; }
        }
        public class Infodata
        {
            public string ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
        }

        public string Data { get; set; }
    }
}
