using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.SubContractInvoice;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubConINVoiceRepository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.SubContractInvoiceRepository
{
    public class SubConInvRepository : Repository<SubConInv>, ISubConInvRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public SubConInvRepository(ApplicationDbContext db, ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }
    }
}
