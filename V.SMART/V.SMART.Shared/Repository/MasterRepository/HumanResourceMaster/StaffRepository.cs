using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.HumanResourceMaster
{
    public class StaffRepository : Repository<Staff>, IStaffRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public StaffRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public override async Task<Staff> UpdateAsync(Staff obj)
        {
            try
            {
                var tracked = await _db.Staff.FirstOrDefaultAsync(s => s.StaffID == obj.StaffID);

                if (tracked == null)
                    return new Staff();

                var originalValues = _db.Entry(tracked).OriginalValues.Clone();

                // Set updated values to the tracked entity
                _db.Entry(tracked).CurrentValues.SetValues(obj);

                var changes = new StringBuilder();

                foreach (var prop in _db.Entry(tracked).Properties)
                {
                    var oldVal = originalValues[prop.Metadata.Name]?.ToString() ?? "null";
                    var newVal = prop.CurrentValue?.ToString() ?? "null";

                    if (oldVal != newVal)
                    {
                        changes.AppendLine($"{prop.Metadata.Name}: '{oldVal}' → '{newVal}'");
                    }
                }

                if (changes.Length == 0)
                    return tracked; // No changes detected

                tracked.CreatedBy = obj.CreatedBy ?? tracked.CreatedBy;
                tracked.CreatedDate = obj.CreatedDate ?? tracked.CreatedDate;

                tracked.ModifiedBy = obj.ModifiedBy;
                tracked.ModifiedDate = DateTime.Now;

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Staff Master",
                    action: "Staff data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in StaffRepository.UpdateAsync");
                return new Staff();
            }
        }

    }
}
