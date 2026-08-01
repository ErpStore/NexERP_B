using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IGenerals;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.Generals
{
    public class VendorInDirectRepository : Repository<VendorInDirect>, IVendorInDirectRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public VendorInDirectRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }
        public async Task<bool> ExistsByNameAsync(string AltVendorName, int? excludeId = null)
        {
            return await _db.VendorInDirect.AnyAsync(c => c.AltVendorName == AltVendorName && (!excludeId.HasValue || c.AltVendorCode != excludeId.Value));
        }

        public override async Task<VendorInDirect> UpdateAsync(VendorInDirect obj)
        {
            try
            {
                var tracked = await _db.VendorInDirect
                    .Include(v => v.Vendor) // To compare Vendor.VendorName
                    .FirstOrDefaultAsync(c => c.AltVendorCode == obj.AltVendorCode);

                if (tracked == null)
                    return new VendorInDirect();

                var entry = _db.Entry(tracked);
                var originalValues = entry.OriginalValues.Clone();

                // Apply updated values
                entry.CurrentValues.SetValues(obj);

                var changes = new StringBuilder();

                // Track property-level changes
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

                // Handle navigation property comparison (VendorName)
                if (tracked.Vendor?.VendorName != obj.Vendor?.VendorName)
                {
                    var oldName = tracked.Vendor?.VendorName ?? "null";
                    var newName = obj.Vendor?.VendorName ?? "null";
                    changes.AppendLine($"VendorName: '{oldName}' → '{newName}'");
                }

                if (changes.Length == 0)
                    return tracked;


                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "VendorIndirect Master",
                    action: "VendorIndirect data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in VendorInDirectRepository.UpdateAsync");
                return new VendorInDirect();
            }
        }

    }
}
