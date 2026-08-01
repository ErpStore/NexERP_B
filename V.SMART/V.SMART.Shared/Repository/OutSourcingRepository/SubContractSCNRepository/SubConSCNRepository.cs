using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.SubContractSCN;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubContractSCNRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.SubContractSCNRepository
{
    public class SubConSCNRepository : Repository<SubConSCN>, ISubConSCNRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public SubConSCNRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }

        public async Task<string> GetLastSCNNoAsync(string suffix)
        {
            // Safely fetch the latest QuoteNo with locking to prevent concurrency issues
            var lastNumberStr = await _db.SubConSCN
                .FromSqlRaw(@"
                SELECT TOP 1 * 
                FROM SubConSCN WITH (UPDLOCK, ROWLOCK)
                WHERE Suffix = {0}
                ORDER BY TRY_CAST(SCNNo AS INT) DESC", suffix)
                .Select(q => q.SCNNo)
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
