using V.SMART.Shared.E_Invoice.E_InvoiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.E_Invoice
{
    public interface IAPIService: IAuthentication, IEncrypt, IDecrypt
    {
        Task<string> PostJsonData(AuthenticationDetails authenticationDetails, AuthResponse authResponse, string encrypteddata);
        Task<Jsonpayload> GetResponseByJsonData(AuthenticationDetails authentication);
    }
}
