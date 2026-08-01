using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionCompRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionReturnAssyRepo;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.ProductionRepository.ProuctionCompRepo
{
    public class ProductionReturnCompTrackRepository : Repository<ProductionReturnCompTrack>, IProductionReturnCompTrackRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public ProductionReturnCompTrackRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }
    }
}
