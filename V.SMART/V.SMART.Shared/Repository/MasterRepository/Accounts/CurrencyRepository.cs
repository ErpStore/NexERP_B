using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAccountsRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.ExpenseRepository
{
    public class CurrencyRepository : Repository<Currency>, ICurrencyRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public CurrencyRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public override async Task<Currency> UpdateAsync(Currency obj)
        {
            try
            {
                var tracked = await _db.Currency.FirstOrDefaultAsync(c => c.CurrId == obj.CurrId);
                if (tracked == null)
                    return new Currency();

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
                    action: "Currency data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in CurrencyRepository.UpdateAsync");
                return new Currency();
            }
        }

    }
}
