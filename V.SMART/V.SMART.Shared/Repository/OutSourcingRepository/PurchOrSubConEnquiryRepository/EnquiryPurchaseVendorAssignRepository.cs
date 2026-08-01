using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchOrSubConRepository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.PurchOrSubConEnquiryRepository
{
    public class EnquiryPurchaseVendorAssignRepository : Repository<EnquiryPurchaseVendorAssign>, IEnquiryPurchaseVendorAssignRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public EnquiryPurchaseVendorAssignRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }
    }
}

