using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesPoRepository
{
    public interface IMfgPoRepository : IRepository<MfgPo>
    {
        Task ReloadAsync(MfgPo entity);
    }
}
