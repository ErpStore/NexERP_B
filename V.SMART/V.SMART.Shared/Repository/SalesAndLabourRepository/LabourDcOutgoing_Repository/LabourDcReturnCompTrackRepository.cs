using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour.LabourDC;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ILabourDcOutgoing_Repository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository.LabourDcOutgoing_Repository
{
    public class LabourDcReturnCompTrackRepository : Repository<LabourDcReturnCompTrack>, ILabourDcReturnCompTrackRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public LabourDcReturnCompTrackRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }
    }
}
