using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.MasterRepository.HumanResourceMaster
{
    public class DailyLeaveRecordRepository : Repository<DailyLeaveRecord>, IDailyLeaveRecordRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public DailyLeaveRecordRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
                  : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        //public override async Task<LeaveType> UpdateAsync(LeaveType obj)
        //{
        //    var existing = await _db.LeaveType.FirstOrDefaultAsync(l => l.LeaveTypeId == obj.LeaveTypeId);
        //    if (existing != null)
        //    {
        //        existing.LeaveTypeId = obj.LeaveTypeId;
        //        existing.LeaveName = obj.LeaveName;
        //        existing.LeaveDescription = obj.LeaveDescription;
        //        existing.MaxAllowedPerYear = obj.MaxAllowedPerYear;
        //        existing.IsCarryForward = obj.IsCarryForward;
        //        existing.IsCashable = obj.IsCashable;
        //        existing.CreatedBy = obj.CreatedBy;
        //        existing.CreatedDate = obj.CreatedDate;
        //        existing.ModifiedBy = obj.ModifiedBy;
        //        existing.ModifiedDate = obj.ModifiedDate;

        //        return existing;
        //    }
        //    return new LeaveType();
        //}
    }
}
