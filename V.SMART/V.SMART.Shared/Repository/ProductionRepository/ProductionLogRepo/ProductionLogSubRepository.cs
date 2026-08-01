using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Production.DailyProductionLog;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionLogRepo;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.ProductionRepository.ProductionLogRepo
{
    public class ProductionLogSubRepository : Repository<ProductionLogSub>, IProductionLogSubRepository
    {

        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public ProductionLogSubRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }

    }
}
