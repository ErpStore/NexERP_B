using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.ItemsRepository
{
    public class LeaveApplicationRepository : Repository<LeaveApplication>, ILeaveApplicationRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public LeaveApplicationRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public override async Task<IEnumerable<LeaveApplication>> GetAllAsync(bool AsNoTracking = false)
        {
            return await _db.LeaveApplication
                .AsNoTracking()
                .Include(i => i.Staff)
                .Include(i => i.LeaveType)
                .ToListAsync();
        }

        public async Task<List<LeaveApplication>> GetPendingByLevelAsync(string Level)
        {
            // Build the query dynamically based on the user's level.
            var query = _db.LeaveApplication
                .Include(l => l.Staff)
                .Include(l => l.LeaveType)
                .Where(l => l.LeaveStatus == "Pending");

            switch (Level)
            {
                case "level1":
                    query = query.Where(l => l.Level1 == true && l.Level1_Sign == null);
                    break;
                case "level2":
                    query = query.Where(l => l.Level2 == true && l.Level2_sign == null);
                    break;
                case "level3":
                    query = query.Where(l => l.Level3 == true && l.Level3_sign == null);
                    break;
                default:
                    // Return an empty list or handle the invalid level case.
                    return new List<LeaveApplication>();
            }

            return await query.ToListAsync();
        }

        public async Task<string> GenerateLeaveApplicationNoAsync()
        {
            var today = DateTime.Now;
            int startYear = today.Month >= 4 ? today.Year : today.Year - 1;
            string financialYear = $"{startYear % 100:D2}-{(startYear + 1) % 100:D2}";

            var lastRecord = await _db.LeaveApplication
                .Where(l => l.LeaveApplicationNo.EndsWith(financialYear))
                .OrderByDescending(l => l.LeaveAppID)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastRecord != null)
            {
                var numberPart = lastRecord.LeaveApplicationNo?.Split('/')[0];
                if (int.TryParse(numberPart, out int num))
                {
                    nextNumber = num + 1;
                }
            }

            return $"{nextNumber}/{financialYear}";
        }

        public override async Task<LeaveApplication> UpdateAsync(LeaveApplication obj)
        {
            try
            {
                var tracked = await _db.LeaveApplication
                    .FirstOrDefaultAsync(l => l.LeaveAppID == obj.LeaveAppID);

                if (tracked == null)
                    return new LeaveApplication();

                var entry = _db.Entry(tracked);
                var originalValues = entry.OriginalValues.Clone();

                // Apply new values from incoming object
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
                    return tracked; // No changes detected

                tracked.ModifiedDate = DateTime.UtcNow;
                tracked.ModifiedBy = await _currentUserService.GetUsernameAsync();

                await _loggingService.LogUserAction(
                    UserName: tracked.ModifiedBy,
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Leave Application",
                    action: "Leave Application modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in {nameof(UpdateAsync)} (LeaveApplication)");
                return new LeaveApplication();
            }
        }
    }

}
