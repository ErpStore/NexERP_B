using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesDCRepoditory
{
    public interface IMfgDcRepository : IRepository<MfgDc>
    {
        Task<IEnumerable<MfgDc>> GetAllWithItemsAsync();
        Task<string> GetLastDcNoAsync(string suffix);
    }
}
