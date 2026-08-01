using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inspection.IncomingInspection;
using V.SMART.Shared.Repository.IRepository.IInspectionRepository.IIncomingInspectionRepository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.InspectionRepository.IncominInspectionRepository
{
    public class IncomingInspectionRefRepository : Repository<IncomingInspectionRef>,IIncomingInspectionRefRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _loggingService;
        public IncomingInspectionRefRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
                : base(db, loggingService)
        {
            _db = db;
            _currentUserService = currentUserService;
            _loggingService = loggingService;
        }
    }
}
