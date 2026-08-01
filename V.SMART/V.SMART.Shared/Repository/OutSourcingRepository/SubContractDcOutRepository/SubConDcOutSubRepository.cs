using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.SubContractDC;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubContractDCOutRepository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.SubContractDcOutRepository
{
    public class SubConDcOutSubRepository : Repository<SubConDcOutSub>, ISubConDCOutSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public SubConDcOutSubRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }
    }
}
