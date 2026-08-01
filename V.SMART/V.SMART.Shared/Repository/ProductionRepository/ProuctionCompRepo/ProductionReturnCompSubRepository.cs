using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionCompRepo;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.ProductionRepository.ProuctionCompRepo
{
    public class ProductionReturnCompSubRepository : Repository<ProductionReturnCompSub>, IProductionReturnCompSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public ProductionReturnCompSubRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }
    }
}
