using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Planning.Estimaton;
using V.SMART.Shared.Repository.IRepository.IPlanningRepository.IEstimatationRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.PlanningRepository.EstimationRepository
{
    public class EstimateSubRepository:Repository<EstimateSub>,IEstimateSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _currentUserService;
        public EstimateSubRepository(ApplicationDbContext db, ILoggingService logs, CurrentUserService currentUserService) : base(db, logs)
        {
            _db = db;
            _logs = logs;
            _currentUserService = currentUserService;
        }
    }
}
