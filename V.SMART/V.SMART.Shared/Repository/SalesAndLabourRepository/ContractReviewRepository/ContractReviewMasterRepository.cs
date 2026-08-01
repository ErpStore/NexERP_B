using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour.ContractReview;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IContractReviewRepository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository.ContractReviewRepository
{
    public class ContractReviewMasterRepository: Repository<ContractReviewMaster>, IContractReviewMasterRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public ContractReviewMasterRepository(
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
