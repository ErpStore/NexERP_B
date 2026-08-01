using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchOrSubConRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.PurchOrSubConEnquiryRepository
{
    public class EnquiryPurchaseRepository : Repository<EnquiryPurchase>, IEnquiryPurchaseRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public EnquiryPurchaseRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<string> GetLastEnqNoAsync(string suffix)
        {
            var lastNumberStr = await _db.EnquiryPurchase
                .FromSqlRaw(@"
                SELECT TOP 1 * 
                FROM EnquiryPurchase WITH (UPDLOCK, ROWLOCK)
                WHERE Suffix = {0}
                ORDER BY TRY_CAST(EnquiryNo AS INT) DESC", suffix)
                .Select(q => q.EnquiryNo)
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
