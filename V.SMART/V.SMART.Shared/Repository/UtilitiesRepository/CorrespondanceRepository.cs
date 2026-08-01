using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Utilities.Correspondence;
using V.SMART.Shared.Repository.IRepository.IUtilitiesRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace V.SMART.Shared.Repository.UtilitiesRepository
{
    public class CorrespondanceRepository : Repository<Correspondence>, ICorrespondanceRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public CorrespondanceRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<(int uploaded, int total)> GetCorrespondenceStatusAsync(string referenceType, int referenceId, string documentType)
        {
            // Uploaded: for this specific record and document type
            int uploaded = await _db.Correspondences
                .Where(c => c.ReferenceType == referenceType &&
                            c.ReferenceId == referenceId &&
                            c.DocumentType == documentType)
                .CountAsync();

            // Total: all documents of this type for the same ReferenceType
            int total = await _db.Correspondences
                .Where(c => c.ReferenceType == referenceType &&
                            c.DocumentType == documentType)
                .CountAsync();

            return (uploaded, total);
        }

    }
}
