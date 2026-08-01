using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseGRN_Repository
{
    public interface IPurchaseGRNSubRepository : IRepository<PurchaseGRNSub>
    {
        Task<List<PurchaseGRNSub>> GetPurchaseGrnSubDataByDcId(int GRNId);
    }
}
