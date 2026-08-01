using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.ICompany;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.Company
{
    public class CompanyRepository : Repository<Companydetails>, ICompanyRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public CompanyRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public override async Task<Companydetails> UpdateAsync(Companydetails obj)
        {
            try
            {
                var tracked = await _db.CompanyDetails
                    .Include(c => c.State)
                    .Include(c => c.Currency)
                    .FirstOrDefaultAsync(c => c.CompanyId == obj.CompanyId);

                if (tracked == null)
                    return new Companydetails();

                var entry = _db.Entry(tracked);
                var originalValues = entry.OriginalValues.Clone();

                // Apply new values from the incoming object
                entry.CurrentValues.SetValues(obj);
                tracked.ModifiedBy = await _currentUserService.GetUsernameAsync();
                tracked.ModifiedDate = DateTime.Now;

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

                // Optionally track related navigation properties like State and Currency
                if (tracked.State?.StateName != obj.State?.StateName)
                {
                    changes.AppendLine($"StateName: '{tracked.State?.StateName ?? "null"}' → '{obj.State?.StateName ?? "null"}'");
                }

                if (tracked.Currency?.CurrName != obj.Currency?.CurrName)
                {
                    changes.AppendLine($"CurrencyName: '{tracked.Currency?.CurrName ?? "null"}' → '{obj.Currency?.CurrName ?? "null"}'");
                }

                // If no changes detected, skip save/log
                if (changes.Length == 0)
                    return tracked;

                // Log the changes
                await _loggingService.LogUserAction(
                    UserName: tracked.ModifiedBy,
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Company Master",
                    action: "Company data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in CompanyRepository.UpdateAsync");
                return new Companydetails();
            }
        }



    }
}
