


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.AccountsModule;
using V.SMART.Shared.Repository;
using V.SMART.Shared.Repository.IRepository.IAccountRepository.IPaymentsRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.AccountsRepository
{
    #region payment
    internal class PaymentsSubRepository : Repository<PaymentsSub>, IPaymentsSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _currentUserService;
        public PaymentsSubRepository(ApplicationDbContext db, ILoggingService logs, CurrentUserService currentUserService) : base(db, logs)
        {
            _db = db;
            _logs = logs;
            _currentUserService = currentUserService;
        }
    }
    #endregion

    #region Advance Adjust
    internal class AdvaceadjustmentSubRepository : Repository<AdvaceadjustmentSub>, IAdvaceadjustmentSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _currentUserService;
        public AdvaceadjustmentSubRepository(ApplicationDbContext db, ILoggingService logs, CurrentUserService currentUserService) : base(db, logs)
        {
            _db = db;
            _logs = logs;
            _currentUserService = currentUserService;
        }
    }
    #endregion

    #region Receipt
    internal class ReceiptsSubRepository : Repository<ReceiptsSub>, IReceiptsSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _currentUserService;
        public ReceiptsSubRepository(ApplicationDbContext db, ILoggingService logs, CurrentUserService currentUserService) : base(db, logs)
        {
            _db = db;
            _logs = logs;
            _currentUserService = currentUserService;
        }
    }
    #endregion
}
