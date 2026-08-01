using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IGeneralRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.GeneralRepository
{
    public class VendorRepository : Repository<Vendor>, IVendorRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public VendorRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<bool> ExistsByNameAsync(string vendorName, int? excludeId = null)
        {
            return await _db.Vendor.AnyAsync(v => v.VendorName == vendorName && (!excludeId.HasValue || v.VendorCode != excludeId.Value));
        }

        public override async Task<Vendor> UpdateAsync(Vendor obj)
        {
            try
            {
                var tracked = await _db.Vendor.FirstOrDefaultAsync(v => v.VendorCode == obj.VendorCode);
                if (tracked == null)
                    return new Vendor();

                var entry = _db.Entry(tracked);
                var originalValues = entry.OriginalValues.Clone();

                // Update modified date/user if needed
                tracked.ModifiedBy = await _currentUserService.GetUsernameAsync();
                tracked.ModifiedDate = DateTime.Now;

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

                if (changes.Length > 0)
                {
                    await _db.SaveChangesAsync();

                    await _loggingService.LogUserAction(
                        UserName: tracked.ModifiedBy,
                        Machine: _currentUserService.MachineName,
                        IP_Address: _currentUserService.IpAddress,
                        screen: "Vendor Master",
                        action: "Vendor data modified",
                        additionalInfo: changes.ToString()
                    );
                }

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "An error occurred while handling the Vendor UpdateAsync() method.");
                return new Vendor();
            }
        }


        public async Task<List<Vendor>> InstantSearchVendorsAsync(VendorVM search)
        {
            try
            {
                _db.Database.SetCommandTimeout(60);

                IQueryable<Vendor> query = _db.Vendor
                    .AsNoTracking()
                    .Include(i => i.State);

                if (!string.IsNullOrWhiteSpace(search.VendorName))
                {
                    var words = search.VendorName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var w in words)
                        query = query.Where(i => EF.Functions.Like(i.VendorName, $"%{w}%"));
                }

                if (!string.IsNullOrWhiteSpace(search.VendorAddress))
                {
                    var words = search.VendorAddress.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var w in words)
                        query = query.Where(i => EF.Functions.Like(i.VendorAddress, $"%{w}%"));
                }

                if (!string.IsNullOrWhiteSpace(search.GSTNo))
                    query = query.Where(i => EF.Functions.Like(i.GSTNo, $"%{search.GSTNo}%"));

                if (!string.IsNullOrWhiteSpace(search.PANNo))
                    query = query.Where(i => EF.Functions.Like(i.PANNo, $"%{search.PANNo}%"));

                if (!string.IsNullOrWhiteSpace(search.PinCode))
                    query = query.Where(i => EF.Functions.Like(i.PinCode, $"%{search.PinCode}%"));

                if (!string.IsNullOrWhiteSpace(search.BusiType))
                    query = query.Where(i => EF.Functions.Like(i.BusiType, $"%{search.BusiType}%"));

                if (!string.IsNullOrWhiteSpace(search.StateName))
                    query = query.Where(i => EF.Functions.Like(i.StateName, $"%{search.StateName}%"));

                //if (search.StateName)
                //    query = query.Where(i => i.StateCode == search.StateCode);

                if (!string.IsNullOrWhiteSpace(search.Email))
                    query = query.Where(i => EF.Functions.Like(i.Email, $"%{search.Email}%"));

                if (!string.IsNullOrWhiteSpace(search.ContactNo))
                    query = query.Where(i => EF.Functions.Like(i.ContactNo, $"%{search.ContactNo}%"));

                //if (search.Status.HasValue)
                //{
                //    query = query.Where(i => i.Inactive == search.Status.Value);
                //}

                if (!string.IsNullOrWhiteSpace(search.CreatedBy))
                    query = query.Where(i => EF.Functions.Like(i.CreatedBy, $"%{search.CreatedBy}%"));

                if (!string.IsNullOrWhiteSpace(search.ModifiedBy))
                    query = query.Where(i => EF.Functions.Like(i.ModifiedBy, $"%{search.ModifiedBy}%"));

                return await query
                    .OrderBy(i => i.VendorName)
                    .Take(100)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in VendorRepository.SearchVendorsAsync");
                return new List<Vendor>();
            }
        }


    }
}
