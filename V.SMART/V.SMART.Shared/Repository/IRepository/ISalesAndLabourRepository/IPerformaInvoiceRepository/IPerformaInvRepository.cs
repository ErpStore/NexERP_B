using V.SMART.Shared.Data.SalesAndLabour.PerformaInvoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IPerformaInvoiceRepository
{
    public interface IPerformaInvRepository : IRepository<PerformaInv>
    {
        Task<string> GetLastPerformaInvNoAsync(string suffix);
    }
}
