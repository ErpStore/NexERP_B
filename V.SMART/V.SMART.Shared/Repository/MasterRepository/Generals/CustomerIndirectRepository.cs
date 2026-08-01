using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IGeneralRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.GeneralRepository
{
    public class CustomerIndirectRepository : Repository<CustomerIndirect>, ICustomerIndirectRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public CustomerIndirectRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task DeleteByCustIdAsync(int custId)
        {
            var consinee = await _db.CustomerIndirect
                .Where(e => e.CustId == custId)
                .ToListAsync();

            if (consinee.Any())
            {
                _db.CustomerIndirect.RemoveRange(consinee);
            }
        }
        public override async Task<CustomerIndirect> UpdateAsync(CustomerIndirect obj)
        {
            try
            {
                var tracked = await _db.CustomerIndirect
                    .FirstOrDefaultAsync(c => c.AltCustId == obj.AltCustId);

                if (tracked == null)
                    return new CustomerIndirect();

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


                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Customer Indirect Master",
                    action: "Customer Indirect data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in CustomerIndirectRepository.UpdateAsync");
                return new CustomerIndirect();
            }
        }

    }
}
