using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.SubContractGRN;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubContractGRNRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace V.SMART.Shared.Repository.OutSourcingRepository.SubContractGRNRepository
{
    public class SubConGRNRepository : Repository<SubConGRN>, ISubConGRNRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;

        public SubConGRNRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }

        public async Task<string> GetLastGRNNoAsync(string suffix)
        {
            var lastNumberStr = await _db.SubConGRN
                .FromSqlRaw(@"SELECT TOP 1 * 
                 FROM SubConGRN WITH (UPDLOCK, ROWLOCK)
                 WHERE Suffix = {0}
                 ORDER BY TRY_CAST(GRNNo AS INT) DESC", suffix)
                .Select(q => q.GRNNo)
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
