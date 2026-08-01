using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inventory_Stock_.InterStoreTransfer;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IStoreInterTransferRepository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.InventoryStockRepository.StoreInterTransferRepository
{
    public class StoreInterTransSubRepository : Repository<StoreInterTransSub>, IStoreInterTransSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;

        public StoreInterTransSubRepository(ApplicationDbContext db, ILoggingService loggingService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
        }

    }
}
