using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IMfgInvoice
{
    public interface IMfgInvRepository : IRepository<MfgInv>
    {
        Task<string> GetLastInvNoAsync(string suffix);
    }
}
