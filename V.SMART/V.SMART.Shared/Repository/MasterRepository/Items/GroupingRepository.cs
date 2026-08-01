using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.ItemsRepository
{
    public class GroupingRepository : Repository<Grouping>, IGroupingRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public GroupingRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService
        ) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<(string Prefix, string FRCNo)> GetInitialPrefixAndFRCNoAsync()
        {
            var lastRecord = await _db.Grouping
                .OrderByDescending(g => g.ID)
                .FirstOrDefaultAsync();

            if (lastRecord != null)
            {
                if (int.TryParse(lastRecord.FRCNo, out int currentNo))
                {
                    return (lastRecord.Prefix, (currentNo + 1).ToString("D3"));
                }
                else
                {
                    return (lastRecord.Prefix, "001");
                }
            }
            return ("F-RC", "001");
        }

        public override async Task<Grouping> UpdateAsync(Grouping obj)
        {
            try
            {
                var tracked = await _db.Grouping.FirstOrDefaultAsync(g => g.ID == obj.ID);
                if (tracked == null)
                    return new Grouping();

                var originalValues = _db.Entry(tracked).OriginalValues.Clone();
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
                    return tracked; // No change detected

                // Preserve created fields
                tracked.CreatedBy = obj.CreatedBy ?? tracked.CreatedBy;
                tracked.CreatedDate = obj.CreatedDate ?? tracked.CreatedDate;

                tracked.ModifiedBy = await _currentUserService.GetUsernameAsync();
                tracked.ModifiedDate = DateTime.UtcNow;


                await _loggingService.LogUserAction(
                    UserName: tracked.ModifiedBy,
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Grouping Master",
                    action: "Grouping data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GroupingRepository.UpdateAsync");
                return new Grouping();
            }
        }

    }
}
