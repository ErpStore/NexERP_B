using V.SMART.Shared.Data.SalesAndLabour_Module;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.SalesAndLabourRepository.MfgQuotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository
{
    public interface ILabInvRepository: IRepository<LabInv>
    {
        Task<IEnumerable<LabInv>> GetAllWithItemsAsync();
        Task<string> GetLastLabInvoiceNoAsync(string suffix);
    }
}
