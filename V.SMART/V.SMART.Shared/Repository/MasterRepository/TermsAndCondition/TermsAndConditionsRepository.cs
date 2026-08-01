using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.ITermsAndConditions;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.MasterRepository.TermsAndCondition
{
    public class TermsAndConditionsRepository : Repository<TermsAndConditions>, ITermsAndConditionsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public TermsAndConditionsRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public override async Task<TermsAndConditions> UpdateAsync(TermsAndConditions obj)
        {
            try
            {
                var tracked = await _db.TermsAndConditions.FirstOrDefaultAsync(tc => tc.Id == obj.Id);
                if (tracked == null)
                    return new TermsAndConditions(); // Or throw exception if required

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
                    screen: "Terms and Conditions Master",
                    action: "Terms and Conditions updated",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in {nameof(UpdateAsync)} (Terms and Conditions)");
                return new TermsAndConditions();
            }
        }
    }

}
