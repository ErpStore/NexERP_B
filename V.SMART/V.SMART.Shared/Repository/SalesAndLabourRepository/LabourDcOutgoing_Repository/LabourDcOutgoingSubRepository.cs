using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour.LabourDC;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ILabourDcOutgoing_Repository;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ILabourGRN_Repository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository.LabourDcOutgoing_Repository
{
    public class LabourDcOutgoingSubRepository: Repository<LabourDcOutgoingSub>, ILabourDcOutgoingSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public LabourDcOutgoingSubRepository(
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
