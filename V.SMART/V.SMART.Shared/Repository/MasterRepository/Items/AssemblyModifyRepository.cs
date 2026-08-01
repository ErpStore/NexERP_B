using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItems;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.MasterRepository.Items
{
    public class AssemblyModifyRepository: Repository<AssemblyModify>, IAssemblyModifyRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public AssemblyModifyRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<int> GetNextRecNoAsync(int? assyId)
        {
            int maxRecNo = await _db.AssemblyModify
                .Where(a => a.AssyId == assyId)
                .MaxAsync(x => (int?)x.RecNo) ?? 0;

            return maxRecNo + 1;
        }


    }
}
