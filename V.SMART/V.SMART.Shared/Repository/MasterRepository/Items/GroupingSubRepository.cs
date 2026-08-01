using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.ItemsRepository
{
    public class GroupingSubRepository : Repository<GroupingSub>, IGroupingSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public GroupingSubRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService
        ) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public override async Task<GroupingSub> UpdateAsync(GroupingSub obj)
        {
            try
            {
                var tracked = await _db.GroupingSub
                    .Include(g => g.Factor)
                    .Include(g => g.Process)
                    .FirstOrDefaultAsync(g => g.GroupingId == obj.GroupingId);

                if (tracked == null)
                    return new GroupingSub();

                // Clone original values before update
                var originalValues = _db.Entry(tracked).OriginalValues.Clone();

                // Set new values
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

                // Additional navigation property comparisons
                var originalFactor = tracked.Factor?.Factors;
                var newFactor = obj.Factor?.Factors;
                if (originalFactor != newFactor)
                    changes.AppendLine($"Factor: '{originalFactor}' → '{newFactor}'");

                var originalProcess = tracked.Process?.ProcessName;
                var newProcess = obj.Process?.ProcessName;
                if (originalProcess != newProcess)
                    changes.AppendLine($"Process: '{originalProcess}' → '{newProcess}'");

                if (changes.Length == 0)
                    return tracked; // No change detected


                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Grouping Sub Master",
                    action: "Sub grouping modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GroupingSubRepository.UpdateAsync");
                return new GroupingSub();
            }
        }


        public async Task<List<ProcessFlowChart>> GetProcessFlowChartsByGroupingId(int groupingId)
        {
            var result = await(from gs in _db.GroupingSub
                               join p in _db.Process on gs.Processid equals p.ProcessId
                               where gs.GroupingId == groupingId
                               select new ProcessFlowChart
                               {
                                   ProcId = p.ProcessId,
                                   RmWt = 0,
                                   CompWt = 0,
                                   ScrapWt = 0,
                                   ScrapEndBit = null,
                                   MachId = null,
                                   VendorCode = null,
                                   UnitPrice = 0,
                                   Program = string.Empty,

                                   // Updated for TimeSpan:
                                   CycleTime = TimeSpan.Zero,
                                   SettingTime = TimeSpan.Zero,

                                   BOM = false,
                                   Inspection = false,
                                   ProcessSkip = false,
                                   Remarks = string.Empty
                               }).ToListAsync();

            // Assign SlNo (1,2,3...)
            for (int i = 0; i < result.Count; i++)
            {
                result[i].SlNo = i + 1;
            }

            return result;
        }



    }
}
