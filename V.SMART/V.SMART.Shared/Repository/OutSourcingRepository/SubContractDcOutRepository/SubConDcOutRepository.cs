using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.SubContractDC;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubContractDCOutRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.SubContractDcOutRepository
{
    public class SubConDcOutRepository : Repository<SubConDcOut>, ISubConDCOutRepository

    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public SubConDcOutRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }

        public async Task<string> GetLastDcNoAsync(string suffix)
        {
            var lastNumberStr = await _db.SubConDcOut
                .FromSqlRaw(@"
                SELECT TOP 1 * 
                FROM SubConDcOut WITH (UPDLOCK, ROWLOCK)
                WHERE Suffix = {0}
                ORDER BY TRY_CAST(DcNo AS INT) DESC", suffix)
                .Select(q => q.DcNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastNumberStr) &&
                int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            return nextNumber.ToString();
        }

    }
}
