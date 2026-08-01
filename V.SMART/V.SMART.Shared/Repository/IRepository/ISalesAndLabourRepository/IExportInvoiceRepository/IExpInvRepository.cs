using V.SMART.Shared.Data.SalesAndLabour.Export;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IExportInvoiceRepository
{
    public interface IExpInvRepository : IRepository<ExpInv>
    {
        Task<string> GetLastExpInvNoAsync(string suffix);
    }
}
