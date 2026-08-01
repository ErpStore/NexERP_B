using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IMaterialRequisitionRepo;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.MaterialRequsiationRepo
{
    public class MaterialReqSubRepository : Repository<MaterialReqSub> , IMaterialReqSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;

        public MaterialReqSubRepository(ApplicationDbContext db, ILoggingService logs) : base(db ,logs)
        {
            _db = db;
            _logs = logs;
        }

    }
}
