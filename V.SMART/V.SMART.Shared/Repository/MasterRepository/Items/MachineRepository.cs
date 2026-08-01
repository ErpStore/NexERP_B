using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.ItemsRepository
{
    public class MachineRepository : Repository<Machine>, IMachineRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public MachineRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService
        ) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public override async Task<Machine> UpdateAsync(Machine obj)
        {
            try
            {
                var tracked = await _db.Machine.FirstOrDefaultAsync(m => m.MachineId == obj.MachineId);
                if (tracked == null)
                    return new Machine();

                var originalValues = _db.Entry(tracked).OriginalValues.Clone();
                var changes = new StringBuilder();

                // Apply updated values
                _db.Entry(tracked).CurrentValues.SetValues(obj);

                // Compare and log changes
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

                // Preserve creation fields
                tracked.CreatedBy = obj.CreatedBy ?? tracked.CreatedBy;
                tracked.CreatedDate = obj.CreatedDate ?? tracked.CreatedDate;

                tracked.ModifiedBy = obj.ModifiedBy;
                tracked.ModifiedDate = DateTime.Now;

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Machine Master",
                    action: "Machine data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in MachineRepository.UpdateAsync");
                return new Machine();
            }
        }


    }
}
