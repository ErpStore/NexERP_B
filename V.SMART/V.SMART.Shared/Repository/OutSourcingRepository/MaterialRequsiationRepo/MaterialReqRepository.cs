using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IMaterialRequisitionRepo;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.MaterialRequsiationRepo
{
    public class MaterialReqRepository : Repository<MaterialReq>, IMaterialReqRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public MaterialReqRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }

        public async Task<string> GetLastMReqNoAsync(string suffix)
        {
            var lastNumberStr = await _db.MaterialReq
                .FromSqlRaw(@"
                SELECT TOP 1 * 
                FROM MaterialReq WITH (UPDLOCK, ROWLOCK)
                WHERE Suffix = {0}
                ORDER BY TRY_CAST(MReqNo AS INT) DESC", suffix)
                .Select(q => q.MReqNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            // Ensure null safety before parsing
            if (!string.IsNullOrWhiteSpace(lastNumberStr) &&
                int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            return nextNumber.ToString();
        }

    }
}
