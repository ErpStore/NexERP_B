using System;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.HumanResource.OfferLetter;
using V.SMART.Shared.Repository.IRepository.IHumanResourceRepository.IOfferLetterRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.HumanResourceRepository.OfferLetterRepository
{
    public class OfferLetterRepository : Repository<OfferLetter>, IOfferLetterRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public OfferLetterRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }
    }
}
