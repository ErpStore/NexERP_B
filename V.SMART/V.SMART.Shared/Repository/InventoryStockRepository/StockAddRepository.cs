using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inventory_Stock_;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IStockAdditionRepository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.InventoryStockRepository
{
    public class StockAddRepository : Repository<StockAdd>, IStockAddRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;

        public StockAddRepository(ApplicationDbContext db, ILoggingService loggingService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
        }
    }
}
