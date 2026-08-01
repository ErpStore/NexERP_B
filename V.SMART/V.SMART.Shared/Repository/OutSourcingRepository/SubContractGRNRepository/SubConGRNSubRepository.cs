using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.SubContractGRN;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubContractGRNRepository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.SubContractGRNRepository
{
    public class SubConGRNSubRepository : Repository<SubConGRNSub>, ISubConGRNSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public SubConGRNSubRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }
    }
}
