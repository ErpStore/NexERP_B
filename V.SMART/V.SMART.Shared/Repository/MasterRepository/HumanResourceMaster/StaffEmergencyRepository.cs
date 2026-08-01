using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.HumanResourceMaster
{
    public class StaffEmergencyRepository : Repository<StaffEmergency>, IStaffEmergencyRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public StaffEmergencyRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<bool> ExistsByNameAsync(string Name, int? excludeId = null)
        {
            return await _db.StaffEmergency.AnyAsync(c => c.Name == Name && (!excludeId.HasValue || c.SlNo != excludeId.Value));
        }
        public async Task DeleteByStaffIdAsync(int staffId)
        {
            var emergencies = await _db.StaffEmergency
                .Where(e => e.StaffID == staffId)
                .ToListAsync();

            if (emergencies.Any())
            {
                _db.StaffEmergency.RemoveRange(emergencies);
            }
        }

        public override async Task<StaffEmergency> UpdateAsync(StaffEmergency obj)
        {
            try
            {
                var tracked = await _db.StaffEmergency.FirstOrDefaultAsync(s => s.SlNo == obj.SlNo);

                if (tracked == null)
                    return new StaffEmergency();

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

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Store Master",
                    action: "Store data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in StaffEmergencyRepository.UpdateAsync");
                return new StaffEmergency();
            }
        }


    }
}

