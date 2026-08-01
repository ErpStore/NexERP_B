using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResource_Master;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.HumanResourceMaster
{
    public class HolidayListRepository : Repository<HolidayList>, IHolidayListRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public HolidayListRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }


        public override async Task<HolidayList> UpdateAsync(HolidayList obj)
        {
            try
            {
                var tracked = await _db.HolidayList.FirstOrDefaultAsync(c => c.Id == obj.Id);
                if (tracked == null)
                    return new HolidayList();

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
                    screen: "Holiday List",
                    action: "Holiday List modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in HolidayList.UpdateAsync");
                return new HolidayList();
            }
        }

    }
}
