using DocumentFormat.OpenXml.InkML;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour_Module;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IMfgQuotation;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository
{
    public class LabInvSubRepository: Repository<LabInvSub>, ILabInvSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public LabInvSubRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<List<LabInvSub>> GetLabInvSubDataByLabInvId(int LabInvId)
        {
            return await _db.LabInvSub
                .Where(s => s.LabInvId == LabInvId)
                .ToListAsync();
        }
    }
   
}
