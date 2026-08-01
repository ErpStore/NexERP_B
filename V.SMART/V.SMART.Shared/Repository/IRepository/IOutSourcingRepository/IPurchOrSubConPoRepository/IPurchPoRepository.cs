using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchOrSubConPoRepository
{
    public interface IPurchPoRepository : IRepository<PurchPo>
    {
        Task<string> GetLastPONoAsync(string suffix);
    }
}
