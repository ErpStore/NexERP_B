using V.SMART.Shared.Data;


using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Inventory_Stock_.StockIssueRequest;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IStockRequestIssueRepository;

namespace V.SMART.Shared.Repository.InventoryStockRepository.StockRequestIssueRepository
{
    public class StockRequestIssueSubRepository : Repository<StockIssueRequestSub>, IStockRequestIssueSubRepository
    {

        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;

        public StockRequestIssueSubRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }


    }
}
