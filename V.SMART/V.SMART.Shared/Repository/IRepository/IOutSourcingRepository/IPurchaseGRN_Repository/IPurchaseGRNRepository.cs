using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseGRN_Repository
{
    public interface IPurchaseGRNRepository: IRepository<PurchaseGRN>
    {
        Task<string> GetLastGRNNoAsync(string suffix);
        Task<IEnumerable<PurchaseGRN>> GetAllWithItemsAsync();

        Task<List<PurchaseGRNVM>> SearchPurchaseGRNAsync(PurchaseGRNVM search);
    }
}
