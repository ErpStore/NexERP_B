using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace V.SMART.Shared.Repository.MasterRepository.HumanResourceMaster
{
    public class EmployeeLeaveBalanceRepository : Repository<EmployeeLeaveBalance>, IEmployeeLeaveBalanceRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public EmployeeLeaveBalanceRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
                  : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public override async Task<IEnumerable<EmployeeLeaveBalance>> GetAllAsync(bool AsNoTracking = false)
        {
            return await _db.EmployeeLeaveBalance
                .AsNoTracking()
                .Include(i => i.Staff)
                .Include(i => i.LeaveType)
                .ToListAsync();
        }

        public override async Task<EmployeeLeaveBalance> UpdateAsync(EmployeeLeaveBalance obj)
        {
            var existing = await _db.EmployeeLeaveBalance.FirstOrDefaultAsync(e => e.LeaveBalanceID == obj.LeaveBalanceID);
            if (existing != null)
            {
                existing.LeaveBalanceID = obj.LeaveBalanceID;
                existing.EmployeeID = obj.EmployeeID;
                existing.LeaveTypeId = obj.LeaveTypeId;
                existing.Year = obj.Year;
                existing.LeavesTaken = obj.LeavesTaken;
                existing.Balance = obj.Balance;
                existing.CreatedBy = obj.CreatedBy;
                existing.CreatedDate = obj.CreatedDate;
                existing.ModifiedBy = obj.ModifiedBy;
                existing.ModifiedDate = obj.ModifiedDate;

                return existing;
            }
            return new EmployeeLeaveBalance();
        }
    }
}
