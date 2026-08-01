using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace V.SMART.Shared.Repository.MasterRepository.HumanResourceMaster
{
    public class ShiftAllocationRepository : Repository<ShiftAllocation>, IShiftAllocationRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public ShiftAllocationRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
                    : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public override async Task<ShiftAllocation> UpdateAsync(ShiftAllocation obj)
        {
            var existing = await _db.ShiftAllocation.FirstOrDefaultAsync(s => s.ShiftId == obj.ShiftId);
            if (existing != null)
            {
                existing.ShiftId = obj.ShiftId;
                existing.ShiftCode = obj.ShiftCode;
                existing.ShiftName = obj.ShiftName;
                existing.ShiftStartTime = obj.ShiftStartTime;
                existing.ShiftEndTime = obj.ShiftEndTime;
                existing.BreakStartTime = obj.BreakStartTime;
                existing.TotalWorkingHours = obj.TotalWorkingHours;
                existing.OverTimeAllowed = obj.OverTimeAllowed;
                existing.LateMarkAllowed = obj.LateMarkAllowed;
                existing.EarlyLeaveAllowed = obj.EarlyLeaveAllowed;
                existing.CreatedBy = obj.CreatedBy;
                existing.CreatedDate = obj.CreatedDate;
                existing.ModifiedBy = obj.ModifiedBy;
                existing.ModifiedDate = obj.ModifiedDate;

                return existing;
            }
            return new ShiftAllocation();
        }
    }
}


