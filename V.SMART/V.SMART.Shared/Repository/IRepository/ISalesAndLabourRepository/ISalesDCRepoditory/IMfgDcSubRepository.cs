using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesDCRepoditory
{
    public interface IMfgDcSubRepository : IRepository<MfgDcSub>
    {
        Task<List<MfgDcSub>> GetMfgSubDataByDcId(int dcId);
    }
}
