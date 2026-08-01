
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.CashFlow.ServiceBills;
using V.SMART.Shared.Repository;
using V.SMART.Shared.Repository.IRepository.ICashFlowRepository.IServiceBillsRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.CashFlowRepository.ServiceBillsRepository
{
    public class ServiceBillsRepository:Repository<ServiceBills>, IServiceBillsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public ServiceBillsRepository(
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
