using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inventory_Stock_;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.InventoryStockRepository
{
    public class StockIssueTrackRepository :Repository<StockIssueTrack>, IStockIssueTrackRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;


        public StockIssueTrackRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }



    }
}
