using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Planning;
using V.SMART.Shared.Repository.IRepository.IPlanningRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.PlanningRepository
{
    public class ApprovalHistoryRepository : Repository<ApprovalHistory>, IApprovalHistoryRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public ApprovalHistoryRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public override async Task<ApprovalHistory> UpdateAsync(ApprovalHistory obj)
        {
            try
            {
                var tracked = await _db.ApprovalHistory.FirstOrDefaultAsync(c => c.Id == obj.Id);
                if (tracked == null)
                    return null;

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
                    return tracked;

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Approval History",
                    action: "Approval history updated",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in {nameof(UpdateAsync)} (ApprovalHistory)");
                return null;
            }
        }
    }
}
