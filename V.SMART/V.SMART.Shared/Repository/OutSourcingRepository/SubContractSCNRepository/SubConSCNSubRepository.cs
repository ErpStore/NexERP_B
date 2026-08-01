using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.SubContractSCN;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubContractSCNRepository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.SubContractSCNRepository
{
    public class SubConSCNSubRepository : Repository<SubConSCNSub>, ISubConSCNSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public SubConSCNSubRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }
    }
}
