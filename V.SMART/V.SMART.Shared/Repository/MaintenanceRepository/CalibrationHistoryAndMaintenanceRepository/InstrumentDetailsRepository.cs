using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Maintenance.CalibrationHistoryAndMaintenance;
using V.SMART.Shared.Repository.IRepository.IMaintenanceRepository.ICalibrationHistoryAndMaintenance;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace V.SMART.Shared.Repository.MaintenanceRepository.CalibrationHistoryAndMaintenanceRepository
{
    public class InstrumentDetailsRepository : Repository<InstrumentDetails>, IInstrumentDetailsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _loggingService;
        public InstrumentDetailsRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _currentUserService = currentUserService;
            _loggingService = loggingService;
        }

        public override async Task<InstrumentDetails> UpdateAsync(InstrumentDetails obj)
        {
            try
            {
                var tracked = await _db.InstrumentDetails.FirstOrDefaultAsync(id => id.ItemId == obj.ItemId && id.UniqueId == obj.UniqueId);
                obj.Id = tracked.Id;
                if (tracked == null)
                    return new InstrumentDetails();

                var entry = _db.Entry(tracked);
                var originalValues = entry.OriginalValues.Clone();

                entry.CurrentValues.SetValues(obj);

                var changes = new StringBuilder();

                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified)
                    {
                        var oldVal = originalValues[prop.Metadata.Name]?.ToString() ?? "null";
                        var newVal = prop.CurrentValue?.ToString() ?? "null";

                        if (oldVal != newVal)
                            changes.AppendLine($"{prop.Metadata.Name}: '{oldVal}' → '{newVal}'");
                    }
                }

                if (changes.Length == 0)
                    return tracked;

                // Set modified tracking info
                tracked.ModifiedDate = DateTime.UtcNow;
                tracked.ModifiedBy = await _currentUserService.GetUsernameAsync();

                await _loggingService.LogUserAction(
                    UserName: tracked.ModifiedBy,
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Maintenance Schedule",
                    action: "Maintenance Schedule modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in MaintenanceSchedule.UpdateAsync");
                return new InstrumentDetails();
            }
        }

    }
}
