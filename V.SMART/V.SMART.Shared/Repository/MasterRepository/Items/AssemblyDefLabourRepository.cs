using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItems;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.Items
{
    public class AssemblyDefLabourRepository : Repository<AssemblyDefLabour>, IAssemblyDefLabourRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public AssemblyDefLabourRepository(
         ApplicationDbContext db,
         ILoggingService loggingService,
         CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<bool> ExistsAsync(int assemblyId, int itemId)
        {
            return await _db.AssmblyDefLabour
                .AnyAsync(x => x.AssmblyID == assemblyId && x.ItemId == itemId);
        }

        public async Task<AssemblyDefLabour?> GetByAssmblyIdAsync(int assmblyId)
        {
            return await _db.AssmblyDefLabour
              .FirstOrDefaultAsync(x => x.AssmblyID == assmblyId);
        }

        public async Task<int> GetNextSlNoAsync(int assemblyId)
        {
            int? maxSlNo = await _db.AssmblyDefLabour
               .Where(x => x.AssmblyID == assemblyId)
               .MaxAsync(x => (int?)x.SlNo);

            return (maxSlNo ?? 0) + 1;
        }
        public override async Task<IEnumerable<AssemblyDefLabour>> GetAllAsync(bool asNoTracking = false)
        {
            return await _db.AssmblyDefLabour
                .AsNoTracking()
                .Include(a => a.AssemblyItem)
                .Include(a => a.PartItem)
                .Include(a => a.Process)
                .ToListAsync();
        }
        public override async Task<AssemblyDefLabour> UpdateAsync(AssemblyDefLabour obj)
        {
            try
            {
                var tracked = await _db.AssmblyDefLabour.FirstOrDefaultAsync(c => c.Id == obj.Id);
                if (tracked == null)
                    return new AssemblyDefLabour();

                var entry = _db.Entry(tracked);
                var originalValues = entry.OriginalValues.Clone();

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
                    return tracked; // No changes to save

                tracked.ModifiedDate = DateTime.UtcNow;
                tracked.ModifiedBy = await _currentUserService.GetUsernameAsync();

                await _loggingService.LogUserAction(
                    UserName: tracked.ModifiedBy,
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "BOM Master",
                    action: "BOM data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in {nameof(UpdateAsync)} (BOM)");
                return new AssemblyDefLabour();
            }
        }


    }
}
