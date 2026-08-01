using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseSCN_Repository
{
    public interface IPurchaseSCNSubRepository : IRepository<PurchaseSCNSub>
    {
        Task<List<PurchaseSCNSub>> GetPurchaseSCNSubDataByDcId(int SCNId);
    }
}
