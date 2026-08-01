using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.ReportViewModel.GRNPendingVM;

namespace V.SMART.Shared.Repository.MasterRepository.HumanResourceMaster
{
    public class CandidateRepository : Repository<Candidate>, ICandidateRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _loggingService;
        public CandidateRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _currentUserService = currentUserService;
            _loggingService = loggingService;
        }

        public async Task<Candidate?> GetCandidateDataAsync(int id)
        {

            return await _db.Candidate
                .FirstOrDefaultAsync(c => c.CandidateID == id);
        }
    
    }
}
