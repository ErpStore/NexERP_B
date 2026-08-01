using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IGenerals;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.Generals
{
    public class VendorContactRepository : Repository<VendorContact>, IVendorContactRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public VendorContactRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }
        public async Task<bool> ExistsByNameAsync(string ContactPerson, int? excludeId = null)
        {
            return await _db.VendorContact.AnyAsync(c => c.ContactPerson == ContactPerson && (!excludeId.HasValue || c.Id != excludeId.Value));
        }
        public override async Task<VendorContact> UpdateAsync(VendorContact obj)
        {
            try
            {
                var tracked = await _db.VendorContact
                    .FirstOrDefaultAsync(c => c.Id == obj.Id);

                if (tracked == null)
                    return new VendorContact();

                var entry = _db.Entry(tracked);
                var originalValues = entry.OriginalValues.Clone();

                // Apply new values
                entry.CurrentValues.SetValues(obj);


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
                    return tracked;

                await _db.SaveChangesAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "VendorContact Master",
                    action: "VendorContact data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in VendorContactRepository.UpdateAsync");
                return new VendorContact();
            }
        }

    }
}
