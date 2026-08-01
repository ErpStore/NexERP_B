using V.SMART.Shared.E_Invoice.E_InvoiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.E_Invoice
{
    public interface ILicenseProductKey
    {
        Task<AuthEinvoice> GetUserName();
        Task<AuthEWay> GetUserNameEway();
        Task<bool> IsValidProductKey();
    }
}
