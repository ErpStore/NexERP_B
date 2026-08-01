using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inspection.FinalInspection;
using V.SMART.Shared.Repository.IRepository.IInspectionRepository.IFinalInspection;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.InspectionRepository.FinalInspectionRepository
{
    public class FinalInspectionRefRepository :Repository<FinalInspectionRef>,IFinalInspectionRefRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _loggingService;
        public FinalInspectionRefRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
                : base(db, loggingService)
        {
            _db = db;
            _currentUserService = currentUserService;
            _loggingService = loggingService;
        }
    }
}
