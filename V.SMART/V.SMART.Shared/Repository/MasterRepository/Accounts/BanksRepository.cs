using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAccounts;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.Accounts
{
    public class BanksRepository : Repository<Banks>, IBanksRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public BanksRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public override async Task<Banks> UpdateAsync(Banks obj)
        {
            try
            {
                var tracked = await _db.Bank.FirstOrDefaultAsync(c => c.BankId == obj.BankId);
                if (tracked == null)
                    return new Banks();

                var entry = _db.Entry(tracked);
                var originalValues = entry.OriginalValues.Clone();

                // Apply updates
                _db.Entry(tracked).CurrentValues.SetValues(obj);

                // Track metadata
                tracked.ModifiedBy = await _currentUserService.GetUsernameAsync();
                tracked.ModifiedDate = DateTime.Now;

                // Detect and log changes
                var changes = new StringBuilder();
                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified)
                    {
                        var original = originalValues[prop.Metadata.Name]?.ToString() ?? "null";
                        var current = prop.CurrentValue?.ToString() ?? "null";

                        if (original != current)
                        {
                            changes.AppendLine($"{prop.Metadata.Name}: '{original}' → '{current}'");
                        }
                    }
                }

                // No changes? Exit early
                if (changes.Length == 0)
                    return tracked;

                await _loggingService.LogUserAction(
                    UserName: tracked.ModifiedBy,
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Bank Master",
                    action: "Bank updated",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in BankRepository.UpdateAsync");
                return new Banks();
            }
        }

    }
}
