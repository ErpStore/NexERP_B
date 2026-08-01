using V.SMART.Shared.Data;
using V.SMART.Shared.Data.InvoiceAutoRunning;
using V.SMART.Shared.Repository.IRepository.IinvoiceRunningNo;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.InvoiceAutoRunnNo
{
    public class InvoiceAutoRunningNumberRepository:Repository<InvoiceAutoRunningNumber>, IInvoiceAutoRunningNumberRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public InvoiceAutoRunningNumberRepository(
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
