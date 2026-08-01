using V.SMART.Shared.Data.Inventory_Stock_.InterStoreTransfer;
using V.SMART.Shared.ViewModels.InventoryViewModel.StoreInterTransferViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IStoreInterTransferRepository
{
    public interface IStoreInterTransRepository : IRepository<StoreInterTrans>
    {
        Task<string> GetLastISTNoAsync(string suffix);
        Task<List<StoreInterTrans>> InstantSearchInterStoreTransAsync(StoreInterTransVM search);
    }
}
