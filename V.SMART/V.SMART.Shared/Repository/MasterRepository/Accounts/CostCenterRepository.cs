using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAccounts;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.Accounts
{
    public class CostCenterRepository : Repository<CostCenter>, ICostCenterRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public CostCenterRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<string?> GetLastProjectNoByTypeAsync(int typeOfProjectId)
        {
            var lastProjectNo = await _db.CostCenter
                .Where(c => c.ProjectTypeId == typeOfProjectId)
                .OrderByDescending(c => c.Id)
                .Select(c => c.ProjectNo)
                .FirstOrDefaultAsync();

            return lastProjectNo;
        }

        public async Task<CostCenter?> GetCostCenterWithAllRelatedDataAsync(int id)
        {
            return await _db.CostCenter
                .Include(c => c.Customer)
                .Include(c => c.ProjectTypeMaster)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<IEnumerable<CostCenter>> GetAllAsync(bool asNoTracking = false)
        {
            var query = _db.CostCenter
                .Include(i => i.Customer);
            return asNoTracking ? await query.AsNoTracking().ToListAsync() : await query.ToListAsync();
        }

        public override async Task<CostCenter> UpdateAsync(CostCenter obj)
        {
            try
            {
                var tracked = await _db.CostCenter.FirstOrDefaultAsync(s => s.Id == obj.Id);

                if (tracked == null)
                    return new CostCenter();

                var originalValues = _db.Entry(tracked).OriginalValues.Clone();
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

                tracked.CreatedBy = obj.CreatedBy ?? tracked.CreatedBy;
                tracked.CreatedDate = obj.CreatedDate ?? tracked.CreatedDate;

                tracked.ModifiedBy = obj.ModifiedBy;
                tracked.ModifiedDate = DateTime.Now;

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Store Master",
                    action: "Store data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in CostCenterRepository.UpdateAsync");
                return new CostCenter();
            }
        }
    }
}
