using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inventory_Stock_.ToolCrib;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IToolCribIssueRepo;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.InventoryStockRepository.TooCribIssueRepo
{
    public class ToolCribIssueSubRepository : Repository<ToolCribIssueSub>, IToolCribIssueSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public ToolCribIssueSubRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }


    }

}
