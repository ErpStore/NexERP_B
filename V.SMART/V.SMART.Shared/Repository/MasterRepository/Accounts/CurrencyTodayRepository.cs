using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAccounts;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.Accounts
{
    public class CurrencyTodayRepository : Repository<CurrencyToday>, ICurrencyTodayRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public CurrencyTodayRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public override async Task<CurrencyToday> UpdateAsync(CurrencyToday obj)
        {
            try
            {
                var tracked = await _db.CurrencyToday.FirstOrDefaultAsync(c => c.Id == obj.Id);
                if (tracked == null)
                    return new CurrencyToday();

                var entry = _db.Entry(tracked);
                var originalValues = entry.OriginalValues.Clone();

                // Apply updated values
                _db.Entry(tracked).CurrentValues.SetValues(obj);
                tracked.ModifiedBy = await _currentUserService.GetUsernameAsync();
                tracked.ModifiedDate = DateTime.Now;

                // Detect changes
                var changes = new StringBuilder();
                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified)
                    {
                        var oldValue = originalValues[prop.Metadata.Name]?.ToString() ?? "null";
                        var newValue = prop.CurrentValue?.ToString() ?? "null";

                        if (oldValue != newValue)
                        {
                            changes.AppendLine($"{prop.Metadata.Name}: '{oldValue}' → '{newValue}'");
                        }
                    }
                }

                if (changes.Length == 0)
                    return tracked; // No actual changes

                // Log the update
                await _loggingService.LogUserAction(
                    UserName: tracked.ModifiedBy,
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Currency Master",
                    action: "CurrencyToday data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in CurrencyTodayRepository.UpdateAsync");
                return new CurrencyToday();
            }
        }

    }
}
