using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace V.SMART.Shared.Repository.MasterRepository.HumanResourceMaster
{
    public class LeaveTypeRepository : Repository<LeaveType>, ILeaveTypeRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public LeaveTypeRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
                  : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public override async Task<LeaveType> UpdateAsync(LeaveType obj)
        {
            var existing = await _db.LeaveType.FirstOrDefaultAsync(l => l.LeaveTypeId == obj.LeaveTypeId);
            if (existing != null)
            {
                existing.LeaveTypeId = obj.LeaveTypeId;
                existing.LeaveName = obj.LeaveName;
                existing.LeaveDescription = obj.LeaveDescription;
                existing.MaxAllowedPerYear = obj.MaxAllowedPerYear;
                existing.IsCarryForward = obj.IsCarryForward;
                existing.IsCashable = obj.IsCashable;
                existing.CreatedBy = obj.CreatedBy;
                existing.CreatedDate = obj.CreatedDate;
                existing.ModifiedBy = obj.ModifiedBy;
                existing.ModifiedDate = obj.ModifiedDate;

                return existing;
            }
            return new LeaveType();
        }
    }
}
