
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.Debit_Note;
using V.SMART.Shared.Repository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IDebitNote_Repository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.OutSourcingRepository.DebitNote_Repository
{
    public class DebitNoteSubRepository : Repository<DebitNoteSub>, IDebitNoteSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public DebitNoteSubRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }
    }
}
