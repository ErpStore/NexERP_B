using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.HumanResourceMaster
{
    public class StaffFamilyDetailRepository : Repository<StaffFamilyDetails>, IStaffFamilyDetailRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public StaffFamilyDetailRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<bool> ExistsByNameAsync(string Name, int? excludeId = null)
        {
            return await _db.StaffFamilyDetail.AnyAsync(c => c.Name == Name && (!excludeId.HasValue || c.Slno != excludeId.Value));
        }
        public async Task DeleteByStaffIdAsync(int staffId)
        {
            var families = await _db.StaffFamilyDetail
                .Where(f => f.StaffID == staffId)
                .ToListAsync();

            if (families.Any())
            {
                _db.StaffFamilyDetail.RemoveRange(families);
            }
        }

        public override async Task<StaffFamilyDetails> UpdateAsync(StaffFamilyDetails obj)
        {
            try
            {
                var tracked = await _db.StaffFamilyDetail.FirstOrDefaultAsync(s => s.Slno == obj.Slno);

                if (tracked == null)
                    return new StaffFamilyDetails();

                var originalValues = _db.Entry(tracked).OriginalValues.Clone();

                // Set updated values to the tracked entity
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
                    return tracked; // No changes detected

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "StaffFamilyDetails Master",
                    action: "StaffFamilyDetails data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in StaffFamilyDetailsRepository.UpdateAsync");
                return new StaffFamilyDetails();
            }
        }


    }
}
