using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IEInvoiceAPI
{
    public interface IEinvoiceDatabaseService
    {
        Task GetRequiredDetailsFromDb(string Invno, string InvTyp, bool isPreView);
    }
}
