using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItems;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.Items
{
    public class ProcessFlowChartRepository : Repository<ProcessFlowChart>, IProcessFlowChartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public ProcessFlowChartRepository(
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
