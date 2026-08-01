using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Repository.IRepository.IPlanningRepository.IJobOrderRepo;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.PlanningRepository.JobOrderRepo
{
    public class JobOrderSubRepository : Repository<JobOrderSub>, IJobOrderSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public JobOrderSubRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }
    }
}
