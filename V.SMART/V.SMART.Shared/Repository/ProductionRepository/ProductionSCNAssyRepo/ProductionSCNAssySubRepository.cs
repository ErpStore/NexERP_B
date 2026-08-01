using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Production.ProductionSCNAssembly;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionSCNAssyRepo;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.ProductionRepository.ProductionSCNAssyRepo
{
    public class ProductionSCNAssySubRepository : Repository<ProductionSCNAssySub>, IProductionSCNAssySubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public ProductionSCNAssySubRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }
    }
}
