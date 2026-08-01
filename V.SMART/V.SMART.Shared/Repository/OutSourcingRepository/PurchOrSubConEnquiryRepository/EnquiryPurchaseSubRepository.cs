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
    public class EnquiryPurchaseSubRepository : Repository<EnquiryPurchaseSub>, IEnquiryPurchaseSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public EnquiryPurchaseSubRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService)
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
