using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesEnquiry;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository.SalesEnquiry
{
    public class EnquirySalesSubRepository : Repository<EnquirySalesSub>, IEnquirySalesSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public EnquirySalesSubRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService
        ) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task RemoveEnquirySubItemAsync(int itemId, int enquiryId, int enquirySubId, CancellationToken cancellationToken = default)
        {
            try
            {
                var item = await _db.EnquirySalesSub
                    .FirstOrDefaultAsync(c =>
                        c.EnquiryId == enquiryId &&
                        c.ItemId == itemId &&
                        c.EnquirySubId == enquirySubId,
                        cancellationToken);

                if (item != null)
                {
                    _db.EnquirySalesSub.Remove(item);
                    await _db.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in EnquirySalesSubRepository.RemoveEnquirySubItemAsync");
            }
        }
    }
}
