using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.ItemsRepository
{
    public class FactorRepository : Repository<Factor>, IFactorRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public FactorRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService
        ) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public override async Task<Factor> UpdateAsync(Factor obj)
        {
            try
            {
                var tracked = await _db.Factors.FirstOrDefaultAsync(c => c.Id == obj.Id);
                if (tracked == null)
                    return new Factor();

                var entry = _db.Entry(tracked);
                var originalValues = entry.OriginalValues.Clone();

                // Apply new values
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
                    screen: "Factor Master",
                    action: "Factor data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in FactorRepository.UpdateAsync");
                return new Factor();
            }
        }

    }
}
