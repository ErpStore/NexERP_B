using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseSCN_Repository
{
    public interface IPurchaseSCNRepository: IRepository<PurchaseSCN>
    {

        Task<string> GetLastSCNNoAsync(string suffix);
        Task<List<PurchaseSCNVM>> SearchPurchaseSCNAsync(PurchaseSCNVM search);
    }
}
